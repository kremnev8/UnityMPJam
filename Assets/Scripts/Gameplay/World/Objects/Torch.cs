using Gameplay.Conrollers;
using Gameplay.Util;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gameplay.World
{
    public class Torch : Interactible
    {
        public DeferredAnimatorEnabler[] fireRenderers;
        public new Light2D light;

        protected override void SetState(bool newState )
        {
            base.SetState(newState);
            light.enabled = newState;
            foreach (DeferredAnimatorEnabler fireRenderer in fireRenderers)
            {
                fireRenderer.SetState(newState);
            }
        }

        public override void Activate(PlayerController player)
        {
            if (isClient)
            {
                CmdSetState(!state);
            }
            else
            {
                SetState(!state);
                RpcSetState(!state);
            }
        }
    }
}