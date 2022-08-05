using Gameplay.Conrollers;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace Gameplay.World
{
    public class Lever : Interactible
    {

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