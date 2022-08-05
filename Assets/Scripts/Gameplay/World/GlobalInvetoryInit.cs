using System;
using System.Collections.Generic;
using Gameplay.Core;
using Gameplay.UI;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public class GlobalInvetoryInit : MonoBehaviour
    {
        public InventoryUI globalInventoryUI;
        public List<string> defaultItems;
        
        
        public void Start()
        {
            GameModel model = Simulation.GetModel<GameModel>();
            model.globalInventoryUI = globalInventoryUI;
            model.globalInventoryUI.inventory = model.globalInventory;
            model.globalInventoryUI.Init();
            if (NetworkServer.active)
            {
                foreach (string item in defaultItems)
                {
                    model.globalInventory.AddItem(item);
                }
            }
        }
    }
}