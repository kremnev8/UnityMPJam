using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.World
{
    public class VisualState : MonoBehaviour
    {
        [SerializeField]
        protected SpriteRenderer spriteRenderer;

        public Sprite idle;
        public Sprite activated;

        protected bool state;
        public UnityEvent<bool> onStateChanged;

        
        public virtual void SetState(bool newState)
        {
            state = newState;
            onStateChanged?.Invoke(state);
            spriteRenderer.sprite = state ? activated : idle;
        }
    }
}