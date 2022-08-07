using System;
using System.Collections.Generic;
using Gameplay.Conrollers;
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

        public static bool added;


        public void Start()
        {
            GameModel model = Simulation.GetModel<GameModel>();
            model.globalInventoryUI = globalInventoryUI;
            model.globalInventoryUI.inventory = model.globalInventory;
            model.globalInventoryUI.Init();
            if (NetworkServer.active)
            {
                PlayerController.ignoreInventoryEvent = true;
                if (Application.isEditor && !added)
                {
                    added = true;
                    var items = model.saveGame.current.globalInventory;
                    if (items != null && items.Count > 0)
                    {
                        PlayerController.ignoreInventoryEvent = false;
                        return;
                    }

                    foreach (string item in defaultItems)
                    {
                        model.globalInventory.AddItem(item);
                    }
                }

                PlayerController.ignoreInventoryEvent = false;
            }
        }
    }
}