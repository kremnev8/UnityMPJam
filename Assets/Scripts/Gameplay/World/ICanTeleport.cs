using Gameplay.Conrollers;
using Mirror;
using UnityEngine;

namespace Gameplay.World
{
    public interface ICanTeleport
    {
        [Server]
        void Teleport(Timeline timeline, Vector2Int position);

    }
}