Shader "CustomShader/CGGen/Unlit/InfToSkyBlenderv2"
{
    Properties
    {
        _ColorFloor("ColorFloor", Color) = (0, 0, 0, 0)
        _ColorSky("ColorSky", Color) = (0, 0, 0, 0)
        _MiddlePoint("MiddlePoint", Range(-100, 400)) = 0
        _Fade("Fade", Range(0, 1)) = 0
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
        float4 _ColorFloor;
        float4 _ColorSky;
        float _MiddlePoint;
        float _Fade;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
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
            float4 _Property_8e127b7d586e43c98c5eaebfce216ede_Out_0 = _ColorSky;
            float4 _Property_41a30041471045439f3dc3148c58d816_Out_0 = _ColorFloor;
            float _Property_8089e8377fef49ee875fa986aacab2b6_Out_0 = _MiddlePoint;
            float4 _UV_2e20f69e14434c93bb1cb6b7350d7385_Out_0 = IN.uv0;
            float _Split_247013a6242b4504975a037c9d873a12_R_1 = _UV_2e20f69e14434c93bb1cb6b7350d7385_Out_0[0];
            float _Split_247013a6242b4504975a037c9d873a12_G_2 = _UV_2e20f69e14434c93bb1cb6b7350d7385_Out_0[1];
            float _Split_247013a6242b4504975a037c9d873a12_B_3 = _UV_2e20f69e14434c93bb1cb6b7350d7385_Out_0[2];
            float _Split_247013a6242b4504975a037c9d873a12_A_4 = _UV_2e20f69e14434c93bb1cb6b7350d7385_Out_0[3];
            float _Multiply_3b01cc14f9904a14932a6e9b561b4285_Out_2;
            Unity_Multiply_float(_Property_8089e8377fef49ee875fa986aacab2b6_Out_0, _Split_247013a6242b4504975a037c9d873a12_G_2, _Multiply_3b01cc14f9904a14932a6e9b561b4285_Out_2);
            float4 _Lerp_bc6004eca6094d269c7a456ff0a5132a_Out_3;
            Unity_Lerp_float4(_Property_8e127b7d586e43c98c5eaebfce216ede_Out_0, _Property_41a30041471045439f3dc3148c58d816_Out_0, (_Multiply_3b01cc14f9904a14932a6e9b561b4285_Out_2.xxxx), _Lerp_bc6004eca6094d269c7a456ff0a5132a_Out_3);
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_R_1 = IN.WorldSpacePosition[0];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2 = IN.WorldSpacePosition[1];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_B_3 = IN.WorldSpacePosition[2];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_A_4 = 0;
            float _Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0 = _MiddlePoint;
            float _Remap_5788c2aebc0148a8acc125d100116178_Out_3;
            Unity_Remap_float(_Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0, float2 (100, 0), float2 (500, 1), _Remap_5788c2aebc0148a8acc125d100116178_Out_3);
            float _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2;
            Unity_Subtract_float(_Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2, _Remap_5788c2aebc0148a8acc125d100116178_Out_3, _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2);
            float _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3;
            Unity_Clamp_float(_Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2, 0, 1, _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3);
            float Slider_aaa9b0ba30044d93b72874a44dd43d15 = 0;
            float _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3;
            Unity_Remap_float(Slider_aaa9b0ba30044d93b72874a44dd43d15, float2 (0, 1), float2 (1, 0), _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3);
            float _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            Unity_Multiply_float(_Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3, _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3, _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2);
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_R_1 = _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_B_3 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_A_4 = 0;
            float _Property_84e56b7c54584a54b01b075bddc2de0e_Out_0 = _MiddlePoint;
            float4 _UV_ab5e333f2999409999b6703d6f61009d_Out_0 = IN.uv0;
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_R_1 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[0];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[1];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_B_3 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[2];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_A_4 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[3];
            float _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2;
            Unity_Multiply_float(_Property_84e56b7c54584a54b01b075bddc2de0e_Out_0, _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2);
            float _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0 = _Fade;
            float _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
            Unity_Lerp_float(_Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2, _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0, _Lerp_3575848615314a9ab24bb697003c63d5_Out_3);
            surface.BaseColor = (_Lerp_bc6004eca6094d269c7a456ff0a5132a_Out_3.xyz);
            surface.Alpha = _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
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
        float4 _ColorFloor;
        float4 _ColorSky;
        float _MiddlePoint;
        float _Fade;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
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
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_R_1 = IN.WorldSpacePosition[0];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2 = IN.WorldSpacePosition[1];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_B_3 = IN.WorldSpacePosition[2];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_A_4 = 0;
            float _Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0 = _MiddlePoint;
            float _Remap_5788c2aebc0148a8acc125d100116178_Out_3;
            Unity_Remap_float(_Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0, float2 (100, 0), float2 (500, 1), _Remap_5788c2aebc0148a8acc125d100116178_Out_3);
            float _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2;
            Unity_Subtract_float(_Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2, _Remap_5788c2aebc0148a8acc125d100116178_Out_3, _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2);
            float _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3;
            Unity_Clamp_float(_Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2, 0, 1, _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3);
            float Slider_aaa9b0ba30044d93b72874a44dd43d15 = 0;
            float _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3;
            Unity_Remap_float(Slider_aaa9b0ba30044d93b72874a44dd43d15, float2 (0, 1), float2 (1, 0), _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3);
            float _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            Unity_Multiply_float(_Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3, _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3, _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2);
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_R_1 = _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_B_3 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_A_4 = 0;
            float _Property_84e56b7c54584a54b01b075bddc2de0e_Out_0 = _MiddlePoint;
            float4 _UV_ab5e333f2999409999b6703d6f61009d_Out_0 = IN.uv0;
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_R_1 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[0];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[1];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_B_3 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[2];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_A_4 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[3];
            float _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2;
            Unity_Multiply_float(_Property_84e56b7c54584a54b01b075bddc2de0e_Out_0, _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2);
            float _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0 = _Fade;
            float _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
            Unity_Lerp_float(_Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2, _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0, _Lerp_3575848615314a9ab24bb697003c63d5_Out_3);
            surface.Alpha = _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
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
        float4 _ColorFloor;
        float4 _ColorSky;
        float _MiddlePoint;
        float _Fade;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
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
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_R_1 = IN.WorldSpacePosition[0];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2 = IN.WorldSpacePosition[1];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_B_3 = IN.WorldSpacePosition[2];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_A_4 = 0;
            float _Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0 = _MiddlePoint;
            float _Remap_5788c2aebc0148a8acc125d100116178_Out_3;
            Unity_Remap_float(_Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0, float2 (100, 0), float2 (500, 1), _Remap_5788c2aebc0148a8acc125d100116178_Out_3);
            float _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2;
            Unity_Subtract_float(_Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2, _Remap_5788c2aebc0148a8acc125d100116178_Out_3, _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2);
            float _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3;
            Unity_Clamp_float(_Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2, 0, 1, _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3);
            float Slider_aaa9b0ba30044d93b72874a44dd43d15 = 0;
            float _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3;
            Unity_Remap_float(Slider_aaa9b0ba30044d93b72874a44dd43d15, float2 (0, 1), float2 (1, 0), _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3);
            float _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            Unity_Multiply_float(_Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3, _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3, _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2);
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_R_1 = _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_B_3 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_A_4 = 0;
            float _Property_84e56b7c54584a54b01b075bddc2de0e_Out_0 = _MiddlePoint;
            float4 _UV_ab5e333f2999409999b6703d6f61009d_Out_0 = IN.uv0;
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_R_1 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[0];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[1];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_B_3 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[2];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_A_4 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[3];
            float _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2;
            Unity_Multiply_float(_Property_84e56b7c54584a54b01b075bddc2de0e_Out_0, _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2);
            float _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0 = _Fade;
            float _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
            Unity_Lerp_float(_Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2, _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0, _Lerp_3575848615314a9ab24bb697003c63d5_Out_3);
            surface.Alpha = _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
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
        float4 _ColorFloor;
        float4 _ColorSky;
        float _MiddlePoint;
        float _Fade;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }

        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
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
            float4 _Property_8e127b7d586e43c98c5eaebfce216ede_Out_0 = _ColorSky;
            float4 _Property_41a30041471045439f3dc3148c58d816_Out_0 = _ColorFloor;
            float _Property_8089e8377fef49ee875fa986aacab2b6_Out_0 = _MiddlePoint;
            float4 _UV_2e20f69e14434c93bb1cb6b7350d7385_Out_0 = IN.uv0;
            float _Split_247013a6242b4504975a037c9d873a12_R_1 = _UV_2e20f69e14434c93bb1cb6b7350d7385_Out_0[0];
            float _Split_247013a6242b4504975a037c9d873a12_G_2 = _UV_2e20f69e14434c93bb1cb6b7350d7385_Out_0[1];
            float _Split_247013a6242b4504975a037c9d873a12_B_3 = _UV_2e20f69e14434c93bb1cb6b7350d7385_Out_0[2];
            float _Split_247013a6242b4504975a037c9d873a12_A_4 = _UV_2e20f69e14434c93bb1cb6b7350d7385_Out_0[3];
            float _Multiply_3b01cc14f9904a14932a6e9b561b4285_Out_2;
            Unity_Multiply_float(_Property_8089e8377fef49ee875fa986aacab2b6_Out_0, _Split_247013a6242b4504975a037c9d873a12_G_2, _Multiply_3b01cc14f9904a14932a6e9b561b4285_Out_2);
            float4 _Lerp_bc6004eca6094d269c7a456ff0a5132a_Out_3;
            Unity_Lerp_float4(_Property_8e127b7d586e43c98c5eaebfce216ede_Out_0, _Property_41a30041471045439f3dc3148c58d816_Out_0, (_Multiply_3b01cc14f9904a14932a6e9b561b4285_Out_2.xxxx), _Lerp_bc6004eca6094d269c7a456ff0a5132a_Out_3);
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_R_1 = IN.WorldSpacePosition[0];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2 = IN.WorldSpacePosition[1];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_B_3 = IN.WorldSpacePosition[2];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_A_4 = 0;
            float _Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0 = _MiddlePoint;
            float _Remap_5788c2aebc0148a8acc125d100116178_Out_3;
            Unity_Remap_float(_Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0, float2 (100, 0), float2 (500, 1), _Remap_5788c2aebc0148a8acc125d100116178_Out_3);
            float _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2;
            Unity_Subtract_float(_Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2, _Remap_5788c2aebc0148a8acc125d100116178_Out_3, _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2);
            float _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3;
            Unity_Clamp_float(_Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2, 0, 1, _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3);
            float Slider_aaa9b0ba30044d93b72874a44dd43d15 = 0;
            float _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3;
            Unity_Remap_float(Slider_aaa9b0ba30044d93b72874a44dd43d15, float2 (0, 1), float2 (1, 0), _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3);
            float _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            Unity_Multiply_float(_Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3, _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3, _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2);
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_R_1 = _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_B_3 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_A_4 = 0;
            float _Property_84e56b7c54584a54b01b075bddc2de0e_Out_0 = _MiddlePoint;
            float4 _UV_ab5e333f2999409999b6703d6f61009d_Out_0 = IN.uv0;
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_R_1 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[0];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[1];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_B_3 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[2];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_A_4 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[3];
            float _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2;
            Unity_Multiply_float(_Property_84e56b7c54584a54b01b075bddc2de0e_Out_0, _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2);
            float _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0 = _Fade;
            float _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
            Unity_Lerp_float(_Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2, _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0, _Lerp_3575848615314a9ab24bb697003c63d5_Out_3);
            surface.BaseColor = (_Lerp_bc6004eca6094d269c7a456ff0a5132a_Out_3.xyz);
            surface.Alpha = _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
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
        float4 _ColorFloor;
        float4 _ColorSky;
        float _MiddlePoint;
        float _Fade;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
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
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_R_1 = IN.WorldSpacePosition[0];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2 = IN.WorldSpacePosition[1];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_B_3 = IN.WorldSpacePosition[2];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_A_4 = 0;
            float _Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0 = _MiddlePoint;
            float _Remap_5788c2aebc0148a8acc125d100116178_Out_3;
            Unity_Remap_float(_Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0, float2 (100, 0), float2 (500, 1), _Remap_5788c2aebc0148a8acc125d100116178_Out_3);
            float _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2;
            Unity_Subtract_float(_Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2, _Remap_5788c2aebc0148a8acc125d100116178_Out_3, _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2);
            float _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3;
            Unity_Clamp_float(_Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2, 0, 1, _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3);
            float Slider_aaa9b0ba30044d93b72874a44dd43d15 = 0;
            float _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3;
            Unity_Remap_float(Slider_aaa9b0ba30044d93b72874a44dd43d15, float2 (0, 1), float2 (1, 0), _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3);
            float _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            Unity_Multiply_float(_Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3, _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3, _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2);
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_R_1 = _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_B_3 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_A_4 = 0;
            float _Property_84e56b7c54584a54b01b075bddc2de0e_Out_0 = _MiddlePoint;
            float4 _UV_ab5e333f2999409999b6703d6f61009d_Out_0 = IN.uv0;
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_R_1 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[0];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[1];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_B_3 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[2];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_A_4 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[3];
            float _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2;
            Unity_Multiply_float(_Property_84e56b7c54584a54b01b075bddc2de0e_Out_0, _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2);
            float _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0 = _Fade;
            float _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
            Unity_Lerp_float(_Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2, _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0, _Lerp_3575848615314a9ab24bb697003c63d5_Out_3);
            surface.Alpha = _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
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
        float4 _ColorFloor;
        float4 _ColorSky;
        float _MiddlePoint;
        float _Fade;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
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
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_R_1 = IN.WorldSpacePosition[0];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2 = IN.WorldSpacePosition[1];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_B_3 = IN.WorldSpacePosition[2];
            float _Split_046f91b035ef48bf8b8ba2a08227dfd3_A_4 = 0;
            float _Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0 = _MiddlePoint;
            float _Remap_5788c2aebc0148a8acc125d100116178_Out_3;
            Unity_Remap_float(_Property_8af78ab9dd7d40fc9596cb36d9b73748_Out_0, float2 (100, 0), float2 (500, 1), _Remap_5788c2aebc0148a8acc125d100116178_Out_3);
            float _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2;
            Unity_Subtract_float(_Split_046f91b035ef48bf8b8ba2a08227dfd3_G_2, _Remap_5788c2aebc0148a8acc125d100116178_Out_3, _Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2);
            float _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3;
            Unity_Clamp_float(_Subtract_cb69cffcc71c4584ad40cb503347f001_Out_2, 0, 1, _Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3);
            float Slider_aaa9b0ba30044d93b72874a44dd43d15 = 0;
            float _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3;
            Unity_Remap_float(Slider_aaa9b0ba30044d93b72874a44dd43d15, float2 (0, 1), float2 (1, 0), _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3);
            float _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            Unity_Multiply_float(_Clamp_c0fb641ae5ef483296bb6fc8f3fcffea_Out_3, _Remap_d4fcf78c286646a1b638f3d2f94cb582_Out_3, _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2);
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_R_1 = _Multiply_c6c0274139b940d898914ebbfa21d4bc_Out_2;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_B_3 = 0;
            float _Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_A_4 = 0;
            float _Property_84e56b7c54584a54b01b075bddc2de0e_Out_0 = _MiddlePoint;
            float4 _UV_ab5e333f2999409999b6703d6f61009d_Out_0 = IN.uv0;
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_R_1 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[0];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[1];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_B_3 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[2];
            float _Split_83f2dfe5cdc6498986ab9c02a4df16e0_A_4 = _UV_ab5e333f2999409999b6703d6f61009d_Out_0[3];
            float _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2;
            Unity_Multiply_float(_Property_84e56b7c54584a54b01b075bddc2de0e_Out_0, _Split_83f2dfe5cdc6498986ab9c02a4df16e0_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2);
            float _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0 = _Fade;
            float _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
            Unity_Lerp_float(_Split_3424f6bf2ef24fd8bb1bd6a7c34ebc41_G_2, _Multiply_f9d562d93cd3479b93f2b696dc0d3b0d_Out_2, _Property_fc4e1f04527b464f87b12cd013ac9d9d_Out_0, _Lerp_3575848615314a9ab24bb697003c63d5_Out_3);
            surface.Alpha = _Lerp_3575848615314a9ab24bb697003c63d5_Out_3;
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