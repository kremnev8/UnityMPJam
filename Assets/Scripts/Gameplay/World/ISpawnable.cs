using Gameplay.Conrollers;
using Gameplay.Util;
using UnityEngine;

namespace Gameplay.World
{
    public interface ISpawnable
    {
        string prefabId { get; }
        void Spawn(PlayerController player, Vector2Int position, Direction direction);
        void Destroy();
    }
}