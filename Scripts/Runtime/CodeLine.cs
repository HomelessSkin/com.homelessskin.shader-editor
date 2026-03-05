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
        [SerializeField] TMP_Text IndexText;
        [SerializeField] TMP_Text Content;
        [SerializeField] TMP_InputField Input;

        [Space]
        [SerializeField] Image Buttons;

        int Index;
        string PureText;

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (Input)
            {
                Content.text = "";

                Input.text = PureText;
                Input.Select();
            }

            Sys.Add(new SetPointerStateRequest { State = MouseState.UI }, World.DefaultGameObjectInjectionWorld.EntityManager);
        }

        public void Init(string text, int index = -1)
        {
            Index = index;
            if (index >= 0)
                IndexText.text = index.ToString();

            SetText(text);
        }
        public void SetText(string text)
        {
            PureText = text;

            // TODO: format text with rich tags
            Content.text = text;

            if (Input)
                Input.text = "";
        }
        public void Move()
        {
            Sys.Add_M(new OuterInput { Title = "Code Line Move", ID = PureText, Index = Index, }, World.DefaultGameObjectInjectionWorld.EntityManager);
        }
        public void Shift(int index, Vector2 pivot)
        {
            Index = index;
            if (index >= 0)
                IndexText.text = index.ToString();

            Parent.anchoredPosition = pivot + new Vector2Int(0, -20 * index);
        }
        public string GetText() => PureText;
        public RectTransform GetParent() => Parent;
    }
}
#endif