using System;
using UnityEngine;

namespace Gameplay.Util
{
    public class WaterSettings : MonoBehaviour
    {
        public float flowSpeed = 1;
        public float waterFlowDirection = 0;
        private static readonly int direction = Shader.PropertyToID("_Direction");
        private static readonly int speed = Shader.PropertyToID("_Speed");

        private void Start()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.material.SetFloat(direction, waterFlowDirection);
            renderer.material.SetFloat(speed, flowSpeed);
        }

        private void OnDrawGizmos()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sharedMaterial.SetFloat(direction, waterFlowDirection);
            renderer.sharedMaterial.SetFloat(speed, flowSpeed);
        }
    }
}