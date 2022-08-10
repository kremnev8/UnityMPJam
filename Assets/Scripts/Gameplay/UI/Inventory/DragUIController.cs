using System;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Gameplay.World;
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

        public GameObject dragDisableHint;
        
        private GameModel model;
        private InputAction mouseClick;
        private InputAction mousePosition;
        
        private InputAction controllerGlobalNext;
        private InputAction controllerGlobalPrev;
        private InputAction controllerTransfer;
        

        private RectTransform rootTransform;
        
        private bool isDragging;
        private bool fromPlayer;

        private float timeSinceControllerInput;

        public RandomAudioSource errorSource;

        private void Start()
        {
            model = Simulation.GetModel<GameModel>();
            rootTransform = (RectTransform)transform;

            mousePosition = model.input.actions["mouse"];
            mouseClick = model.input.actions["click"];
            controllerGlobalNext = model.input.actions["globalNext"];
            controllerGlobalPrev = model.input.actions["globalPrev"];
            controllerTransfer = model.input.actions["controllerTranfer"];
            
            mouseClick.performed += OnLeftClick;
            mouseClick.canceled += OnLeftMouseUp;

            controllerGlobalNext.performed += OnNext;
            controllerGlobalPrev.performed += OnPrev;
            controllerTransfer.performed += Transfer;
        }

        private void Transfer(InputAction.CallbackContext obj)
        {
            if (globalInventoryUI != null && globalInventoryUI.inventory != null)
            {
                PlayerController local = GetPlayer();
                if (local != null)
                {
                    int index = globalInventoryUI.inventory.selectedIndex;
                    if (index >= 0)
                    {
                        ItemDesc item = globalInventoryUI.inventory.GetItem(index);
                        if (item != null)
                        {
                            local.CmdTransferItem(false, item.itemId);
                        }

                        globalInventoryUI.inventory.selectedIndex = -1;
                        globalInventoryUI.RefreshUI();
                    }
                    else
                    {
                        ItemDesc item = playerInventory.inventory.SelectedItem();
                        if (item != null)
                        {
                            local.CmdTransferItem(true, item.itemId);
                        }
                    }
                }
            }
        }

        private void OnPrev(InputAction.CallbackContext obj)
        {
            if (globalInventoryUI != null)
            {
                globalInventoryUI.showScrollSlot = true;
                globalInventoryUI.inventory.SelectPrevious();
                globalInventoryUI.RefreshUI();
            }
        }

        private void OnNext(InputAction.CallbackContext obj)
        {
            if (globalInventoryUI != null)
            {
                globalInventoryUI.showScrollSlot = true;
                globalInventoryUI.inventory.SelectNext();
                globalInventoryUI.RefreshUI();
            }
        }

        private void OnDestroy()
        {
            if (mouseClick != null)
            {
                mouseClick.performed -= OnLeftClick;
                mouseClick.canceled -= OnLeftMouseUp;
            }

            if (controllerGlobalNext != null)
            {
                controllerGlobalNext.performed -= OnNext;
                controllerGlobalPrev.performed -= OnPrev;
                controllerTransfer.performed -= Transfer;
            }
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
            PlayerController player = GetPlayer();
            if (player == null || !player.allowSwap)
            {
                errorSource.Play();
                return;
            }
            
            if (globalInventoryUI != null && globalInventoryUI.inventory != null)
            {
                try
                {
                    InventorySlotUI slotUI = globalInventoryUI.slots.First(slot => slot.IsPointerInside);
                    if (slotUI.item != null && slotUI.item.canBeDragged)
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
                    if (slotUI.item != null && slotUI.item.canBeDragged)
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
            
            PlayerController player = GetPlayer();
            if (player != null)
            {
                dragDisableHint.SetActive(!player.allowSwap);
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