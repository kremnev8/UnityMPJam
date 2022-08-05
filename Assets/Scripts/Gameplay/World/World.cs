using System.Collections.Generic;
using Gameplay.Conrollers;
using Gameplay.World.Spacetime;
using TileMaps;
using UnityEngine;
using Util;

namespace Gameplay.World
{
    public class World : MonoBehaviour
    {
        public TileSetController tileSet;
        public Transform objectTransform;

        public Dictionary<string, WorldElement> objects = new Dictionary<string, WorldElement>();

        public void Init()
        {
            WorldElement[] objArr = objectTransform.GetComponentsInChildren<WorldElement>(true);
            objects.Clear();
            objects.EnsureCapacity(objArr.Length);

            foreach (WorldElement timeObject in objArr)
            {
                objects.Add(timeObject.UniqueId, timeObject);
                
                ILinked linked = timeObject.GetComponent<ILinked>();
                if (timeObject == null || linked == null)
                {
                    Debug.Log(
                        $"Failed to link object {timeObject.name}, because timeObject is {(timeObject == null ? "Null" : "Not Null")} and linked is {(linked == null ? "Null" : "Not Null")}");
                    continue;
                }

                timeObject.Set(linked);
                if (Application.isPlaying)
                {
                    SpriteRenderer[] renderers = timeObject.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer spriteRenderer in renderers)
                    {
                        spriteRenderer.color = Color.white;
                    }
                }
            }
        }

        public WorldElement GetObject(string id)
        {
            if (objects.ContainsKey(id))
            {
                return objects[id];
            }

            return null;
        }
        
        public Vector2 GetWorldSpacePos(Vector2Int pos)
        {
            return (Vector2)transform.position + pos.ToWorldPos();
        }

        public Vector2Int GetGridPos(Vector3 worldPos)
        {
            return (worldPos - transform.position).ToGridPos();
        }
    }
}