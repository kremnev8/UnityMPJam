using System;
using System.Linq;
using Gameplay.Util;
using UnityEngine;
using Util;

namespace ScriptableObjects
{
    [Serializable]
    public class SimpleAnim
    {
        public Sprite[] frames;
    }
    
    
    [Serializable]
    public class PlayerAnim
    {
        public Sprite[] frames;
        public Vector3[] staffLight;
        public Vector3[] eyePos;

        public Vector3 globalEyePos;
        public bool useGlobalPos;

        public Vector3 GetEyePos(int frame)
        {
            if (useGlobalPos)
            {
                return globalEyePos;
            }

            return eyePos[frame];
        }
    }

    /// <summary>
    /// Player default controls configuration
    /// </summary>
    [CreateAssetMenu(fileName = "Player Config", menuName = "SO/New Player Config", order = 0)]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Appearance")]
        public Color iceMageLightColor;
        public Color fireMageLightColor;

        public PlayerAnim[] iceMageWalk;
        public PlayerAnim[] iceMageIdle;
        public PlayerAnim iceMageDeath;
        public SimpleAnim[] iceMageEyes;
        
        public PlayerAnim[] fireMageWalk;
        public PlayerAnim[] fireMageIdle;
        public PlayerAnim fireMageDeath;
        public SimpleAnim[] fireMageEyes;

        public int idleFrameTime;
        public int walkFrameTime;
        public int deathFrameTime;

        public int eyeHoldMinTime;
        public int eyeHoldMaxTime;
        public int eyeBlinkTime;
        
        [Header("Movement")]
        public float moveTime = 1f;
        public float failMoveTime = 1f;
        public float dashTime = 0.5f;
        public LayerMask wallMask;
        public LayerMask ghostMask;

        public AnimationCurve stuckAnim;

        [Header("Other")] 
        public float respawnTime = 4;
    }
}