using Gameplay.Conrollers;
using Gameplay.Util;
using UnityEngine;

namespace Gameplay.World
{
    public interface ISpawnable
    {
        void Spawn(PlayerController player, Vector2Int position, Direction direction);
        void Destroy();
    }
}