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
                Destroy(gameObject);
                return;
            }
            waterRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (masterTilemap != null)
            {
                masterTilemap.CompressBounds();
                waterRenderer.size = masterTilemap.size.ToVector2() * 2;
                Vector3 center = masterTilemap.cellBounds.center;
                center.z = 0;
                center.x += 1;
                center.y += 1;
                transform.localPosition = center;
            }
        }
    }
}