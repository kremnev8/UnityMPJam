using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.World
{
    public class Interactible : NetworkBehaviour
    {
        [SerializeField]
        protected SpriteRenderer spriteRenderer;

        public Sprite idle;
        public Sprite activated;

        protected bool state;
        public UnityEvent<bool> onStateChanged;

        [ClientRpc]
        public void RpcSetState(bool newState)
        {
            if (isClientOnly)
            {
                SetState(newState);
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdSetState(bool newState)
        {
            SetState(newState);
            RpcSetState(newState);
        }
        
        
        protected virtual void SetState(bool newState)
        {
            state = newState;
            onStateChanged?.Invoke(state);
            spriteRenderer.sprite = state ? activated : idle;
        }
    }
}