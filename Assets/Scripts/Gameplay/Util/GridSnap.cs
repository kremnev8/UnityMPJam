using System;
using Gameplay.World.Spacetime;
using UnityEngine;

namespace Gameplay.Util
{
    [ExecuteInEditMode]
    public class GridSnap : MonoBehaviour
    {
        public Vector2 grid = new Vector2(2,2);
        public Vector2 offset = Vector2.one;

        private void OnDrawGizmos()
        {
            SnapToGrid();
        }

        private void SnapToGrid()
        {
            if (grid.x == 0 || grid.y == 0) return;
            
            Vector2 position = transform.position;
            position -= offset;

            Vector2 newPos = new Vector2(
                Mathf.Round(position.x / grid.x) * grid.x,
                Mathf.Round(position.y / grid.y) * grid.y);
            newPos += offset;

            transform.position = newPos;
        }
        
        private void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }
            else if (Application.isEditor)
            {
                if (transform.parent == null)
                {
                    SpacetimeController spacetime = FindObjectOfType<SpacetimeController>();
                    transform.parent = spacetime.pastWorld.objectTransform;
                }
            }
        }
    }
}