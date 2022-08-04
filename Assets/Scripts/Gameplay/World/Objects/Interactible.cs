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
        public SpaceTimeObject target;
        public bool value;
    }

    [Serializable]
    public class ValueConnection
    {
        public SpaceTimeObject target;
        public bool invert;
    }
    
    [RequireComponent(typeof(SpaceTimeObject))]
    public class Interactible : NetworkBehaviour, IInteractable, ITimeLinked
    {
        public Vector2Int forward;
        public Vector2Int FacingDirection => forward;
        
        [SerializeField]
        protected SpriteRenderer spriteRenderer;

        public Sprite idle;
        public Sprite activated;
        public Sprite destroyed;
        
        public ParticleLight timeGlitchEffect;

        protected bool interactible = true; 
        
        protected bool state;
        public bool initialState;
        [HideInInspector]
        public bool fromTimeEvent;
        
        
        [HideInInspector]
        [FormerlySerializedAs("connections")] 
        public List<SpaceTimeObject> oldConnections;
        
        public List<ValueConnection> newConnections;


        private void OnValidate()
        {
            if (oldConnections is { Count: > 0 })
            {
                newConnections.AddRange(oldConnections.Select(o => new ValueConnection(){target = o}));
                oldConnections.Clear();
            }
        }

        [HideInInspector]
        [SerializeField] 
        private SpaceTimeObject m_timeObject;
        
        private void Start()
        {
            SpaceTimeObject to = GetComponent<SpaceTimeObject>();
            if (to.GetState(to.timeline) == ObjectState.DOES_NOT_EXIST)
            {
                gameObject.SetActive(false);
            }
        }
        
        private void Invoke(List<ValueConnection> objects, bool value, bool isPermanent = true)
        {
            foreach (ValueConnection item in objects)
            {
                bool setValue = item.invert ? !value : value;
                timeObject.SendLogicState(item.target.UniqueId, setValue, isPermanent);
            }
        }

        [ClientRpc]
        public void RpcSetState(bool newState, bool isPermanent)
        {
            if (isClientOnly)
            {
                if (!interactible) return;
                
                SetState(newState, isPermanent);
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdSetState(bool newState, bool isPermanent)
        {
            if (!interactible) return;
            
            SetState(newState, isPermanent);
            RpcSetState(newState, isPermanent);
        }
        
        
        protected virtual void SetState(bool newState, bool isPermanent = true)
        {
            if (!interactible) return;
            
            state = newState;
            spriteRenderer.sprite = state ? activated : idle;
            try
            {
                Invoke(newConnections, newState, isPermanent);

                if (!fromTimeEvent)
                {
                    timeObject.SendTimeEvent(new[] { newState ? 1 : 0 });
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to propagate state on object {gameObject.name}:\n{e}");
            }

        }
        
        
        
        public virtual void Activate(PlayerController player)
        {
        }

        public SpaceTimeObject timeObject
        {
            get => m_timeObject;
            set => m_timeObject = value;
        }

        public virtual void Configure(ObjectState state)
        {
            switch (state)
            {
                case ObjectState.DOES_NOT_EXIST:
                    gameObject.SetActive(false);
                    return;
                case ObjectState.BROKEN:
                    SetState(false);
                    interactible = false;
                    spriteRenderer.sprite = destroyed;
                    break;
                default:
                    interactible = true;
                    SetState(initialState);
                    break;
            }
        }

        public virtual void ReciveTimeEvent(int[] args)
        {
            fromTimeEvent = true;
            SetState(args[0] == 1);
            fromTimeEvent = false;
            
            if (timeGlitchEffect != null)
                timeGlitchEffect.Play();
        }

        public virtual void ReciveStateChange(bool value, bool isPermanent)
        {
            
        }
    }
}