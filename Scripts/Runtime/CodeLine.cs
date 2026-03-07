#if UNITY_EDITOR
using Core;

using Input;

using TMPro;

using Unity.Entities;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ShaderEditor
{
    public class CodeLine : Selectable
    {
        [Space]
        [SerializeField] RectTransform Parent;

        [Space]
        [SerializeField] TMP_Text Content;
        [SerializeField] TMP_InputField Input;

        [Space]
        [SerializeField] TMP_Text[] Indices;

        bool IsEditStopped;
        int Index;
        int CaretPosition;
        string PureText;

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (Input)
                SetPointerStateUI();
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (Input)
                Sys.Add_M(new OuterInput { Title = "Code Line Edit", Index = Index }, World.DefaultGameObjectInjectionWorld.EntityManager);
        }

        public void Init(string text, int index = -1)
        {
            Index = index;
            if (index >= 0)
                SetIndices(index);

            SetText(text);
        }
        public void StartEdit()
        {
            Content.text = "";

            Input.text = PureText;
            Input.Select();
            Input.MoveToEndOfLine(false, false);
        }
        public void StopEdit()
        {
            IsEditStopped = true;

            Input.DeactivateInputField(false);
        }
        public void SetText(string text)
        {
            if (Input)
            {
                if (string.IsNullOrEmpty(Content.text))
                    CaretPosition = Input.caretPosition;

                Input.text = "";
            }

            PureText = text;

            // TODO: format text with rich tags
            Content.text = text;
        }
        public void EndEdit()
        {
            if (IsEditStopped)
                IsEditStopped = false;
            else
                Sys.Add_M(new OuterInput { Title = "Code Line Edit End" }, World.DefaultGameObjectInjectionWorld.EntityManager);
        }
        public void SetPointerStateUI() =>
                Sys.Add(new SetPointerStateRequest { State = MouseState.UI }, World.DefaultGameObjectInjectionWorld.EntityManager);
        public void Move() =>
            Sys.Add_M(new OuterInput { Title = "Code Line Move", ID = PureText, Index = Index, }, World.DefaultGameObjectInjectionWorld.EntityManager);
        public void Duplicate() =>
            Sys.Add_M(new OuterInput { Title = "Code Line Duplicate", ID = PureText }, World.DefaultGameObjectInjectionWorld.EntityManager);
        public void Remove() =>
            Sys.Add_M(new OuterInput { Title = "Code Line Remove", Index = Index }, World.DefaultGameObjectInjectionWorld.EntityManager);
        public void Shift(int index, Vector2 pivot)
        {
            Index = index;
            if (index >= 0)
                SetIndices(index);

            Parent.anchoredPosition = pivot + new Vector2Int(0, -20 * index);
        }
        public string GetText() => PureText;
        public string GetOverText()
        {
            var result = PureText.Substring(CaretPosition);
            SetText(PureText.Remove(CaretPosition));

            return result;
        }
        public RectTransform GetParent() => Parent;

        void SetIndices(int index)
        {
            if (Indices == null)
                return;

            var text = index.ToString();
            for (int i = 0; i < Indices.Length; i++)
                Indices[i].text = text;
        }
    }
}
#endif