using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.ScriptableObjects;
using UnityEngine;
using UnityEditor;
using Util;

namespace Editor
{

    [CustomPropertyDrawer(typeof(ItemIdAttribute))]
    public class ItemIdEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ItemDB items = Resources.Load<ItemDB>("Settings/ScriptableObject/ItemDB");

            if (items == null)
            {
                Debug.Log("Items is null!");
                EditorGUI.PropertyField(position, property, new GUIContent((attribute as RenameAttribute).NewName));
            }
            else
            {
                
                
                string[] itemIds = items.GetAll().Select(desc => desc.itemId).ToArray();
                
                int index = Array.IndexOf(itemIds, property.stringValue);
                if (index == -1) index = 0;

                int newIndex = EditorGUI.Popup(position,label, index, itemIds.Select(s => new GUIContent(s)).ToArray());
                property.stringValue = itemIds[newIndex];
            }
        }
    }
}