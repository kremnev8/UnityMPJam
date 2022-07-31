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
            if (lastLiveUpdate != liveUpdate)
            {
                lastLiveUpdate = liveUpdate;
                Tilemap.tilemapTileChanged -= OnTileMapChanged;
                Tilemap.tilemapTileChanged += OnTileMapChanged;
            }

            if (tilemaps == null)
            {
                GetTilemaps();
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
        }

        [InspectorButton]
        public void Rebuild()
        {
            foreach (var pair in tilemaps)
            {
                pair.Value.ClearAllTiles();
            }

            int count = master.GetTilesRangeCount(master.cellBounds.min, master.cellBounds.max);

            if (positions.Length <= count)
            {
                Array.Resize(ref positions, count);
                Array.Resize(ref tiles, count);
            }

            count = master.GetTilesRangeNonAlloc(master.cellBounds.min, master.cellBounds.max, positions, tiles);

            for (int i = 0; i < count; i++)
            {
                Vector3Int pos = positions[i];
                TileBase tile = tiles[i];

                PaintAt(pos, tileSet.GetGroup(tile));
            }
        }

        private void PaintAt(Vector3Int pos, TileGroup group)
        {
            pos *= 2;

            foreach (TileMapping mapping in group.tileMappings)
            {
                foreach (TileMapType target in mapping.targets)
                {
                    try
                    {
                        Tilemap tileMap = tilemaps[target];
                        PaintMultiply(pos, tileMap, mapping.tile);
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
                    PaintMultiply(pos, pair.Value, null);
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
                    try
                    {
                        if (syncTile.tile == null)
                        {
                            ClearAt(syncTile.position);
                        }
                        else
                        {
                            TileGroup group = tileSet.GetGroup(syncTile.tile);
                            ClearAt(syncTile.position);
                            PaintAt(syncTile.position, group);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private void PaintMultiply(Vector3Int pos, Tilemap tilemap, TileBase tile)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Vector3Int mulPos = pos + new Vector3Int(i, j);
                    tilemap.SetTile(mulPos, tile);
                }
            }
        }
    }
}