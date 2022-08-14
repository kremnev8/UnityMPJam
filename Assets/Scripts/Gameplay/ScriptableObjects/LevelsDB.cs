using System;
using System.Collections.Generic;
using Mirror;
using ScriptableObjects;
using UnityEngine;
using Util;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.ScriptableObjects
{
    [Serializable]
    public class LevelData : GenericItem
    {
        public string levelName;

        [Scene]
        public string scene;

        [ItemId]
        public string[] defaultItems;

        public string ItemId => levelName;
    }


    [CreateAssetMenu(fileName = "Levels DB", menuName = "SO/New Levels DB", order = 0)]
    public class LevelsDB : GenericDB<LevelData>
    {
        [Scene]
        public string[] baseScenes;
        
        public LevelData GetLevel(int index)
        {
            if (index >= 0 && index < items.Length)
            {
                return items[index];
            }

            throw new InvalidOperationException($"There is no level with index {index}!");
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {

                List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

                foreach (var scenePath in baseScenes)
                {
                    if (!string.IsNullOrEmpty(scenePath))
                        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }

                foreach (var scenePath in items)
                {
                    if (!string.IsNullOrEmpty(scenePath.scene))
                        scenes.Add(new EditorBuildSettingsScene(scenePath.scene, true));
                }

                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
#endif
    }
}