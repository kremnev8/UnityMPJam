using Gameplay.Conrollers;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace Gameplay.World
{
    [SelectionBase]
    public class Lever : Interactible
    {
        public RandomAudioSource audioSource;
    
        public override void RpcSetState(bool newState)
        {
            base.RpcSetState(newState);
            audioSource.Play();
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