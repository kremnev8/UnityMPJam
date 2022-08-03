using UnityEngine;

namespace TileMaps
{
    public enum TileMapType
    {
        PIT = 1,
        PIT_OVERLAY_1,
        PIT_OVERLAY_2,
        PIT_OVERLAY_3,
        FLOOR = 10,
        FLOOR_OVERLAY_1,
        FLOOR_OVERLAY_2,
        FLOOR_OVERLAY_3,
        WALLS = 20,
        WALL_OVERLAY_1,
        WALL_OVERLAY_2,
        WALL_OVERLAY_3,
        DECORATION = 30
        
    }
    
    public class TilemapId : MonoBehaviour
    {
        public TileMapType type;
    }
}