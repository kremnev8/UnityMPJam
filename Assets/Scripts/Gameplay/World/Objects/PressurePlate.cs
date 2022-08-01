using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public class PressurePlate : Interactible
    {
        public List<Collider2D> inside = new List<Collider2D>();
        public float disableDelay;

        [Server]
        private void UpdateState(bool newState)
        {
            if (state != newState)
            {
                if (!newState && disableDelay > 0.1f)
                {
                    Invoke(nameof(DisableLater), disableDelay);
                }
                else
                {
                    SetState(newState);
                    RpcSetState(newState);
                }
            }
        }
        
        [Server]
        private void DisableLater()
        {
            if (inside.Count == 0)
            {
                SetState(false);
                RpcSetState(false);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (isServer)
            {
                if (!interactible) return;
                
                if (!inside.Contains(col))
                {
                    inside.Add(col);
                }

                UpdateState(inside.Count > 0);
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (isServer)
            {
                if (!interactible) return;
                
                if (inside.Contains(col))
                {
                    inside.Remove(col);
                }

                UpdateState(inside.Count > 0);
            }
        }
    }
}