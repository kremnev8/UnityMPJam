using UnityEngine;
using UnityEngine.Events;
using Util;

namespace Gameplay.World
{
    public class Lever : Interactible
    {
        
        public override void Activate()
        {
            CmdSetState(!state);
        }
    }
}