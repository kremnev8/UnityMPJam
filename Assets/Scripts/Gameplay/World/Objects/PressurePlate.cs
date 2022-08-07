using System;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

namespace Gameplay.World
{
    public class PressurePlate : Interactible
    {
        public List<Collider2D> inside = new List<Collider2D>();
        public float disableDelay;
        public float reactivateDelay;
        public int minMass = 10;

        private float sinceLastActive;

        public RandomAudioSource audioSource;

        protected override void Start()
        {
            base.Start();
            sinceLastActive = reactivateDelay;
        }


        [Server]
        private void UpdateState(bool newState)
        {
            if (state != newState)
            {
                if (sinceLastActive < reactivateDelay && newState)
                {
                    Invoke(nameof(EnableLater), reactivateDelay - sinceLastActive);
                    return;
                } 
                
                if (!newState && disableDelay > 0.1f)
                {
                    Invoke(nameof(DisableLater), disableDelay);
                }
                else
                {
                    SetState(newState);
                    RpcSetState(newState);
                }

                if (newState)
                {
                    sinceLastActive = 0;
                }
            }
        }

        public override void RpcSetState(bool newState)
        {
            base.RpcSetState(newState);
            audioSource.Play();
        }

        private void Update()
        {
            if (isServer)
            {
                sinceLastActive += Time.deltaTime;
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
        
        
        [Server]
        private void EnableLater()
        {
            if (inside.Count != 0)
            {
                SetState(true);
                RpcSetState(true);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (isServer)
            {
                if (!interactible) return;

                IHeavyObject heavyObject = col.GetComponent<IHeavyObject>();
                if (heavyObject == null || heavyObject.Mass < minMass)
                {
                    return;
                }
                
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
                
                IHeavyObject heavyObject = col.GetComponent<IHeavyObject>();
                if (heavyObject == null || heavyObject.Mass < minMass)
                {
                    return;
                }
                
                if (inside.Contains(col))
                {
                    inside.Remove(col);
                }

                UpdateState(inside.Count > 0);
            }
        }
    }
}