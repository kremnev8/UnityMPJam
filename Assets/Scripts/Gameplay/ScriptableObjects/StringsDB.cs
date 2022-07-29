using System;
using ScriptableObjects;
using UnityEngine;

namespace Gameplay.ScriptableObjects
{
    [Serializable]
    public class StringEntry : GenericItem
    {
        public string ItemId
        {
            get => _itemId;
            set => _itemId = value;
        }

        [SerializeField]
        private string _itemId;
        public string text;
    }
    
    [CreateAssetMenu(fileName = "Strings DB", menuName = "SO/New Strings DB", order = 0)]
    public class StringsDB : GenericDB<StringEntry>
    {
        public override StringEntry Get(string key)
        {
            try
            {
                return base.Get(key);
            }
            catch (IndexOutOfRangeException)
            {
                return new StringEntry()
                {
                    ItemId = key,
                    text = key
                };
            }
            
        }
    }
}