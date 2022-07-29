using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering
{

    public class BlurPass : ScriptableRenderPass
    {
        private int _tmpBlurRTId1 = Shader.PropertyToID("_TempBlurTexture1");
    private int _tmpBlurRTId2 = Shader.PropertyToID("_TempBlurTexture2");

    private RenderTargetIdentifier _tmpBlurRT1;
    private RenderTargetIdentifier _tmpBlurRT2;

    private RenderTargetIdentifier _source;
    private RenderTargetHandle _destination;

    private int _passesCount;
    private int _downSample;
    private Material _blurMaterial;

    private float _pixelDensity;


    public BlurPass(RenderTargetIdentifier source, RenderTargetHandle destination, Material blurMaterial, int downSample, int passesCount, float pixelDensity)
    {
        _source = source;
        _destination = destination;

        _blurMaterial = blurMaterial;
        _downSample = downSample;
        _passesCount = passesCount;
        _pixelDensity = pixelDensity;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        var width = Mathf.Max(1, cameraTextureDescriptor.width >> _downSample);
        var height = Mathf.Max(1, cameraTextureDescriptor.height >> _downSample);
        //width = Mathf.RoundToInt(width / _pixelDensity);
       // height = Mathf.RoundToInt(height / _pixelDensity);
        
        var blurTextureDesc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0, 0);

        _tmpBlurRT1 = new RenderTargetIdentifier(_tmpBlurRTId1);
        _tmpBlurRT2 = new RenderTargetIdentifier(_tmpBlurRTId2);

        cmd.GetTemporaryRT(_tmpBlurRTId1, blurTextureDesc, FilterMode.Point);
        cmd.GetTemporaryRT(_tmpBlurRTId2, blurTextureDesc, FilterMode.Point);
        cmd.GetTemporaryRT(_destination.id, blurTextureDesc, FilterMode.Point);

        ConfigureTarget(_destination.Identifier());
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get("BlurPass");

        if (_passesCount > 0)
        {
            cmd.Blit(_source, _tmpBlurRT1, _blurMaterial, 0);
            for (int i = 0; i < _passesCount - 1; i++)
            {
                cmd.Blit(_tmpBlurRT1, _tmpBlurRT2, _blurMaterial, 0);
                var t = _tmpBlurRT1;
                _tmpBlurRT1 = _tmpBlurRT2;
                _tmpBlurRT2 = t;
            }
            cmd.Blit(_tmpBlurRT1, _destination.Identifier());
        }
        else
            cmd.Blit(_source, _destination.Identifier());

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    }
}