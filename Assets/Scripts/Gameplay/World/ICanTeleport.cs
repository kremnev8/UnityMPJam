using Gameplay.Conrollers;
using Gameplay.Util;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public interface ICanTeleport
    {
        [Server]
        bool Teleport(Vector2Int position, Direction newDirection);

    }
}