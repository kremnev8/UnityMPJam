using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.World
{
    public class PressurePlate : VisualState
    {
        public List<Collider2D> inside = new List<Collider2D>();
        public float disableDelay;
        
        public override void SetState(bool newState)
        {
            if (state != newState)
            {
                if (!newState && disableDelay > 0.1f)
                {
                    Invoke(nameof(DisableLater), disableDelay);
                }
                else
                {
                    base.SetState(newState);
                }
            }
        }

        private void DisableLater()
        {
            base.SetState(false);
        }
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!inside.Contains(col))
            {
                inside.Add(col);
            }

            SetState(inside.Count > 0);
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (inside.Contains(col))
            {
                inside.Remove(col);
            }
            
            SetState(inside.Count > 0);
        }
    }
}