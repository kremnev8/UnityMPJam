using System.Collections.Generic;
using Gameplay.Conrollers;
using Gameplay.World.Spacetime;
using TileMaps;
using UnityEngine;

namespace Gameplay.World
{
    public class World : MonoBehaviour
    {
        public Timeline worldTimeline;
        public TileSetController tileSet;
        public Transform objectTransform;

        public Dictionary<string, SpaceTimeObject> objects = new Dictionary<string, SpaceTimeObject>();

        public void Init()
        {
            SpaceTimeObject[] objArr = objectTransform.GetComponentsInChildren<SpaceTimeObject>(true);
            objects.Clear();
            objects.EnsureCapacity(objArr.Length);

            foreach (SpaceTimeObject timeObject in objArr)
            {
                if (timeObject.GetState(worldTimeline) == ObjectState.DOES_NOT_EXIST)
                {
                    timeObject.gameObject.SetActive(false);
                    continue;
                }
                
                objects.Add(timeObject.UniqueId, timeObject);
                
                ITimeLinked linked = timeObject.GetComponent<ITimeLinked>();
                if (timeObject == null || linked == null)
                {
                    Debug.Log(
                        $"Failed to link object {timeObject.name}, because timeObject is {(timeObject == null ? "Null" : "Not Null")} and linked is {(linked == null ? "Null" : "Not Null")}");
                    continue;
                }

                timeObject.Set(worldTimeline, linked);
            }
        }

        public SpaceTimeObject GetObject(string id)
        {
            if (objects.ContainsKey(id))
            {
                return objects[id];
            }

            return null;
        }
    }
}