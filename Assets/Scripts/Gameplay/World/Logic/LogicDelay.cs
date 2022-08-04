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
        
        protected void OnDrawGizmosSelected()
        {
            DrawWireGizmo(timeEnded);
        }

        protected void DrawWireGizmo(List<LogicConnection> connections)
        {
            foreach (LogicConnection connection in connections)
            {
                Gizmos.DrawLine(transform.position, connection.target.transform.position);
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