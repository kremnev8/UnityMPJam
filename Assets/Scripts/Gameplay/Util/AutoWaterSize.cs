using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Util;

namespace Gameplay.Util
{
    [ExecuteInEditMode]
    public class AutoWaterSize : MonoBehaviour
    {
        public Tilemap masterTilemap;
        public SpriteRenderer waterRenderer;
        
        
        private void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
                return;
            }
            waterRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (masterTilemap != null)
            {
                masterTilemap.CompressBounds();

                Bounds bounds = masterTilemap.localBounds;
                
                Vector3 worldMin = masterTilemap.transform.TransformPoint(bounds.min); 
                Vector3 worldMax = masterTilemap.transform.TransformPoint(bounds.max);

                Vector3 center = (worldMin + worldMax) / 2f;
                Vector2 size = worldMax - worldMin;

                waterRenderer.size = size;
                transform.localPosition = center;
            }
        }
    }
}