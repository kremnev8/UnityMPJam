using System;
using System.Collections.Generic;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Gameplay.UI
{
    public class InventoryUI : MonoBehaviour
    {
        public Inventory inventory;
        
        public RectTransform slotTransform;
        public InventorySlotUI slotPrefab;

        public bool canScroll;
        public bool showScrollSlot;
        
        private GameModel model;
        private InputAction mouseScroll;
        private InputAction prev;
        private InputAction next;
        
        public List<InventorySlotUI> slots = new List<InventorySlotUI>();
        

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            mouseScroll = model.input.actions["mouseScroll"];
            prev = model.input.actions["prevItem"];
            next = model.input.actions["nextItem"];
            
            if (inventory != null)
            {
                Init();
            }

            prev.performed += OnPrev;
            next.performed += OnNext;

            showScrollSlot = canScroll;
        }
        
        

        private void OnNext(InputAction.CallbackContext obj)
        {
            if (!canScroll) return;
            
            inventory.SelectNext();
            RefreshUI();
        }

        private void OnPrev(InputAction.CallbackContext obj)
        {
            if (!canScroll) return;
            
            inventory.SelectPrevious();
            RefreshUI();
        }

        public void Init()
        {
            ItemDesc[] items = inventory.GetItems();
            for (int i = 0; i < inventory.maxSize; i++)
            {
                InventorySlotUI slotUI = Instantiate(slotPrefab, slotTransform);
                ItemDesc item = i < items.Length ? items[i] : null;
                slotUI.Set(item, i);
                slots.Add(slotUI);
                slots[i].SetSelected(i == inventory.selectedIndex && showScrollSlot);
                slots[i].gameObject.SetActive(i < inventory.InventoryCap);
            }

            inventory.inventoryChanged += RefreshUI;
        }

        private void OnDestroy()
        {
            if (inventory != null)
                inventory.inventoryChanged -= RefreshUI;

            if (next != null)
            {
                prev.performed -= OnPrev;
                next.performed -= OnNext;
            }
        }

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.inventoryChanged -= RefreshUI;
                inventory.inventoryChanged += RefreshUI;
            }

            if (next != null)
            {
                prev.performed -= OnPrev;
                next.performed -= OnNext;
                prev.performed += OnPrev;
                next.performed += OnNext;
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.inventoryChanged -= RefreshUI;

            if (next != null)
            {
                prev.performed -= OnPrev;
                next.performed -= OnNext;
            }
        }

        public void RefreshUI()
        {
            if (inventory != null)
            {
                ItemDesc[] items = inventory.GetItems();
                for (int i = 0; i < slots.Count; i++)
                {
                    ItemDesc item = i < items.Length ? items[i] : null;
                    slots[i].Set(item, i);
                    slots[i].SetSelected(i == inventory.selectedIndex && showScrollSlot);
                    slots[i].gameObject.SetActive(i < inventory.InventoryCap);
                }
            }
        }

        private void Update()
        {
            if (!canScroll) return;
            
            Vector2 scroll = mouseScroll.ReadValue<Vector2>();
            if (scroll.y == 0) return;
            
            if (scroll.y > 0)
            {
                inventory.SelectNext();
            }
            else
            {
                inventory.SelectPrevious();
            }

            RefreshUI();
        }
    }
}