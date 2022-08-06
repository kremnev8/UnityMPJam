using System;
using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Gameplay.World;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;

namespace Gameplay.Logic
{
    public enum ActivateMode
    {
        TRIGGER,
        ACTIVATE
    }

    public class TriggerAddItem : NetworkBehaviour, ILinked, IInteractable
    {
        public bool checkFacing => false;
        public Vector2Int FacingDirection => Vector2Int.zero;

        public ActivateMode activateMode;

        public void Activate(PlayerController player)
        {
            if (player != null && isClient)
            {
                CmdAddItem();
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdAddItem()
        {
            if (isUsed) return;
            if (activateMode != ActivateMode.ACTIVATE) return;

            AddItem();
        }

        public SpriteRenderer itemRenderer;

        public string itemId;
        private bool isUsed;
        public bool replaceIcon;

        private GameModel model;

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            if (replaceIcon)
            {
                try
                {
                    ItemDesc itemDesc = model.items.Get(itemId);
                    itemRenderer.sprite = itemDesc.itemIcon;
                }
                catch (Exception)
                {
                    Debug.Log($"Failed to set item {itemId} icon!");
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (isUsed) return;
            if (activateMode != ActivateMode.TRIGGER) return;

            if (isServer && col != null)
            {
                PlayerController player = col.GetComponent<PlayerController>();
                if (player != null && model.globalInventory != null)
                {
                    AddItem();
                }
            }
        }

        [Server]
        private void AddItem()
        {
            if (model.globalInventory.AddItem(itemId))
            {
                isUsed = true;
                RpcHideItem();
            }
        }

        [ClientRpc]
        private void RpcHideItem()
        {
            itemRenderer.enabled = false;
        }

        public WorldElement element { get; set; }

        public void ReciveStateChange(bool value) { }
    }
}