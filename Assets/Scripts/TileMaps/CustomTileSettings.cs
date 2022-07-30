using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMaps
{
    [Serializable]
    public struct TileSet
    {
        public TileBase wallTile;
        public TileBase floorTile;
    }
    public class CustomTileSettings : MonoBehaviour
    {
        public TileSet[] tileSets;
    }
}