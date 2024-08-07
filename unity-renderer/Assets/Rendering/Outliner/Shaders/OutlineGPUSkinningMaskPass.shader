Shader "Hidden/DCL/OutlineGPUSkinningMaskPass"
{
    Properties
    {
        [NoScaleOffset]_AvatarMap1("AvatarMap1", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap2("AvatarMap2", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap3("AvatarMap3", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap4("AvatarMap4", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap5("AvatarMap5", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap6("AvatarMap6", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap7("AvatarMap7", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap8("AvatarMap8", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap9("AvatarMap9", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap10("AvatarMap10", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap11("AvatarMap11", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap12("AvatarMap12", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
            Name "Outliner"
            Tags
            {
                "LightMode" = "Outliner"
                "RenderPipeline" = "UniversalPipeline"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4x4 _WorldInverse;
            float4x4 _Matrices[100];
            float4x4 _BindPoses[100];
            float4 _AvatarMap1_TexelSize;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            
            float4 _AvatarMap2_TexelSize;
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            
            float4 _AvatarMap3_TexelSize;
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            
            float4 _AvatarMap4_TexelSize;
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            
            float4 _AvatarMap5_TexelSize;
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            
            float4 _AvatarMap6_TexelSize;
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            
            float4 _AvatarMap7_TexelSize;
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            
            float4 _AvatarMap8_TexelSize;
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            
            float4 _AvatarMap9_TexelSize;
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            
            float4 _AvatarMap10_TexelSize;
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            
            float4 _AvatarMap11_TexelSize;
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            
            float4 _AvatarMap12_TexelSize;
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);
            
            #include "./../../Shaders/Toon/Compiled/GPUSkinning.hlsl"
            #include "./../../Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 uv0: TEXCOORD0;
                float4 uv1: TEXCOORD1;
                float4 uv2: TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 gpuSkinnedPositionOS = float3(0,0,0);
                ApplyGPUSkinning(input.positionOS, gpuSkinnedPositionOS, input.tangentOS, input.uv1);
                input.positionOS = gpuSkinnedPositionOS;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv0 = input.uv0;
                output.uv1 = input.uv1;
                output.uv2 = input.uv2;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float4 out_AlbedoColor;
                SampleTexture_float(float4(1, 1, 1, 1), input.uv2.r, input.uv0, out_AlbedoColor);
                const float alphaThreshold = input.uv2.b;
                clip(out_AlbedoColor.a - alphaThreshold - 0.05f);
                return half4(1, 1, 1, 1);
            }
            ENDHLSL
        }
    }
}