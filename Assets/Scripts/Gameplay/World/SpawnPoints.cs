using System;
using Mirror;
using UnityEngine;
using Util;

namespace Gameplay.World
{
    [ExecuteInEditMode]
    public class SpawnPoints : MonoBehaviour
    {
        public DungeonEntrance[] spawns;
        public static bool stateChecked;
        
        private void Awake()
        {
            spawns = FindObjectsOfType<DungeonEntrance>();
        }

        [InspectorButton]
        public void Refresh()
        {
            spawns = FindObjectsOfType<DungeonEntrance>();
        }

        private void Update()
        {
            if (!stateChecked)
            {
                spawns = FindObjectsOfType<DungeonEntrance>();
                stateChecked = true;
            }
        }
    }
}