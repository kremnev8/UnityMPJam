// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Unlit/SobelFilter"
{
	Properties 
	{
	    [HideInInspector]_MainTex ("Base (RGB)", 2D) = "white" {}
		_PixelDensity ("Pixel Density", float) = 10
        _Power ("Power", float) = 50
		_PosterizationCount ("Count", int) = 8

        _Color ("Glow Color", Color ) = ( 1, 1, 1, 1)
        _Intensity ("Intensity", Float) = 2
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass
		{

            Blend SrcAlpha OneMinusSrcAlpha
            
            ZWrite On

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            
            TEXTURE2D(_DepthTex);
            SAMPLER(sampler_DepthTex);

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            TEXTURE2D(_CameraColorTexture);
            SAMPLER(sampler_CameraColorTexture);
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D_X(_OutlineRenderTexture);
            SAMPLER(sampler_OutlineRenderTexture);
 
            TEXTURE2D_X(_OutlineBluredTexture);
            SAMPLER(sampler_OutlineBluredTexture);

            float _PixelDensity;
            int _PosterizationCount;
            float _Power;

            half4 _Color;
            half _Intensity;
            
            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv        : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            float SampleDepth(float2 uv)
            {
                return SAMPLE_DEPTH_TEXTURE(_DepthTex, sampler_DepthTex, uv);
            }
            
            float2 sobel (float2 uv) 
            {
                float2 delta = float2(_PixelDensity / _ScreenParams.x, _PixelDensity / _ScreenParams.y);

                float up = SampleDepth(uv + float2(0.0, 1.0) * delta);
                float down = SampleDepth(uv + float2(0.0, -1.0) * delta);
                float left = SampleDepth(uv + float2(1.0, 0.0) * delta);
                float right = SampleDepth(uv + float2(-1.0, 0.0) * delta);
                float centre = SampleDepth(uv);

                float depth = max(max(up, down), max(left, right));
                return float2(clamp(up - centre, 0, 1) + clamp(down - centre, 0, 1) + clamp(left - centre, 0, 1) + clamp(right - centre, 0, 1), depth);
            }

            half4 pixelate(half4 input, int strength)
            {
                input = pow(abs(input), 0.4545);
                float3 c = RgbToHsv(input);
                c.z = round(c.z * strength) / strength;
                input = float4(HsvToRgb(c), input.a);
                return pow(abs(input), 2.2);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = input.uv;
                
                return output;
            }
            
            half4 frag (Varyings input, out float depth : SV_Depth) : SV_Target 
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float2 sobelData = sobel(input.uv);
                float s = pow(abs(1 - saturate(sobelData.x)), _Power);
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 d = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv);
                float x = ceil(SampleDepth(input.uv) - d.x);

               // float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
                half4 prepassColor = SAMPLE_TEXTURE2D(_OutlineRenderTexture, sampler_OutlineRenderTexture, input.uv);
                half4 bluredColor = SAMPLE_TEXTURE2D(_OutlineBluredTexture, sampler_OutlineBluredTexture, input.uv);
                half4 difColor = max( 0, bluredColor - prepassColor);
                half4 color = difColor * _Color * _Intensity;
                //color.a = 1;  

                color = pixelate(color, _PosterizationCount); 

                col = pixelate(col, _PosterizationCount); 
                
                s = floor(s+0.2);
                
                s = lerp(1.0, s, ceil(sobelData.y - d.x));
                depth = lerp(sobelData.y, SampleDepth(input.uv), s);
                
               col.rgb *= s;
                col.a += 1 - s;
                
                col += color;

                
                return col;
            }
            
			#pragma vertex vert
			#pragma fragment frag
			
			ENDHLSL
		}
        
	} 
	FallBack "Diffuse"
}
