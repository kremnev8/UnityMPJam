using System.Collections.Generic;
using Gameplay.World;
using Gameplay.World.Spacetime;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Logic
{
    [SelectionBase]
    public class LogicDelay : MonoBehaviour, ILinked
    {
        public List<LogicConnection> timeEnded;
        public List<ValueConnection> timeEndedValue;
        public float timeout = 60f;

        public bool triggerOnAny;
        
        private bool lastValue;

        private void Invoke(List<LogicConnection> objects)
        {
            foreach (LogicConnection item in objects)
            {
                element.SendLogicState(item.target.UniqueId, item.value);
            }
        }
        
        protected void Invoke(List<ValueConnection> objects, bool value)
        {
            foreach (ValueConnection item in objects)
            {
                bool setValue = item.invert ? !value : value;
                element.SendLogicState(item.target.UniqueId, setValue);
            }
        }
        
        protected void OnDrawGizmosSelected()
        {
            DrawWireGizmo(timeEnded);
        }

        protected void DrawWireGizmo(List<LogicConnection> connections)
        {
            Gizmos.color = Color.magenta;
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
            lastValue = value;
            if (value || triggerOnAny)
            {
                Trigger();
            }
        }
    }
}