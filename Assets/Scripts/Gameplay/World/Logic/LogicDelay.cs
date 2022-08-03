using System.Collections.Generic;
using Gameplay.World;
using Gameplay.World.Spacetime;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Logic
{
    public class LogicDelay : MonoBehaviour, ITimeLinked
    {
        public List<LogicConnection> timeEnded;
        public float timeout = 60f;

        private void Invoke(List<LogicConnection> objects, bool isPermanent = true)
        {
            foreach (LogicConnection item in objects)
            {
                timeObject.SendLogicState(item.target.UniqueId, item.value, isPermanent);
            }
        }
        

        public void Trigger()
        {
            Invoke(nameof(TriggerLater), timeout);
        }

        private void TriggerLater()
        {
            Invoke(timeEnded);
        }

        public SpaceTimeObject timeObject { get; set; }
        public void Configure(ObjectState state)
        {
            if (state == ObjectState.DOES_NOT_EXIST)
            {
                gameObject.SetActive(false);
            }
        }

        public void ReciveTimeEvent(int[] args)
        {
        }

        public void ReciveStateChange(bool value, bool isPermanent)
        {
            if (value)
            {
                Trigger();
            }
        }
    }
}