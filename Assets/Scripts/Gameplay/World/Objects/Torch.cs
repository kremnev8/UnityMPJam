using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gameplay.World
{
    public class Torch : Interactible
    {
        public new Light2D light;

        protected override void SetState(bool newState)
        {
            base.SetState(newState);
            light.enabled = newState;
        }

        public override void Activate()
        {
            CmdSetState(!state);
        }
    }
}