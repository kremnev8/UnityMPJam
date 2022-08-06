using System;
using UnityEngine;

namespace Gameplay.Util
{
    [ExecuteInEditMode]
    public class TriggerVisualizer : MonoBehaviour
    {
        public SpriteRenderer boxRenderer;
        public new Collider2D collider;
        private void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
                return;
            }
            
            boxRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnDrawGizmos()
        {
            if (collider != null)
            {
                boxRenderer.size = collider.bounds.size;
                transform.localPosition = collider.offset;
            }
        }
    }
}