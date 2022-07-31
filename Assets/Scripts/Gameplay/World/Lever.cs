using UnityEngine;
using UnityEngine.Events;
using Util;

namespace Gameplay.World
{
    public class Lever : VisualState, IInteractable
    {
        public Vector2Int forward;
        public Vector2Int FacingDirection => forward;
        
        public void Activate()
        {
            SetState(!state);
        }
    }
}