using Gameplay.ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public int itemIndex;
        public ItemDesc item;
        
        public Image border;
        public Image icon;

        public Sprite normalBorder;
        public Sprite selectedBorder;

        public bool IsPointerInside { get; private set; }
        
        public void Set(ItemDesc itemDesc, int index)
        {
            item = itemDesc;
            itemIndex = index;
            if (itemDesc != null)
            {
                icon.sprite = itemDesc.itemIcon;
                icon.color = Color.white;
            }
            else
            {
                icon.sprite = null;
                icon.color = Color.clear;
            }

            border.sprite = normalBorder;
        }

        public void SetSelected(bool value)
        {
            border.sprite = value ? selectedBorder : normalBorder;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            IsPointerInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsPointerInside = false;
        }
        
    }
}