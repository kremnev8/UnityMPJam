using Gameplay.Conrollers;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public interface ICanTeleport
    {
        [Server]
        bool Teleport(Vector2Int position);
        
    }
}