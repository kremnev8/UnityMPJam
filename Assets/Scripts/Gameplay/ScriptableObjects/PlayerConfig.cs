﻿using System;
using System.Linq;
using UnityEngine;
using Util;

namespace ScriptableObjects
{
    [Serializable]
    public class PlayerAnim
    {
        public Sprite[] frames;
        public Vector3[] staffLight;
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
        
        public PlayerAnim[] fireMageWalk;
        public PlayerAnim[] fireMageIdle;
        
        public int idleFrameTime;
        public int walkFrameTime;

        [Header("Movement")]
        public float moveTime = 1f;
        public float failMoveTime = 1f;
        public LayerMask wallMask;
        public LayerMask ghostMask;

        public AnimationCurve stuckAnim;

        [InspectorButton]
        public void CopyToFire()
        {
            fireMageWalk = iceMageWalk.ToArray();
            fireMageIdle = iceMageIdle.ToArray();

        }
    }
}