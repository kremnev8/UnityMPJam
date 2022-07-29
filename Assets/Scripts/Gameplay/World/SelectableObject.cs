using Gameplay.Core;
using Player;
using UnityEngine;

namespace Gameplay.World
{
    public class SelectableObject : MonoBehaviour
    {
        [HideInInspector]
        public bool isSelected;

        public Transform visualRoot;
        [HideInInspector]
        public Renderer[] renderers;

        private LayerSettings layers;
        
        private static MaterialPropertyBlock _propertyBlock;
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        
        private void Start()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }
            
            renderers = visualRoot.GetComponentsInChildren<Renderer>();
            layers = Simulation.GetModel<GameModel>().layers;
        }

        public virtual void SetVisible(bool value, Material mat)
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            _propertyBlock ??= new MaterialPropertyBlock();
            
            renderers = visualRoot.GetComponentsInChildren<Renderer>();
            foreach (Renderer meshRenderer in renderers)
            {
                meshRenderer.enabled = value;
                meshRenderer.material = mat;

            }
        }

        public void SetSelected(bool value)
        {
            isSelected = value;
            foreach (Renderer meshRenderer in renderers)
            {
                meshRenderer.gameObject.layer = isSelected ? layers.outlineLayer : layers.defaultLayer;
            }
        }

    }
}