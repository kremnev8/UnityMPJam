using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering
{
    public class ObjectsPass : ScriptableRenderPass
    {
        private RenderTargetHandle _destination;
        private Material _overrideMaterial;
        
        private List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };
        private FilteringSettings _filteringSettings;
        private RenderStateBlock _renderStateBlock;

        private float _pixelDensity;
        
        public ObjectsPass(RenderTargetHandle destination, int layerMask, Material overrideMaterial, float pixelDensity)
        {
            _destination = destination;
            _overrideMaterial = overrideMaterial;
            
            _filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
            _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            _pixelDensity = pixelDensity;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            int pixelWidth = (int) (cameraTextureDescriptor.width / _pixelDensity);
            int pixelHeight = (int) (cameraTextureDescriptor.height / _pixelDensity);
            
            cmd.GetTemporaryRT(_destination.id, pixelWidth, pixelHeight, 0, FilterMode.Point);
            ConfigureTarget(_destination.Identifier());
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = _overrideMaterial;

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings, ref _renderStateBlock);
        }
    }
}