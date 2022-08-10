using System;
using System.Reflection;
using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace Gameplay.World.Spacetime
{
    public class LevelElementController : MonoBehaviour
    {
        [FormerlySerializedAs("pastWorld")] 
        public World theWorld;

        public TileSet pastTileset;

        public static bool eventListenerUp = false;
        
        
#if UNITY_EDITOR
        [InspectorButton]
        public void Generate()
        {
            theWorld.tileSet.tileSet = pastTileset;
            
            theWorld.tileSet.Rebuild();
            theWorld.Init();

            SpawnPoints.stateChecked = false;

            if (Application.isPlaying)
            {
                eventListenerUp = true;
            }
        }
#endif

        public void Configure()
        {
            theWorld.Init();

            if (Application.isPlaying)
            {
                eventListenerUp = true;
            }
        }

        private void Awake()
        {
            Configure();
        }

        private void Start()
        {
            Simulation.GetModel<GameModel>().levelElement = this;
        }

        private void DestroyGO(GameObject o)
        {
            if (o == null) return;
            
            if (Application.isPlaying)
            {
                Destroy(o);
            }
            else
            {
                DestroyImmediate(o);
            }
        }

        #region Runtime

        public World GetWorld()
        {
            return theWorld;
        }

        public void ChangeLogicState(string targetId, bool value)
        {
            World world = GetWorld();
            WorldElement timeObject = world.GetObject(targetId);
            if (timeObject != null)
            {
                timeObject.target.ReciveStateChange(value);
            }
        }
        
        #endregion
        
    }
}