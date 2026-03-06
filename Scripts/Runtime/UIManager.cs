#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Core;

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
            public string SavePath;
            public float RecompilePeriod = 5f;

            bool AutoRecompile = false;
            bool Recompiled = false;

            float LastInputTime = 0f;

            [Space]
            public int ScrollPower = 10;
            public float ScrollSpeed = 10f;

            bool IsEditActive;
            bool BraceStep;

            int MinY;
            int MaxY;
            int CurrentTab = 0;

            Vector2 CodeContentShift;
            Vector2Int CodeContentCurrent;
            Vector2Int CodeContentTarget;

            Task FormatTask;

            [Space]
            public RectTransform Content;
            public RectTransform GhostLine;
            public GameObject LinePrefab;

            bool IsGhostActive;

            int GhostOrigin;

            GameObject GhostObject => GhostLine.gameObject;

            [Space]
            public string[] StartingLines;

            List<CodeLine> Input = new List<CodeLine>();

            public void Start()
            {
                FormatTask = Task.Delay(10);

                MinY = (int)Content.anchoredPosition.y - 40;
                CodeContentShift = Content.anchoredPosition;
                CodeContentTarget = CodeContentCurrent = Vector2Int.RoundToInt(CodeContentShift) + 40 * Vector2Int.down;

                for (int s = 0; s < StartingLines.Length; s++)
                    AddLine(s, StartingLines[s]);
            }
            public void Update()
            {
                CodeContentShift += Time.deltaTime * ScrollSpeed * (CodeContentTarget - CodeContentShift);
                CodeContentCurrent = Vector2Int.RoundToInt(CodeContentShift);

                if (Input.IsEmpty())
                    return;

                if (FormatTask.IsCompleted)
                {
                    if (BraceStep)
                        FormatTask = FormatBraces();
                    else
                        FormatTask = FormatByLine();

                    BraceStep = !BraceStep;
                }

                if (AutoRecompile)
                    Recompile();

                if (IsGhostActive)
                    MoveGhost();
            }
            public void SetAutoRecompile(bool value) => AutoRecompile = value;
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
            public void InitGhost(string input, int index = -1)
            {
                if (IsGhostActive)
                    return;

                IsGhostActive = true;
                GhostOrigin = index;

                GhostLine
                    .GetComponentInChildren<CodeLine>()
                    .Init(input);
                GhostLine
                    .gameObject
                    .SetActive(true);
            }
            public void CancelGhosting()
            {
                if (!IsGhostActive)
                    return;
                IsGhostActive = false;

                if (GhostOrigin >= 0)
                {
                    InsertLine(GetText(GhostObject), GhostOrigin);

                    GhostOrigin = -1;
                }

                GhostObject.SetActive(false);

                SetEditActive(false);
            }
            public void CopyToClipboard()
            {
                GUIUtility.systemCopyBuffer = CollectAllData();
            }
            public void Save()
            {
                if (Input.Count == 0 ||
                     string.IsNullOrEmpty(SavePath))
                    return;

                var name = GetText(Input[0])
                    .Replace("Shader", "")
                    .Replace("Custom/", "")
                    .Replace("\"", "")
                    .Trim();

                if (string.IsNullOrEmpty(name))
                    return;

                var text = CollectAllData();
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

            public async void SetEditActive(bool value)
            {
                await FormatTask;

                IsEditActive = value;

                if (!value)
                    LastInputTime = Time.realtimeSinceStartup;
            }
            public async void StartEdit(int index)
            {
                await FormatTask;

                Input[index].StartEdit();
            }
            public async void DropGhost()
            {
                if (!IsGhostActive)
                    return;
                GhostOrigin = -1;

                await FormatTask;

                var pos = (int)PointerSystem.Current.y;
                var content = CodeContentCurrent.y;
                var dif = (pos - content) / -20;

                if (dif >= Input.Count)
                    InsertLine(GetText(GhostObject), Input.Count);
                else if (dif < 0)
                    InsertLine(GetText(GhostObject), 0);
                else
                    InsertLine(GetText(GhostObject), dif);

                CancelGhosting();
            }
            public async void RemoveAt(int index)
            {
                if (IsGhostActive)
                    return;

                await FormatTask;

                Destroy(Input[index].GetParent().gameObject);
                Input.RemoveAt(index);

                SetEditActive(false);
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

            void AddLine(int index, string text)
            {
                if (!LinePrefab ||
                     !Content)
                    return;

                Input.Add(CreateLine(text, index));

                MaxY = GetMaxY();
            }
            void MoveGhost()
            {
                var pos = GhostLine.position;
                pos.y = (int)PointerSystem.Current.y;
                GhostLine.position = pos;
            }

            async void InsertLine(string text, int index)
            {
                await FormatTask;

                Input.Insert(index, CreateLine(text, index));

                MaxY = GetMaxY();
            }
            async Task FormatBraces()
            {
                if (IsEditActive)
                    return;

                CurrentTab = 0;
                for (int i = 0; i < Input.Count; i++)
                {
                    var line = Input[i];
                    var text = GetText(line).Trim();

                    if (text.Contains("}"))
                        CurrentTab--;

                    line.SetText(GetTab() + text);

                    if (text.Contains("{"))
                        CurrentTab++;
                }

                await Task.Yield();
            }
            async Task FormatByLine()
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

                if (!IsEditActive)
                {
                    // TODO: format text here
                    //var text = line.GetText();
                }

                await Task.Yield();
            }

            int GetMaxY()
            {
                if (Input.Count == 0)
                    return MinY;

                return 20 * (Input.Count - Screen.height / 20 + 3) + MinY + 40;
            }
            string GetText(CodeLine line) => line.GetText();
            string GetText(GameObject gameObject) => gameObject.GetComponentInChildren<CodeLine>().GetText();
            string GetTab()
            {
                var tab = "";
                for (int t = 0; t < CurrentTab; t++)
                    tab += "    ";

                return tab;
            }
            string CollectAllData()
            {
                var text = "";
                for (int c = 0; c < Input.Count; c++)
                {
                    var input = Input[c];
                    if (!input)
                        continue;

                    text += GetText(input) + "\n";
                }

                return text;
            }
            CodeLine CreateLine(string text, int index)
            {
                var go = Instantiate(LinePrefab, Content);
                var line = go.GetComponentInChildren<CodeLine>();
                line.Init(text, index);
                line.GetParent().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Content.rect.width);

                return line;
            }
        }

        public void ScrollContentDown() => _Codex.ScrollContentDown();
        public void ScrollContentUp() => _Codex.ScrollContentUp();
        public void SetAutoRecompile(bool value) => _Codex.SetAutoRecompile(value);
        public void StartEditMessage(OuterInput input, Command command)
        {
            _Codex.SetEditActive(true);
            _Codex.StartEdit(input.Index);
        }
        public void EndEditMessage() => _Codex.SetEditActive(false);
        public void StartCodeLineMove(OuterInput input, Command command)
        {
            _Codex.RemoveAt(input.Index);
            _Codex.InitGhost(input.ID, input.Index);
        }
        public void DuplicateCodeLine(OuterInput input, Command command) => _Codex.InitGhost(input.ID);
        public void RemoveCodeLine(OuterInput input, Command command) => _Codex.RemoveAt(input.Index);
        public void DropGhost() => _Codex.DropGhost();
        public void CancelGhosting() => _Codex.CancelGhosting();
        public void CopyToClipboard() => _Codex.CopyToClipboard();
        public void SaveShader() => _Codex.Save();
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