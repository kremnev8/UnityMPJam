using Gameplay.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        public Image border;
        public Image icon;

        public Sprite normalBorder;
        public Sprite selectedBorder;

        public void Set(ItemDesc itemDesc)
        {
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
        
    }
}