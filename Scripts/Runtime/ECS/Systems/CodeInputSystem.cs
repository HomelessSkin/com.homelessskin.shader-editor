using Core;

using Input;

using Unity.Entities;

namespace Shaders
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