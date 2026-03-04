using Core;

using Input;

using Unity.Entities;

using UnityEngine;

namespace Shaders
{
    [UpdateInGroup(typeof(InputSystemGroup))]
    partial class CodePointerSystem : PointerSystem, ICustomizable<CodePointerSettings>
    {
        public bool IsActive => TypeSettings && PlayerCamera;
        public CodePointerSettings TypeSettings => Settings as CodePointerSettings;

        protected override void Proceed()
        {
            if (!IsActive)
                return;

            base.Proceed();
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