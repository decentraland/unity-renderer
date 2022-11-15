Shader "HLSL/CustomShaders/URP/SGConv/OutlineObject"
{
    Properties
    {
        Vector1_eacf5e5d73a24957b64ff9be4232b8e9("Outline (Pixel)", Float) = 0
        Vector1_42d718e6203d4c6bb33f61bb447249ba("Outline Fade", Range(-1.5, 1)) = 0.5
        [HDR]_OutlineColor("Outline Color", Color) = (4, 1.539547, 0, 0)
        _OutlineOpacity("OutlineOpacity", Range(0, 1)) = 0
        Vector1_d0d90240ab69433a9d9c08a5c9e80a31("CameraFarPlaneFade", Float) = 0
        Color_d326f9101fd94f2b8ca8431c31e97ef6("FillColor", Color) = (0, 0, 0, 0)
        Vector1_255b438dff5e475da252ca15498d846d("FillOpacity", Range(0.3, 1)) = 1
        [ToggleUI]_isOutlineEnabled("isOutlineEnabled", Float) = 0
        [ToggleUI]_isOutlineEnabled_1("isFillEnabled", Float) = 0
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
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
            
//            Blend[_SrcBlend][_DstBlend]
//            ZWrite[_ZWrite]
//            Cull[_Cull]
            
            //ZWrite On
            //ZTest LEqual
            ColorMask 0
            Cull[_Cull]

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
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
        #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_FORWARD
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float3 normalWS;
            float4 tangentWS;
            float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 sh;
            #endif
            float4 fogFactorAndVertexLight;
            float4 shadowCoord;
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
            float3 TangentSpaceNormal;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
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
            float3 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
            float3 interp3 : TEXCOORD3;
            #if defined(LIGHTMAP_ON)
            float2 interp4 : TEXCOORD4;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 interp5 : TEXCOORD5;
            #endif
            float4 interp6 : TEXCOORD6;
            float4 interp7 : TEXCOORD7;
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
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp4.xy =  input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp5.xyz =  input.sh;
            #endif
            output.interp6.xyzw =  input.fogFactorAndVertexLight;
            output.interp7.xyzw =  input.shadowCoord;
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
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.viewDirectionWS = input.interp3.xyz;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.interp4.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp5.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp6.xyzw;
            output.shadowCoord = input.interp7.xyzw;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_550c83ba40b043bb858c8536cf7d60a3_Out_0 = _isOutlineEnabled_1;
            float4 _Property_2e648cf98e494065861b36d2d8779a21_Out_0 = Color_d326f9101fd94f2b8ca8431c31e97ef6;
            float4 _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3;
            Unity_Branch_float4(_Property_550c83ba40b043bb858c8536cf7d60a3_Out_0, _Property_2e648cf98e494065861b36d2d8779a21_Out_0, float4(0, 0, 0, 0), _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3);
            float _Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float4 _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2;
            Unity_Multiply_float(_Branch_0db834dae9444ee09bc2260c444c1c97_Out_3, (_Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0.xxxx), _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2);
            float _Property_171fc7588f4f4e698cde0c6e747792d6_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float _Property_b343b793b67d4099b44487e9222ad82e_Out_0 = _OutlineOpacity;
            float4 _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2;
            Unity_Multiply_float(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, (_Property_b343b793b67d4099b44487e9222ad82e_Out_0.xxxx), _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2);
            float4 _Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3;
            Unity_Branch_float4(_Property_171fc7588f4f4e698cde0c6e747792d6_Out_0, _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2, float4(0, 0, 0, 0), _Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3);
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.BaseColor = (_Multiply_c11991fca47f4ffca467dcec31b28860_Out_2.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = (_Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3.xyz);
            surface.Metallic = 0;
            surface.Smoothness = 0.5;
            surface.Occlusion = 1;
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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



            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
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
        //#pragma multi_compile_fog
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile _ _GBUFFER_NORMALS_OCT
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_GBUFFER
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float3 normalWS;
            float4 tangentWS;
            float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 sh;
            #endif
            float4 fogFactorAndVertexLight;
            float4 shadowCoord;
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
            float3 TangentSpaceNormal;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
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
            float3 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
            float3 interp3 : TEXCOORD3;
            #if defined(LIGHTMAP_ON)
            float2 interp4 : TEXCOORD4;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 interp5 : TEXCOORD5;
            #endif
            float4 interp6 : TEXCOORD6;
            float4 interp7 : TEXCOORD7;
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
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp4.xy =  input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp5.xyz =  input.sh;
            #endif
            output.interp6.xyzw =  input.fogFactorAndVertexLight;
            output.interp7.xyzw =  input.shadowCoord;
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
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.viewDirectionWS = input.interp3.xyz;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.interp4.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp5.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp6.xyzw;
            output.shadowCoord = input.interp7.xyzw;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_550c83ba40b043bb858c8536cf7d60a3_Out_0 = _isOutlineEnabled_1;
            float4 _Property_2e648cf98e494065861b36d2d8779a21_Out_0 = Color_d326f9101fd94f2b8ca8431c31e97ef6;
            float4 _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3;
            Unity_Branch_float4(_Property_550c83ba40b043bb858c8536cf7d60a3_Out_0, _Property_2e648cf98e494065861b36d2d8779a21_Out_0, float4(0, 0, 0, 0), _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3);
            float _Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float4 _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2;
            Unity_Multiply_float(_Branch_0db834dae9444ee09bc2260c444c1c97_Out_3, (_Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0.xxxx), _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2);
            float _Property_171fc7588f4f4e698cde0c6e747792d6_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float _Property_b343b793b67d4099b44487e9222ad82e_Out_0 = _OutlineOpacity;
            float4 _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2;
            Unity_Multiply_float(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, (_Property_b343b793b67d4099b44487e9222ad82e_Out_0.xxxx), _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2);
            float4 _Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3;
            Unity_Branch_float4(_Property_171fc7588f4f4e698cde0c6e747792d6_Out_0, _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2, float4(0, 0, 0, 0), _Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3);
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.BaseColor = (_Multiply_c11991fca47f4ffca467dcec31b28860_Out_2.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = (_Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3.xyz);
            surface.Metallic = 0;
            surface.Smoothness = 0.5;
            surface.Occlusion = 1;
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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



            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl"

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
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define VARYINGS_NEED_POSITION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define REQUIRE_DEPTH_TEXTURE
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
            float4 ScreenPosition;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define VARYINGS_NEED_POSITION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
        #define REQUIRE_DEPTH_TEXTURE
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
            float4 ScreenPosition;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

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
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
        #define REQUIRE_DEPTH_TEXTURE
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float3 normalWS;
            float4 tangentWS;
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
            float3 TangentSpaceNormal;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
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
            float3 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
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
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
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
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float3 NormalTS;
            float Alpha;
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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



            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            // Render State
            Cull Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define VARYINGS_NEED_POSITION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_META
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
            float4 ScreenPosition;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float3 Emission;
            float Alpha;
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_550c83ba40b043bb858c8536cf7d60a3_Out_0 = _isOutlineEnabled_1;
            float4 _Property_2e648cf98e494065861b36d2d8779a21_Out_0 = Color_d326f9101fd94f2b8ca8431c31e97ef6;
            float4 _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3;
            Unity_Branch_float4(_Property_550c83ba40b043bb858c8536cf7d60a3_Out_0, _Property_2e648cf98e494065861b36d2d8779a21_Out_0, float4(0, 0, 0, 0), _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3);
            float _Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float4 _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2;
            Unity_Multiply_float(_Branch_0db834dae9444ee09bc2260c444c1c97_Out_3, (_Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0.xxxx), _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2);
            float _Property_171fc7588f4f4e698cde0c6e747792d6_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float _Property_b343b793b67d4099b44487e9222ad82e_Out_0 = _OutlineOpacity;
            float4 _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2;
            Unity_Multiply_float(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, (_Property_b343b793b67d4099b44487e9222ad82e_Out_0.xxxx), _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2);
            float4 _Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3;
            Unity_Branch_float4(_Property_171fc7588f4f4e698cde0c6e747792d6_Out_0, _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2, float4(0, 0, 0, 0), _Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3);
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.BaseColor = (_Multiply_c11991fca47f4ffca467dcec31b28860_Out_2.xyz);
            surface.Emission = (_Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3.xyz);
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            // Name: <None>
            Tags
            {
                "LightMode" = "Universal2D"
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
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define VARYINGS_NEED_POSITION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_2D
        #define REQUIRE_DEPTH_TEXTURE
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
            float4 ScreenPosition;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_550c83ba40b043bb858c8536cf7d60a3_Out_0 = _isOutlineEnabled_1;
            float4 _Property_2e648cf98e494065861b36d2d8779a21_Out_0 = Color_d326f9101fd94f2b8ca8431c31e97ef6;
            float4 _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3;
            Unity_Branch_float4(_Property_550c83ba40b043bb858c8536cf7d60a3_Out_0, _Property_2e648cf98e494065861b36d2d8779a21_Out_0, float4(0, 0, 0, 0), _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3);
            float _Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float4 _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2;
            Unity_Multiply_float(_Branch_0db834dae9444ee09bc2260c444c1c97_Out_3, (_Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0.xxxx), _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2);
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.BaseColor = (_Multiply_c11991fca47f4ffca467dcec31b28860_Out_2.xyz);
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"

            ENDHLSL
        }
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
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
        //#pragma multi_compile_fog
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
        #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_FORWARD
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float3 normalWS;
            float4 tangentWS;
            float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            float2 lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 sh;
            #endif
            float4 fogFactorAndVertexLight;
            float4 shadowCoord;
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
            float3 TangentSpaceNormal;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
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
            float3 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
            float3 interp3 : TEXCOORD3;
            #if defined(LIGHTMAP_ON)
            float2 interp4 : TEXCOORD4;
            #endif
            #if !defined(LIGHTMAP_ON)
            float3 interp5 : TEXCOORD5;
            #endif
            float4 interp6 : TEXCOORD6;
            float4 interp7 : TEXCOORD7;
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
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp4.xy =  input.lightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp5.xyz =  input.sh;
            #endif
            output.interp6.xyzw =  input.fogFactorAndVertexLight;
            output.interp7.xyzw =  input.shadowCoord;
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
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.viewDirectionWS = input.interp3.xyz;
            #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.interp4.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp5.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp6.xyzw;
            output.shadowCoord = input.interp7.xyzw;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_550c83ba40b043bb858c8536cf7d60a3_Out_0 = _isOutlineEnabled_1;
            float4 _Property_2e648cf98e494065861b36d2d8779a21_Out_0 = Color_d326f9101fd94f2b8ca8431c31e97ef6;
            float4 _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3;
            Unity_Branch_float4(_Property_550c83ba40b043bb858c8536cf7d60a3_Out_0, _Property_2e648cf98e494065861b36d2d8779a21_Out_0, float4(0, 0, 0, 0), _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3);
            float _Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float4 _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2;
            Unity_Multiply_float(_Branch_0db834dae9444ee09bc2260c444c1c97_Out_3, (_Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0.xxxx), _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2);
            float _Property_171fc7588f4f4e698cde0c6e747792d6_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float _Property_b343b793b67d4099b44487e9222ad82e_Out_0 = _OutlineOpacity;
            float4 _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2;
            Unity_Multiply_float(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, (_Property_b343b793b67d4099b44487e9222ad82e_Out_0.xxxx), _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2);
            float4 _Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3;
            Unity_Branch_float4(_Property_171fc7588f4f4e698cde0c6e747792d6_Out_0, _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2, float4(0, 0, 0, 0), _Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3);
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.BaseColor = (_Multiply_c11991fca47f4ffca467dcec31b28860_Out_2.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = (_Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3.xyz);
            surface.Metallic = 0;
            surface.Smoothness = 0.5;
            surface.Occlusion = 1;
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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



            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

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
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define VARYINGS_NEED_POSITION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define REQUIRE_DEPTH_TEXTURE
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
            float4 ScreenPosition;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define VARYINGS_NEED_POSITION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
        #define REQUIRE_DEPTH_TEXTURE
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
            float4 ScreenPosition;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

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
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
        #define REQUIRE_DEPTH_TEXTURE
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float3 normalWS;
            float4 tangentWS;
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
            float3 TangentSpaceNormal;
            float3 WorldSpacePosition;
            float4 ScreenPosition;
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
            float3 interp1 : TEXCOORD1;
            float4 interp2 : TEXCOORD2;
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
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
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
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float3 NormalTS;
            float Alpha;
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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



            output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);


            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            // Render State
            Cull Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define VARYINGS_NEED_POSITION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_META
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv1 : TEXCOORD1;
            float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
            float4 ScreenPosition;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float3 Emission;
            float Alpha;
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_550c83ba40b043bb858c8536cf7d60a3_Out_0 = _isOutlineEnabled_1;
            float4 _Property_2e648cf98e494065861b36d2d8779a21_Out_0 = Color_d326f9101fd94f2b8ca8431c31e97ef6;
            float4 _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3;
            Unity_Branch_float4(_Property_550c83ba40b043bb858c8536cf7d60a3_Out_0, _Property_2e648cf98e494065861b36d2d8779a21_Out_0, float4(0, 0, 0, 0), _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3);
            float _Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float4 _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2;
            Unity_Multiply_float(_Branch_0db834dae9444ee09bc2260c444c1c97_Out_3, (_Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0.xxxx), _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2);
            float _Property_171fc7588f4f4e698cde0c6e747792d6_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float _Property_b343b793b67d4099b44487e9222ad82e_Out_0 = _OutlineOpacity;
            float4 _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2;
            Unity_Multiply_float(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, (_Property_b343b793b67d4099b44487e9222ad82e_Out_0.xxxx), _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2);
            float4 _Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3;
            Unity_Branch_float4(_Property_171fc7588f4f4e698cde0c6e747792d6_Out_0, _Multiply_e9e1556517134f34a8d4158e4d4c61cb_Out_2, float4(0, 0, 0, 0), _Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3);
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.BaseColor = (_Multiply_c11991fca47f4ffca467dcec31b28860_Out_2.xyz);
            surface.Emission = (_Branch_a4fd9f454c114161a224dce3adeb24ce_Out_3.xyz);
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            // Name: <None>
            Tags
            {
                "LightMode" = "Universal2D"
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
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _AlphaClip 1
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define VARYINGS_NEED_POSITION_WS
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_2D
        #define REQUIRE_DEPTH_TEXTURE
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
            float4 ScreenPosition;
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
        float Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
        float Vector1_42d718e6203d4c6bb33f61bb447249ba;
        float4 _OutlineColor;
        float _OutlineOpacity;
        float Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
        float4 Color_d326f9101fd94f2b8ca8431c31e97ef6;
        float Vector1_255b438dff5e475da252ca15498d846d;
        float _isOutlineEnabled;
        float _isOutlineEnabled_1;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
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

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
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

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void MultiBranch_float(float Case1, float Input1, float Case2, float Input2, float Case3, float Input3, out float Out){
            if(Case1 == true )
            {
                Out = Input1;
            }

            if(Case1 == false )
            {
                Out = Input2;
            }

            if(Case2 == true)
            {
                Out = Input3;
            }


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
            float AlphaClipThreshold;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_550c83ba40b043bb858c8536cf7d60a3_Out_0 = _isOutlineEnabled_1;
            float4 _Property_2e648cf98e494065861b36d2d8779a21_Out_0 = Color_d326f9101fd94f2b8ca8431c31e97ef6;
            float4 _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3;
            Unity_Branch_float4(_Property_550c83ba40b043bb858c8536cf7d60a3_Out_0, _Property_2e648cf98e494065861b36d2d8779a21_Out_0, float4(0, 0, 0, 0), _Branch_0db834dae9444ee09bc2260c444c1c97_Out_3);
            float _Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float4 _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2;
            Unity_Multiply_float(_Branch_0db834dae9444ee09bc2260c444c1c97_Out_3, (_Property_c63a641c71fc421aa7c2e3bb3f5d3a25_Out_0.xxxx), _Multiply_c11991fca47f4ffca467dcec31b28860_Out_2);
            float _Property_06388718d66b4594b7b751ba7bd2093a_Out_0 = _isOutlineEnabled;
            float4 _Property_83642bf497ea4c568b380bd3876986b9_Out_0 = IsGammaSpace() ? LinearToSRGB(_OutlineColor) : _OutlineColor;
            float4 _ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1;
            Unity_Absolute_float4(_ScreenPosition_2cf07fdb84c444f387e20b148b23f7d1_Out_0, _Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1);
            float4 _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2;
            Unity_Add_float4(_Absolute_25f36f6d637d4826a1133e81a6175dc4_Out_1, float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2);
            float Slider_8b40d4a2a0644469b79f4b3170e2027b = 1;
            float4 _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Add_18902f7b2b47417aa06b4b5bed78590a_Out_2, (Slider_8b40d4a2a0644469b79f4b3170e2027b.xxxx), _Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3);
            float _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1;
            Unity_SceneDepth_Linear01_float(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1);
            float _Property_22b18c745a76403987ad4334d7a6c884_Out_0 = Vector1_eacf5e5d73a24957b64ff9be4232b8e9;
            float _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.x, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2);
            float _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2;
            Unity_Divide_float(_Property_22b18c745a76403987ad4334d7a6c884_Out_0, _ScreenParams.y, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2);
            float4 _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_c0dca0e0b61f434e9acb7d2ddeddaf6c_Out_0, _Add_6892f7e42c014f77b6dbf26c86a70584_Out_2);
            float _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_6892f7e42c014f77b6dbf26c86a70584_Out_2, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1);
            float _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_42e2bb347da44f128cf6da269e8eeab7_Out_1, _Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2);
            float _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1;
            Unity_Absolute_float(_Subtract_7116277fb5fb4b49a8cd83888c59d527_Out_2, _Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1);
            float _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2;
            Unity_Subtract_float(0, _Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2);
            float _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2;
            Unity_Subtract_float(0, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2);
            float4 _Vector4_85230910efeb48cdab1b253e1a840414_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_85230910efeb48cdab1b253e1a840414_Out_0, _Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2);
            float _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_fea32fbb9b924595be48a3ff4bf392dc_Out_2, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1);
            float _Subtract_25a433401aec403fb62ba411b1304b13_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_43dfca494d9a49e7b70c6e03f72e7e8b_Out_1, _Subtract_25a433401aec403fb62ba411b1304b13_Out_2);
            float _Absolute_d2fab04d1fe8422e814299f506597589_Out_1;
            Unity_Absolute_float(_Subtract_25a433401aec403fb62ba411b1304b13_Out_2, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1);
            float _Add_1781a1bcd160462fbb9d8d577b565010_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Absolute_d2fab04d1fe8422e814299f506597589_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2);
            float _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2;
            Unity_Add_float(_Absolute_895d45d01b2b4796a55cc80960a1ccec_Out_1, _Add_1781a1bcd160462fbb9d8d577b565010_Out_2, _Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2);
            float4 _Vector4_900a76d61708419fb6021c84b708d84f_Out_0 = float4(_Divide_6260bd01d6ab43a0812b9f1575e55e60_Out_2, _Subtract_d41d3c33ee534d39a66645cf71525325_Out_2, 0, 0);
            float4 _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_900a76d61708419fb6021c84b708d84f_Out_0, _Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2);
            float _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_45f98ee681804fa4aeb0bfeba96a3ecd_Out_2, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1);
            float _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_fe3b2ac421de49d5bc1433a687cde2d9_Out_1, _Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2);
            float _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1;
            Unity_Absolute_float(_Subtract_bde1b356ab024e99ba2d6fc0fd55c2f5_Out_2, _Absolute_a0de308946b745c3a85077e2c604da7e_Out_1);
            float4 _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0 = float4(_Subtract_f8d2280b63ab44c08772cedc888ede42_Out_2, _Divide_0f6cefc62ec44f02b62a805bcc51171b_Out_2, 0, 0);
            float4 _Add_c7b5c51a73674631ac99988258917202_Out_2;
            Unity_Add_float4(_Lerp_125659c7e70c4331a882ad2c8cab8aa5_Out_3, _Vector4_e1e48ba1e16f413192d63ee65ce68b5d_Out_0, _Add_c7b5c51a73674631ac99988258917202_Out_2);
            float _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1;
            Unity_SceneDepth_Linear01_float(_Add_c7b5c51a73674631ac99988258917202_Out_2, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1);
            float _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2;
            Unity_Subtract_float(_SceneDepth_7c52a8a1028e4b489ff3512e6ec8e67b_Out_1, _SceneDepth_9a2e6360ea2f42d7a32c5cd811d3357e_Out_1, _Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2);
            float _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1;
            Unity_Absolute_float(_Subtract_ea6ff47d77df4baebf0b0f5d57eb6162_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1);
            float _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2;
            Unity_Add_float(_Absolute_a0de308946b745c3a85077e2c604da7e_Out_1, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2);
            float _Add_800b0db2c1c84983930e7a438b6cac95_Out_2;
            Unity_Add_float(_Add_ab85d9f45a2f435880bd6d9d4bb54362_Out_2, _Absolute_c2581a81b89a407aacebd7dd93e7a00f_Out_1, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2);
            float _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2;
            Unity_Add_float(_Add_f15bc499f78a4aac8dc2eb1eebefa2d9_Out_2, _Add_800b0db2c1c84983930e7a438b6cac95_Out_2, _Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2);
            float _Property_598ca99daef74fb6ac0ed190a1469550_Out_0 = Vector1_d0d90240ab69433a9d9c08a5c9e80a31;
            float _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2;
            Unity_Divide_float(_ProjectionParams.z, _Property_598ca99daef74fb6ac0ed190a1469550_Out_0, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2);
            float _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2;
            Unity_Multiply_float(_Add_dc4b260adfac4db8a2f50c49f0a2bae4_Out_2, _Divide_113c8b743e0244eeb555e646fd4f5388_Out_2, _Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2);
            float _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1);
            float _Property_1d659b85e2254ad88d02374d222c038d_Out_0 = Vector1_42d718e6203d4c6bb33f61bb447249ba;
            float _Add_612835ad21974cc7a5da97963ec40fcb_Out_2;
            Unity_Add_float(_Property_1d659b85e2254ad88d02374d222c038d_Out_0, 1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2);
            float _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2;
            Unity_Power_float(_SceneDepth_3a2d346088744aeb8aa52dd1f37250c1_Out_1, _Add_612835ad21974cc7a5da97963ec40fcb_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2);
            float _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2;
            Unity_Divide_float(_Multiply_03e4fe9939f849fb9368990bcbc9a1e7_Out_2, _Power_e43d32d24a62496fbb2ab741c3c8cdfa_Out_2, _Divide_e4251240cb534f4c869a32c9654b99d4_Out_2);
            float _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2;
            Unity_Comparison_GreaterOrEqual_float(_Divide_e4251240cb534f4c869a32c9654b99d4_Out_2, 0.01, _Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2);
            float _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3;
            Unity_Branch_float(_Comparison_ebf1348a796747aaa88194edd80bd7b5_Out_2, 1, 0, _Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3);
            float4 _Lerp_1f9c78919c754001b429deb504b23d71_Out_3;
            Unity_Lerp_float4(float4(0, 0, 0, 0), _Property_83642bf497ea4c568b380bd3876986b9_Out_0, (_Branch_d3a2de2ff3bd4f0bbdf5bc45aab30153_Out_3.xxxx), _Lerp_1f9c78919c754001b429deb504b23d71_Out_3);
            float4 _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1;
            Unity_OneMinus_float4(_Lerp_1f9c78919c754001b429deb504b23d71_Out_3, _OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1);
            float4 _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1;
            Unity_OneMinus_float4(_OneMinus_f11ecae5b4fa43fcbac35b3bc1420361_Out_1, _OneMinus_c873738cfc33466d8921c7b436c62409_Out_1);
            float _Property_7589d370c41e4be483d935b3660b6951_Out_0 = _isOutlineEnabled_1;
            float _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0 = 0;
            float _Float_e46f9d93edec49fa82450821c4313327_Out_0 = 1;
            float _Property_39773efc545a45998dc93fa4ca781db2_Out_0 = Vector1_255b438dff5e475da252ca15498d846d;
            float _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2;
            Unity_Multiply_float(_Float_e46f9d93edec49fa82450821c4313327_Out_0, _Property_39773efc545a45998dc93fa4ca781db2_Out_0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2);
            float _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            MultiBranch_float(_Property_06388718d66b4594b7b751ba7bd2093a_Out_0, (_OneMinus_c873738cfc33466d8921c7b436c62409_Out_1).x, _Property_7589d370c41e4be483d935b3660b6951_Out_0, _Float_c55f62d44f3a48fa966eece3536ca5d9_Out_0, 0, _Multiply_303c391ab2634ae9ab75cd51b7d12839_Out_2, _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4);
            surface.BaseColor = (_Multiply_c11991fca47f4ffca467dcec31b28860_Out_2.xyz);
            surface.Alpha = _MultiBranchCustomFunction_7606472de7b044c780095f17c340449a_Out_4;
            surface.AlphaClipThreshold = 0.3;
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
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"

            ENDHLSL
        }
    }
    CustomEditor "ShaderGraph.PBRMasterGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}