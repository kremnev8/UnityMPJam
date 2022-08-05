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
        public int selectedIndex;

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
            int index = items.FindIndex(item => item.itemId.Equals(itemId));
            if (index != -1)
            {
                items.RemoveAt(index);
                return true;
            }

            return false;
        }

        [Server]
        public bool AddItem(string itemId)
        {
            try
            {
                if (items.Count + 1 >= maxSize) return false;
                
                ItemDesc item = model.items.Get(itemId);
                items.Add(item);
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
            if (items.Count + 1 >= maxSize) return false;

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
            return items[selectedIndex];
        }


    }
}