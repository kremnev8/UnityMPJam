using System;
using System.Collections.Generic;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.UI
{
    public class InventoryUI : MonoBehaviour
    {
        public Inventory inventory;
        
        public RectTransform slotTransform;
        public InventorySlotUI slotPrefab;

        public bool canScroll;
        
        private GameModel model;
        private InputAction mouseScrollUp;
        private InputAction mouseScrollDown;
        
        private List<InventorySlotUI> slots = new List<InventorySlotUI>();

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            mouseScrollUp = model.input.actions["scrollUp"];
            mouseScrollDown = model.input.actions["scrollDown"];
            if (inventory != null)
            {
                Init();
            }
        }

        private void Init()
        {
            ItemDesc[] items = inventory.GetItems();
            for (int i = 0; i < inventory.maxSize; i++)
            {
                InventorySlotUI slotUI = Instantiate(slotPrefab, slotTransform);
                ItemDesc item = i < items.Length ? items[i] : null;
                slotUI.Set(item);
                slots.Add(slotUI);
                slots[i].SetSelected(i == inventory.selectedIndex);
            }

            inventory.inventoryChanged += RefreshUI;
            if (canScroll)
            {
                mouseScrollUp.performed += OnMouseScrollUp;
                mouseScrollDown.performed += OnMouseScrollDown;
            }
        }

        private void OnMouseScrollUp(InputAction.CallbackContext obj)
        {
            inventory.SelectNext();
            RefreshUI();
        }
        
        private void OnMouseScrollDown(InputAction.CallbackContext obj)
        {
            inventory.SelectPrevious();
            RefreshUI();
        }

        private void OnDestroy()
        {
            inventory.inventoryChanged -= RefreshUI;
        }

        private void OnEnable()
        {
            inventory.inventoryChanged -= RefreshUI;
            inventory.inventoryChanged += RefreshUI;
        }

        private void OnDisable()
        {
            inventory.inventoryChanged -= RefreshUI;
        }

        private void RefreshUI()
        {
            if (inventory != null)
            {
                ItemDesc[] items = inventory.GetItems();
                for (int i = 0; i < slots.Count; i++)
                {
                    ItemDesc item = i < items.Length ? items[i] : null;
                    slots[i].Set(item);
                    slots[i].SetSelected(i == inventory.selectedIndex);
                }
            }
        }
    }
}