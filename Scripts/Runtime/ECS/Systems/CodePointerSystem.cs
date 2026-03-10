#if UNITY_EDITOR
using Core;

using Input;

using Unity.Entities;

namespace ShaderEditor
{
    [UpdateInGroup(typeof(InputSystemGroup))]
    partial class CodePointerSystem : PointerSystem, IGroupSystem<CodePointerSettings>
    {
        public bool IsActive => TypeSettings && PlayerCamera;
        public CodePointerSettings TypeSettings => Settings as CodePointerSettings;

        protected override void SetState(MouseState state)
        {
            if (!IsActive)
                return;

            base.SetState(state);
        }
        protected override void Proceed()
        {
            if (!IsActive)
                return;

            base.Proceed();
        }
        protected override void ClickAction()
        {
            base.ClickAction();

            Sys.Add_M(new OuterInput { Title = "Click" }, EntityManager);
        }
        protected override void DownScrollAction()
        {
            base.DownScrollAction();

            Sys.Add_M(new OuterInput { Title = "Wheel Down" }, EntityManager);
        }
        protected override void UpScrollAction()
        {
            base.UpScrollAction();

            Sys.Add_M(new OuterInput { Title = "Wheel Up" }, EntityManager);
        }
    }
}
#endif