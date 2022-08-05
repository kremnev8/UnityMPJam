using System;
using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using Gameplay.Core;
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

        private GameModel model;

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (isUsed) return;
            
            if (isServer && col != null)
            {
                PlayerController player = col.GetComponent<PlayerController>();
                if (player != null && model.globalInventory != null)
                {
                    if (model.globalInventory.AddItem(itemId))
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