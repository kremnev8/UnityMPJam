using System;
using System.Collections.Generic;
using Gameplay.ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;
using Util;

namespace TileMaps
{
    public class ShadowInfo
    {
        public GameObject top;
        public GameObject left;
    }

    [ExecuteInEditMode]
    public class TileSetController : MonoBehaviour
    {
        public TileSet tileSet;
        public Dictionary<TileMapType, Tilemap> tilemaps;

        public Tilemap master;
        public bool liveUpdate;

        public Transform shadows;
        public GameObject topShadowPrefab;
        public GameObject leftShadowPrefab;


        [NonSerialized] private bool lastLiveUpdate = false;


        private static Vector3Int[] positions = new Vector3Int[100];
        private static TileBase[] tiles = new TileBase[100];
        private static Dictionary<Vector3Int, TileBase> tileDict = new Dictionary<Vector3Int, TileBase>(100);
        private static Dictionary<Vector3Int, ShadowInfo> shadowsDict = new Dictionary<Vector3Int, ShadowInfo>(100);


#if UNITY_EDITOR
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

                shadowsDict.Clear();

                foreach (Transform shadow in shadows)
                {
                    Vector3Int pos = shadow.localPosition.ToVector3Int();
                    AddShadowInfo(pos, shadow);
                }
            }
        }

        private static void AddShadowInfo(Vector3Int pos, Transform shadow)
        {
            if (shadowsDict.ContainsKey(pos))
            {
                if (shadow.name.Contains("top"))
                {
                    shadowsDict[pos].top = shadow.gameObject;
                }
                else
                {
                    shadowsDict[pos].left = shadow.gameObject;
                }
            }
            else
            {
                ShadowInfo info = new ShadowInfo();
                if (shadow.name.Contains("top"))
                {
                    info.top = shadow.gameObject;
                }
                else
                {
                    info.left = shadow.gameObject;
                }

                shadowsDict.Add(pos, info);
            }
        }

        [InspectorButton]
        public void Rebuild()
        {
            foreach (var pair in tilemaps)
            {
                pair.Value.ClearAllTiles();
            }

            shadows.ClearChildren();

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
                CheckShadowAt(pos, tile);
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
            if (type == TileSideType.TOP_LOOK_UP && hasTop) return TileSideType.ALL;
            if (type == TileSideType.BOTTOM_LOOK_DOWN && hasBottom) return TileSideType.ALL;

            if (type == TileSideType.TOP_IF_NOT_THIS && !hasTop) return TileSideType.TOP;
            if (type == TileSideType.TOP_IF_NOT_THIS) return TileSideType.NONE;
            
            if (type == TileSideType.BOTTOM_IF_NOT_THIS && !hasBottom) return TileSideType.BOTTOM;
            if (type == TileSideType.BOTTOM_IF_NOT_THIS) return TileSideType.NONE;

            return type;
        }

        public bool CheckTileEqual(TileBase a, TileBase b)
        {
            if (a == b) return true;

            foreach (TilePair pair in tileSet.siblings)
            {
                if (pair.tile1 == a && pair.tile2 == b) return true;
                if (pair.tile1 == b && pair.tile2 == a) return true;
            }

            return false;
        }

        private void CheckShadowAt(Vector3Int pos, TileBase brush)
        {
            if (tileSet.wallBrush == brush)
            {
                TileBase top = GetTileAt(pos + Vector3Int.up);
                TileBase left = GetTileAt(pos + Vector3Int.left);
                pos *= 2;

                if (top != brush)
                {
                    AddWallShadow(pos, topShadowPrefab);
                }

                if (left != brush)
                {
                    AddWallShadow(pos, leftShadowPrefab);
                }
            }
        }

        private void AddWallShadow(Vector3Int pos, GameObject prefab)
        {
            GameObject shadow = Instantiate(prefab, shadows);
            shadow.transform.localPosition = pos;
            AddShadowInfo(pos, shadow.transform); 
        }

        private void PaintAt(Vector3Int pos, TileBase brush)
        {
            try
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
                            TileSideType sideType = DetermineSideType(mapping.sideType, 
                                CheckTileEqual(top, brush), 
                                CheckTileEqual(bottom, brush));
                            PaintMultiply(pos, tileMap, mapping.tile, sideType);
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"Failed to paint TileMapping:\n{e}");
                        }
                    }
                }
            }
            catch (InvalidOperationException)
            {
                Debug.Log($"Failed to paint tile at pos: {pos}, brush: {(brush != null ? brush.name : "null")}");
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
                        ClearAt(up);
                        ClearAt(down);

                        ClearShadowAt(syncTile.position);
                        ClearShadowAt(up);
                        ClearShadowAt(down);

                        if (syncTile.tile != null)
                        {
                            PaintAt(syncTile.position, syncTile.tile);
                            CheckShadowAt(syncTile.position, syncTile.tile);
                        }

                        if (top != null)
                        {
                            PaintAt(up, top);
                            CheckShadowAt(up, top);
                        }

                        if (bottom != null)
                        {
                            PaintAt(down, bottom);
                            CheckShadowAt(down, bottom);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error syncing!\n{e}");
                        // ignored
                    }
                }
            }
        }

        private static void ClearShadowAt(Vector3Int position)
        {
            position *= 2;

            if (shadowsDict.ContainsKey(position))
            {
                ShadowInfo info = shadowsDict[position];
                DestroyImmediate(info.top);
                DestroyImmediate(info.left);
            }
        }

        private void PaintMultiply(Vector3Int pos, Tilemap tilemap, TileBase tile, TileSideType sideType)
        {
            if (sideType == TileSideType.NONE) return;
            
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (sideType is TileSideType.TOP or TileSideType.TOP_LOOK_UP && j == 0) continue;
                    if (sideType is TileSideType.BOTTOM or TileSideType.BOTTOM_LOOK_DOWN && j != 0) continue;

                    Vector3Int mulPos = pos + new Vector3Int(i, j);
                    tilemap.SetTile(mulPos, tile);
                }
            }
        }
        
#endif
    }
}