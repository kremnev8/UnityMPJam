using System;
using System.Collections.Generic;
using Mirror;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.ScriptableObjects
{
    [Serializable]
    public class LevelData : GenericItem
    {
        public string levelName;
        public string levelId;

        [Scene]
        public string scene;

        public string[] defaultItems;

        public string ItemId => levelId;
    }


    [CreateAssetMenu(fileName = "Levels DB", menuName = "SO/New Levels DB", order = 0)]
    public class LevelsDB : GenericDB<LevelData>
    {
        public LevelData GetLevel(int index)
        {
            if (index >= 0 && index < items.Length)
            {
                return items[index];
            }

            throw new InvalidOperationException($"There is no level with index {index}!");
        }
        
    }
}