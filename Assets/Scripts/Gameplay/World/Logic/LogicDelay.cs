using System.Collections.Generic;
using Gameplay.World;
using Gameplay.World.Spacetime;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Logic
{
    public class LogicDelay : MonoBehaviour, ILinked
    {
        public List<LogicConnection> timeEnded;
        public float timeout = 60f;

        private void Invoke(List<LogicConnection> objects)
        {
            foreach (LogicConnection item in objects)
            {
                element.SendLogicState(item.target.UniqueId, item.value);
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
                if (connection != null && connection.target != null)
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

        public WorldElement element { get; set; }

        public void ReciveStateChange(bool value)
        {
            if (value)
            {
                Trigger();
            }
        }
    }
}