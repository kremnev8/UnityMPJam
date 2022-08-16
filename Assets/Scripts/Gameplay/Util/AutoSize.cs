using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Util;

namespace Gameplay.Util
{
    [ExecuteInEditMode]
    public class AutoSize : MonoBehaviour
    {
        public Tilemap masterTilemap;
        public SpriteRenderer waterRenderer;

        public SpriteRenderer darkTopRenderer;
        public SpriteRenderer darkRightRenderer;
        public SpriteRenderer darkBottomRenderer;
        public SpriteRenderer darkLeftRenderer;

        public float darkThinkness;
        
        
        private void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            if (masterTilemap != null)
            {
                masterTilemap.CompressBounds();

                Bounds bounds = masterTilemap.localBounds;
                
                Vector2 worldMin = masterTilemap.transform.TransformPoint(bounds.min); 
                Vector2 worldMax = masterTilemap.transform.TransformPoint(bounds.max);

                
                Vector2 center = (worldMin + worldMax) / 2f;
                Vector2 size = worldMax - worldMin;

                waterRenderer.size = size;
                waterRenderer.transform.localPosition = center;

                Vector2 darkOffset = size / 2f + new Vector2(darkThinkness / 2f - 1, darkThinkness / 2f - 1);
                Vector2 darkArea = size + new Vector2(darkThinkness * 2f - 2, darkThinkness * 2f - 2);
                
                Vector2 darkPos = center + new Vector2(0, darkOffset.y);
                Vector2 darkSize = new Vector2(darkArea.x, darkThinkness);
                darkTopRenderer.size = darkSize;
                darkTopRenderer.transform.localPosition = darkPos;

                darkPos = center + new Vector2(darkOffset.x, 0);
                darkSize = new Vector2(darkThinkness, darkArea.y);
                darkRightRenderer.size = darkSize;
                darkRightRenderer.transform.localPosition = darkPos;
                
                darkPos = center + new Vector2(0, -darkOffset.y + 1);
                darkSize = new Vector2(darkArea.x, darkThinkness + 2);
                darkBottomRenderer.size = darkSize;
                darkBottomRenderer.transform.localPosition = darkPos;
                
                darkPos = center + new Vector2(-darkOffset.x, 0);
                darkSize = new Vector2(darkThinkness, darkArea.y);
                darkLeftRenderer.size = darkSize;
                darkLeftRenderer.transform.localPosition = darkPos;
            }
        }
    }
}