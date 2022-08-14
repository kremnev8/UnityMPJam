using System.Collections.Generic;
using Gameplay.World;
using Gameplay.World.Spacetime;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace Gameplay.Logic
{
    [SelectionBase]
    public class LogicDelay : MonoBehaviour, ILinked, IValueConnectable
    {
        public List<LogicConnection> timeEnded;
        public List<ValueConnection> timeEndedValue;
        public float timeout = 60f;

        public bool triggerOnAny;
        
        private bool lastValue;
        
        public List<ValueConnection> Connections
        {
            get => timeEndedValue;
            set => timeEndedValue = value;
        }

        private void Invoke(List<LogicConnection> objects)
        {
            foreach (LogicConnection item in objects)
            {
                if (item.target == null) continue;
                element.SendLogicState(item.target.UniqueId, item.value);
            }
        }
        
        protected void Invoke(List<ValueConnection> objects, bool value)
        {
            foreach (ValueConnection item in objects)
            {
                if (item.target == null) continue;
                bool setValue = item.invert ? !value : value;
                element.SendLogicState(item.target.UniqueId, setValue);
            }
        }
        
        protected void OnDrawGizmosSelected()
        {
            this.DrawWireGizmo(timeEnded);
            this.DrawWireGizmo(timeEndedValue);
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