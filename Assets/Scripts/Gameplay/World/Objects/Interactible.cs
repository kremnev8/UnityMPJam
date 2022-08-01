using System;
using System.Collections.Generic;
using Gameplay.Core;
using Gameplay.World.Spacetime;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.World
{
    public class Interactible : NetworkBehaviour, IInteractable, ITimeLinked
    {
        public Vector2Int forward;
        public Vector2Int FacingDirection => forward;
        
        [SerializeField]
        protected SpriteRenderer spriteRenderer;

        public Sprite idle;
        public Sprite activated;
        public Sprite destroyed;

        protected bool interactible = true; 
        
        protected bool state;
        public bool initialState;

        public List<SpaceTimeObject> connections;
        
        [SerializeField] 
        private SpaceTimeObject m_timeObject;

        private void Start()
        {
           /* SpaceTimeObject to = GetComponent<SpaceTimeObject>();
            if (to.GetState(to.timeline) == ObjectState.DOES_NOT_EXIST)
            {
                gameObject.SetActive(false);
            }*/
        }

        [ClientRpc]
        public void RpcSetState(bool newState)
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
            spriteRenderer.sprite = state ? activated : idle;
            try
            {
                foreach (SpaceTimeObject spaceTimeObject in connections)
                {
                    timeObject.SendLogicState(spaceTimeObject.UniqueId, newState);
                }
                timeObject.SendTimeEvent(new []{ newState ? 1 : 0});
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to propagate state on object {gameObject.name}:\n{e}");
            }

        }
        
        
        
        public virtual void Activate()
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
            SetState(args[0] == 1);
        }

        public virtual void ReciveStateChange(bool value)
        {

        }
    }
}