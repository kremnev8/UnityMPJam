using Mirror;
using UnityEngine;

namespace Gameplay.World.Spacetime
{
    public interface ILinked
    {
        public WorldElement element { get; set; }

        public void ReciveStateChange(bool value);
    }
}