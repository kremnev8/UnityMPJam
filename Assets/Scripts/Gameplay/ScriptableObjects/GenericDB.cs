using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    /// <summary>
    /// Base class for generic items keyed by string id's
    /// </summary>
    public interface GenericItem<T>
    {
        T ItemId { get; }
    }

    public interface GenericItem : GenericItem<string>
    {
        
    }

    public class GenericDB<T> : GenericDB<string, T>
        where T : GenericItem
    {
        
    }

    /// <summary>
    /// Generic data store for any item keyed by string id's
    /// </summary>
    /// <typeparam name="T">Model class, implementing <see cref="GenericItem"/></typeparam>
    public class GenericDB<TI, TV> : ScriptableObject
        where TV : GenericItem<TI>
    {
        [SerializeField] protected TV[] items;
        protected Dictionary<TI, TV> itemsDictionary;

        public virtual TV Get(TI key)
        {
            if (itemsDictionary == null)
            {
                InitDictionary();
            }

            if (itemsDictionary.ContainsKey(key))
            {
                return itemsDictionary[key];
            }

            throw new IndexOutOfRangeException($"Item with id {key} is not registered!");
        }

        public virtual TV[] GetAll()
        {
            return items;
        }

        public int Count()
        {
            return GetAll().Length;
        }

        protected virtual void InitDictionary()
        {
            itemsDictionary = new Dictionary<TI, TV>();

            foreach (TV hint in items)
            {
                itemsDictionary.Add(hint.ItemId, hint);
            }
        }
    }
}