using System;
using System.Collections.Generic;
using Gameplay.Controllers;
using Gameplay.World;
using Gameplay.World.Spacetime;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Logic
{
    public class LogicAuto : MonoBehaviour, ITimeLinked
    {
        public List<LogicConnection> connections;

        private void Invoke(List<LogicConnection> objects, bool isPermanent)
        {
            foreach (LogicConnection item in objects)
            {
                timeObject.SendLogicState(item.target.UniqueId, item.value, isPermanent);
            }
        }
        
        protected void OnDrawGizmosSelected()
        {
            DrawWireGizmo(connections);
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
            GameNetworkManager.MapStarted += OnMapStarted;
        }

        private void OnMapStarted()
        {
            Invoke(connections, true);
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
        }
    }
}