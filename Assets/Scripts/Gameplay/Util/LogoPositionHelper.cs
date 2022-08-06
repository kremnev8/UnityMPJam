using System;
using UnityEngine;

namespace Gameplay.Util
{
    [ExecuteInEditMode]
    public class LogoPositionHelper : MonoBehaviour
    {
        public RectTransform parent;
        public RectTransform target;

        public Material logoMaterial;

        public Vector3 position;
        private static readonly int center = Shader.PropertyToID("_Center");

        private void Update()
        {
            if (target != null && parent != null)
            {

                position = target.position - parent.position;
                logoMaterial.SetVector(center, position);
            }
        }
    }
}