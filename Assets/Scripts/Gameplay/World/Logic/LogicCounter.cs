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
        private int current;
        public bool isPermanent;

        public List<LogicConnection> hitMax;
        public List<LogicConnection> hitMin;


        private void Invoke(List<LogicConnection> objects, bool isPermanent = true)
        {
            foreach (LogicConnection item in objects)
            {
                timeObject.SendLogicState(item.target.UniqueId, item.value, isPermanent);
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
        }

        public void Decrement()
        {
            current--;
            if (current <= minCount)
            {
                current = minCount;
                Invoke(hitMin);
            }
        }

        public void Reset()
        {
            current = minCount;
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