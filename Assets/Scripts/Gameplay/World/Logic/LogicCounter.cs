using System;
using System.Collections.Generic;
using Gameplay.World;
using Gameplay.World.Spacetime;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Logic
{
    public class LogicCounter : MonoBehaviour, ITimeLinked
    {
        public int minCount = 0;
        public int maxCount = 2;
        public int initialCount = 0;
        
        private int current;
        public bool isPermanent;

        public List<LogicConnection> hitMax;
        public List<LogicConnection> hitMin;
        
        public List<LogicConnection> changedFromMax;
        public List<LogicConnection> changedFromMin;


        private void Invoke(List<LogicConnection> objects, bool isPermanent = true)
        {
            foreach (LogicConnection item in objects)
            {
                timeObject.SendLogicState(item.target.UniqueId, item.value, isPermanent);
            }
        }
        
        protected void OnDrawGizmosSelected()
        {
            DrawWireGizmo(hitMax);
            DrawWireGizmo(hitMin);
            DrawWireGizmo(changedFromMax);
            DrawWireGizmo(changedFromMin);
        }

        protected void DrawWireGizmo(List<LogicConnection> connections)
        {
            foreach (LogicConnection connection in connections)
            {
                if (connection != null)
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
        }

        public void Reset()
        {
            current = initialCount;
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
                Increment();
            }
            else
            {
                Decrement();
            }
        }
    }
}