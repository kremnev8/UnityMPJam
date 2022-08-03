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
        TOP_LOOK_UP,
        BOTTOM_LOOK_DOWN,
        TOP_IF_NOT_THIS,
        BOTTOM_IF_NOT_THIS,
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

    [CreateAssetMenu(fileName = "Tile Set", menuName = "SO/New Tile Set", order = 0)]
    public class TileSet : ScriptableObject
    {
        public string tilesetName;
        public List<TileGroup> tileGroups;

        public TileBase wallBrush;

        public TileGroup GetGroup(TileBase tileBase)
        {
            return tileGroups.First(group => group.brushTile == tileBase);
        }
    }
}