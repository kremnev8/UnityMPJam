﻿using Gameplay.Conrollers;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gameplay.World
{
    public class Torch : Interactible
    {
        public new Light2D light;

        protected override void SetState(bool newState, bool isPermanent )
        {
            base.SetState(newState, isPermanent);
            light.enabled = newState;
        }

        public override void Activate(PlayerController player)
        {
            CmdSetState(!state, true);
        }
    }
}