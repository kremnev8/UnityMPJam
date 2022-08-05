using System;
using Gameplay.Controllers.Player;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;

namespace Gameplay.Logic
{
    public class TriggerAddItem : NetworkBehaviour, ILinked
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

        public WorldElement element { get; set; }

        public void ReciveStateChange(bool value)
        {
        }
    }
}