using System;
using UnityEngine;

namespace Gameplay.Util
{
    public enum Direction : byte
    {
        FORWARD = 0,
        RIGHT,
        BACKWARD,
        LEFT
    }
    
    public static class DirectionExtension{

        public static Direction GetDirection(this Vector2Int vector)
        {
            return vector.x switch
            {
                0 when vector.y == 1 => Direction.FORWARD,
                1 when vector.y == 0 => Direction.RIGHT,
                0 when vector.y == -1 => Direction.BACKWARD,
                -1 when vector.y == 0 => Direction.LEFT,
                _ => Direction.FORWARD
            };
        }

        public static Vector2Int GetVector(this Direction direction)
        {
            switch (direction)
            {
                case Direction.FORWARD:
                    return Vector2Int.up;
                case Direction.RIGHT:
                    return Vector2Int.right;
                case Direction.BACKWARD:
                    return Vector2Int.down;
                case Direction.LEFT:
                    return Vector2Int.left;
                default:
                    return Vector2Int.up;
            }
        }

        public static Direction GetOpposite(this Direction direction)
        {
            switch (direction)
            {
                case Direction.FORWARD:
                    return Direction.BACKWARD;
                case Direction.RIGHT:
                    return Direction.LEFT;
                case Direction.BACKWARD:
                    return Direction.FORWARD;
                case Direction.LEFT:
                    return Direction.RIGHT;
                default:
                    return Direction.FORWARD;
            }
        }
        
    }
}