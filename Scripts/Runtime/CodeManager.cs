using System.Collections.Generic;
using System.IO;

using TMPro;

using UnityEditor;

using UnityEngine;

namespace Shaders
{
    public class CodeManager : MonoBehaviour
    {
        [SerializeField] bool AutoRecompile = false;
        [SerializeField] float RecompilePeriod = 5f;
        [SerializeField] string SavePath;

        [Space]
        [SerializeField] RectTransform Content;
        [SerializeField] GameObject LinePrefab;

        [Space]
        [SerializeField] string[] StartingLines;

        bool LastBranch = false;
        bool Recompiled = false;
        int CurrentFormatLine = 0;
        int CurrentTab = 0;
        public float LastInputTime = 0f;

        List<string> Code = new List<string>();
        List<TMP_InputField> Input = new List<TMP_InputField>();

        void Start()
        {
            for (int s = 0; s < StartingLines.Length; s++)
                AddLine(StartingLines[s]);
        }
        void Update()
        {
            if (Code.Count == 0)
                return;

            Format();

            if (AutoRecompile)
                Recompile();
        }

        void Format()
        {
            var line = Code[CurrentFormatLine];



            Code[CurrentFormatLine] = line;
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
            if (Code.Count == 0 ||
                 string.IsNullOrEmpty(SavePath))
                return;

            var name = Code[0]
                .Replace("Shader", "")
                .Replace("Custom/", "")
                .Replace("\"", "")
                .Trim();

            if (string.IsNullOrEmpty(name))
                return;

            var text = "";
            for (int c = 0; c < Code.Count; c++)
            {
                var input = Input[c];
                if (!input)
                    continue;
                
                text += input.text + "\n";
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
        void AddLine(string text)
        {
            if (!LinePrefab ||
                 !Content)
                return;

            var go = Instantiate(LinePrefab, Content);
            PrefabUtility.ConvertToPrefabInstance(
                go,
                LinePrefab,
                new ConvertToPrefabInstanceSettings { },
                InteractionMode.AutomatedAction);

            var line = go.GetComponent<TMP_InputField>();
            line.text = text;

            Code.Add(text);
            Input.Add(line);
        }
    }
}