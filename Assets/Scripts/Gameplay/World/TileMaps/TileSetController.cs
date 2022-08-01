using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;
using Util;

namespace TileMaps
{
    [ExecuteInEditMode]
    public class TileSetController : MonoBehaviour
    {
        public TileSet tileSet;
        public Dictionary<TileMapType, Tilemap> tilemaps;

        public Tilemap master;
        public bool liveUpdate;

        [NonSerialized] private bool lastLiveUpdate = false;


        private static Vector3Int[] positions = new Vector3Int[100];
        private static TileBase[] tiles = new TileBase[100];
        private static Dictionary<Vector3Int, TileBase> tileDict = new Dictionary<Vector3Int, TileBase>(100);

        private void Awake()
        {
            GetTilemaps();
        }

        private void OnDestroy()
        {
            Tilemap.tilemapTileChanged -= OnTileMapChanged;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (lastLiveUpdate != liveUpdate)
                {
                    lastLiveUpdate = liveUpdate;
                    Tilemap.tilemapTileChanged -= OnTileMapChanged;
                    if (liveUpdate)
                    {
                        Tilemap.tilemapTileChanged += OnTileMapChanged;
                    }
                }

                if (tilemaps == null)
                {
                    GetTilemaps();
                }
            }
        }
        

        private void GetTilemaps()
        {
            Tilemap[] tmp_maps = GetComponentsInChildren<Tilemap>();
            tilemaps = new Dictionary<TileMapType, Tilemap>(tmp_maps.Length);
            foreach (Tilemap map in tmp_maps)
            {
                TilemapId id = map.GetComponent<TilemapId>();
                if (id != null)
                {
                    tilemaps.Add(id.type, map);
                }
            }

            if (master != null)
            {
                tileDict.Clear();
                int count = master.GetTilesRangeCount(master.cellBounds.min, master.cellBounds.max);

                if (positions.Length <= count)
                {
                    Array.Resize(ref positions, count);
                    Array.Resize(ref tiles, count);
                    tileDict.EnsureCapacity(count);
                }

                count = master.GetTilesRangeNonAlloc(master.cellBounds.min, master.cellBounds.max, positions, tiles);

                for (int i = 0; i < count; i++)
                {
                    Vector3Int pos = positions[i];
                    TileBase tile = tiles[i];
                    tileDict.Add(pos, tile);
                }
            }
        }

        [InspectorButton]
        public void Rebuild()
        {
            foreach (var pair in tilemaps)
            {
                pair.Value.ClearAllTiles();
            }

            tileDict.Clear();
            
            int count = master.GetTilesRangeCount(master.cellBounds.min, master.cellBounds.max);

            if (positions.Length <= count)
            {
                Array.Resize(ref positions, count);
                Array.Resize(ref tiles, count);
                tileDict.EnsureCapacity(count);
            }

            count = master.GetTilesRangeNonAlloc(master.cellBounds.min, master.cellBounds.max, positions, tiles);

            for (int i = 0; i < count; i++)
            {
                Vector3Int pos = positions[i];
                TileBase tile = tiles[i];
                tileDict.Add(pos, tile);
            }
            
            for (int i = 0; i < count; i++)
            {
                Vector3Int pos = positions[i];
                TileBase tile = tiles[i];
                
                PaintAt(pos, tile);
            }
        }

        private TileBase GetTileAt(Vector3Int pos)
        {
            if (tileDict.TryGetValue(pos, out TileBase output))
            {
                return output;
            }

            return null;
        }

        private TileSideType DetermineSideType(TileSideType type, bool hasTop, bool hasBottom)
        {
            if (type == TileSideType.TOP && hasBottom) return TileSideType.ALL;
            if (type == TileSideType.BOTTOM && hasTop) return TileSideType.ALL;

            return type;
        }

        private void PaintAt(Vector3Int pos, TileBase brush)
        {
            TileGroup group = tileSet.GetGroup(brush);
            TileBase top = GetTileAt(pos + Vector3Int.up);
            TileBase bottom = GetTileAt(pos + Vector3Int.down);

            pos *= 2;

            foreach (TileMapping mapping in group.tileMappings)
            {
                foreach (TileMapType target in mapping.targets)
                {
                    try
                    {
                        Tilemap tileMap = tilemaps[target];
                        TileSideType sideType = DetermineSideType(mapping.sideType, top == brush, bottom == brush);
                        PaintMultiply(pos, tileMap, mapping.tile, sideType);
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Failed to paint TileMapping:\n{e}");
                    }
                }
            }
        }

        private void ClearAt(Vector3Int pos)
        {
            pos *= 2;


            foreach (var pair in tilemaps)
            {
                try
                {
                    PaintMultiply(pos, pair.Value, null, TileSideType.ALL);
                }
                catch (Exception e)
                {
                    Debug.Log($"Failed to paint TileMapping:\n{e}");
                }
            }
        }

        public void OnTileMapChanged(Tilemap tilemap, Tilemap.SyncTile[] syncTiles)
        {
            if (tilemap == master)
            {
                foreach (Tilemap.SyncTile syncTile in syncTiles)
                {
                    if (tileDict.ContainsKey(syncTile.position))
                    {
                        tileDict[syncTile.position] = syncTile.tile;
                    }
                    else
                    {
                        tileDict.Add(syncTile.position, syncTile.tile);
                    }
                }

                foreach (Tilemap.SyncTile syncTile in syncTiles)
                {
                    try
                    {
                        Vector3Int up = syncTile.position + Vector3Int.up;
                        Vector3Int down = syncTile.position + Vector3Int.down;
                        TileBase top = GetTileAt(up);
                        TileBase bottom = GetTileAt(down);
                        
                        ClearAt(syncTile.position);
                        if (syncTile.tile != null)
                        {
                            PaintAt(syncTile.position, syncTile.tile);
                        }
                        
                        ClearAt(up);
                        PaintAt(up, top);
                        
                        ClearAt(down);
                        PaintAt(down, bottom);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private void PaintMultiply(Vector3Int pos, Tilemap tilemap, TileBase tile, TileSideType sideType)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (sideType == TileSideType.TOP && j == 0) continue;
                    if (sideType == TileSideType.BOTTOM && j != 0) continue;
                    
                    Vector3Int mulPos = pos + new Vector3Int(i, j);
                    tilemap.SetTile(mulPos, tile);
                }
            }
        }
    }
}