Shader "DCL/LW Toon Shader"
{
    Properties
    {
        [NoScaleOffset] _MatCap("MatCap", 2D) = "white" {}
        _GlossIntensity("GlossIntensity", Color) = (0.4811321, 0.4811321, 0.4811321, 1)
        _MultiplyColor("MultiplyColor", Color) = (0.3679245, 0.3679245, 0.3679245, 1)
        _AddColor("AddColor", Color) = (0.2358491, 0.2358491, 0.2358491, 1)
        [HDR]_FresnelIntensity("FresnelIntensity", Color) = (0.8705506, 0.8705506, 0.8705506, 1)
        [NoScaleOffset] _AvatarMap1("AvatarMap1", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap2("AvatarMap2", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap3("AvatarMap3", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap4("AvatarMap4", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap5("AvatarMap5", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap6("AvatarMap6", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap7("AvatarMap7", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap8("AvatarMap8", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap9("AvatarMap9", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap10("AvatarMap10", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap11("AvatarMap11", 2D) = "white" {}
        [NoScaleOffset] _AvatarMap12("AvatarMap12", 2D) = "white" {}
        _DitherFade("_DitherFade", Float) = 1
        _RevealPosition("RevealPosition", Vector) = (0, 100, 0, 0)
        _RevealNormal("RevealNormal", Vector) = (0, 1, 0, 0)
        [HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1
        [HideInInspector] [NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector] [NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector] [NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

        _ShadeColor ("Shade Color", Color) = (0.70, 0.70, 0.70, 1)
        _TintColor ("Tint Color", Color) = (0.70, 0.70, 0.70, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }
        Pass
        {
            LOD 100

            CGPROGRAM
            #include "LWToonShaderUtils.cginc"

            //#include "PBRForwardPass.hlsl"
            //#include "DepthOnlyPass.hlsl"
            //#include "DepthNormalsOnlyPass.hlsl"
            //#include "ShadowCasterPass.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            #define _ALPHATEST_ON 1

            CBUFFER_START(UnityPerMaterial)
            float4x4 _WorldInverse;
            float4x4 _Matrices[100];
            float4x4 _BindPoses[100];
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            float _DitherFade;
            float3 _RevealPosition;
            float3 _RevealNormal;
            float4 _ShadeColor;
            float4 _TintColor;
            CBUFFER_END

            sampler2D _MatCap;
            sampler2D _AvatarMap1;
            sampler2D _AvatarMap2;
            sampler2D _AvatarMap3;
            sampler2D _AvatarMap4;
            sampler2D _AvatarMap5;
            sampler2D _AvatarMap6;
            sampler2D _AvatarMap7;
            sampler2D _AvatarMap8;
            sampler2D _AvatarMap9;
            sampler2D _AvatarMap10;
            sampler2D _AvatarMap11;
            sampler2D _AvatarMap12;
            float4 _MatCap_ST;
            float4 _AvatarMap1_ST;
            float4 _AvatarMap2_ST;
            float4 _AvatarMap3_ST;
            float4 _AvatarMap4_ST;
            float4 _AvatarMap5_ST;
            float4 _AvatarMap6_ST;
            float4 _AvatarMap7_ST;
            float4 _AvatarMap8_ST;
            float4 _AvatarMap9_ST;
            float4 _AvatarMap10_ST;
            float4 _AvatarMap11_ST;
            float4 _AvatarMap12_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float4 uv0 : TEXCOORD3;
                float4 uv1 : TEXCOORD4;
                float4 uv2 : TEXCOORD5;
                float4 uv3 : TEXCOORD6;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = mul(unity_ObjectToWorld, v.normal).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - v.vertex);
                o.uv0 = v.uv0;
                o.uv1 = v.uv1;
                o.uv2 = v.uv2;
                o.uv3 = v.uv3;
                o.color = v.color;
                return o;
            }

            // TODO: Replace this with an atlas of 4x4 textures, with some smart math we can have just 1 sampler
            float4 SampleTextureByIndex(float4 defaultColor, float textureIndex, float2 uv)
            {
                if (textureIndex < 0)
                {
                    return defaultColor;
                }

                textureIndex = abs(textureIndex);

                if (textureIndex < 0.01)
                    return tex2D(_AvatarMap1, uv);
                if (abs(1 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap2, uv);
                if (abs(2 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap3, uv);
                if (abs(3 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap4, uv);
                if (abs(4 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap5, uv);
                if (abs(5 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap6, uv);
                if (abs(6 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap7, uv);
                if (abs(7 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap8, uv);
                if (abs(8 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap9, uv);
                if (abs(9 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap10, uv);
                if (abs(10 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap11, uv);
                if (abs(11 - textureIndex) < 0.01)
                    return tex2D(_AvatarMap12, uv);

                return defaultColor;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                const fixed4 whiteColor = fixed4(1, 1, 1, 1);

                // hardcoded values came from the properties of shader graph version of this shader that weren't exposed
                fixed4 toonBlend = GetToonBlend(i.worldNormal, i.viewDir, _MatCap, _FresnelIntensity, _GlossIntensity,
                                                float3(-0.1, 0.8, -0.4),
                                                whiteColor);

                fixed4 finalToonBlend = Blend(toonBlend, _MultiplyColor, 1) + _AddColor;

                // Encoded Atributes in UVs

                float albedoIndex = i.uv2.r;
                float emissionIndex = i.uv2.g;
                fixed4 albedoMap = SampleTextureByIndex(whiteColor, albedoIndex, i.uv0);
                fixed4 emissionMap = SampleTextureByIndex(whiteColor, emissionIndex, i.uv0);
                fixed4 albedoBase = fixed4(ColorspaceConversion_RGB_Linear(i.color.rgb), i.color.a);
                fixed4 emissionBase = fixed4(ColorspaceConversion_RGB_Linear(i.uv3.rgb), 1);
                float alphaThreshold = i.uv2.b;

                fixed albedoAlpha = albedoBase.a * albedoMap.a;
                const fixed4 albedoColor = albedoBase * albedoMap;
                fixed4 emission = emissionBase + emissionMap;
                fixed4 finalAlbedoColor = Blend(albedoColor, whiteColor + finalToonBlend, 1);

                // Output: albedoAlpha, combined, alphaTrheshold, emission
                float4 screenPos = i.vertex.xyww * _ProjectionParams.zwzz + _ProjectionParams.xyxy;
                float3 viewDir = _WorldSpaceCameraPos.xyz - i.vertex;
                float alphaClipThreshold = DitherAlpha(_DitherFade, alphaThreshold, _RevealPosition, _RevealNormal,
                                                       screenPos, viewDir);
                // alpha = albedoAlpha
                // albedoColor = finalAlbedoColor
                // alphaClipThreshold = alphaClipThreshold
                // emission = emission

                clip(albedoAlpha - alphaClipThreshold);
                return finalAlbedoColor + emission;
            }
            ENDCG
        }

        /*Pass
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

            #include "GPUSkinning.hlsl"
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

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
                float3 gpuSkinnedPositionOS = float3(0, 0, 0);
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
        }*/
    }
}