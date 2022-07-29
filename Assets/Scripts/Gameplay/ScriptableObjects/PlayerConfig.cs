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
        public float normalMaxSpeed = 5f;
        public float moveSmoothTime = 1f;
        public float normalSmoothTime = 1f;
        public float friction = 0.8f;
    }
}