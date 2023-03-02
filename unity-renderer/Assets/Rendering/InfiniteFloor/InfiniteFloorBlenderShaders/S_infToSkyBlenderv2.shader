Shader "CustomShader/CGGen/Unlit/InfToSkyBlenderv2"
{
    Properties
    {
        _colorFloor("ColorFloor", Color) = (0, 0, 0, 0)
        _colorSky("ColorSky", Color) = (0, 0, 0, 0)
        _colorArea("ColorArea", Range(0, 1)) = 0
        _horizonStartPoint("HorizonStartPoint", Range(0, 1)) = 0
        _fade("Fade", Range(0, 1)) = 0
        _opacity("Opacity", Range(0, 1)) = 0
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
            Cull Off
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
        //#pragma multi_compile_fog
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
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_UNLIT
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
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
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
            float3 WorldSpacePosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
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
            output.interp1.xyzw =  input.texCoord0;
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
            output.texCoord0 = input.interp1.xyzw;
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
            CBUFFER_START(UnityPerMaterial)
        float4 _colorFloor;
        float4 _colorSky;
        float _colorArea;
        float _horizonStartPoint;
        float _fade;
        float _opacity;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
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
            description.Position = IN.ObjectSpacePosition;
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
            float4 _Property_ebb921ea819640ab8212a37e253e2371_Out_0 = _colorSky;
            float4 _Property_d510806a90eb44d8a3358e1bae617f9e_Out_0 = _colorFloor;
            float _Property_a970125d69574360bc269817531f2345_Out_0 = _colorArea;
            float _Remap_89c31f73da0141d7b734d7d8d6ab2542_Out_3;
            Unity_Remap_float(_Property_a970125d69574360bc269817531f2345_Out_0, float2 (0, 1), float2 (0, 100), _Remap_89c31f73da0141d7b734d7d8d6ab2542_Out_3);
            float4 _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RGBA_4;
            float3 _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RGB_5;
            float2 _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RG_6;
            Unity_Combine_float(0, _Remap_89c31f73da0141d7b734d7d8d6ab2542_Out_3, 0, 0, _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RGBA_4, _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RGB_5, _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RG_6);
            float2 _TilingAndOffset_78d2fa950a3a4edfb08d76cd28defd41_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RG_6, float2 (0, 0), _TilingAndOffset_78d2fa950a3a4edfb08d76cd28defd41_Out_3);
            float _Split_f102f27ffa9b42a381a7f2b776c604cd_R_1 = _TilingAndOffset_78d2fa950a3a4edfb08d76cd28defd41_Out_3[0];
            float _Split_f102f27ffa9b42a381a7f2b776c604cd_G_2 = _TilingAndOffset_78d2fa950a3a4edfb08d76cd28defd41_Out_3[1];
            float _Split_f102f27ffa9b42a381a7f2b776c604cd_B_3 = 0;
            float _Split_f102f27ffa9b42a381a7f2b776c604cd_A_4 = 0;
            float _Property_624ca2d844594690af1d834a1a61c282_Out_0 = _colorArea;
            float _Multiply_0934024c0e2a4a51a39a23874978c7ec_Out_2;
            Unity_Multiply_float(_Split_f102f27ffa9b42a381a7f2b776c604cd_G_2, _Property_624ca2d844594690af1d834a1a61c282_Out_0, _Multiply_0934024c0e2a4a51a39a23874978c7ec_Out_2);
            float4 _Lerp_66cf411ef0c647d6bc391b3e6e0f175d_Out_3;
            Unity_Lerp_float4(_Property_ebb921ea819640ab8212a37e253e2371_Out_0, _Property_d510806a90eb44d8a3358e1bae617f9e_Out_0, (_Multiply_0934024c0e2a4a51a39a23874978c7ec_Out_2.xxxx), _Lerp_66cf411ef0c647d6bc391b3e6e0f175d_Out_3);
            float _Split_aeeafb27b6b14fdcae69f448e2260726_R_1 = IN.WorldSpacePosition[0];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_G_2 = IN.WorldSpacePosition[1];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_B_3 = IN.WorldSpacePosition[2];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_A_4 = 0;
            float _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0 = _horizonStartPoint;
            float _Subtract_a8807f3cb202422081af18e991784035_Out_2;
            Unity_Subtract_float(_Split_aeeafb27b6b14fdcae69f448e2260726_G_2, _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0, _Subtract_a8807f3cb202422081af18e991784035_Out_2);
            float _Property_f7c05ac277ed4f2292888504564fd941_Out_0 = _colorArea;
            float _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            Unity_Multiply_float(_Subtract_a8807f3cb202422081af18e991784035_Out_2, _Property_f7c05ac277ed4f2292888504564fd941_Out_0, _Multiply_1170df6f23a446b3af978d503196a379_Out_2);
            float _Split_feba676e1c5e4f709179250fe78fb1bb_R_1 = _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_G_2 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_B_3 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_A_4 = 0;
            float _Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0 = _fade;
            float4 _UV_e14cf8c76e164723aebfe22b814798b3_Out_0 = IN.uv0;
            float _Split_a54861b234184c03a9499c1168c06c3a_R_1 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[0];
            float _Split_a54861b234184c03a9499c1168c06c3a_G_2 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[1];
            float _Split_a54861b234184c03a9499c1168c06c3a_B_3 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[2];
            float _Split_a54861b234184c03a9499c1168c06c3a_A_4 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[3];
            float _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2;
            Unity_Multiply_float(_Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0, _Split_a54861b234184c03a9499c1168c06c3a_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2);
            float _Property_5bf34e958fa5418f96c21e781585c51c_Out_0 = _fade;
            float _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3;
            Unity_Lerp_float(_Split_feba676e1c5e4f709179250fe78fb1bb_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2, _Property_5bf34e958fa5418f96c21e781585c51c_Out_0, _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3);
            float _Property_14f624362d86444fb3dc22672a6eca9f_Out_0 = _opacity;
            float _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
            Unity_Lerp_float(_Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3, 1, _Property_14f624362d86444fb3dc22672a6eca9f_Out_0, _Lerp_9acfe57675d14994af96573154250d0d_Out_3);
            surface.BaseColor = (_Lerp_66cf411ef0c647d6bc391b3e6e0f175d_Out_3.xyz);
            surface.Alpha = _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
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

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.uv0 =                         input.texCoord0;
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
            Cull Off
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
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
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
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
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
            float3 WorldSpacePosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
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
            output.interp1.xyzw =  input.texCoord0;
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
            output.texCoord0 = input.interp1.xyzw;
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
            CBUFFER_START(UnityPerMaterial)
        float4 _colorFloor;
        float4 _colorSky;
        float _colorArea;
        float _horizonStartPoint;
        float _fade;
        float _opacity;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _Split_aeeafb27b6b14fdcae69f448e2260726_R_1 = IN.WorldSpacePosition[0];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_G_2 = IN.WorldSpacePosition[1];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_B_3 = IN.WorldSpacePosition[2];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_A_4 = 0;
            float _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0 = _horizonStartPoint;
            float _Subtract_a8807f3cb202422081af18e991784035_Out_2;
            Unity_Subtract_float(_Split_aeeafb27b6b14fdcae69f448e2260726_G_2, _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0, _Subtract_a8807f3cb202422081af18e991784035_Out_2);
            float _Property_f7c05ac277ed4f2292888504564fd941_Out_0 = _colorArea;
            float _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            Unity_Multiply_float(_Subtract_a8807f3cb202422081af18e991784035_Out_2, _Property_f7c05ac277ed4f2292888504564fd941_Out_0, _Multiply_1170df6f23a446b3af978d503196a379_Out_2);
            float _Split_feba676e1c5e4f709179250fe78fb1bb_R_1 = _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_G_2 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_B_3 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_A_4 = 0;
            float _Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0 = _fade;
            float4 _UV_e14cf8c76e164723aebfe22b814798b3_Out_0 = IN.uv0;
            float _Split_a54861b234184c03a9499c1168c06c3a_R_1 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[0];
            float _Split_a54861b234184c03a9499c1168c06c3a_G_2 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[1];
            float _Split_a54861b234184c03a9499c1168c06c3a_B_3 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[2];
            float _Split_a54861b234184c03a9499c1168c06c3a_A_4 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[3];
            float _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2;
            Unity_Multiply_float(_Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0, _Split_a54861b234184c03a9499c1168c06c3a_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2);
            float _Property_5bf34e958fa5418f96c21e781585c51c_Out_0 = _fade;
            float _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3;
            Unity_Lerp_float(_Split_feba676e1c5e4f709179250fe78fb1bb_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2, _Property_5bf34e958fa5418f96c21e781585c51c_Out_0, _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3);
            float _Property_14f624362d86444fb3dc22672a6eca9f_Out_0 = _opacity;
            float _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
            Unity_Lerp_float(_Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3, 1, _Property_14f624362d86444fb3dc22672a6eca9f_Out_0, _Lerp_9acfe57675d14994af96573154250d0d_Out_3);
            surface.Alpha = _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
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

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.uv0 =                         input.texCoord0;
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

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
            Cull Off
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
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
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
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
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
            float3 WorldSpacePosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
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
            output.interp1.xyzw =  input.texCoord0;
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
            output.texCoord0 = input.interp1.xyzw;
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
            CBUFFER_START(UnityPerMaterial)
        float4 _colorFloor;
        float4 _colorSky;
        float _colorArea;
        float _horizonStartPoint;
        float _fade;
        float _opacity;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _Split_aeeafb27b6b14fdcae69f448e2260726_R_1 = IN.WorldSpacePosition[0];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_G_2 = IN.WorldSpacePosition[1];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_B_3 = IN.WorldSpacePosition[2];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_A_4 = 0;
            float _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0 = _horizonStartPoint;
            float _Subtract_a8807f3cb202422081af18e991784035_Out_2;
            Unity_Subtract_float(_Split_aeeafb27b6b14fdcae69f448e2260726_G_2, _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0, _Subtract_a8807f3cb202422081af18e991784035_Out_2);
            float _Property_f7c05ac277ed4f2292888504564fd941_Out_0 = _colorArea;
            float _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            Unity_Multiply_float(_Subtract_a8807f3cb202422081af18e991784035_Out_2, _Property_f7c05ac277ed4f2292888504564fd941_Out_0, _Multiply_1170df6f23a446b3af978d503196a379_Out_2);
            float _Split_feba676e1c5e4f709179250fe78fb1bb_R_1 = _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_G_2 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_B_3 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_A_4 = 0;
            float _Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0 = _fade;
            float4 _UV_e14cf8c76e164723aebfe22b814798b3_Out_0 = IN.uv0;
            float _Split_a54861b234184c03a9499c1168c06c3a_R_1 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[0];
            float _Split_a54861b234184c03a9499c1168c06c3a_G_2 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[1];
            float _Split_a54861b234184c03a9499c1168c06c3a_B_3 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[2];
            float _Split_a54861b234184c03a9499c1168c06c3a_A_4 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[3];
            float _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2;
            Unity_Multiply_float(_Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0, _Split_a54861b234184c03a9499c1168c06c3a_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2);
            float _Property_5bf34e958fa5418f96c21e781585c51c_Out_0 = _fade;
            float _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3;
            Unity_Lerp_float(_Split_feba676e1c5e4f709179250fe78fb1bb_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2, _Property_5bf34e958fa5418f96c21e781585c51c_Out_0, _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3);
            float _Property_14f624362d86444fb3dc22672a6eca9f_Out_0 = _opacity;
            float _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
            Unity_Lerp_float(_Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3, 1, _Property_14f624362d86444fb3dc22672a6eca9f_Out_0, _Lerp_9acfe57675d14994af96573154250d0d_Out_3);
            surface.Alpha = _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
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

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.uv0 =                         input.texCoord0;
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

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
            Cull Off
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
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_UNLIT
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
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
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
            float3 WorldSpacePosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
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
            output.interp1.xyzw =  input.texCoord0;
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
            output.texCoord0 = input.interp1.xyzw;
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
            CBUFFER_START(UnityPerMaterial)
        float4 _colorFloor;
        float4 _colorSky;
        float _colorArea;
        float _horizonStartPoint;
        float _fade;
        float _opacity;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
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
            description.Position = IN.ObjectSpacePosition;
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
            float4 _Property_ebb921ea819640ab8212a37e253e2371_Out_0 = _colorSky;
            float4 _Property_d510806a90eb44d8a3358e1bae617f9e_Out_0 = _colorFloor;
            float _Property_a970125d69574360bc269817531f2345_Out_0 = _colorArea;
            float _Remap_89c31f73da0141d7b734d7d8d6ab2542_Out_3;
            Unity_Remap_float(_Property_a970125d69574360bc269817531f2345_Out_0, float2 (0, 1), float2 (0, 100), _Remap_89c31f73da0141d7b734d7d8d6ab2542_Out_3);
            float4 _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RGBA_4;
            float3 _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RGB_5;
            float2 _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RG_6;
            Unity_Combine_float(0, _Remap_89c31f73da0141d7b734d7d8d6ab2542_Out_3, 0, 0, _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RGBA_4, _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RGB_5, _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RG_6);
            float2 _TilingAndOffset_78d2fa950a3a4edfb08d76cd28defd41_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Combine_101f5e7e40a544d3b4b210dbbd2cc135_RG_6, float2 (0, 0), _TilingAndOffset_78d2fa950a3a4edfb08d76cd28defd41_Out_3);
            float _Split_f102f27ffa9b42a381a7f2b776c604cd_R_1 = _TilingAndOffset_78d2fa950a3a4edfb08d76cd28defd41_Out_3[0];
            float _Split_f102f27ffa9b42a381a7f2b776c604cd_G_2 = _TilingAndOffset_78d2fa950a3a4edfb08d76cd28defd41_Out_3[1];
            float _Split_f102f27ffa9b42a381a7f2b776c604cd_B_3 = 0;
            float _Split_f102f27ffa9b42a381a7f2b776c604cd_A_4 = 0;
            float _Property_624ca2d844594690af1d834a1a61c282_Out_0 = _colorArea;
            float _Multiply_0934024c0e2a4a51a39a23874978c7ec_Out_2;
            Unity_Multiply_float(_Split_f102f27ffa9b42a381a7f2b776c604cd_G_2, _Property_624ca2d844594690af1d834a1a61c282_Out_0, _Multiply_0934024c0e2a4a51a39a23874978c7ec_Out_2);
            float4 _Lerp_66cf411ef0c647d6bc391b3e6e0f175d_Out_3;
            Unity_Lerp_float4(_Property_ebb921ea819640ab8212a37e253e2371_Out_0, _Property_d510806a90eb44d8a3358e1bae617f9e_Out_0, (_Multiply_0934024c0e2a4a51a39a23874978c7ec_Out_2.xxxx), _Lerp_66cf411ef0c647d6bc391b3e6e0f175d_Out_3);
            float _Split_aeeafb27b6b14fdcae69f448e2260726_R_1 = IN.WorldSpacePosition[0];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_G_2 = IN.WorldSpacePosition[1];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_B_3 = IN.WorldSpacePosition[2];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_A_4 = 0;
            float _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0 = _horizonStartPoint;
            float _Subtract_a8807f3cb202422081af18e991784035_Out_2;
            Unity_Subtract_float(_Split_aeeafb27b6b14fdcae69f448e2260726_G_2, _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0, _Subtract_a8807f3cb202422081af18e991784035_Out_2);
            float _Property_f7c05ac277ed4f2292888504564fd941_Out_0 = _colorArea;
            float _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            Unity_Multiply_float(_Subtract_a8807f3cb202422081af18e991784035_Out_2, _Property_f7c05ac277ed4f2292888504564fd941_Out_0, _Multiply_1170df6f23a446b3af978d503196a379_Out_2);
            float _Split_feba676e1c5e4f709179250fe78fb1bb_R_1 = _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_G_2 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_B_3 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_A_4 = 0;
            float _Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0 = _fade;
            float4 _UV_e14cf8c76e164723aebfe22b814798b3_Out_0 = IN.uv0;
            float _Split_a54861b234184c03a9499c1168c06c3a_R_1 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[0];
            float _Split_a54861b234184c03a9499c1168c06c3a_G_2 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[1];
            float _Split_a54861b234184c03a9499c1168c06c3a_B_3 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[2];
            float _Split_a54861b234184c03a9499c1168c06c3a_A_4 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[3];
            float _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2;
            Unity_Multiply_float(_Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0, _Split_a54861b234184c03a9499c1168c06c3a_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2);
            float _Property_5bf34e958fa5418f96c21e781585c51c_Out_0 = _fade;
            float _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3;
            Unity_Lerp_float(_Split_feba676e1c5e4f709179250fe78fb1bb_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2, _Property_5bf34e958fa5418f96c21e781585c51c_Out_0, _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3);
            float _Property_14f624362d86444fb3dc22672a6eca9f_Out_0 = _opacity;
            float _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
            Unity_Lerp_float(_Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3, 1, _Property_14f624362d86444fb3dc22672a6eca9f_Out_0, _Lerp_9acfe57675d14994af96573154250d0d_Out_3);
            surface.BaseColor = (_Lerp_66cf411ef0c647d6bc391b3e6e0f175d_Out_3.xyz);
            surface.Alpha = _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
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

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.uv0 =                         input.texCoord0;
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
            Cull Off
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
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
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
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
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
            float3 WorldSpacePosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
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
            output.interp1.xyzw =  input.texCoord0;
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
            output.texCoord0 = input.interp1.xyzw;
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
            CBUFFER_START(UnityPerMaterial)
        float4 _colorFloor;
        float4 _colorSky;
        float _colorArea;
        float _horizonStartPoint;
        float _fade;
        float _opacity;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _Split_aeeafb27b6b14fdcae69f448e2260726_R_1 = IN.WorldSpacePosition[0];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_G_2 = IN.WorldSpacePosition[1];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_B_3 = IN.WorldSpacePosition[2];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_A_4 = 0;
            float _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0 = _horizonStartPoint;
            float _Subtract_a8807f3cb202422081af18e991784035_Out_2;
            Unity_Subtract_float(_Split_aeeafb27b6b14fdcae69f448e2260726_G_2, _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0, _Subtract_a8807f3cb202422081af18e991784035_Out_2);
            float _Property_f7c05ac277ed4f2292888504564fd941_Out_0 = _colorArea;
            float _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            Unity_Multiply_float(_Subtract_a8807f3cb202422081af18e991784035_Out_2, _Property_f7c05ac277ed4f2292888504564fd941_Out_0, _Multiply_1170df6f23a446b3af978d503196a379_Out_2);
            float _Split_feba676e1c5e4f709179250fe78fb1bb_R_1 = _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_G_2 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_B_3 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_A_4 = 0;
            float _Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0 = _fade;
            float4 _UV_e14cf8c76e164723aebfe22b814798b3_Out_0 = IN.uv0;
            float _Split_a54861b234184c03a9499c1168c06c3a_R_1 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[0];
            float _Split_a54861b234184c03a9499c1168c06c3a_G_2 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[1];
            float _Split_a54861b234184c03a9499c1168c06c3a_B_3 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[2];
            float _Split_a54861b234184c03a9499c1168c06c3a_A_4 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[3];
            float _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2;
            Unity_Multiply_float(_Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0, _Split_a54861b234184c03a9499c1168c06c3a_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2);
            float _Property_5bf34e958fa5418f96c21e781585c51c_Out_0 = _fade;
            float _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3;
            Unity_Lerp_float(_Split_feba676e1c5e4f709179250fe78fb1bb_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2, _Property_5bf34e958fa5418f96c21e781585c51c_Out_0, _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3);
            float _Property_14f624362d86444fb3dc22672a6eca9f_Out_0 = _opacity;
            float _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
            Unity_Lerp_float(_Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3, 1, _Property_14f624362d86444fb3dc22672a6eca9f_Out_0, _Lerp_9acfe57675d14994af96573154250d0d_Out_3);
            surface.Alpha = _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
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

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.uv0 =                         input.texCoord0;
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

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
            Cull Off
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
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
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
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
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
            float3 WorldSpacePosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
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
            output.interp1.xyzw =  input.texCoord0;
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
            output.texCoord0 = input.interp1.xyzw;
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
            CBUFFER_START(UnityPerMaterial)
        float4 _colorFloor;
        float4 _colorSky;
        float _colorArea;
        float _horizonStartPoint;
        float _fade;
        float _opacity;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _Split_aeeafb27b6b14fdcae69f448e2260726_R_1 = IN.WorldSpacePosition[0];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_G_2 = IN.WorldSpacePosition[1];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_B_3 = IN.WorldSpacePosition[2];
            float _Split_aeeafb27b6b14fdcae69f448e2260726_A_4 = 0;
            float _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0 = _horizonStartPoint;
            float _Subtract_a8807f3cb202422081af18e991784035_Out_2;
            Unity_Subtract_float(_Split_aeeafb27b6b14fdcae69f448e2260726_G_2, _Property_e129408d0454460ba9a61e6799a8e3c6_Out_0, _Subtract_a8807f3cb202422081af18e991784035_Out_2);
            float _Property_f7c05ac277ed4f2292888504564fd941_Out_0 = _colorArea;
            float _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            Unity_Multiply_float(_Subtract_a8807f3cb202422081af18e991784035_Out_2, _Property_f7c05ac277ed4f2292888504564fd941_Out_0, _Multiply_1170df6f23a446b3af978d503196a379_Out_2);
            float _Split_feba676e1c5e4f709179250fe78fb1bb_R_1 = _Multiply_1170df6f23a446b3af978d503196a379_Out_2;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_G_2 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_B_3 = 0;
            float _Split_feba676e1c5e4f709179250fe78fb1bb_A_4 = 0;
            float _Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0 = _fade;
            float4 _UV_e14cf8c76e164723aebfe22b814798b3_Out_0 = IN.uv0;
            float _Split_a54861b234184c03a9499c1168c06c3a_R_1 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[0];
            float _Split_a54861b234184c03a9499c1168c06c3a_G_2 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[1];
            float _Split_a54861b234184c03a9499c1168c06c3a_B_3 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[2];
            float _Split_a54861b234184c03a9499c1168c06c3a_A_4 = _UV_e14cf8c76e164723aebfe22b814798b3_Out_0[3];
            float _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2;
            Unity_Multiply_float(_Property_de3c163d6a884249bead5ad2e4b0fa89_Out_0, _Split_a54861b234184c03a9499c1168c06c3a_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2);
            float _Property_5bf34e958fa5418f96c21e781585c51c_Out_0 = _fade;
            float _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3;
            Unity_Lerp_float(_Split_feba676e1c5e4f709179250fe78fb1bb_G_2, _Multiply_8925ba594e074c09a5ffb93a6f0ab647_Out_2, _Property_5bf34e958fa5418f96c21e781585c51c_Out_0, _Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3);
            float _Property_14f624362d86444fb3dc22672a6eca9f_Out_0 = _opacity;
            float _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
            Unity_Lerp_float(_Lerp_9db127345b514726b5026df3c9cc8fb9_Out_3, 1, _Property_14f624362d86444fb3dc22672a6eca9f_Out_0, _Lerp_9acfe57675d14994af96573154250d0d_Out_3);
            surface.Alpha = _Lerp_9acfe57675d14994af96573154250d0d_Out_3;
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

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.uv0 =                         input.texCoord0;
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}