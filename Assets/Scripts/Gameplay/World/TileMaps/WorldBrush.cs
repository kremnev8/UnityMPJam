#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Gameplay.ScriptableObjects;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMaps
{
    
    [CustomGridBrush(true, false, false, "TileSet Brush")]
    public class WorldBrush : GridBrush
    {
        public TileSet tileSet;

        [SerializeField]
        [HideInInspector]
        private List<TileChangeData> TileChangeDataList;
    
        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            Vector3Int min = position - pivot;
            BoundsInt bounds = new BoundsInt(min, size);

            DetermineAndPaint(gridLayout, brushTarget, bounds);
        }

        private void DetermineAndPaint(GridLayout gridLayout, GameObject brushTarget, BoundsInt bounds)
        {
            if (cells.Length > 0)
            {
                BrushCell cell = cells[0];
                try
                {
                    TileGroup group = tileSet.GetGroup(cell.tile);
                    CustomBoxFill(gridLayout, brushTarget, bounds, group);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
                return;
            }

            CustomBoxFill(gridLayout, brushTarget, bounds, null);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            Vector3Int min = position - pivot;
            BoundsInt bounds = new BoundsInt(min, size);
            CustomBoxFill(gridLayout, brushTarget, bounds, null);
        }

        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            DetermineAndPaint(gridLayout, brushTarget, position);
        }

        public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            CustomBoxFill(gridLayout, brushTarget, position, null);
        }


        /// <summary>Box fills tiles and GameObjects into given bounds within the selected layers.</summary>
        /// <param name="gridLayout">Grid to box fill data to.</param>
        /// <param name="brushTarget">Target of the box fill operation. By default the currently selected GameObject.</param>
        /// <param name="position">The bounds to box fill data into.</param>
        public void CustomBoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, TileGroup group)
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
                TilemapId settings = tilemap.GetComponent<TilemapId>();

                if (settings != null)
                {
                    if (group != null)
                    {
                        bool any = false;
                        foreach (TileMapping mapping in group.tileMappings)
                        {
                            if (mapping.targets.Contains(settings.type))
                            {
                                PaintTileMap(tilemap, mapping.tile, position);
                                any = true;
                                break;
                            }
                        }

                        if (!any)
                        {
                            PaintTileMap(tilemap, null, position);
                        }
                    }
                    else
                    {
                        PaintTileMap(tilemap, null, position);
                    }
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
#endif