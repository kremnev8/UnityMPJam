using System;
using System.Collections.Generic;
using Gameplay.World;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace Gameplay.Logic
{
    [SelectionBase]
    public class TriggerGeneric : NetworkBehaviour, ILinked,ILogicConnectable
    {
        public List<LogicConnection> onEnter;
        public List<LogicConnection> onExit;

        public bool useTriggers = false;
        public bool onlyPlayer = false;
        public bool onlyOnce = false;
        private bool wasTriggered;
        
        public List<LogicConnection> Connections
        {
            get => onEnter;
            set => onEnter = value;
        }

        protected void OnDrawGizmosSelected()
        {
            this.DrawWireGizmo(onEnter);
            this.DrawWireGizmo(onExit);
        }


        private void Invoke(List<LogicConnection> objects)
        {
            foreach (LogicConnection item in objects)
            {
                element.SendLogicState(item.target.UniqueId, item.value);
            }
        }

        [ClientRpc]
        public void RpcOnEntered()
        {
            if (isClientOnly)
            {
                Invoke(onEnter);
            }
        }
        
        [ClientRpc]
        public void RpcOnExit()
        {
            if (isClientOnly)
            {
                Invoke(onExit);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (onlyOnce && wasTriggered) return;
            if (other.isTrigger && !useTriggers) return;
            if (onlyPlayer && !other.gameObject.CompareTag("Player")) return;
            if (other.CompareTag("NoPhysics")) return;
            
            Invoke(onEnter);
            RpcOnEntered();
            wasTriggered = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (onlyOnce && wasTriggered) return;
            if (other.isTrigger && !useTriggers) return;
            if (onlyPlayer && !other.gameObject.CompareTag("Player")) return;
            
            Invoke(onExit);
            RpcOnExit();
            wasTriggered = true;
        }

        public WorldElement element { get; set; }

        public void ReciveStateChange(bool value)
        {
        }
    }
}