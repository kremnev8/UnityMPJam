using UnityEngine;

namespace Gameplay.Util
{
    public class DeferredAnimatorEnabler : MonoBehaviour {
        
        private Animator animator;
        private new SpriteRenderer renderer;
        
        public void Start()
        {
            animator = GetComponent<Animator>();
            renderer = GetComponent<SpriteRenderer>();
            Invoke(nameof(EnableAnimator), Random.value);
        }

        public void SetState(bool value)
        {
            if (!value)
            {
                animator.enabled = false;
                renderer.enabled = false;
            }
            else
            {
                Invoke(nameof(EnableAnimator), Random.value);
            }
        }

        private void EnableAnimator()
        {
            renderer.enabled = true;
            animator.enabled = true;
        }
    }
}