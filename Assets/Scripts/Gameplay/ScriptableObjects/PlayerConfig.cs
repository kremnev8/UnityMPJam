using UnityEngine;

namespace ScriptableObjects
{
    /// <summary>
    /// Player default controls configuration
    /// </summary>
    [CreateAssetMenu(fileName = "Player Config", menuName = "SO/New Player Config", order = 0)]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Zoom")]
        public float zoomSpeed;
        public float minZoom;
        public float maxZoom;

        public float scrollSensitivity;
        
        [Header("Movement")]
        public float moveTime = 1f;
        public float failMoveTime = 1f;
        public LayerMask wallMask;

        public AnimationCurve stuckAnim;
    }
}