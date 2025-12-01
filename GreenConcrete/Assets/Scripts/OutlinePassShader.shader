Shader "Hidden/OutlinePass"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineThickness ("Outline Thickness", Float) = 1.0
        _DepthSensitivity ("Depth Sensitivity", Range(0, 1)) = 0.1
        _NormalsSensitivity ("Normals Sensitivity", Range(0, 1)) = 0.5
        _ColorSensitivity ("Color Sensitivity", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }
        
        Pass
        {
            Name "Outline"
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5
            #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            TEXTURE2D_X(_CameraDepthTexture);
            TEXTURE2D_X(_CameraNormalsTexture);
            TEXTURE2D_X(_CameraColorTexture);
            
            SAMPLER(sampler_CameraDepthTexture);
            SAMPLER(sampler_CameraNormalsTexture);
            SAMPLER(sampler_CameraColorTexture);

            float4 _OutlineColor;
            float _OutlineThickness;
            float _DepthSensitivity;
            float _NormalsSensitivity;
            float _ColorSensitivity;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            float SampleDepth(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
            }

            float3 SampleNormal(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv).rgb;
            }

            float3 SampleColor(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_CameraColorTexture, sampler_CameraColorTexture, uv).rgb;
            }

            float SobelDepth(float2 uv, float2 texelSize)
            {
                float offset = _OutlineThickness * 0.001;
                
                float d00 = SampleDepth(uv + float2(-offset, -offset) * texelSize);
                float d01 = SampleDepth(uv + float2(0, -offset) * texelSize);
                float d02 = SampleDepth(uv + float2(offset, -offset) * texelSize);
                
                float d10 = SampleDepth(uv + float2(-offset, 0) * texelSize);
                float d12 = SampleDepth(uv + float2(offset, 0) * texelSize);
                
                float d20 = SampleDepth(uv + float2(-offset, offset) * texelSize);
                float d21 = SampleDepth(uv + float2(0, offset) * texelSize);
                float d22 = SampleDepth(uv + float2(offset, offset) * texelSize);
                
                float sobelX = d00 + 2.0 * d10 + d20 - d02 - 2.0 * d12 - d22;
                float sobelY = d00 + 2.0 * d01 + d02 - d20 - 2.0 * d21 - d22;
                
                return sqrt(sobelX * sobelX + sobelY * sobelY);
            }

            float SobelNormal(float2 uv, float2 texelSize)
            {
                float offset = _OutlineThickness * 0.001;
                
                float3 n00 = SampleNormal(uv + float2(-offset, -offset) * texelSize);
                float3 n01 = SampleNormal(uv + float2(0, -offset) * texelSize);
                float3 n02 = SampleNormal(uv + float2(offset, -offset) * texelSize);
                
                float3 n10 = SampleNormal(uv + float2(-offset, 0) * texelSize);
                float3 n12 = SampleNormal(uv + float2(offset, 0) * texelSize);
                
                float3 n20 = SampleNormal(uv + float2(-offset, offset) * texelSize);
                float3 n21 = SampleNormal(uv + float2(0, offset) * texelSize);
                float3 n22 = SampleNormal(uv + float2(offset, offset) * texelSize);
                
                float3 sobelX = n00 + 2.0 * n10 + n20 - n02 - 2.0 * n12 - n22;
                float3 sobelY = n00 + 2.0 * n01 + n02 - n20 - 2.0 * n21 - n22;
                
                return length(sobelX) + length(sobelY);
            }

            float SobelColor(float2 uv, float2 texelSize)
            {
                float offset = _OutlineThickness * 0.001;
                
                float3 c00 = SampleColor(uv + float2(-offset, -offset) * texelSize);
                float3 c01 = SampleColor(uv + float2(0, -offset) * texelSize);
                float3 c02 = SampleColor(uv + float2(offset, -offset) * texelSize);
                
                float3 c10 = SampleColor(uv + float2(-offset, 0) * texelSize);
                float3 c12 = SampleColor(uv + float2(offset, 0) * texelSize);
                
                float3 c20 = SampleColor(uv + float2(-offset, offset) * texelSize);
                float3 c21 = SampleColor(uv + float2(0, offset) * texelSize);
                float3 c22 = SampleColor(uv + float2(offset, offset) * texelSize);
                
                float3 sobelX = c00 + 2.0 * c10 + c20 - c02 - 2.0 * c12 - c22;
                float3 sobelY = c00 + 2.0 * c01 + c02 - c20 - 2.0 * c21 - c22;
                
                return length(sobelX) + length(sobelY);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float2 texelSize = _ScreenSize.zw;
                float2 uv = input.texcoord;
                
                float depthEdge = SobelDepth(uv, texelSize) * _DepthSensitivity * 100.0;
                float normalEdge = SobelNormal(uv, texelSize) * _NormalsSensitivity * 10.0;
                float colorEdge = SobelColor(uv, texelSize) * _ColorSensitivity * 10.0;
                
                float edge = saturate(depthEdge + normalEdge + colorEdge);
                
                float4 outlineColorWithAlpha = float4(_OutlineColor.rgb, edge * _OutlineColor.a);
                
                return outlineColorWithAlpha;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
