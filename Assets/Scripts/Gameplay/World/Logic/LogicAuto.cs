using System;
using System.Collections.Generic;
using Gameplay.Controllers;
using Gameplay.World;
using Gameplay.World.Spacetime;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Logic
{
    public class LogicAuto : MonoBehaviour, ILinked
    {
        public List<LogicConnection> connections;

        private void Invoke(List<LogicConnection> objects)
        {
            foreach (LogicConnection item in objects)
            {
                element.SendLogicState(item.target.UniqueId, item.value);
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
                if (connection != null && connection.target != null)
                    Gizmos.DrawLine(transform.position, connection.target.transform.position);
            }
        }
        
        private void Awake()
        {
            GameNetworkManager.MapStarted += OnMapStarted;
        }

        private void OnMapStarted()
        {
            Invoke(connections);
        }

        public WorldElement element { get; set; }

        public void ReciveStateChange(bool value)
        {
        }
    }
}