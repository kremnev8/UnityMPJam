using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.Util;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Gameplay.World
{
    [Serializable]
    public class LogicConnection
    {
        public WorldElement target;
        public bool value;
    }

    [Serializable]
    public class ValueConnection
    {
        public WorldElement target;
        public bool invert;
    }
    
    [SelectionBase]
    [RequireComponent(typeof(WorldElement))]
    public class Interactible : NetworkBehaviour, IInteractable, ILinked
    {
        public Vector2Int forward;
        public bool checkFacing => true;
        public Vector2Int FacingDirection => forward;
        
        [SerializeField]
        protected SpriteRenderer spriteRenderer;

        public Sprite idle;
        public Sprite activated;

        protected bool interactible = true; 
        
        protected bool state;
        public bool initialState;
        
        
        public List<ValueConnection> newConnections;
        

        [HideInInspector]
        [SerializeField] 
        private WorldElement m_timeObject;
        
        protected virtual void Start()
        {
            SetState(initialState);
        }

        protected void OnDrawGizmosSelected()
        {
            DrawWireGizmo(newConnections);
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
        
        public void Invoke(List<ValueConnection> objects, bool value)
        {
            foreach (ValueConnection item in objects)
            {
                bool setValue = item.invert ? !value : value;
                element.SendLogicState(item.target.UniqueId, setValue);
            }
        }
        

        [ClientRpc]
        public virtual void RpcSetState(bool newState)
        {
            if (isClientOnly)
            {
                if (!interactible) return;
                
                SetState(newState);
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdSetState(bool newState)
        {
            if (!interactible) return;
            
            SetState(newState);
            RpcSetState(newState);
        }
        
        
        protected virtual void SetState(bool newState)
        {
            if (!interactible) return;
            
            state = newState;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = state ? activated : idle;
            }
            try
            {
                Invoke(newConnections, newState);
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to propagate state on object {gameObject.name}:\n{e}");
            }

        }
        
        
        
        public virtual void Activate(PlayerController player)
        {
        }

        public WorldElement element
        {
            get => m_timeObject;
            set => m_timeObject = value;
        }
        
        public virtual void ReciveStateChange(bool value)
        {
            
        }
    }
}