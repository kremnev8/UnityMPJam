using System;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Core;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.UI
{
    public class DragUIController : MonoBehaviour
    {
        public RectTransform dragSlotTransform;
        public InventorySlotUI dragSlot;

        public RectTransform playerInventoryRect;
        public RectTransform globalInventoryRect;

        public InventoryUI globalInventoryUI;
        public InventoryUI playerInventory;
        

        private GameModel model;
        private InputAction mouseClick;
        private InputAction mousePosition;

        private RectTransform rootTransform;
        
        private bool isDragging;
        private bool fromPlayer;

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            rootTransform = (RectTransform)transform;

            mousePosition = model.input.actions["mouse"];
            mouseClick = model.input.actions["click"];
            mouseClick.performed += OnLeftClick;
            mouseClick.canceled += OnLeftMouseUp;
        }

        private void StartDrag()
        {
            isDragging = true;
            dragSlot.gameObject.SetActive(true);
        }

        private void EndDrag()
        {
            isDragging = false;
            dragSlot.gameObject.SetActive(false);
        }

        private PlayerController GetPlayer()
        {
            if (NetworkClient.active && NetworkClient.localPlayer != null)
            {
                return NetworkClient.localPlayer.GetComponent<PlayerController>();
            }
            return null;
        }
        
        private void OnLeftMouseUp(InputAction.CallbackContext obj)
        {
            if (!isDragging) return;

            Vector2 mousePos = mousePosition.ReadValue<Vector2>();
            
            if (globalInventoryUI != null && globalInventoryUI.inventory != null)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(playerInventoryRect, mousePos) && !fromPlayer)
                {
                    PlayerController local = GetPlayer();
                    if (local != null)
                    {
                        local.CmdTransferItem(fromPlayer, dragSlot.item.itemId);
                    }
                }
            }

            if (playerInventory != null && playerInventory.inventory != null)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(globalInventoryRect, mousePos) && fromPlayer)
                {
                    PlayerController local = GetPlayer();
                    if (local != null)
                    {
                        local.CmdTransferItem(fromPlayer, dragSlot.item.itemId);
                    }
                }
            }

            EndDrag();
        }

        private void OnLeftClick(InputAction.CallbackContext obj)
        {
            if (isDragging) return;
            
            if (globalInventoryUI != null && globalInventoryUI.inventory != null)
            {
                try
                {
                    InventorySlotUI slotUI = globalInventoryUI.slots.First(slot => slot.IsPointerInside);
                    if (slotUI.item != null)
                    {
                        dragSlot.Set(slotUI.item, slotUI.itemIndex);
                        fromPlayer = false;
                        StartDrag();
                        return;
                    }
                    
                }
                catch (InvalidOperationException) { }
            }

            if (playerInventory != null && playerInventory.inventory != null)
            {
                try
                {
                    InventorySlotUI slotUI = playerInventory.slots.First(slot => slot.IsPointerInside);
                    if (slotUI.item != null)
                    {
                        dragSlot.Set(slotUI.item, slotUI.itemIndex);
                        fromPlayer = true;
                        StartDrag();
                    }
                    
                }
                catch (InvalidOperationException) { }
            }
        }

        private void Update()
        {
            if (playerInventory == null)
            {
                playerInventory = model.playerInventoryUI;
            }
            
            if (isDragging)
            {
                Vector2 mousePos = mousePosition.ReadValue<Vector2>();
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rootTransform, mousePos, null, out Vector2 localpoint))
                {
                    dragSlotTransform.anchoredPosition = localpoint;
                }
            }
        }
    }
}