using System;
using System.Collections.Generic;
using Gameplay.World;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Logic
{
    public class TriggerGeneric : NetworkBehaviour, ITimeLinked
    {
        public List<LogicConnection> onEnter;
        public List<LogicConnection> onExit;

        public bool useTriggers = false;
        public bool onlyPlayer = false;
        public bool onlyOnce = false;
        private bool wasTriggered;

        private void Invoke(List<LogicConnection> objects, bool isPermanent = true)
        {
            foreach (LogicConnection item in objects)
            {
                timeObject.SendLogicState(item.target.UniqueId, item.value, isPermanent);
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