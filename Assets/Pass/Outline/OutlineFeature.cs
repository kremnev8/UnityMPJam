using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Rendering
{
    public class OutlineFeature : ScriptableRendererFeature
    {

        [Serializable]
        public class RenderSettings
        {
            public Material OverrideMaterial = null;
            public LayerMask LayerMask = 0;
        }
        
        [Serializable]
        public class BlurSettings
        {
            public Material BlurMaterial;
            public int DownSample = 1;
            public int PassesCount = 1;
        }
        
        [System.Serializable]
        public class PixelArtFeatureSettings {

            public LayerMask layerMask = 0;

            public RenderPassEvent Event = RenderPassEvent.BeforeRenderingTransparents;
            
            public Material blitMat = null;

            [Range(1f, 15f)]
            public float pixelDensity = 1f;
        }
        
        [SerializeField] private string _renderTextureName;
        [SerializeField] private RenderSettings _renderSettings;
        [SerializeField] private RenderPassEvent _renderPassEvent;
        
        [SerializeField] private Material _outlineMaterial;
       // private OutlinePass _outlinePass;

        [SerializeField] private string _bluredTextureName;
        [SerializeField] private BlurSettings _blurSettings;
        private RenderTargetHandle _bluredTexture;
        private BlurPass _blurPass;
        
        private RenderTargetHandle _renderTexture;
        private ObjectsPass _renderPass;
        
        [SerializeField] private string _transferTextureName;
        private RenderTargetHandle _toPixelTexture;

        [FormerlySerializedAs("settings")] 
        public PixelArtFeatureSettings pixelateSettings = new PixelArtFeatureSettings();

        PixelArtFeaturePass pixelArtPass;

        public override void Create()
        {
            _renderTexture.Init(_renderTextureName);
            _bluredTexture.Init(_bluredTextureName);
            _toPixelTexture.Init(_transferTextureName);
            
            _renderPass = new ObjectsPass(_renderTexture, _renderSettings.LayerMask, _renderSettings.OverrideMaterial, pixelateSettings.pixelDensity);
            _blurPass = new BlurPass(_renderTexture.Identifier(), _bluredTexture, _blurSettings.BlurMaterial, _blurSettings.DownSample, _blurSettings.PassesCount, pixelateSettings.pixelDensity);
            pixelArtPass = new PixelArtFeaturePass(pixelateSettings.Event, pixelateSettings.blitMat, pixelateSettings.pixelDensity, pixelateSettings.layerMask, _outlineMaterial);
            
            //  _outlinePass = new OutlinePass(_outlineMaterial);
            
            _renderPass.renderPassEvent = _renderPassEvent;
            _blurPass.renderPassEvent = _renderPassEvent;
           // _outlinePass.renderPassEvent = _renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_renderPass);
            renderer.EnqueuePass(_blurPass);
            //renderer.EnqueuePass(_outlinePass); 
            renderer.EnqueuePass(pixelArtPass);
        }
    }
}