using System;
using System.Collections.Generic;
using Gameplay.Controllers;
using Gameplay.World;
using Gameplay.World.Spacetime;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace Gameplay.Logic
{
    [SelectionBase]
    public class LogicAuto : MonoBehaviour, ILinked, ILogicConnectable
    {
        public List<LogicConnection> connections;

        
        public List<LogicConnection> Connections
        {
            get => connections;
            set => connections = value;
        }
        
        private void Invoke(List<LogicConnection> objects)
        {
            foreach (LogicConnection item in objects)
            {
                if (item.target == null) continue;
                element.SendLogicState(item.target.UniqueId, item.value);
            }
        }
        
        protected void OnDrawGizmosSelected()
        {
            this.DrawWireGizmo(connections);
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