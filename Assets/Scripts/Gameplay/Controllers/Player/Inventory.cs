using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Mirror;
using UnityEngine;

namespace Gameplay.Controllers.Player
{
    public class Inventory : NetworkBehaviour
    {
        public int maxSize;
        private readonly SyncList<ItemDesc> items = new SyncList<ItemDesc>();
        private GameModel model;

        public Action inventoryChanged;
        public Action serverInventoryChanged;

        public int selectedIndex;
        
        [SerializeField]
        private int inventoryCap;

        public int InventoryCap
        {
            get => inventoryCap;
            set
            {
                inventoryCap = value;
                inventoryChanged?.Invoke();
            }
        }

        public void SelectNext()
        {
            if (items.Count == 0) return;

            selectedIndex++;
            if (selectedIndex >= items.Count)
            {
                selectedIndex = 0;
            }
        }

        public void SelectPrevious()
        {
            if (items.Count == 0) return;

            selectedIndex--;
            if (selectedIndex < 0)
            {
                selectedIndex = items.Count - 1;
            }
        }

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
        }

        public override void OnStartClient()
        {
            items.Callback += OnInventoryUpdated;

            OnInventoryUpdated(SyncList<ItemDesc>.Operation.OP_ADD, 0, null, null);
        }

        void OnInventoryUpdated(SyncList<ItemDesc>.Operation op, int index, ItemDesc oldItem, ItemDesc newItem)
        {
            inventoryChanged?.Invoke();
        }

        [Server]
        public bool TryConsume(string itemId)
        {
            if (model == null)
            {
                model = Simulation.GetModel<GameModel>();
            }

            int index = items.FindIndex(item => item.itemId.Equals(itemId));
            if (index != -1)
            {
                items.RemoveAt(index);
                serverInventoryChanged?.Invoke();
                return true;
            }

            return false;
        }

        [Server]
        public bool AddItem(string itemId)
        {
            if (model == null)
            {
                model = Simulation.GetModel<GameModel>();
            }

            try
            {
                int cap = Mathf.Min(inventoryCap, maxSize);
                if (items.Count + 1 > cap) return false;

                ItemDesc item = model.items.Get(itemId);
                items.Add(item);
                serverInventoryChanged?.Invoke();
                return true;
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.Log($"Failed to add item {itemId}, because it is missing!");
            }

            return false;
        }

        public bool Transfer(Inventory from, string itemId)
        {
            int cap = Mathf.Min(inventoryCap, maxSize);
            if (items.Count + 1 > cap) return false;

            if (from.TryConsume(itemId))
            {
                AddItem(itemId);
                return true;
            }

            return false;
        }

        public ItemDesc[] GetItems()
        {
            return items.ToArray();
        }

        public ItemDesc SelectedItem()
        {
            if (selectedIndex < items.Count)
            {
                return items[selectedIndex];
            }

            return null;
        }
    }
}