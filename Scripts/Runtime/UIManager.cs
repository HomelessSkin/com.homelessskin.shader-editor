using UI;

using UnityEngine;

namespace Shaders
{
    public class UIManager : UIManagerBase
    {
        [Space]
        [SerializeField] float ScrollPower = 10f;
        [SerializeField] float ScrollSpeed = 10f;
        [SerializeField] RectTransform CodeContent;

        Vector2 CodeContentTarget;

        protected override void Awake()
        {
            base.Awake();

            CodeContentTarget = CodeContent.anchoredPosition;
        }
        protected override void Update()
        {
            base.Update();

            CodeContent.anchoredPosition += Time.deltaTime * ScrollSpeed * (CodeContentTarget - CodeContent.anchoredPosition);
        }

        public void ScrollContentDown() => CodeContentTarget.y -= ScrollPower;
        public void ScrollContentUp() => CodeContentTarget.y += ScrollPower;
    }
}