using System;
using Gameplay.Core;
using Mirror;
using ScriptableObjects;
using UnityEngine;

namespace Gameplay.ScriptableObjects
{
    [Serializable]
    public class ItemDesc : GenericItem
    {
        public string itemName;
        public string itemId;

        public Sprite itemIcon;

        public string ItemId => itemId;
    }

    public static class ItemDescSerializer 
    {
        public static void WriteItemDesc(this NetworkWriter writer, ItemDesc armor)
        {
            // no need to serialize the data, just the name of the armor
            writer.WriteString(armor.itemId);
        }

        public static ItemDesc ReadArmor(this NetworkReader reader)
        {
            // load the same armor by name.  The data will come from the asset in Resources folder
            GameModel model = Simulation.GetModel<GameModel>();
            return model.items.Get(reader.ReadString());
        }
    }
    
    
    [CreateAssetMenu(fileName = "Item DB", menuName = "SO/New Item DB", order = 0)]
    public class ItemDB : GenericDB<ItemDesc>
    {
        
    }
}