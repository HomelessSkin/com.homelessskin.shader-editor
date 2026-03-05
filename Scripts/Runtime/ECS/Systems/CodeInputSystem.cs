#if UNITY_EDITOR
using Core;

using Input;

using Unity.Entities;

namespace ShaderEditor
{
    [UpdateInGroup(typeof(InputSystemGroup))]
    partial class CodeInputSystem : HandleSystem
    {
        protected override void OnCreate()
        {
            Group = "Code";

            base.OnCreate();
        }
    }
}
#endif