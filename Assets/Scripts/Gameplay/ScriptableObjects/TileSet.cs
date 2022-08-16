using System;
using System.Collections.Generic;
using System.Linq;
using TileMaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Gameplay.ScriptableObjects
{
    public enum TileSideType
    {
        ALL,
        TOP,
        BOTTOM,
        TOP_OR_ALL_IF_THIS,
        BOTTOM_OR_ALL_IF_THIS,
        TOP_IF_NOT_THIS,
        BOTTOM_IF_NOT_THIS,
        TOP_IF_FLOOR,
        BOTTOM_IF_FLOOR,
        TOP_ONLY,
        BOTTOM_ONLY,
        TOP_IF_NOT_FLOOR,
        BOTTOM_IF_NOT_FLOOR,
        NONE
    }

    [Serializable]
    public class TileMapping
    {
        public TileBase tile;
        public List<TileMapType> targets;
        public TileSideType sideType = TileSideType.ALL;
    }

    [Serializable]
    public class TileGroup
    {
        public TileBase brushTile;
        public List<TileMapping> tileMappings;
    }

    [Serializable]
    public class TilePair
    {
        public TileBase tile1;
        public TileBase tile2;
    }

    [CreateAssetMenu(fileName = "Tile Set", menuName = "SO/New Tile Set", order = 0)]
    public class TileSet : ScriptableObject
    {
        public string tilesetName;
        public List<TileGroup> tileGroups;
        public List<TilePair> siblings;

        public TileBase[] wallBrushes;
        public TileBase[] floorBrushes;
        
        public TileGroup GetGroup(TileBase tileBase)
        {
            return tileGroups.First(group => group.brushTile == tileBase);
        }
    }
}