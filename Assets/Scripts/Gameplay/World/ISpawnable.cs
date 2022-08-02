using Gameplay.Conrollers;
using Gameplay.Util;
using UnityEngine;

namespace Gameplay.World
{
    public interface ISpawnable
    {
        public Timeline timeline {get;set;}

        void Spawn(PlayerController player, Timeline timeline, Vector2Int position, Direction direction);
        void Destroy();
    }
}