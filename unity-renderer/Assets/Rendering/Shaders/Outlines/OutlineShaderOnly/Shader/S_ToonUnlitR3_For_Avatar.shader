Shader "S_ToonUnlitR3_For_Avatar_Outliner"
{
    Properties
    {
        _CellShading("CellShading", Range(0, 255)) = 251
        Vector1_533C9C05("ToonFade", Range(0, 1)) = 0.4
        _Outline("Outline (Pixel)", Int) = 1
        Color_5DF75355("Outline Color", Color) = (0, 0, 0, 0)
        _OutlineFade("Outline Fade", Range(0, 1)) = 0.1
        [NoScaleOffset]Texture2D_E0E90B4F("Effect", 2D) = "white" {}
        [ToggleUI]_isOutlineDisabled("isOutlineEnabled", Float) = 0
        Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49("OutlineOpacity", Range(0, 1)) = 0
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
        }
        Pass
        {
            Name "Pass"
            Tags
            {
                // LightMode: <None>
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma shader_feature _ _SAMPLE_GI
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_UNLIT
        #define REQUIRE_DEPTH_TEXTURE
        #define REQUIRE_OPAQUE_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 ObjectSpacePosition;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float4 uv1;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.color = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial) float4x4 _WorldInverse; float4x4 _Matrices[100]; float4x4 _BindPoses[100];
        float _CellShading;
        float Vector1_533C9C05;
        float _Outline;
        float4 Color_5DF75355;
        float _OutlineFade;
        float4 Texture2D_E0E90B4F_TexelSize;
        float _isOutlineDisabled;
        float Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
        CBUFFER_END

        // Object and Global properties
        TEXTURE2D(Texture2D_E0E90B4F);
        SAMPLER(samplerTexture2D_E0E90B4F);

            // Graph Functions
            
        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_Round_float3(float3 In, out float3 Out)
        {
            Out = round(In);
        }

        void Unity_Divide_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A / B;
        }

        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_SceneDepth_Linear01_float(float4 UV, out float Out)
        {
            Out = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }

        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }

        void Unity_Comparison_GreaterOrEqual_float(float A, float B, out float Out)
        {
            Out = A >= B ? 1 : 0;
        }

        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Minimum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = min(A, B);
        };

        void Unity_Multiply_float(float4x4 A, float4 B, out float4 Out)
        {
            Out = mul(A, B);
        }

        void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A - B;
        }

        void Unity_OneMinus_float3(float3 In, out float3 Out)
        {
            Out = 1 - In;
        }

        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float4 _UV_b8a348e3911942a998d5a3fa0c678007_Out_0 = IN.uv1;
            float4 _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2;
            Unity_Multiply_float(_UV_b8a348e3911942a998d5a3fa0c678007_Out_0, float4(0, 0, 0, 0), _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2);
            float3 _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2.xyz), _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2);
            description.Position = _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_fc1b3fec95bd4feda1957567138194b6_Out_0 = _isOutlineDisabled;
            float3 _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1;
            Unity_SceneColor_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1);
            float _Property_17f88eedb22aa082b26367443907b457_Out_0 = _CellShading;
            float _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2;
            Unity_Subtract_float(256, _Property_17f88eedb22aa082b26367443907b457_Out_0, _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2);
            float3 _Multiply_7da903b43163338186b43865b8741fbc_Out_2;
            Unity_Multiply_float(_SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Multiply_7da903b43163338186b43865b8741fbc_Out_2);
            float3 _Round_5423730bc0c6c8818922975c01b91056_Out_1;
            Unity_Round_float3(_Multiply_7da903b43163338186b43865b8741fbc_Out_2, _Round_5423730bc0c6c8818922975c01b91056_Out_1);
            float3 _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2;
            Unity_Divide_float3(_Round_5423730bc0c6c8818922975c01b91056_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2);
            float _Property_11a06759662e5e88ac3f86982ceb6894_Out_0 = Vector1_533C9C05;
            float3 _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3;
            Unity_Lerp_float3(_Divide_29ba04366d96cb8da628d49d448e43cb_Out_2, _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Property_11a06759662e5e88ac3f86982ceb6894_Out_0.xxx), _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3);
            float4 _Property_2aa665ef010e3b81be432eeb62e801d2_Out_0 = Color_5DF75355;
            float4 _ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1;
            Unity_Absolute_float4(_ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0, _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1);
            float4 _Add_5147db6674014cc299d1bde82971b8e1_Out_2;
            Unity_Add_float4(_Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1, float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2);
            float Slider_a6daac9d997e408c90a2ebfebd5ac2c8 = 1;
            float4 _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2, (Slider_a6daac9d997e408c90a2ebfebd5ac2c8.xxxx), _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3);
            float _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1);
            float _Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0 = _Outline;
            float _Divide_353d1eaf295af18aa690543fa6687d92_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.x, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2);
            float _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.y, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2);
            float4 _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0, _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2);
            float _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_27a9d4e21cd1ea80a5a9103191908694_Out_2, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1);
            float _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1, _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2);
            float _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1;
            Unity_Absolute_float(_Subtract_030ff9c570cf228080e59107aae83a7b_Out_2, _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1);
            float _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2;
            Unity_Subtract_float(0, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2);
            float _Subtract_b2532dbe871964878032f187cbebd765_Out_2;
            Unity_Subtract_float(0, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2);
            float4 _Vector4_1593a891d741d686900e4556b2149ee4_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_227726cfbe798889876fd156a0242ccd_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_1593a891d741d686900e4556b2149ee4_Out_0, _Add_227726cfbe798889876fd156a0242ccd_Out_2);
            float _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_227726cfbe798889876fd156a0242ccd_Out_2, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1);
            float _Subtract_050e8def12697986bad41f8aadf517c2_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1, _Subtract_050e8def12697986bad41f8aadf517c2_Out_2);
            float _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1;
            Unity_Absolute_float(_Subtract_050e8def12697986bad41f8aadf517c2_Out_2, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1);
            float _Add_e4821fbf8a5e628085df8210f262057d_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2);
            float _Add_97a0d08feac04573835bbc78ddb7485a_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2, _Add_97a0d08feac04573835bbc78ddb7485a_Out_2);
            float4 _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0, _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2);
            float _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1);
            float _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1, _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2);
            float _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1;
            Unity_Absolute_float(_Subtract_cdd8828f44dfec87be1a856545e05681_Out_2, _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1);
            float4 _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_6de672402b44fb8d98184769ec42ebec_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0, _Add_6de672402b44fb8d98184769ec42ebec_Out_2);
            float _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6de672402b44fb8d98184769ec42ebec_Out_2, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1);
            float _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1, _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2);
            float _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1;
            Unity_Absolute_float(_Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1);
            float _Add_389d560813dd198aa5e64a302b5942af_Out_2;
            Unity_Add_float(_Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_389d560813dd198aa5e64a302b5942af_Out_2);
            float _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2;
            Unity_Add_float(_Add_389d560813dd198aa5e64a302b5942af_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2);
            float _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2;
            Unity_Add_float(_Add_97a0d08feac04573835bbc78ddb7485a_Out_2, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2, _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2);
            float _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2;
            Unity_Divide_float(_ProjectionParams.z, 10, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2);
            float _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2;
            Unity_Multiply_float(_Add_f8053a2eef9de1898c32d6efe4251f82_Out_2, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2, _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2);
            float _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1);
            float _Property_3d31d0933367c38688b099bb23c954d3_Out_0 = _OutlineFade;
            float _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2;
            Unity_Add_float(_Property_3d31d0933367c38688b099bb23c954d3_Out_0, 1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2);
            float _Power_0b2fb1311c40038f895495bc455d938e_Out_2;
            Unity_Power_float(_SceneDepth_45cda59e81fd96839f89657068c67112_Out_1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2);
            float _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2;
            Unity_Divide_float(_Multiply_18108578fe7e608a8acc84aab2986c47_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2, _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2);
            float _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2, 0.01, _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2);
            float _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3;
            Unity_Branch_float(_Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2, 1, 0, _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3);
            float3 _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3;
            Unity_Lerp_float3(_Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3, (_Property_2aa665ef010e3b81be432eeb62e801d2_Out_0.xyz), (_Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3.xxx), _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3);
            float3 _Branch_dfbceb6fdf8f4c4c887de76b5ecce53e_Out_3;
            Unity_Branch_float3(_Property_fc1b3fec95bd4feda1957567138194b6_Out_0, _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3, float3(0, 0, 0), _Branch_dfbceb6fdf8f4c4c887de76b5ecce53e_Out_3);
            float _Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0 = _isOutlineDisabled;
            float3 _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2;
            Unity_Minimum_float3((IN.VertexColor.xyz), IN.ObjectSpacePosition, _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2);
            float _Split_196288f9704a43fcb98e6f6036932753_R_1 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[0];
            float _Split_196288f9704a43fcb98e6f6036932753_G_2 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[1];
            float _Split_196288f9704a43fcb98e6f6036932753_B_3 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[2];
            float _Split_196288f9704a43fcb98e6f6036932753_A_4 = 0;
            float4 _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0 = float4(_Split_196288f9704a43fcb98e6f6036932753_R_1, _Split_196288f9704a43fcb98e6f6036932753_G_2, _Split_196288f9704a43fcb98e6f6036932753_B_3, _Split_196288f9704a43fcb98e6f6036932753_A_4);
            float4 _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2;
            Unity_Multiply_float(UNITY_MATRIX_P, _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0, _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2);
            float3 _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2;
            Unity_Subtract_float3(_Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3, (_Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2.xyz), _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2);
            float3 _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2;
            Unity_Divide_float3(_Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2, float3(4, 4, 4), _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2);
            float3 _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1;
            Unity_OneMinus_float3(_Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2, _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1);
            float4 Color_3829a5c39aa649529f84951651c2303c = IsGammaSpace() ? float4(0, 0, 0, 0) : float4(SRGBToLinear(float3(0, 0, 0)), 0);
            float _Property_b897d223351b410e886999af2c214c36_Out_0 = Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
            float _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3;
            Unity_Remap_float(_Property_b897d223351b410e886999af2c214c36_Out_0, float2 (0, 1), float2 (1, 0), _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3);
            float3 _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3;
            Unity_Lerp_float3(_OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1, (Color_3829a5c39aa649529f84951651c2303c.xyz), (_Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3.xxx), _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3);
            float3 _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3;
            Unity_Branch_float3(_Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0, _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3, float3(0, 0, 0), _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3);
            surface.BaseColor = _Branch_dfbceb6fdf8f4c4c887de76b5ecce53e_Out_3;
            surface.Alpha = (_Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3).x;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.uv1 =                         input.uv1;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ObjectSpacePosition =         TransformWorldToObject(input.positionWS);
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.VertexColor =                 input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define REQUIRE_DEPTH_TEXTURE
        #define REQUIRE_OPAQUE_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 ObjectSpacePosition;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float4 uv1;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.color = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial) float4x4 _WorldInverse; float4x4 _Matrices[100]; float4x4 _BindPoses[100];
        float _CellShading;
        float Vector1_533C9C05;
        float _Outline;
        float4 Color_5DF75355;
        float _OutlineFade;
        float4 Texture2D_E0E90B4F_TexelSize;
        float _isOutlineDisabled;
        float Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
        CBUFFER_END

        // Object and Global properties
        TEXTURE2D(Texture2D_E0E90B4F);
        SAMPLER(samplerTexture2D_E0E90B4F);

            // Graph Functions
            
        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_Round_float3(float3 In, out float3 Out)
        {
            Out = round(In);
        }

        void Unity_Divide_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A / B;
        }

        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_SceneDepth_Linear01_float(float4 UV, out float Out)
        {
            Out = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }

        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }

        void Unity_Comparison_GreaterOrEqual_float(float A, float B, out float Out)
        {
            Out = A >= B ? 1 : 0;
        }

        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Minimum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = min(A, B);
        };

        void Unity_Multiply_float(float4x4 A, float4 B, out float4 Out)
        {
            Out = mul(A, B);
        }

        void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A - B;
        }

        void Unity_OneMinus_float3(float3 In, out float3 Out)
        {
            Out = 1 - In;
        }

        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float4 _UV_b8a348e3911942a998d5a3fa0c678007_Out_0 = IN.uv1;
            float4 _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2;
            Unity_Multiply_float(_UV_b8a348e3911942a998d5a3fa0c678007_Out_0, float4(0, 0, 0, 0), _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2);
            float3 _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2.xyz), _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2);
            description.Position = _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0 = _isOutlineDisabled;
            float3 _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1;
            Unity_SceneColor_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1);
            float _Property_17f88eedb22aa082b26367443907b457_Out_0 = _CellShading;
            float _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2;
            Unity_Subtract_float(256, _Property_17f88eedb22aa082b26367443907b457_Out_0, _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2);
            float3 _Multiply_7da903b43163338186b43865b8741fbc_Out_2;
            Unity_Multiply_float(_SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Multiply_7da903b43163338186b43865b8741fbc_Out_2);
            float3 _Round_5423730bc0c6c8818922975c01b91056_Out_1;
            Unity_Round_float3(_Multiply_7da903b43163338186b43865b8741fbc_Out_2, _Round_5423730bc0c6c8818922975c01b91056_Out_1);
            float3 _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2;
            Unity_Divide_float3(_Round_5423730bc0c6c8818922975c01b91056_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2);
            float _Property_11a06759662e5e88ac3f86982ceb6894_Out_0 = Vector1_533C9C05;
            float3 _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3;
            Unity_Lerp_float3(_Divide_29ba04366d96cb8da628d49d448e43cb_Out_2, _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Property_11a06759662e5e88ac3f86982ceb6894_Out_0.xxx), _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3);
            float4 _Property_2aa665ef010e3b81be432eeb62e801d2_Out_0 = Color_5DF75355;
            float4 _ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1;
            Unity_Absolute_float4(_ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0, _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1);
            float4 _Add_5147db6674014cc299d1bde82971b8e1_Out_2;
            Unity_Add_float4(_Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1, float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2);
            float Slider_a6daac9d997e408c90a2ebfebd5ac2c8 = 1;
            float4 _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2, (Slider_a6daac9d997e408c90a2ebfebd5ac2c8.xxxx), _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3);
            float _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1);
            float _Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0 = _Outline;
            float _Divide_353d1eaf295af18aa690543fa6687d92_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.x, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2);
            float _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.y, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2);
            float4 _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0, _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2);
            float _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_27a9d4e21cd1ea80a5a9103191908694_Out_2, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1);
            float _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1, _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2);
            float _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1;
            Unity_Absolute_float(_Subtract_030ff9c570cf228080e59107aae83a7b_Out_2, _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1);
            float _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2;
            Unity_Subtract_float(0, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2);
            float _Subtract_b2532dbe871964878032f187cbebd765_Out_2;
            Unity_Subtract_float(0, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2);
            float4 _Vector4_1593a891d741d686900e4556b2149ee4_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_227726cfbe798889876fd156a0242ccd_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_1593a891d741d686900e4556b2149ee4_Out_0, _Add_227726cfbe798889876fd156a0242ccd_Out_2);
            float _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_227726cfbe798889876fd156a0242ccd_Out_2, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1);
            float _Subtract_050e8def12697986bad41f8aadf517c2_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1, _Subtract_050e8def12697986bad41f8aadf517c2_Out_2);
            float _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1;
            Unity_Absolute_float(_Subtract_050e8def12697986bad41f8aadf517c2_Out_2, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1);
            float _Add_e4821fbf8a5e628085df8210f262057d_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2);
            float _Add_97a0d08feac04573835bbc78ddb7485a_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2, _Add_97a0d08feac04573835bbc78ddb7485a_Out_2);
            float4 _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0, _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2);
            float _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1);
            float _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1, _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2);
            float _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1;
            Unity_Absolute_float(_Subtract_cdd8828f44dfec87be1a856545e05681_Out_2, _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1);
            float4 _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_6de672402b44fb8d98184769ec42ebec_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0, _Add_6de672402b44fb8d98184769ec42ebec_Out_2);
            float _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6de672402b44fb8d98184769ec42ebec_Out_2, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1);
            float _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1, _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2);
            float _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1;
            Unity_Absolute_float(_Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1);
            float _Add_389d560813dd198aa5e64a302b5942af_Out_2;
            Unity_Add_float(_Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_389d560813dd198aa5e64a302b5942af_Out_2);
            float _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2;
            Unity_Add_float(_Add_389d560813dd198aa5e64a302b5942af_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2);
            float _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2;
            Unity_Add_float(_Add_97a0d08feac04573835bbc78ddb7485a_Out_2, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2, _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2);
            float _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2;
            Unity_Divide_float(_ProjectionParams.z, 10, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2);
            float _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2;
            Unity_Multiply_float(_Add_f8053a2eef9de1898c32d6efe4251f82_Out_2, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2, _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2);
            float _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1);
            float _Property_3d31d0933367c38688b099bb23c954d3_Out_0 = _OutlineFade;
            float _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2;
            Unity_Add_float(_Property_3d31d0933367c38688b099bb23c954d3_Out_0, 1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2);
            float _Power_0b2fb1311c40038f895495bc455d938e_Out_2;
            Unity_Power_float(_SceneDepth_45cda59e81fd96839f89657068c67112_Out_1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2);
            float _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2;
            Unity_Divide_float(_Multiply_18108578fe7e608a8acc84aab2986c47_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2, _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2);
            float _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2, 0.01, _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2);
            float _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3;
            Unity_Branch_float(_Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2, 1, 0, _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3);
            float3 _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3;
            Unity_Lerp_float3(_Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3, (_Property_2aa665ef010e3b81be432eeb62e801d2_Out_0.xyz), (_Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3.xxx), _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3);
            float3 _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2;
            Unity_Minimum_float3((IN.VertexColor.xyz), IN.ObjectSpacePosition, _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2);
            float _Split_196288f9704a43fcb98e6f6036932753_R_1 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[0];
            float _Split_196288f9704a43fcb98e6f6036932753_G_2 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[1];
            float _Split_196288f9704a43fcb98e6f6036932753_B_3 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[2];
            float _Split_196288f9704a43fcb98e6f6036932753_A_4 = 0;
            float4 _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0 = float4(_Split_196288f9704a43fcb98e6f6036932753_R_1, _Split_196288f9704a43fcb98e6f6036932753_G_2, _Split_196288f9704a43fcb98e6f6036932753_B_3, _Split_196288f9704a43fcb98e6f6036932753_A_4);
            float4 _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2;
            Unity_Multiply_float(UNITY_MATRIX_P, _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0, _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2);
            float3 _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2;
            Unity_Subtract_float3(_Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3, (_Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2.xyz), _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2);
            float3 _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2;
            Unity_Divide_float3(_Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2, float3(4, 4, 4), _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2);
            float3 _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1;
            Unity_OneMinus_float3(_Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2, _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1);
            float4 Color_3829a5c39aa649529f84951651c2303c = IsGammaSpace() ? float4(0, 0, 0, 0) : float4(SRGBToLinear(float3(0, 0, 0)), 0);
            float _Property_b897d223351b410e886999af2c214c36_Out_0 = Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
            float _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3;
            Unity_Remap_float(_Property_b897d223351b410e886999af2c214c36_Out_0, float2 (0, 1), float2 (1, 0), _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3);
            float3 _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3;
            Unity_Lerp_float3(_OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1, (Color_3829a5c39aa649529f84951651c2303c.xyz), (_Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3.xxx), _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3);
            float3 _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3;
            Unity_Branch_float3(_Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0, _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3, float3(0, 0, 0), _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3);
            surface.Alpha = (_Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3).x;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.uv1 =                         input.uv1;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ObjectSpacePosition =         TransformWorldToObject(input.positionWS);
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.VertexColor =                 input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "ShadowCasterPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
        #define REQUIRE_DEPTH_TEXTURE
        #define REQUIRE_OPAQUE_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 ObjectSpacePosition;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float4 uv1;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.color = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial) float4x4 _WorldInverse; float4x4 _Matrices[100]; float4x4 _BindPoses[100];
        float _CellShading;
        float Vector1_533C9C05;
        float _Outline;
        float4 Color_5DF75355;
        float _OutlineFade;
        float4 Texture2D_E0E90B4F_TexelSize;
        float _isOutlineDisabled;
        float Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
        CBUFFER_END

        // Object and Global properties
        TEXTURE2D(Texture2D_E0E90B4F);
        SAMPLER(samplerTexture2D_E0E90B4F);

            // Graph Functions
            
        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_Round_float3(float3 In, out float3 Out)
        {
            Out = round(In);
        }

        void Unity_Divide_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A / B;
        }

        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_SceneDepth_Linear01_float(float4 UV, out float Out)
        {
            Out = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }

        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }

        void Unity_Comparison_GreaterOrEqual_float(float A, float B, out float Out)
        {
            Out = A >= B ? 1 : 0;
        }

        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Minimum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = min(A, B);
        };

        void Unity_Multiply_float(float4x4 A, float4 B, out float4 Out)
        {
            Out = mul(A, B);
        }

        void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A - B;
        }

        void Unity_OneMinus_float3(float3 In, out float3 Out)
        {
            Out = 1 - In;
        }

        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float4 _UV_b8a348e3911942a998d5a3fa0c678007_Out_0 = IN.uv1;
            float4 _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2;
            Unity_Multiply_float(_UV_b8a348e3911942a998d5a3fa0c678007_Out_0, float4(0, 0, 0, 0), _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2);
            float3 _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2.xyz), _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2);
            description.Position = _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0 = _isOutlineDisabled;
            float3 _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1;
            Unity_SceneColor_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1);
            float _Property_17f88eedb22aa082b26367443907b457_Out_0 = _CellShading;
            float _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2;
            Unity_Subtract_float(256, _Property_17f88eedb22aa082b26367443907b457_Out_0, _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2);
            float3 _Multiply_7da903b43163338186b43865b8741fbc_Out_2;
            Unity_Multiply_float(_SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Multiply_7da903b43163338186b43865b8741fbc_Out_2);
            float3 _Round_5423730bc0c6c8818922975c01b91056_Out_1;
            Unity_Round_float3(_Multiply_7da903b43163338186b43865b8741fbc_Out_2, _Round_5423730bc0c6c8818922975c01b91056_Out_1);
            float3 _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2;
            Unity_Divide_float3(_Round_5423730bc0c6c8818922975c01b91056_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2);
            float _Property_11a06759662e5e88ac3f86982ceb6894_Out_0 = Vector1_533C9C05;
            float3 _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3;
            Unity_Lerp_float3(_Divide_29ba04366d96cb8da628d49d448e43cb_Out_2, _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Property_11a06759662e5e88ac3f86982ceb6894_Out_0.xxx), _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3);
            float4 _Property_2aa665ef010e3b81be432eeb62e801d2_Out_0 = Color_5DF75355;
            float4 _ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1;
            Unity_Absolute_float4(_ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0, _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1);
            float4 _Add_5147db6674014cc299d1bde82971b8e1_Out_2;
            Unity_Add_float4(_Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1, float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2);
            float Slider_a6daac9d997e408c90a2ebfebd5ac2c8 = 1;
            float4 _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2, (Slider_a6daac9d997e408c90a2ebfebd5ac2c8.xxxx), _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3);
            float _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1);
            float _Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0 = _Outline;
            float _Divide_353d1eaf295af18aa690543fa6687d92_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.x, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2);
            float _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.y, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2);
            float4 _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0, _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2);
            float _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_27a9d4e21cd1ea80a5a9103191908694_Out_2, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1);
            float _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1, _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2);
            float _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1;
            Unity_Absolute_float(_Subtract_030ff9c570cf228080e59107aae83a7b_Out_2, _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1);
            float _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2;
            Unity_Subtract_float(0, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2);
            float _Subtract_b2532dbe871964878032f187cbebd765_Out_2;
            Unity_Subtract_float(0, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2);
            float4 _Vector4_1593a891d741d686900e4556b2149ee4_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_227726cfbe798889876fd156a0242ccd_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_1593a891d741d686900e4556b2149ee4_Out_0, _Add_227726cfbe798889876fd156a0242ccd_Out_2);
            float _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_227726cfbe798889876fd156a0242ccd_Out_2, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1);
            float _Subtract_050e8def12697986bad41f8aadf517c2_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1, _Subtract_050e8def12697986bad41f8aadf517c2_Out_2);
            float _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1;
            Unity_Absolute_float(_Subtract_050e8def12697986bad41f8aadf517c2_Out_2, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1);
            float _Add_e4821fbf8a5e628085df8210f262057d_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2);
            float _Add_97a0d08feac04573835bbc78ddb7485a_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2, _Add_97a0d08feac04573835bbc78ddb7485a_Out_2);
            float4 _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0, _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2);
            float _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1);
            float _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1, _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2);
            float _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1;
            Unity_Absolute_float(_Subtract_cdd8828f44dfec87be1a856545e05681_Out_2, _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1);
            float4 _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_6de672402b44fb8d98184769ec42ebec_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0, _Add_6de672402b44fb8d98184769ec42ebec_Out_2);
            float _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6de672402b44fb8d98184769ec42ebec_Out_2, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1);
            float _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1, _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2);
            float _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1;
            Unity_Absolute_float(_Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1);
            float _Add_389d560813dd198aa5e64a302b5942af_Out_2;
            Unity_Add_float(_Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_389d560813dd198aa5e64a302b5942af_Out_2);
            float _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2;
            Unity_Add_float(_Add_389d560813dd198aa5e64a302b5942af_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2);
            float _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2;
            Unity_Add_float(_Add_97a0d08feac04573835bbc78ddb7485a_Out_2, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2, _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2);
            float _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2;
            Unity_Divide_float(_ProjectionParams.z, 10, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2);
            float _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2;
            Unity_Multiply_float(_Add_f8053a2eef9de1898c32d6efe4251f82_Out_2, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2, _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2);
            float _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1);
            float _Property_3d31d0933367c38688b099bb23c954d3_Out_0 = _OutlineFade;
            float _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2;
            Unity_Add_float(_Property_3d31d0933367c38688b099bb23c954d3_Out_0, 1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2);
            float _Power_0b2fb1311c40038f895495bc455d938e_Out_2;
            Unity_Power_float(_SceneDepth_45cda59e81fd96839f89657068c67112_Out_1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2);
            float _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2;
            Unity_Divide_float(_Multiply_18108578fe7e608a8acc84aab2986c47_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2, _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2);
            float _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2, 0.01, _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2);
            float _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3;
            Unity_Branch_float(_Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2, 1, 0, _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3);
            float3 _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3;
            Unity_Lerp_float3(_Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3, (_Property_2aa665ef010e3b81be432eeb62e801d2_Out_0.xyz), (_Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3.xxx), _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3);
            float3 _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2;
            Unity_Minimum_float3((IN.VertexColor.xyz), IN.ObjectSpacePosition, _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2);
            float _Split_196288f9704a43fcb98e6f6036932753_R_1 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[0];
            float _Split_196288f9704a43fcb98e6f6036932753_G_2 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[1];
            float _Split_196288f9704a43fcb98e6f6036932753_B_3 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[2];
            float _Split_196288f9704a43fcb98e6f6036932753_A_4 = 0;
            float4 _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0 = float4(_Split_196288f9704a43fcb98e6f6036932753_R_1, _Split_196288f9704a43fcb98e6f6036932753_G_2, _Split_196288f9704a43fcb98e6f6036932753_B_3, _Split_196288f9704a43fcb98e6f6036932753_A_4);
            float4 _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2;
            Unity_Multiply_float(UNITY_MATRIX_P, _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0, _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2);
            float3 _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2;
            Unity_Subtract_float3(_Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3, (_Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2.xyz), _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2);
            float3 _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2;
            Unity_Divide_float3(_Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2, float3(4, 4, 4), _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2);
            float3 _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1;
            Unity_OneMinus_float3(_Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2, _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1);
            float4 Color_3829a5c39aa649529f84951651c2303c = IsGammaSpace() ? float4(0, 0, 0, 0) : float4(SRGBToLinear(float3(0, 0, 0)), 0);
            float _Property_b897d223351b410e886999af2c214c36_Out_0 = Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
            float _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3;
            Unity_Remap_float(_Property_b897d223351b410e886999af2c214c36_Out_0, float2 (0, 1), float2 (1, 0), _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3);
            float3 _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3;
            Unity_Lerp_float3(_OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1, (Color_3829a5c39aa649529f84951651c2303c.xyz), (_Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3.xxx), _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3);
            float3 _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3;
            Unity_Branch_float3(_Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0, _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3, float3(0, 0, 0), _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3);
            surface.Alpha = (_Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3).x;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.uv1 =                         input.uv1;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ObjectSpacePosition =         TransformWorldToObject(input.positionWS);
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.VertexColor =                 input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
       #include "DepthOnlyPass.hlsl"

            ENDHLSL
        }
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
        }
        Pass
        {
            Name "Pass"
            Tags
            {
                // LightMode: <None>
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma shader_feature _ _SAMPLE_GI
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_UNLIT
        #define REQUIRE_DEPTH_TEXTURE
        #define REQUIRE_OPAQUE_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 ObjectSpacePosition;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float4 uv1;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.color = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial) float4x4 _WorldInverse; float4x4 _Matrices[100]; float4x4 _BindPoses[100];
        float _CellShading;
        float Vector1_533C9C05;
        float _Outline;
        float4 Color_5DF75355;
        float _OutlineFade;
        float4 Texture2D_E0E90B4F_TexelSize;
        float _isOutlineDisabled;
        float Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
        CBUFFER_END

        // Object and Global properties
        TEXTURE2D(Texture2D_E0E90B4F);
        SAMPLER(samplerTexture2D_E0E90B4F);

            // Graph Functions
            
        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_Round_float3(float3 In, out float3 Out)
        {
            Out = round(In);
        }

        void Unity_Divide_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A / B;
        }

        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_SceneDepth_Linear01_float(float4 UV, out float Out)
        {
            Out = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }

        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }

        void Unity_Comparison_GreaterOrEqual_float(float A, float B, out float Out)
        {
            Out = A >= B ? 1 : 0;
        }

        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Minimum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = min(A, B);
        };

        void Unity_Multiply_float(float4x4 A, float4 B, out float4 Out)
        {
            Out = mul(A, B);
        }

        void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A - B;
        }

        void Unity_OneMinus_float3(float3 In, out float3 Out)
        {
            Out = 1 - In;
        }

        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float4 _UV_b8a348e3911942a998d5a3fa0c678007_Out_0 = IN.uv1;
            float4 _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2;
            Unity_Multiply_float(_UV_b8a348e3911942a998d5a3fa0c678007_Out_0, float4(0, 0, 0, 0), _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2);
            float3 _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2.xyz), _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2);
            description.Position = _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_fc1b3fec95bd4feda1957567138194b6_Out_0 = _isOutlineDisabled;
            float3 _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1;
            Unity_SceneColor_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1);
            float _Property_17f88eedb22aa082b26367443907b457_Out_0 = _CellShading;
            float _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2;
            Unity_Subtract_float(256, _Property_17f88eedb22aa082b26367443907b457_Out_0, _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2);
            float3 _Multiply_7da903b43163338186b43865b8741fbc_Out_2;
            Unity_Multiply_float(_SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Multiply_7da903b43163338186b43865b8741fbc_Out_2);
            float3 _Round_5423730bc0c6c8818922975c01b91056_Out_1;
            Unity_Round_float3(_Multiply_7da903b43163338186b43865b8741fbc_Out_2, _Round_5423730bc0c6c8818922975c01b91056_Out_1);
            float3 _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2;
            Unity_Divide_float3(_Round_5423730bc0c6c8818922975c01b91056_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2);
            float _Property_11a06759662e5e88ac3f86982ceb6894_Out_0 = Vector1_533C9C05;
            float3 _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3;
            Unity_Lerp_float3(_Divide_29ba04366d96cb8da628d49d448e43cb_Out_2, _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Property_11a06759662e5e88ac3f86982ceb6894_Out_0.xxx), _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3);
            float4 _Property_2aa665ef010e3b81be432eeb62e801d2_Out_0 = Color_5DF75355;
            float4 _ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1;
            Unity_Absolute_float4(_ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0, _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1);
            float4 _Add_5147db6674014cc299d1bde82971b8e1_Out_2;
            Unity_Add_float4(_Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1, float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2);
            float Slider_a6daac9d997e408c90a2ebfebd5ac2c8 = 1;
            float4 _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2, (Slider_a6daac9d997e408c90a2ebfebd5ac2c8.xxxx), _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3);
            float _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1);
            float _Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0 = _Outline;
            float _Divide_353d1eaf295af18aa690543fa6687d92_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.x, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2);
            float _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.y, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2);
            float4 _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0, _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2);
            float _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_27a9d4e21cd1ea80a5a9103191908694_Out_2, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1);
            float _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1, _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2);
            float _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1;
            Unity_Absolute_float(_Subtract_030ff9c570cf228080e59107aae83a7b_Out_2, _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1);
            float _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2;
            Unity_Subtract_float(0, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2);
            float _Subtract_b2532dbe871964878032f187cbebd765_Out_2;
            Unity_Subtract_float(0, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2);
            float4 _Vector4_1593a891d741d686900e4556b2149ee4_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_227726cfbe798889876fd156a0242ccd_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_1593a891d741d686900e4556b2149ee4_Out_0, _Add_227726cfbe798889876fd156a0242ccd_Out_2);
            float _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_227726cfbe798889876fd156a0242ccd_Out_2, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1);
            float _Subtract_050e8def12697986bad41f8aadf517c2_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1, _Subtract_050e8def12697986bad41f8aadf517c2_Out_2);
            float _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1;
            Unity_Absolute_float(_Subtract_050e8def12697986bad41f8aadf517c2_Out_2, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1);
            float _Add_e4821fbf8a5e628085df8210f262057d_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2);
            float _Add_97a0d08feac04573835bbc78ddb7485a_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2, _Add_97a0d08feac04573835bbc78ddb7485a_Out_2);
            float4 _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0, _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2);
            float _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1);
            float _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1, _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2);
            float _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1;
            Unity_Absolute_float(_Subtract_cdd8828f44dfec87be1a856545e05681_Out_2, _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1);
            float4 _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_6de672402b44fb8d98184769ec42ebec_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0, _Add_6de672402b44fb8d98184769ec42ebec_Out_2);
            float _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6de672402b44fb8d98184769ec42ebec_Out_2, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1);
            float _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1, _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2);
            float _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1;
            Unity_Absolute_float(_Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1);
            float _Add_389d560813dd198aa5e64a302b5942af_Out_2;
            Unity_Add_float(_Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_389d560813dd198aa5e64a302b5942af_Out_2);
            float _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2;
            Unity_Add_float(_Add_389d560813dd198aa5e64a302b5942af_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2);
            float _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2;
            Unity_Add_float(_Add_97a0d08feac04573835bbc78ddb7485a_Out_2, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2, _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2);
            float _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2;
            Unity_Divide_float(_ProjectionParams.z, 10, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2);
            float _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2;
            Unity_Multiply_float(_Add_f8053a2eef9de1898c32d6efe4251f82_Out_2, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2, _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2);
            float _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1);
            float _Property_3d31d0933367c38688b099bb23c954d3_Out_0 = _OutlineFade;
            float _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2;
            Unity_Add_float(_Property_3d31d0933367c38688b099bb23c954d3_Out_0, 1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2);
            float _Power_0b2fb1311c40038f895495bc455d938e_Out_2;
            Unity_Power_float(_SceneDepth_45cda59e81fd96839f89657068c67112_Out_1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2);
            float _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2;
            Unity_Divide_float(_Multiply_18108578fe7e608a8acc84aab2986c47_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2, _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2);
            float _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2, 0.01, _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2);
            float _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3;
            Unity_Branch_float(_Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2, 1, 0, _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3);
            float3 _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3;
            Unity_Lerp_float3(_Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3, (_Property_2aa665ef010e3b81be432eeb62e801d2_Out_0.xyz), (_Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3.xxx), _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3);
            float3 _Branch_dfbceb6fdf8f4c4c887de76b5ecce53e_Out_3;
            Unity_Branch_float3(_Property_fc1b3fec95bd4feda1957567138194b6_Out_0, _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3, float3(0, 0, 0), _Branch_dfbceb6fdf8f4c4c887de76b5ecce53e_Out_3);
            float _Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0 = _isOutlineDisabled;
            float3 _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2;
            Unity_Minimum_float3((IN.VertexColor.xyz), IN.ObjectSpacePosition, _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2);
            float _Split_196288f9704a43fcb98e6f6036932753_R_1 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[0];
            float _Split_196288f9704a43fcb98e6f6036932753_G_2 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[1];
            float _Split_196288f9704a43fcb98e6f6036932753_B_3 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[2];
            float _Split_196288f9704a43fcb98e6f6036932753_A_4 = 0;
            float4 _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0 = float4(_Split_196288f9704a43fcb98e6f6036932753_R_1, _Split_196288f9704a43fcb98e6f6036932753_G_2, _Split_196288f9704a43fcb98e6f6036932753_B_3, _Split_196288f9704a43fcb98e6f6036932753_A_4);
            float4 _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2;
            Unity_Multiply_float(UNITY_MATRIX_P, _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0, _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2);
            float3 _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2;
            Unity_Subtract_float3(_Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3, (_Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2.xyz), _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2);
            float3 _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2;
            Unity_Divide_float3(_Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2, float3(4, 4, 4), _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2);
            float3 _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1;
            Unity_OneMinus_float3(_Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2, _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1);
            float4 Color_3829a5c39aa649529f84951651c2303c = IsGammaSpace() ? float4(0, 0, 0, 0) : float4(SRGBToLinear(float3(0, 0, 0)), 0);
            float _Property_b897d223351b410e886999af2c214c36_Out_0 = Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
            float _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3;
            Unity_Remap_float(_Property_b897d223351b410e886999af2c214c36_Out_0, float2 (0, 1), float2 (1, 0), _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3);
            float3 _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3;
            Unity_Lerp_float3(_OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1, (Color_3829a5c39aa649529f84951651c2303c.xyz), (_Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3.xxx), _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3);
            float3 _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3;
            Unity_Branch_float3(_Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0, _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3, float3(0, 0, 0), _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3);
            surface.BaseColor = _Branch_dfbceb6fdf8f4c4c887de76b5ecce53e_Out_3;
            surface.Alpha = (_Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3).x;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.uv1 =                         input.uv1;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ObjectSpacePosition =         TransformWorldToObject(input.positionWS);
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.VertexColor =                 input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define REQUIRE_DEPTH_TEXTURE
        #define REQUIRE_OPAQUE_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 ObjectSpacePosition;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float4 uv1;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.color = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial) float4x4 _WorldInverse; float4x4 _Matrices[100]; float4x4 _BindPoses[100];
        float _CellShading;
        float Vector1_533C9C05;
        float _Outline;
        float4 Color_5DF75355;
        float _OutlineFade;
        float4 Texture2D_E0E90B4F_TexelSize;
        float _isOutlineDisabled;
        float Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
        CBUFFER_END

        // Object and Global properties
        TEXTURE2D(Texture2D_E0E90B4F);
        SAMPLER(samplerTexture2D_E0E90B4F);

            // Graph Functions
            
        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_Round_float3(float3 In, out float3 Out)
        {
            Out = round(In);
        }

        void Unity_Divide_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A / B;
        }

        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_SceneDepth_Linear01_float(float4 UV, out float Out)
        {
            Out = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }

        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }

        void Unity_Comparison_GreaterOrEqual_float(float A, float B, out float Out)
        {
            Out = A >= B ? 1 : 0;
        }

        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Minimum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = min(A, B);
        };

        void Unity_Multiply_float(float4x4 A, float4 B, out float4 Out)
        {
            Out = mul(A, B);
        }

        void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A - B;
        }

        void Unity_OneMinus_float3(float3 In, out float3 Out)
        {
            Out = 1 - In;
        }

        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float4 _UV_b8a348e3911942a998d5a3fa0c678007_Out_0 = IN.uv1;
            float4 _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2;
            Unity_Multiply_float(_UV_b8a348e3911942a998d5a3fa0c678007_Out_0, float4(0, 0, 0, 0), _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2);
            float3 _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2.xyz), _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2);
            description.Position = _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0 = _isOutlineDisabled;
            float3 _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1;
            Unity_SceneColor_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1);
            float _Property_17f88eedb22aa082b26367443907b457_Out_0 = _CellShading;
            float _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2;
            Unity_Subtract_float(256, _Property_17f88eedb22aa082b26367443907b457_Out_0, _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2);
            float3 _Multiply_7da903b43163338186b43865b8741fbc_Out_2;
            Unity_Multiply_float(_SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Multiply_7da903b43163338186b43865b8741fbc_Out_2);
            float3 _Round_5423730bc0c6c8818922975c01b91056_Out_1;
            Unity_Round_float3(_Multiply_7da903b43163338186b43865b8741fbc_Out_2, _Round_5423730bc0c6c8818922975c01b91056_Out_1);
            float3 _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2;
            Unity_Divide_float3(_Round_5423730bc0c6c8818922975c01b91056_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2);
            float _Property_11a06759662e5e88ac3f86982ceb6894_Out_0 = Vector1_533C9C05;
            float3 _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3;
            Unity_Lerp_float3(_Divide_29ba04366d96cb8da628d49d448e43cb_Out_2, _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Property_11a06759662e5e88ac3f86982ceb6894_Out_0.xxx), _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3);
            float4 _Property_2aa665ef010e3b81be432eeb62e801d2_Out_0 = Color_5DF75355;
            float4 _ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1;
            Unity_Absolute_float4(_ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0, _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1);
            float4 _Add_5147db6674014cc299d1bde82971b8e1_Out_2;
            Unity_Add_float4(_Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1, float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2);
            float Slider_a6daac9d997e408c90a2ebfebd5ac2c8 = 1;
            float4 _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2, (Slider_a6daac9d997e408c90a2ebfebd5ac2c8.xxxx), _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3);
            float _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1);
            float _Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0 = _Outline;
            float _Divide_353d1eaf295af18aa690543fa6687d92_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.x, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2);
            float _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.y, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2);
            float4 _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0, _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2);
            float _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_27a9d4e21cd1ea80a5a9103191908694_Out_2, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1);
            float _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1, _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2);
            float _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1;
            Unity_Absolute_float(_Subtract_030ff9c570cf228080e59107aae83a7b_Out_2, _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1);
            float _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2;
            Unity_Subtract_float(0, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2);
            float _Subtract_b2532dbe871964878032f187cbebd765_Out_2;
            Unity_Subtract_float(0, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2);
            float4 _Vector4_1593a891d741d686900e4556b2149ee4_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_227726cfbe798889876fd156a0242ccd_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_1593a891d741d686900e4556b2149ee4_Out_0, _Add_227726cfbe798889876fd156a0242ccd_Out_2);
            float _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_227726cfbe798889876fd156a0242ccd_Out_2, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1);
            float _Subtract_050e8def12697986bad41f8aadf517c2_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1, _Subtract_050e8def12697986bad41f8aadf517c2_Out_2);
            float _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1;
            Unity_Absolute_float(_Subtract_050e8def12697986bad41f8aadf517c2_Out_2, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1);
            float _Add_e4821fbf8a5e628085df8210f262057d_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2);
            float _Add_97a0d08feac04573835bbc78ddb7485a_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2, _Add_97a0d08feac04573835bbc78ddb7485a_Out_2);
            float4 _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0, _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2);
            float _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1);
            float _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1, _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2);
            float _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1;
            Unity_Absolute_float(_Subtract_cdd8828f44dfec87be1a856545e05681_Out_2, _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1);
            float4 _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_6de672402b44fb8d98184769ec42ebec_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0, _Add_6de672402b44fb8d98184769ec42ebec_Out_2);
            float _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6de672402b44fb8d98184769ec42ebec_Out_2, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1);
            float _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1, _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2);
            float _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1;
            Unity_Absolute_float(_Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1);
            float _Add_389d560813dd198aa5e64a302b5942af_Out_2;
            Unity_Add_float(_Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_389d560813dd198aa5e64a302b5942af_Out_2);
            float _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2;
            Unity_Add_float(_Add_389d560813dd198aa5e64a302b5942af_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2);
            float _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2;
            Unity_Add_float(_Add_97a0d08feac04573835bbc78ddb7485a_Out_2, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2, _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2);
            float _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2;
            Unity_Divide_float(_ProjectionParams.z, 10, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2);
            float _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2;
            Unity_Multiply_float(_Add_f8053a2eef9de1898c32d6efe4251f82_Out_2, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2, _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2);
            float _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1);
            float _Property_3d31d0933367c38688b099bb23c954d3_Out_0 = _OutlineFade;
            float _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2;
            Unity_Add_float(_Property_3d31d0933367c38688b099bb23c954d3_Out_0, 1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2);
            float _Power_0b2fb1311c40038f895495bc455d938e_Out_2;
            Unity_Power_float(_SceneDepth_45cda59e81fd96839f89657068c67112_Out_1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2);
            float _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2;
            Unity_Divide_float(_Multiply_18108578fe7e608a8acc84aab2986c47_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2, _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2);
            float _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2, 0.01, _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2);
            float _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3;
            Unity_Branch_float(_Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2, 1, 0, _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3);
            float3 _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3;
            Unity_Lerp_float3(_Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3, (_Property_2aa665ef010e3b81be432eeb62e801d2_Out_0.xyz), (_Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3.xxx), _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3);
            float3 _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2;
            Unity_Minimum_float3((IN.VertexColor.xyz), IN.ObjectSpacePosition, _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2);
            float _Split_196288f9704a43fcb98e6f6036932753_R_1 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[0];
            float _Split_196288f9704a43fcb98e6f6036932753_G_2 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[1];
            float _Split_196288f9704a43fcb98e6f6036932753_B_3 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[2];
            float _Split_196288f9704a43fcb98e6f6036932753_A_4 = 0;
            float4 _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0 = float4(_Split_196288f9704a43fcb98e6f6036932753_R_1, _Split_196288f9704a43fcb98e6f6036932753_G_2, _Split_196288f9704a43fcb98e6f6036932753_B_3, _Split_196288f9704a43fcb98e6f6036932753_A_4);
            float4 _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2;
            Unity_Multiply_float(UNITY_MATRIX_P, _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0, _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2);
            float3 _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2;
            Unity_Subtract_float3(_Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3, (_Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2.xyz), _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2);
            float3 _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2;
            Unity_Divide_float3(_Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2, float3(4, 4, 4), _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2);
            float3 _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1;
            Unity_OneMinus_float3(_Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2, _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1);
            float4 Color_3829a5c39aa649529f84951651c2303c = IsGammaSpace() ? float4(0, 0, 0, 0) : float4(SRGBToLinear(float3(0, 0, 0)), 0);
            float _Property_b897d223351b410e886999af2c214c36_Out_0 = Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
            float _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3;
            Unity_Remap_float(_Property_b897d223351b410e886999af2c214c36_Out_0, float2 (0, 1), float2 (1, 0), _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3);
            float3 _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3;
            Unity_Lerp_float3(_OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1, (Color_3829a5c39aa649529f84951651c2303c.xyz), (_Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3.xxx), _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3);
            float3 _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3;
            Unity_Branch_float3(_Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0, _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3, float3(0, 0, 0), _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3);
            surface.Alpha = (_Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3).x;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.uv1 =                         input.uv1;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ObjectSpacePosition =         TransformWorldToObject(input.positionWS);
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.VertexColor =                 input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "ShadowCasterPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
        #define REQUIRE_DEPTH_TEXTURE
        #define REQUIRE_OPAQUE_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 ObjectSpacePosition;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
            float4 uv1;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.color = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial) float4x4 _WorldInverse; float4x4 _Matrices[100]; float4x4 _BindPoses[100];
        float _CellShading;
        float Vector1_533C9C05;
        float _Outline;
        float4 Color_5DF75355;
        float _OutlineFade;
        float4 Texture2D_E0E90B4F_TexelSize;
        float _isOutlineDisabled;
        float Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
        CBUFFER_END

        // Object and Global properties
        TEXTURE2D(Texture2D_E0E90B4F);
        SAMPLER(samplerTexture2D_E0E90B4F);

            // Graph Functions
            
        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }

        void Unity_SceneColor_float(float4 UV, out float3 Out)
        {
            Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_Round_float3(float3 In, out float3 Out)
        {
            Out = round(In);
        }

        void Unity_Divide_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A / B;
        }

        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_SceneDepth_Linear01_float(float4 UV, out float Out)
        {
            Out = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }

        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }

        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }

        void Unity_Comparison_GreaterOrEqual_float(float A, float B, out float Out)
        {
            Out = A >= B ? 1 : 0;
        }

        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Minimum_float3(float3 A, float3 B, out float3 Out)
        {
            Out = min(A, B);
        };

        void Unity_Multiply_float(float4x4 A, float4 B, out float4 Out)
        {
            Out = mul(A, B);
        }

        void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A - B;
        }

        void Unity_OneMinus_float3(float3 In, out float3 Out)
        {
            Out = 1 - In;
        }

        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float4 _UV_b8a348e3911942a998d5a3fa0c678007_Out_0 = IN.uv1;
            float4 _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2;
            Unity_Multiply_float(_UV_b8a348e3911942a998d5a3fa0c678007_Out_0, float4(0, 0, 0, 0), _Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2);
            float3 _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            Unity_Add_float3(IN.ObjectSpacePosition, (_Multiply_0639ca77da4448f4bc46bd434a400cbd_Out_2.xyz), _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2);
            description.Position = _Add_3534c3a0675a4cc4a79075d0443435b6_Out_2;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0 = _isOutlineDisabled;
            float3 _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1;
            Unity_SceneColor_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1);
            float _Property_17f88eedb22aa082b26367443907b457_Out_0 = _CellShading;
            float _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2;
            Unity_Subtract_float(256, _Property_17f88eedb22aa082b26367443907b457_Out_0, _Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2);
            float3 _Multiply_7da903b43163338186b43865b8741fbc_Out_2;
            Unity_Multiply_float(_SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Multiply_7da903b43163338186b43865b8741fbc_Out_2);
            float3 _Round_5423730bc0c6c8818922975c01b91056_Out_1;
            Unity_Round_float3(_Multiply_7da903b43163338186b43865b8741fbc_Out_2, _Round_5423730bc0c6c8818922975c01b91056_Out_1);
            float3 _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2;
            Unity_Divide_float3(_Round_5423730bc0c6c8818922975c01b91056_Out_1, (_Subtract_d3ce1aaf475855838f202bda5c87385a_Out_2.xxx), _Divide_29ba04366d96cb8da628d49d448e43cb_Out_2);
            float _Property_11a06759662e5e88ac3f86982ceb6894_Out_0 = Vector1_533C9C05;
            float3 _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3;
            Unity_Lerp_float3(_Divide_29ba04366d96cb8da628d49d448e43cb_Out_2, _SceneColor_a25f5ad1851d018fb231a691bfb6e249_Out_1, (_Property_11a06759662e5e88ac3f86982ceb6894_Out_0.xxx), _Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3);
            float4 _Property_2aa665ef010e3b81be432eeb62e801d2_Out_0 = Color_5DF75355;
            float4 _ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1;
            Unity_Absolute_float4(_ScreenPosition_0734e4c791dc0587a30205707e8db8ba_Out_0, _Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1);
            float4 _Add_5147db6674014cc299d1bde82971b8e1_Out_2;
            Unity_Add_float4(_Absolute_a61143b9f17543548fbfa0a34e535d1a_Out_1, float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2);
            float Slider_a6daac9d997e408c90a2ebfebd5ac2c8 = 1;
            float4 _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_5147db6674014cc299d1bde82971b8e1_Out_2, (Slider_a6daac9d997e408c90a2ebfebd5ac2c8.xxxx), _Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3);
            float _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1);
            float _Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0 = _Outline;
            float _Divide_353d1eaf295af18aa690543fa6687d92_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.x, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2);
            float _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2;
            Unity_Divide_float(_Property_e9f1d7088d9e8d80aaeb9708c66b0ea1_Out_0, _ScreenParams.y, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2);
            float4 _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_70a2fb083d233c819db7b796e073e7a4_Out_0, _Add_27a9d4e21cd1ea80a5a9103191908694_Out_2);
            float _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_27a9d4e21cd1ea80a5a9103191908694_Out_2, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1);
            float _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_437245d8e7dd8786a2d9544bb8af1094_Out_1, _Subtract_030ff9c570cf228080e59107aae83a7b_Out_2);
            float _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1;
            Unity_Absolute_float(_Subtract_030ff9c570cf228080e59107aae83a7b_Out_2, _Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1);
            float _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2;
            Unity_Subtract_float(0, _Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2);
            float _Subtract_b2532dbe871964878032f187cbebd765_Out_2;
            Unity_Subtract_float(0, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2);
            float4 _Vector4_1593a891d741d686900e4556b2149ee4_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_227726cfbe798889876fd156a0242ccd_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_1593a891d741d686900e4556b2149ee4_Out_0, _Add_227726cfbe798889876fd156a0242ccd_Out_2);
            float _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_227726cfbe798889876fd156a0242ccd_Out_2, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1);
            float _Subtract_050e8def12697986bad41f8aadf517c2_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_6e1e6e68e429ab8ebf4eedc8c28e389a_Out_1, _Subtract_050e8def12697986bad41f8aadf517c2_Out_2);
            float _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1;
            Unity_Absolute_float(_Subtract_050e8def12697986bad41f8aadf517c2_Out_2, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1);
            float _Add_e4821fbf8a5e628085df8210f262057d_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Absolute_0546cab52b2ed88c976bddec305c8170_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2);
            float _Add_97a0d08feac04573835bbc78ddb7485a_Out_2;
            Unity_Add_float(_Absolute_882fe6920634ce888be3e1d3c440ca98_Out_1, _Add_e4821fbf8a5e628085df8210f262057d_Out_2, _Add_97a0d08feac04573835bbc78ddb7485a_Out_2);
            float4 _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0 = float4(_Divide_353d1eaf295af18aa690543fa6687d92_Out_2, _Subtract_b2532dbe871964878032f187cbebd765_Out_2, 0, 0);
            float4 _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_e7ae0c7101fb898796e892a2908fec02_Out_0, _Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2);
            float _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_15a7a9878b8dcd82b3cb46c239a2f415_Out_2, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1);
            float _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_3e490f280e18268380d678e8537294d2_Out_1, _Subtract_cdd8828f44dfec87be1a856545e05681_Out_2);
            float _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1;
            Unity_Absolute_float(_Subtract_cdd8828f44dfec87be1a856545e05681_Out_2, _Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1);
            float4 _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0 = float4(_Subtract_04b0c168355bb48da79ed760637d5f5a_Out_2, _Divide_0e84d3f056d1688fa2923fd76befbfaa_Out_2, 0, 0);
            float4 _Add_6de672402b44fb8d98184769ec42ebec_Out_2;
            Unity_Add_float4(_Lerp_047d5a76d4a64812b89db1f5315d5913_Out_3, _Vector4_4115a3ff67647f8c8a6bcb7abf05ca7b_Out_0, _Add_6de672402b44fb8d98184769ec42ebec_Out_2);
            float _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6de672402b44fb8d98184769ec42ebec_Out_2, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1);
            float _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2;
            Unity_Subtract_float(_SceneDepth_532aaaac0cf1d481a5309a0b68dad8b3_Out_1, _SceneDepth_032ee35ec078818ca6af6035eff40a36_Out_1, _Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2);
            float _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1;
            Unity_Absolute_float(_Subtract_207b1967063de1889f2fd1eb68668e4e_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1);
            float _Add_389d560813dd198aa5e64a302b5942af_Out_2;
            Unity_Add_float(_Absolute_d7a504777b2bee84aa7f9dd8880d88ad_Out_1, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_389d560813dd198aa5e64a302b5942af_Out_2);
            float _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2;
            Unity_Add_float(_Add_389d560813dd198aa5e64a302b5942af_Out_2, _Absolute_30755f485bcbb8829ac5d95de2492e2e_Out_1, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2);
            float _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2;
            Unity_Add_float(_Add_97a0d08feac04573835bbc78ddb7485a_Out_2, _Add_e60ff5e058d3405ba32ee8f072dca8b4_Out_2, _Add_f8053a2eef9de1898c32d6efe4251f82_Out_2);
            float _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2;
            Unity_Divide_float(_ProjectionParams.z, 10, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2);
            float _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2;
            Unity_Multiply_float(_Add_f8053a2eef9de1898c32d6efe4251f82_Out_2, _Divide_9abeceb917d66b888769cbf8fc3d6906_Out_2, _Multiply_18108578fe7e608a8acc84aab2986c47_Out_2);
            float _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_45cda59e81fd96839f89657068c67112_Out_1);
            float _Property_3d31d0933367c38688b099bb23c954d3_Out_0 = _OutlineFade;
            float _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2;
            Unity_Add_float(_Property_3d31d0933367c38688b099bb23c954d3_Out_0, 1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2);
            float _Power_0b2fb1311c40038f895495bc455d938e_Out_2;
            Unity_Power_float(_SceneDepth_45cda59e81fd96839f89657068c67112_Out_1, _Add_ee689266f8e9ad86a3e504bf4b6e5dd0_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2);
            float _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2;
            Unity_Divide_float(_Multiply_18108578fe7e608a8acc84aab2986c47_Out_2, _Power_0b2fb1311c40038f895495bc455d938e_Out_2, _Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2);
            float _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_bd11f09281d6da8f847b7c17a4581c66_Out_2, 0.01, _Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2);
            float _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3;
            Unity_Branch_float(_Comparison_3f21efc7d95dc78a8e86f9deb9b115e2_Out_2, 1, 0, _Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3);
            float3 _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3;
            Unity_Lerp_float3(_Lerp_925803dabf20668cb89c07a4a1ce6b7f_Out_3, (_Property_2aa665ef010e3b81be432eeb62e801d2_Out_0.xyz), (_Branch_cd0e4f3253cfe882a65d6a967afb6938_Out_3.xxx), _Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3);
            float3 _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2;
            Unity_Minimum_float3((IN.VertexColor.xyz), IN.ObjectSpacePosition, _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2);
            float _Split_196288f9704a43fcb98e6f6036932753_R_1 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[0];
            float _Split_196288f9704a43fcb98e6f6036932753_G_2 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[1];
            float _Split_196288f9704a43fcb98e6f6036932753_B_3 = _Minimum_0850f1bfee38433491beaeb850570e1d_Out_2[2];
            float _Split_196288f9704a43fcb98e6f6036932753_A_4 = 0;
            float4 _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0 = float4(_Split_196288f9704a43fcb98e6f6036932753_R_1, _Split_196288f9704a43fcb98e6f6036932753_G_2, _Split_196288f9704a43fcb98e6f6036932753_B_3, _Split_196288f9704a43fcb98e6f6036932753_A_4);
            float4 _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2;
            Unity_Multiply_float(UNITY_MATRIX_P, _Vector4_395c2cc2aa2341c88725e41c2fcedf92_Out_0, _Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2);
            float3 _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2;
            Unity_Subtract_float3(_Lerp_1ce2ce00a4fcfc8984222d990c3a9268_Out_3, (_Multiply_69a2f5fd75af4e5b9df336f03b614d92_Out_2.xyz), _Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2);
            float3 _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2;
            Unity_Divide_float3(_Subtract_9e396cf3ae2b4b3d9efe1c6379305030_Out_2, float3(4, 4, 4), _Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2);
            float3 _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1;
            Unity_OneMinus_float3(_Divide_ffb5e4b9e5794bd4b9ea2bb0b80bac93_Out_2, _OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1);
            float4 Color_3829a5c39aa649529f84951651c2303c = IsGammaSpace() ? float4(0, 0, 0, 0) : float4(SRGBToLinear(float3(0, 0, 0)), 0);
            float _Property_b897d223351b410e886999af2c214c36_Out_0 = Vector1_a6322ea535ad4b8ea9f2b0a1ae114a49;
            float _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3;
            Unity_Remap_float(_Property_b897d223351b410e886999af2c214c36_Out_0, float2 (0, 1), float2 (1, 0), _Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3);
            float3 _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3;
            Unity_Lerp_float3(_OneMinus_6b3fd880d66b4c74a643f0afe537209d_Out_1, (Color_3829a5c39aa649529f84951651c2303c.xyz), (_Remap_0a5fec86b32f41ea9af9740629c4c897_Out_3.xxx), _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3);
            float3 _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3;
            Unity_Branch_float3(_Property_f7b8f08f8ccc4f4fa0d70c167ad35714_Out_0, _Lerp_8f44e3154d2043eab5f2dba3680e3e7c_Out_3, float3(0, 0, 0), _Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3);
            surface.Alpha = (_Branch_c6dbdfb1fa6b41ec9b4d06a3ebcd49cb_Out_3).x;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;
            output.uv1 =                         input.uv1;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ObjectSpacePosition =         TransformWorldToObject(input.positionWS);
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.VertexColor =                 input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
       #include "DepthOnlyPass.hlsl"

            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}