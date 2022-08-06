using System;
using System.Collections.Generic;
using Gameplay.Core;
using Gameplay.UI;
using Mirror;
using UnityEngine;
using Util;

namespace Gameplay.World
{
    public class GlobalInvetoryInit : MonoBehaviour
    {
        public InventoryUI globalInventoryUI;
        public List<string> defaultItems;

        public string backpackId;
        
        public void Start()
        {
            if (NetworkServer.active)
            {
                GameModel model = Simulation.GetModel<GameModel>();
                model.globalInventoryUI = globalInventoryUI;
                model.globalInventoryUI.inventory = model.globalInventory;
                model.globalInventoryUI.Init();
                model.globalInventory.InventoryCap = 6;
                if (NetworkServer.active)
                {
                    foreach (string item in defaultItems)
                    {
                        model.globalInventory.AddItem(item);
                    }
                }
            }
        }

        [InspectorButton]
        public void AddBackpack()
        {
            GameModel model = Simulation.GetModel<GameModel>();
            model.globalInventory.AddItem(backpackId);
        }
        
        
        [InspectorButton]
        public void RemoveBackpack()
        {
            GameModel model = Simulation.GetModel<GameModel>();
            model.globalInventory.TryConsume(backpackId);
        }
    }
}