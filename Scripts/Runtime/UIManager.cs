#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Input;

using UI;

using UnityEditor;

using UnityEngine;

namespace ShaderEditor
{
    public class UIManager : UIManagerBase
    {
        [Space]
        [SerializeField] Codex _Codex;
        #region CODEX
        [Serializable]
        class Codex
        {
            public bool AutoRecompile = false;
            public float RecompilePeriod = 5f;
            public string SavePath;

            [Space]
            public int ScrollPower = 10;
            public float ScrollSpeed = 10f;

            int MinY;
            int MaxY;

            Vector2 CodeContentShift;
            Vector2Int CodeContentCurrent;
            Vector2Int CodeContentTarget;

            Task FormatTask;

            [Space]
            public RectTransform Content;
            public RectTransform GhostLine;
            public GameObject LinePrefab;

            bool IsGhostActive;

            [Space]
            public string[] StartingLines;

            bool LastBranch = false;
            bool Recompiled = false;

            int CurrentTab = 0;
            public float LastInputTime = 0f;

            List<CodeLine> Input = new List<CodeLine>();

            public void Start()
            {
                MinY = (int)Content.anchoredPosition.y;
                CodeContentShift = Content.anchoredPosition;
                CodeContentTarget = CodeContentCurrent = Vector2Int.RoundToInt(CodeContentShift);

                for (int s = 0; s < StartingLines.Length; s++)
                    AddLine(s, StartingLines[s]);

                FormatTask = Task.Delay(100);
            }
            public void Update()
            {
                CodeContentShift += Time.deltaTime * ScrollSpeed * (CodeContentTarget - CodeContentShift);
                CodeContentCurrent = Vector2Int.RoundToInt(CodeContentShift);

                if (Input.Count == 0)
                    return;

                if (FormatTask.IsCompleted)
                    FormatTask = Format();

                if (AutoRecompile)
                    Recompile();

                if (IsGhostActive)
                    MoveGhost();
            }
            public void ScrollContentDown()
            {
                CodeContentTarget.y -= ScrollPower;
                if (CodeContentTarget.y < MinY)
                    CodeContentTarget.y = MinY;
            }
            public void ScrollContentUp()
            {
                CodeContentTarget.y += ScrollPower;
                if (CodeContentTarget.y > MaxY)
                    CodeContentTarget.y = MaxY;
            }
            public async void InitGhost(OuterInput input)
            {
                if (IsGhostActive)
                    return;
                IsGhostActive = true;

                GhostLine
                    .GetComponentInChildren<CodeLine>()
                    .Init(input.ID);
                GhostLine
                    .gameObject
                    .SetActive(true);

                await FormatTask;

                Destroy(Input[input.Index].GetParent().gameObject);
                Input.RemoveAt(input.Index);
            }

            void Recompile()
            {
                if (Recompiled &&
                     LastInputTime != 0f &&
                     LastInputTime + RecompilePeriod < Time.realtimeSinceStartup)
                {
                    Recompiled = false;
                    LastInputTime = 0f;
                }

                if (!Recompiled)
                {
                    Recompiled = true;

                    Save();
                }
            }
            void Save()
            {
                if (Input.Count == 0 ||
                     string.IsNullOrEmpty(SavePath))
                    return;

                var name = Input[0]
                    .GetText()
                    .Replace("Shader", "")
                    .Replace("Custom/", "")
                    .Replace("\"", "")
                    .Trim();

                if (string.IsNullOrEmpty(name))
                    return;

                var text = "";
                for (int c = 0; c < Input.Count; c++)
                {
                    var input = Input[c];
                    if (!input)
                        continue;

                    text += input.GetText() + "\n";
                }

                if (string.IsNullOrEmpty(text))
                    return;

                var dir = $"{Application.dataPath}/Resources/{SavePath}";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                Debug.Log(name);

                File.WriteAllText($"{dir}/{name}.shader", text);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            void AddLine(int index, string text)
            {
                if (!LinePrefab ||
                     !Content)
                    return;

                var go = Instantiate(LinePrefab, Content);
                var line = go.GetComponentInChildren<CodeLine>();
                line.Init(text, index);
                line.GetParent().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Content.rect.width);

                Input.Add(line);

                MaxY = GetMaxY();
            }
            void MoveGhost()
            {
                var pos = GhostLine.position;
                pos.y = (int)PointerSystem.Current.y;
                GhostLine.position = pos;
            }
            int GetMaxY()
            {
                if (Input.Count == 0)
                    return MinY;

                return 20 * (Input.Count - Screen.height / 20 + 3) + MinY;
            }

            async Task Format()
            {
                var list = new List<Task>();
                for (int i = 0; i < Input.Count; i++)
                    list.Add(FormatLine(i));

                await Task.WhenAll(list);
            }
            async Task FormatLine(int index)
            {
                var line = Input[index];
                line.Shift(index, CodeContentCurrent);

                // TODO: format text here
                //var text = line.GetText();

                await Task.Yield();
            }
        }

        public void ScrollContentDown() => _Codex.ScrollContentDown();
        public void ScrollContentUp() => _Codex.ScrollContentUp();
        public void StartCodeLineMove(OuterInput input, Command command) => _Codex.InitGhost(input);
        #endregion

        protected override void Awake()
        {
            LockFPS(1);

            base.Awake();

            _Codex.Start();
        }
        protected override void Update()
        {
            base.Update();

            _Codex.Update();
        }
    }
}
#endif