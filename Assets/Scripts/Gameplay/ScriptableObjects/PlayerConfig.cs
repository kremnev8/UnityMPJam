using UnityEngine;

namespace ScriptableObjects
{
    /// <summary>
    /// Player default controls configuration
    /// </summary>
    [CreateAssetMenu(fileName = "Player Config", menuName = "SO/New Player Config", order = 0)]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Appearance")]
        public Color iceMageLightColor;
        public Color fireMageLightColor;

        public Sprite[] iceMageSprites;
        public Sprite[] fireMageSprites;

        public Vector3[] iceStaffLightPos;
        public Vector3[] fireStaffLightPos;
        

        [Header("Movement")]
        public float moveTime = 1f;
        public float failMoveTime = 1f;
        public LayerMask wallMask;
        public LayerMask ghostMask;

        public AnimationCurve stuckAnim;
    }
}