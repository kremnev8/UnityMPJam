using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMaps
{
    [CustomGridBrush(true, false, false, "TileSet Brush")]
    public class WorldBrush : GridBrush
    {
        public int currentTileSet;
        
        [SerializeField]
        [HideInInspector]
        private List<TileChangeData> TileChangeDataList;
    
        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            Vector3Int min = position - pivot;
            BoundsInt bounds = new BoundsInt(min, size);
            CustomBoxFill(gridLayout, brushTarget, bounds, false);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            Vector3Int min = position - pivot;
            BoundsInt bounds = new BoundsInt(min, size);
            CustomBoxFill(gridLayout, brushTarget, bounds, true);
        }

        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            CustomBoxFill(gridLayout, brushTarget, position, false);
        }

        public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            CustomBoxFill(gridLayout, brushTarget, position, true);
        }


        /// <summary>Box fills tiles and GameObjects into given bounds within the selected layers.</summary>
        /// <param name="gridLayout">Grid to box fill data to.</param>
        /// <param name="brushTarget">Target of the box fill operation. By default the currently selected GameObject.</param>
        /// <param name="position">The bounds to box fill data into.</param>
        public void CustomBoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, bool isErase)
        {
            if (brushTarget == null)
                return;

            GameObject actualTarget = brushTarget;
            
            if (actualTarget.GetComponent<Grid>() == null)
            {
                Transform parent = actualTarget.transform.parent;
                if (parent != null)
                {
                    actualTarget = parent.gameObject;
                }
            }

            Tilemap[] maps = actualTarget.GetComponentsInChildren<Tilemap>();

            foreach (Tilemap tilemap in maps)
            {
                CustomTileSettings settings = tilemap.GetComponent<CustomTileSettings>();

                if (settings != null && currentTileSet < settings.tileSets.Length)
                {
                    TileSet tileSet = settings.tileSets[currentTileSet];
                    TileBase tileBase = isErase ? tileSet.floorTile : tileSet.wallTile;

                    PaintTileMap(tilemap, tileBase, position);
                }
            }
        }

        private void PaintTileMap(Tilemap map, TileBase tile, BoundsInt position)
        {
            if (map == null) return;

            int count = 0;
            var listSize = position.size.x * position.size.y * position.size.z;
            if (TileChangeDataList == null || TileChangeDataList.Capacity != listSize)
                TileChangeDataList = new List<TileChangeData>(listSize);
            TileChangeDataList.Clear();
            foreach (Vector3Int location in position.allPositionsWithin)
            {
                var tcd = new TileChangeData
                {
                    position = location,
                    tile = tile,
                    transform = Matrix4x4.identity,
                    color = Color.white
                };
                TileChangeDataList.Add(tcd);
                count++;
            }

            // Duplicate empty slots in the list, as ExtractArrayFromListT returns full list
            if (0 < count && count < listSize)
            {
                var tcd = TileChangeDataList[count - 1];
                for (int i = count; i < listSize; ++i)
                {
                    TileChangeDataList.Add(tcd);
                }
            }

            map.SetTiles(TileChangeDataList.ToArray(), false);
        }
    }
}