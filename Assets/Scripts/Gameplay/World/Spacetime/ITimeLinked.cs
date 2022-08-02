using Mirror;
using UnityEngine;

namespace Gameplay.World.Spacetime
{
    public interface ITimeLinked
    {
        public SpaceTimeObject timeObject { get; set; }

        public void Configure(ObjectState state);
        public void ReciveTimeEvent(int[] args);
        
        public void ReciveStateChange(bool value, bool isPermanent);
    }
}