using System;
using System.Collections.Generic;
using Gameplay.World;
using Gameplay.World.Spacetime;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Logic
{
    [SelectionBase]
    public class LogicCounter : MonoBehaviour, ILinked
    {
        public int minCount = 0;
        public int maxCount = 2;
        public int threshold = 1;
        public int initialCount = 0;
        
        private int current;

        public List<LogicConnection> hitMax;
        public List<LogicConnection> hitMin;
        
        public List<LogicConnection> changedFromMax;
        public List<LogicConnection> changedFromMin;

        public List<ValueConnection> greaterThanThreshold;


        private void Invoke(List<LogicConnection> objects)
        {
            foreach (LogicConnection item in objects)
            {
                element.SendLogicState(item.target.UniqueId, item.value);
            }
        }
        
        protected void OnDrawGizmosSelected()
        {
            DrawWireGizmo(hitMax);
            DrawWireGizmo(hitMin);
            DrawWireGizmo(changedFromMax);
            DrawWireGizmo(changedFromMin);
            DrawWireGizmo(greaterThanThreshold);
        }
        
        protected void DrawWireGizmo(List<ValueConnection> connections)
        {
            Gizmos.color = Color.magenta;
            foreach (ValueConnection connection in connections)
            {
                if (connection != null && connection.target != null)
                    Gizmos.DrawLine(transform.position, connection.target.transform.position);
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

        protected void DrawWireGizmo(List<LogicConnection> connections)
        {
            Gizmos.color = Color.magenta;
            foreach (LogicConnection connection in connections)
            {
                if (connection != null && connection.target != null)
                    Gizmos.DrawLine(transform.position, connection.target.transform.position);
            }
        }
        
        
        private void Awake()
        {
            Reset();
        }

        public void Increment()
        {
            current++;
            if (current >= maxCount)
            {
                current = maxCount;
                Invoke(hitMax);
            }
            
            if (current - 1 == minCount)
            {
                Invoke(changedFromMin);
            }
            Invoke(greaterThanThreshold, current >= threshold);
        }

        public void Decrement()
        {
            current--;
            if (current <= minCount)
            {
                current = minCount;
                Invoke(hitMin);
            }

            if (current + 1 == maxCount)
            {
                Invoke(changedFromMax);
            }
            
            Invoke(greaterThanThreshold, current >= threshold);
        }

        public void Reset()
        {
            current = initialCount;
        }

        public WorldElement element { get; set; }

        public void ReciveStateChange(bool value)
        {
            if (value)
            {
                Increment();
            }
            else
            {
                Decrement();
            }
        }
    }
}