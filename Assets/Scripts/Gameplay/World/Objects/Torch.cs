﻿using Gameplay.Conrollers;
using Gameplay.Util;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gameplay.World
{
    [SelectionBase]
    public class Torch : Interactible
    {
        public bool m_checkFacing = true;
        public override bool checkFacing => m_checkFacing;
        public DeferredAnimatorEnabler[] fireRenderers;
        public new Light2D light;

        public RandomAudioSource audioSource;
        
        protected override void SetState(bool newState )
        {
            base.SetState(newState);
            light.enabled = newState;
            foreach (DeferredAnimatorEnabler fireRenderer in fireRenderers)
            {
                fireRenderer.SetState(newState);
            }

            if (newState)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
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