using Gameplay.World.Spacetime;
using UnityEngine;
using UnityEngine.Tilemaps;
using Util;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Gameplay.Util
{
    [ExecuteInEditMode]
    public class GridSnap : MonoBehaviour
    {
        public Vector2 grid = new Vector2(2,2);
        public Vector2 offset = Vector2.one;
        
        public TileBase tileUnder;
        
        private Vector2Int lastPlacedPos;
        private Vector2Int currenPos;
        private TileBase prevTile;

        private static LevelElementController cachedController;
        
#if UNITY_EDITOR
        
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
            currenPos = transform.localPosition.ToGridPos();
        }


        private void Update()
        {
            if (!Application.isPlaying)
            {
                bool isPrefabInstance = PrefabStageUtility.GetCurrentPrefabStage() != null;
                if (isPrefabInstance) return;
                
                if (Selection.count > 1) return;

                if (currenPos.Equals(lastPlacedPos)) return;

                if (cachedController == null)
                {
                    cachedController = FindObjectOfType<LevelElementController>();
                }

                if (cachedController != null && tileUnder != null)
                {
                    Tilemap primaryTilemap = cachedController.theWorld.tileSet.primaryTilemap;
                    if (primaryTilemap == null) return;

                    if (prevTile != null)
                    {
                        primaryTilemap.SetTile(lastPlacedPos.ToVector3Int(), prevTile);
                    }

                    Vector3Int pos3 = currenPos.ToVector3Int();

                    prevTile = primaryTilemap.GetTile(pos3);
                    primaryTilemap.SetTile(pos3, tileUnder);
                    lastPlacedPos = currenPos;
                }
            }
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                if (cachedController == null)
                {
                    cachedController = FindObjectOfType<LevelElementController>();
                }

                if (cachedController != null && tileUnder != null && prevTile != null)
                {
                    Tilemap primaryTilemap = cachedController.theWorld.tileSet.primaryTilemap;
                    if (primaryTilemap == null) return;
                    primaryTilemap.SetTile(lastPlacedPos.ToVector3Int(), prevTile);
                }
            }
        }

        private void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }

            else if (Application.isEditor)
            {
                bool isPrefabInstance = PrefabStageUtility.GetCurrentPrefabStage() != null;
                if (isPrefabInstance) return;
                if (transform.parent != null) return;
                if (cachedController == null)
                {
                    cachedController = FindObjectOfType<LevelElementController>();
                }
                
                if (cachedController != null)
                {
                    transform.parent = cachedController.theWorld.objectTransform;
                }
            }
        }
#endif
    }
}