using System;
using System.Reflection;
using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.ScriptableObjects;
using Mirror;
using UnityEngine;
using Util;

namespace Gameplay.World.Spacetime
{
    public class SpacetimeController : MonoBehaviour
    {
        public World pastWorld;
        public World futureWorld;

        public Transform worldsTransfrom;
        public Vector3 futureWorldPos;
        
        public World worldTemplate;
        
        public TileSet pastTileset;
        public TileSet futureTileset;

        public static bool eventListenerUp = false;
        
        
        
        [InspectorButton]
        public void Generate()
        {
            if (futureWorld != null)
            {
                DestroyGO(futureWorld.gameObject);
            }

            futureWorld = Instantiate(worldTemplate, futureWorldPos, Quaternion.identity, worldsTransfrom);
            futureWorld.worldTimeline = Timeline.FUTURE;
            
            futureWorld.tileSet.master = pastWorld.tileSet.master;
            pastWorld.tileSet.tileSet = pastTileset;
            futureWorld.tileSet.tileSet = futureTileset;
            
            pastWorld.tileSet.Rebuild();
            futureWorld.tileSet.Rebuild();

            SpaceTimeObject[] objects = pastWorld.objectTransform.GetComponentsInChildren<SpaceTimeObject>(true);

            foreach (SpaceTimeObject timeObject in objects)
            {
                ITimeLinked linked = timeObject.GetComponent<ITimeLinked>();
                if (timeObject == null || linked == null)
                {
                    Debug.Log($"Failed to link object {timeObject.name}, because timeObject is {(timeObject == null ? "Null" : "Not Null")} and linked is {(linked == null ? "Null" : "Not Null")}");
                    continue;
                }

                if (timeObject.futureState != ObjectState.DOES_NOT_EXIST)
                {
                    SpaceTimeObject futureTimeObj = Instantiate(timeObject, futureWorld.objectTransform);
                    NetworkIdentity identity = futureTimeObj.GetComponent<NetworkIdentity>();
                    if (identity != null)
                    {
                        MethodInfo info = typeof(NetworkIdentity).GetMethod("AssignSceneID", BindingFlags.Instance | BindingFlags.NonPublic);
                        info.Invoke(identity, Array.Empty<object>());
                    }
                    
                    futureTimeObj.transform.localPosition = timeObject.transform.localPosition;
                    futureTimeObj.gameObject.SetActive(true);
                    futureTimeObj.Set(Timeline.FUTURE, futureTimeObj.GetComponent<ITimeLinked>());
                }
                
                if (timeObject.pastState == ObjectState.DOES_NOT_EXIST)
                {
                    timeObject.gameObject.SetActive(false);
                }
                else
                {
                    timeObject.gameObject.SetActive(true);
                    timeObject.Set(Timeline.PAST, timeObject.GetComponent<ITimeLinked>());
                }

            }
            
            pastWorld.Init();
            futureWorld.Init();

            if (Application.isPlaying)
            {
                eventListenerUp = true;
            }
        }

        public void Configure()
        {
            pastWorld.Init();
            futureWorld.Init();

            if (Application.isPlaying)
            {
                eventListenerUp = true;
            }
        }

        private void Start()
        {
            Simulation.GetModel<GameModel>().spacetime = this;
            Configure();
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

        public World GetWorld(Timeline timeline)
        {
            if (timeline == Timeline.PAST)
            {
                return pastWorld;
            }

            return futureWorld;
        }

        public void CallTimeEvent(Timeline timeline, string sourceId, int[] data)
        {
            if (timeline == Timeline.PAST)
            {
                SpaceTimeObject timeObject = futureWorld.GetObject(sourceId);
                timeObject.target.ReciveTimeEvent(data);
            }
        }

        public void ChangeLogicState(Timeline timeline, string targetId, bool value)
        {
            World world = GetWorld(timeline);
            SpaceTimeObject timeObject = world.GetObject(targetId);
            timeObject.target.ReciveStateChange(value);
        }
        
        

        #endregion
        
    }
}