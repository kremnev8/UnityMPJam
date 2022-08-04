using System;
using Gameplay.Controllers.Player;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;

namespace Gameplay.Logic
{
    public class TriggerAddItem : NetworkBehaviour, ITimeLinked
    {
        public SpriteRenderer itemRenderer;
        
        public string itemId;
        private bool isUsed;
        
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (isUsed) return;
            
            if (isServer && col != null)
            {
                Inventory inventory = col.GetComponent<Inventory>();
                if (inventory != null)
                {
                    if (inventory.AddItem(itemId))
                    {
                        isUsed = true;
                        RpcHideItem();
                    }
                }
            }
        }

        [ClientRpc]
        private void RpcHideItem()
        {
            itemRenderer.enabled = false;
        }

        public SpaceTimeObject timeObject { get; set; }
        public void Configure(ObjectState state)
        {
            if (state == ObjectState.EXISTS)
            {
                isUsed = false;
                itemRenderer.enabled = true;
            }
            else
            {
                isUsed = true;
                itemRenderer.enabled = false;
            }
            
        }

        public void ReciveTimeEvent(int[] args)
        {
        }

        public void ReciveStateChange(bool value, bool isPermanent)
        {
        }
    }
}