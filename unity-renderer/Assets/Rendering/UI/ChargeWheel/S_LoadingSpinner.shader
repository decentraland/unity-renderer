Shader "S_LoadingSpinner"
{
    Properties
    {
        _color01("Color01", Color) = (1, 0, 0.2605209, 1)
        _fillHead("FillHead", Range(0, 1)) = 0.5
        _fillTail("FillTail", Range(0, 1)) = 0.1
        Vector1_3862b5470ce94ed68d6ccd8d5aa57614("Thickness", Range(0, 1)) = 0.15
        [NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
        [KeywordEnum(Sharp, Rounded)]ENUM_173FCA8E617C4725B6502C6FBE756CEC("RoundedCorners", Float) = 1

        [HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask("Color Mask", Float) = 15
    }
        SubShader
        {
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "RenderType" = "Transparent"
                "UniversalMaterialType" = "Unlit"
                "Queue" = "Transparent"
            }
            Pass
            {
                Name "Sprite Unlit"
                Tags
                {
                    "LightMode" = "Universal2D"
                }

            // Render State
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            ZTest[unity_GUIZTestMode]
            ZWrite Off

            Stencil
            {
              Ref[_Stencil]
              Comp[_StencilComp]
              Pass[_StencilOp]
              ReadMask[_StencilReadMask]
              WriteMask[_StencilWriteMask]
            }
            ColorMask[_ColorMask]

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            #pragma shader_feature_local ENUM_173FCA8E617C4725B6502C6FBE756CEC_SHARP ENUM_173FCA8E617C4725B6502C6FBE756CEC_ROUNDED

        #if defined(ENUM_173FCA8E617C4725B6502C6FBE756CEC_SHARP)
            #define KEYWORD_PERMUTATION_0
        #else
            #define KEYWORD_PERMUTATION_1
        #endif


            // Defines
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        #define _SURFACE_TYPE_TRANSPARENT 1
        #endif

        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        #define _AlphaClip 1
        #endif

        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        #define ATTRIBUTES_NEED_NORMAL
        #endif

        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        #define ATTRIBUTES_NEED_TANGENT
        #endif

        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        #define ATTRIBUTES_NEED_TEXCOORD0
        #endif

        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        #define ATTRIBUTES_NEED_COLOR
        #endif

        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        #define VARYINGS_NEED_TEXCOORD0
        #endif

        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        #define VARYINGS_NEED_COLOR
        #endif

            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SPRITEUNLIT
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
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float3 positionOS : POSITION;
            #endif
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float3 normalOS : NORMAL;
            #endif
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float4 tangentOS : TANGENT;
            #endif
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float4 uv0 : TEXCOORD0;
            #endif
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float4 color : COLOR;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
            #endif
        };
        struct Varyings
        {
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float4 positionCS : SV_POSITION;
            #endif
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float4 texCoord0;
            #endif
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float4 color;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float4 uv0;
            #endif
        };
        struct VertexDescriptionInputs
        {
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float3 ObjectSpaceNormal;
            #endif
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float3 ObjectSpaceTangent;
            #endif
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float3 ObjectSpacePosition;
            #endif
        };
        struct PackedVaryings
        {
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float4 positionCS : SV_POSITION;
            #endif
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float4 interp0 : TEXCOORD0;
            #endif
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            float4 interp1 : TEXCOORD1;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
            #endif
        };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        PackedVaryings PackVaryings(Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyzw = input.texCoord0;
            output.interp1.xyzw = input.color;
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
        Varyings UnpackVaryings(PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
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
        #endif

        // --------------------------------------------------
        // Graph

        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
    float4 _color01;
    float _fillHead;
    float _fillTail;
    float Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    float4 _MainTex_TexelSize;
    CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        // Graph Functions

    void Unity_Clamp_float(float In, float Min, float Max, out float Out)
    {
        Out = clamp(In, Min, Max);
    }

    void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
    {
        float2 delta = UV - Center;
        float radius = length(delta) * 2 * RadialScale;
        float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
        Out = float2(radius, angle);
    }

    void Unity_Subtract_float(float A, float B, out float Out)
    {
        Out = A - B;
    }

    void Unity_DDXY_float(float In, out float Out)
    {
        Out = abs(ddx(In)) + abs(ddy(In));
    }

    void Unity_Multiply_float(float A, float B, out float Out)
    {
        Out = A * B;
    }

    void Unity_Subtract_float4(float4 A, float4 B, out float4 Out)
    {
        Out = A - B;
    }

    void Unity_Add_float4(float4 A, float4 B, out float4 Out)
    {
        Out = A + B;
    }

    void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
    {
        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
    }

    void Unity_Saturate_float(float In, out float Out)
    {
        Out = saturate(In);
    }

    void Unity_OneMinus_float(float In, out float Out)
    {
        Out = 1 - In;
    }

    void Unity_Add_float(float A, float B, out float Out)
    {
        Out = A + B;
    }

    void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
    {
        //rotation matrix
        Rotation = Rotation * (3.1415926f / 180.0f);
        UV -= Center;
        float s = sin(Rotation);
        float c = cos(Rotation);

        //center rotation matrix
        float2x2 rMatrix = float2x2(c, -s, s, c);
        rMatrix *= 0.5;
        rMatrix += 0.5;
        rMatrix = rMatrix * 2 - 1;

        //multiply the UVs by the rotation matrix
        UV.xy = mul(UV.xy, rMatrix);
        UV += Center;

        Out = UV;
    }

    void Unity_Branch_float(float Predicate, float True, float False, out float Out)
    {
        Out = Predicate ? True : False;
    }

    void Unity_Floor_float(float In, out float Out)
    {
        Out = floor(In);
    }

    struct Bindings_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30
    {
        half4 uv0;
    };

    void SG_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30(float Vector1_aaadcc587b384367b08909fb525fb26d, float Vector1_3e1c52735b2e4d27a6e7bf56ebb50e52, float Boolean_af78085877e144a8b493499b7e0f1c46, Bindings_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30 IN, out float4 Out_1)
    {
        float _Property_137f50ba36ae418a929202d699bc499e_Out_0 = Vector1_3e1c52735b2e4d27a6e7bf56ebb50e52;
        float _Property_cf25c1bf1067470b95529f4d38f87846_Out_0 = Boolean_af78085877e144a8b493499b7e0f1c46;
        float _Property_63367afbdb2d448eb8aff4a786030cf0_Out_0 = Vector1_aaadcc587b384367b08909fb525fb26d;
        float _Add_24434867cef447769badb4bb3879b3b3_Out_2;
        Unity_Add_float(_Property_63367afbdb2d448eb8aff4a786030cf0_Out_0, 0.5, _Add_24434867cef447769badb4bb3879b3b3_Out_2);
        float2 _Rotate_6c4160bf69a94b7d8e745a7ef35dde6c_Out_3;
        Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_6c4160bf69a94b7d8e745a7ef35dde6c_Out_3);
        float _Split_8e18d6de9cfa49498f69e811db060b6f_R_1 = _Rotate_6c4160bf69a94b7d8e745a7ef35dde6c_Out_3[0];
        float _Split_8e18d6de9cfa49498f69e811db060b6f_G_2 = _Rotate_6c4160bf69a94b7d8e745a7ef35dde6c_Out_3[1];
        float _Split_8e18d6de9cfa49498f69e811db060b6f_B_3 = 0;
        float _Split_8e18d6de9cfa49498f69e811db060b6f_A_4 = 0;
        float _OneMinus_5fc0773156204c808a58e9c19e812934_Out_1;
        Unity_OneMinus_float(_Split_8e18d6de9cfa49498f69e811db060b6f_R_1, _OneMinus_5fc0773156204c808a58e9c19e812934_Out_1);
        float2 _Vector2_f6556f360a284dbab1d148851cdff07d_Out_0 = float2(_OneMinus_5fc0773156204c808a58e9c19e812934_Out_1, _Split_8e18d6de9cfa49498f69e811db060b6f_G_2);
        float2 _PolarCoordinates_75b99fc82bd84e6a884dddd32e22933e_Out_4;
        Unity_PolarCoordinates_float(_Vector2_f6556f360a284dbab1d148851cdff07d_Out_0, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_75b99fc82bd84e6a884dddd32e22933e_Out_4);
        float _Split_bf54a59e944c4401bfb24aa58d76e76d_R_1 = _PolarCoordinates_75b99fc82bd84e6a884dddd32e22933e_Out_4[0];
        float _Split_bf54a59e944c4401bfb24aa58d76e76d_G_2 = _PolarCoordinates_75b99fc82bd84e6a884dddd32e22933e_Out_4[1];
        float _Split_bf54a59e944c4401bfb24aa58d76e76d_B_3 = 0;
        float _Split_bf54a59e944c4401bfb24aa58d76e76d_A_4 = 0;
        float _Add_097bd1547e3047758ec904d203391d0b_Out_2;
        Unity_Add_float(_Add_24434867cef447769badb4bb3879b3b3_Out_2, _Split_bf54a59e944c4401bfb24aa58d76e76d_G_2, _Add_097bd1547e3047758ec904d203391d0b_Out_2);
        float _Subtract_2be3d3659cc24a4693b18e30af9d213f_Out_2;
        Unity_Subtract_float(_Add_24434867cef447769badb4bb3879b3b3_Out_2, _Split_bf54a59e944c4401bfb24aa58d76e76d_G_2, _Subtract_2be3d3659cc24a4693b18e30af9d213f_Out_2);
        float _Branch_9aba1e17f8de4a378062933b4cdf5437_Out_3;
        Unity_Branch_float(_Property_cf25c1bf1067470b95529f4d38f87846_Out_0, _Add_097bd1547e3047758ec904d203391d0b_Out_2, _Subtract_2be3d3659cc24a4693b18e30af9d213f_Out_2, _Branch_9aba1e17f8de4a378062933b4cdf5437_Out_3);
        float _Floor_df80cb102d1a41f8b565c41a47b2449f_Out_1;
        Unity_Floor_float(_Branch_9aba1e17f8de4a378062933b4cdf5437_Out_3, _Floor_df80cb102d1a41f8b565c41a47b2449f_Out_1);
        float _Multiply_7ef19861976b442aaae43e822cb07c40_Out_2;
        Unity_Multiply_float(_Property_137f50ba36ae418a929202d699bc499e_Out_0, _Floor_df80cb102d1a41f8b565c41a47b2449f_Out_1, _Multiply_7ef19861976b442aaae43e822cb07c40_Out_2);
        Out_1 = (_Multiply_7ef19861976b442aaae43e822cb07c40_Out_2.xxxx);
    }

    void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
    {
        Out = A * B;
    }

    void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
    {
        Out = UV * Tiling + Offset;
    }

    void Unity_Ellipse_float(float2 UV, float Width, float Height, out float Out)
    {
        float d = length((UV * 2 - 1) / float2(Width, Height));
        Out = saturate((1 - d) / fwidth(d));
    }

    struct Bindings_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6
    {
        half4 uv0;
    };

    void SG_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6(float Vector1_d021bfa5510841c68b0da5eb35c7ef57, float Vector1_001a62ee7d39438b8ddee32fa34d4381, Bindings_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6 IN, out float Out_1)
    {
        float2 _Rotate_399a3128b83c4274ab44c43c14ab0b42_Out_3;
        Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_399a3128b83c4274ab44c43c14ab0b42_Out_3);
        float _Split_c99ae83b143c4a7e91459920f997ddd5_R_1 = _Rotate_399a3128b83c4274ab44c43c14ab0b42_Out_3[0];
        float _Split_c99ae83b143c4a7e91459920f997ddd5_G_2 = _Rotate_399a3128b83c4274ab44c43c14ab0b42_Out_3[1];
        float _Split_c99ae83b143c4a7e91459920f997ddd5_B_3 = 0;
        float _Split_c99ae83b143c4a7e91459920f997ddd5_A_4 = 0;
        float _OneMinus_71f3a86901e74ade9cab784b480a2d19_Out_1;
        Unity_OneMinus_float(_Split_c99ae83b143c4a7e91459920f997ddd5_R_1, _OneMinus_71f3a86901e74ade9cab784b480a2d19_Out_1);
        float2 _Vector2_b7857123941e4b308bcc03df889a8cc0_Out_0 = float2(_OneMinus_71f3a86901e74ade9cab784b480a2d19_Out_1, _Split_c99ae83b143c4a7e91459920f997ddd5_G_2);
        float _Property_a817896828c949108c7a90e03f6d1087_Out_0 = Vector1_001a62ee7d39438b8ddee32fa34d4381;
        float _Multiply_e6786901258743b1ad600175b42585d7_Out_2;
        Unity_Multiply_float(_Property_a817896828c949108c7a90e03f6d1087_Out_0, 360, _Multiply_e6786901258743b1ad600175b42585d7_Out_2);
        float2 _Rotate_7861e59713c84421bf662882f24d3ee7_Out_3;
        Unity_Rotate_Degrees_float(_Vector2_b7857123941e4b308bcc03df889a8cc0_Out_0, float2 (0.5, 0.5), _Multiply_e6786901258743b1ad600175b42585d7_Out_2, _Rotate_7861e59713c84421bf662882f24d3ee7_Out_3);
        float _Property_f58f50f1f6444b6ab7cba6ced2ca75d1_Out_0 = Vector1_d021bfa5510841c68b0da5eb35c7ef57;
        float _Multiply_77728f00d9a74e8b9d190cc8317e7599_Out_2;
        Unity_Multiply_float(_Property_f58f50f1f6444b6ab7cba6ced2ca75d1_Out_0, 0.5, _Multiply_77728f00d9a74e8b9d190cc8317e7599_Out_2);
        float _Multiply_35757b763d77403b82a191b6a91b17e9_Out_2;
        Unity_Multiply_float(_Multiply_77728f00d9a74e8b9d190cc8317e7599_Out_2, -0.5, _Multiply_35757b763d77403b82a191b6a91b17e9_Out_2);
        float _Add_6cb4a86d30b1423e9b6b75ce71a3dcbf_Out_2;
        Unity_Add_float(0.5, _Multiply_35757b763d77403b82a191b6a91b17e9_Out_2, _Add_6cb4a86d30b1423e9b6b75ce71a3dcbf_Out_2);
        float2 _Vector2_67c3058fbedd474980b5c708bba63abf_Out_0 = float2(0, _Add_6cb4a86d30b1423e9b6b75ce71a3dcbf_Out_2);
        float2 _TilingAndOffset_1df6dd5b61a34af79c740672d363c60f_Out_3;
        Unity_TilingAndOffset_float(_Rotate_7861e59713c84421bf662882f24d3ee7_Out_3, float2 (1, 1), _Vector2_67c3058fbedd474980b5c708bba63abf_Out_0, _TilingAndOffset_1df6dd5b61a34af79c740672d363c60f_Out_3);
        float _Ellipse_cd0353e235654f4c8d74a95959501f93_Out_4;
        Unity_Ellipse_float(_TilingAndOffset_1df6dd5b61a34af79c740672d363c60f_Out_3, _Multiply_77728f00d9a74e8b9d190cc8317e7599_Out_2, _Multiply_77728f00d9a74e8b9d190cc8317e7599_Out_2, _Ellipse_cd0353e235654f4c8d74a95959501f93_Out_4);
        Out_1 = _Ellipse_cd0353e235654f4c8d74a95959501f93_Out_4;
    }

    void Unity_Saturate_float4(float4 In, out float4 Out)
    {
        Out = saturate(In);
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
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Property_59090694f6954a52b5e6153e0ee8c1be_Out_0 = _color01;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_59fe34c75d414e64ad726de1e5f63fe1_R_1 = _Property_59090694f6954a52b5e6153e0ee8c1be_Out_0[0];
    float _Split_59fe34c75d414e64ad726de1e5f63fe1_G_2 = _Property_59090694f6954a52b5e6153e0ee8c1be_Out_0[1];
    float _Split_59fe34c75d414e64ad726de1e5f63fe1_B_3 = _Property_59090694f6954a52b5e6153e0ee8c1be_Out_0[2];
    float _Split_59fe34c75d414e64ad726de1e5f63fe1_A_4 = _Property_59090694f6954a52b5e6153e0ee8c1be_Out_0[3];
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_7ff1c03310924ee3836050c91f7b71e3_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_2b45fd59e1d745beaf0ef81a8b18a41c_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Clamp_1d1e5e4c10634750b75bbbd0035a27f5_Out_3;
    Unity_Clamp_float(_Property_7ff1c03310924ee3836050c91f7b71e3_Out_0, _Property_2b45fd59e1d745beaf0ef81a8b18a41c_Out_0, 1, _Clamp_1d1e5e4c10634750b75bbbd0035a27f5_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _PolarCoordinates_841781112cf84bd389805bd6983afcd8_Out_4;
    Unity_PolarCoordinates_float(IN.uv0.xy, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_841781112cf84bd389805bd6983afcd8_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_9bbf972d48324f92ae19d0f282d8e0f0_R_1 = _PolarCoordinates_841781112cf84bd389805bd6983afcd8_Out_4[0];
    float _Split_9bbf972d48324f92ae19d0f282d8e0f0_G_2 = _PolarCoordinates_841781112cf84bd389805bd6983afcd8_Out_4[1];
    float _Split_9bbf972d48324f92ae19d0f282d8e0f0_B_3 = 0;
    float _Split_9bbf972d48324f92ae19d0f282d8e0f0_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Float_cead0e992bc449df8d9dce54181339c6_Out_0 = 1;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_3520cd1cbed7442db074fe761994f38d_Out_0 = Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Subtract_0f65a36c972844a3900a158bb05718ec_Out_2;
    Unity_Subtract_float(_Float_cead0e992bc449df8d9dce54181339c6_Out_0, _Property_3520cd1cbed7442db074fe761994f38d_Out_0, _Subtract_0f65a36c972844a3900a158bb05718ec_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _PolarCoordinates_fa4cbfeb86174bee9fb41d7108aa458f_Out_4;
    Unity_PolarCoordinates_float(IN.uv0.xy, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_fa4cbfeb86174bee9fb41d7108aa458f_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_c24ddeeafb2b43df852a13ffb9baae4f_R_1 = _PolarCoordinates_fa4cbfeb86174bee9fb41d7108aa458f_Out_4[0];
    float _Split_c24ddeeafb2b43df852a13ffb9baae4f_G_2 = _PolarCoordinates_fa4cbfeb86174bee9fb41d7108aa458f_Out_4[1];
    float _Split_c24ddeeafb2b43df852a13ffb9baae4f_B_3 = 0;
    float _Split_c24ddeeafb2b43df852a13ffb9baae4f_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _DDXY_6da06c77dad94e40803df8ba82245303_Out_1;
    Unity_DDXY_float(_Split_c24ddeeafb2b43df852a13ffb9baae4f_R_1, _DDXY_6da06c77dad94e40803df8ba82245303_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2;
    Unity_Multiply_float(_DDXY_6da06c77dad94e40803df8ba82245303_Out_1, 0.5, _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2;
    Unity_Subtract_float4((_Subtract_0f65a36c972844a3900a158bb05718ec_Out_2.xxxx), (_Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2.xxxx), _Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Add_1434307ef67b463a952ae990b52d0f19_Out_2;
    Unity_Add_float4((_Subtract_0f65a36c972844a3900a158bb05718ec_Out_2.xxxx), (_Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2.xxxx), _Add_1434307ef67b463a952ae990b52d0f19_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_441455d41e894365b4544ab412ff17da_Out_0 = float2((_Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2).x, (_Add_1434307ef67b463a952ae990b52d0f19_Out_2).x);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Remap_44ff9bb3e0c44baaab52e11fb31882ad_Out_3;
    Unity_Remap_float(_Split_9bbf972d48324f92ae19d0f282d8e0f0_R_1, _Vector2_441455d41e894365b4544ab412ff17da_Out_0, float2 (0, 1), _Remap_44ff9bb3e0c44baaab52e11fb31882ad_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Saturate_2ce89ed81cfa4d09bbffbd0af1bc9409_Out_1;
    Unity_Saturate_float(_Remap_44ff9bb3e0c44baaab52e11fb31882ad_Out_3, _Saturate_2ce89ed81cfa4d09bbffbd0af1bc9409_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2;
    Unity_Subtract_float4((_Float_cead0e992bc449df8d9dce54181339c6_Out_0.xxxx), (_Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2.xxxx), _Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Add_61ac46313d39426f9d62990bead131b2_Out_2;
    Unity_Add_float4((_Float_cead0e992bc449df8d9dce54181339c6_Out_0.xxxx), (_Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2.xxxx), _Add_61ac46313d39426f9d62990bead131b2_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_6fd7dbbacf17437ba05862d85169dead_Out_0 = float2((_Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2).x, (_Add_61ac46313d39426f9d62990bead131b2_Out_2).x);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Remap_03df3856ba5c44f79b7a13415f56162f_Out_3;
    Unity_Remap_float(_Split_9bbf972d48324f92ae19d0f282d8e0f0_R_1, _Vector2_6fd7dbbacf17437ba05862d85169dead_Out_0, float2 (0, 1), _Remap_03df3856ba5c44f79b7a13415f56162f_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Saturate_5bff97ab53984a47857bc3b35beee2ea_Out_1;
    Unity_Saturate_float(_Remap_03df3856ba5c44f79b7a13415f56162f_Out_3, _Saturate_5bff97ab53984a47857bc3b35beee2ea_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_602397fa45324aa89e7b3f2a2f985338_Out_1;
    Unity_OneMinus_float(_Saturate_5bff97ab53984a47857bc3b35beee2ea_Out_1, _OneMinus_602397fa45324aa89e7b3f2a2f985338_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2;
    Unity_Multiply_float(_Saturate_2ce89ed81cfa4d09bbffbd0af1bc9409_Out_1, _OneMinus_602397fa45324aa89e7b3f2a2f985338_Out_1, _Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    Bindings_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30 _SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262;
    _SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262.uv0 = IN.uv0;
    float4 _SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262_Out_1;
    SG_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30(_Clamp_1d1e5e4c10634750b75bbbd0035a27f5_Out_3, _Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2, 1, _SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262, _SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_a5d5680cb3a94bbf86bdedb7384a8d8a_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_672711a1ef644f24803c57c3177839b3_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Clamp_1b1e0ae952a3401cbd0d0c6d77cb181d_Out_3;
    Unity_Clamp_float(_Property_a5d5680cb3a94bbf86bdedb7384a8d8a_Out_0, 0, _Property_672711a1ef644f24803c57c3177839b3_Out_0, _Clamp_1b1e0ae952a3401cbd0d0c6d77cb181d_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1;
    Unity_OneMinus_float(_Clamp_1b1e0ae952a3401cbd0d0c6d77cb181d_Out_3, _OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    Bindings_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30 _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11;
    _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11.uv0 = IN.uv0;
    float4 _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11_Out_1;
    SG_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30(_OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1, _Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2, 0, _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11, _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2;
    Unity_Multiply_float(_SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262_Out_1, _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11_Out_1, _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_5468e17a0c1f4b3e8c6ba37d7812cc28_Out_0 = Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_5c413e8f88bb4463a98c98139a9312a9_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_cd882648aaa447d898c35d6ca457b138_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Clamp_8e127e83ba944d0e82ed5247cfde69fc_Out_3;
    Unity_Clamp_float(_Property_5c413e8f88bb4463a98c98139a9312a9_Out_0, _Property_cd882648aaa447d898c35d6ca457b138_Out_0, 1, _Clamp_8e127e83ba944d0e82ed5247cfde69fc_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    Bindings_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6 _SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6;
    _SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6.uv0 = IN.uv0;
    float _SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6_Out_1;
    SG_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6(_Property_5468e17a0c1f4b3e8c6ba37d7812cc28_Out_0, _Clamp_8e127e83ba944d0e82ed5247cfde69fc_Out_3, _SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6, _SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Add_9afa425d6b4a4484b424ad62e647beac_Out_2;
    Unity_Add_float4(_Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2, (_SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6_Out_1.xxxx), _Add_9afa425d6b4a4484b424ad62e647beac_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_b709b96791b94b8b9ff4dd5b2d8f9b5a_Out_0 = Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_d3c5646ff5704d228e6a26b2d3f98752_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_21976e95748248d39a26b4d2639a344b_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Clamp_0751c1483d864290943e5ff2f83c0582_Out_3;
    Unity_Clamp_float(_Property_d3c5646ff5704d228e6a26b2d3f98752_Out_0, 0, _Property_21976e95748248d39a26b4d2639a344b_Out_0, _Clamp_0751c1483d864290943e5ff2f83c0582_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    Bindings_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6 _SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b;
    _SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b.uv0 = IN.uv0;
    float _SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b_Out_1;
    SG_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6(_Property_b709b96791b94b8b9ff4dd5b2d8f9b5a_Out_0, _Clamp_0751c1483d864290943e5ff2f83c0582_Out_3, _SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b, _SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2;
    Unity_Add_float4(_Add_9afa425d6b4a4484b424ad62e647beac_Out_2, (_SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b_Out_1.xxxx), _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #if defined(ENUM_173FCA8E617C4725B6502C6FBE756CEC_SHARP)
    float4 _RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0 = _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2;
    #else
    float4 _RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0 = _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2;
    #endif
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Saturate_737696eb34c241d6b897441762787779_Out_1;
    Unity_Saturate_float4(_RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0, _Saturate_737696eb34c241d6b897441762787779_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    UnityTexture2D _Property_92b3b90fb1c84c859c075f236c70e399_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0 = SAMPLE_TEXTURE2D(_Property_92b3b90fb1c84c859c075f236c70e399_Out_0.tex, _Property_92b3b90fb1c84c859c075f236c70e399_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_db4059a956484545adfbe855ead6e884_R_4 = _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0.r;
    float _SampleTexture2D_db4059a956484545adfbe855ead6e884_G_5 = _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0.g;
    float _SampleTexture2D_db4059a956484545adfbe855ead6e884_B_6 = _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0.b;
    float _SampleTexture2D_db4059a956484545adfbe855ead6e884_A_7 = _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0.a;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Multiply_83aae9860a96466b9d32a13ab9801b27_Out_2;
    Unity_Multiply_float(_Saturate_737696eb34c241d6b897441762787779_Out_1, _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0, _Multiply_83aae9860a96466b9d32a13ab9801b27_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Multiply_095c15108eac4b039bd50b69d7042144_Out_2;
    Unity_Multiply_float((_Split_59fe34c75d414e64ad726de1e5f63fe1_A_4.xxxx), _Multiply_83aae9860a96466b9d32a13ab9801b27_Out_2, _Multiply_095c15108eac4b039bd50b69d7042144_Out_2);
    #endif
    surface.BaseColor = (_Property_59090694f6954a52b5e6153e0ee8c1be_Out_0.xyz);
    surface.Alpha = (_Multiply_095c15108eac4b039bd50b69d7042144_Out_2).x;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

#if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
output.ObjectSpaceNormal = input.normalOS;
#endif

#if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
output.ObjectSpaceTangent = input.tangentOS.xyz;
#endif

#if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
output.ObjectSpacePosition = input.positionOS;
#endif


    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





#if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
output.uv0 = input.texCoord0;
#endif

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
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SpriteUnlitPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "Sprite Unlit"
    Tags
    {
        "LightMode" = "UniversalForward"
    }

        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest[unity_GUIZTestMode]
        ZWrite Off

        Stencil
        {
          Ref[_Stencil]
          Comp[_StencilComp]
          Pass[_StencilOp]
          ReadMask[_StencilReadMask]
          WriteMask[_StencilWriteMask]
        }
        ColorMask[_ColorMask]

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma exclude_renderers d3d11_9x
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        #pragma shader_feature_local ENUM_173FCA8E617C4725B6502C6FBE756CEC_SHARP ENUM_173FCA8E617C4725B6502C6FBE756CEC_ROUNDED

    #if defined(ENUM_173FCA8E617C4725B6502C6FBE756CEC_SHARP)
        #define KEYWORD_PERMUTATION_0
    #else
        #define KEYWORD_PERMUTATION_1
    #endif


        // Defines
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #define _SURFACE_TYPE_TRANSPARENT 1
    #endif

    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #define _AlphaClip 1
    #endif

    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #define ATTRIBUTES_NEED_NORMAL
    #endif

    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #define ATTRIBUTES_NEED_TANGENT
    #endif

    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #define ATTRIBUTES_NEED_TEXCOORD0
    #endif

    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #define ATTRIBUTES_NEED_COLOR
    #endif

    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #define VARYINGS_NEED_TEXCOORD0
    #endif

    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #define VARYINGS_NEED_COLOR
    #endif

        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SPRITEFORWARD
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
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float3 positionOS : POSITION;
        #endif
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float3 normalOS : NORMAL;
        #endif
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float4 tangentOS : TANGENT;
        #endif
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float4 uv0 : TEXCOORD0;
        #endif
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float4 color : COLOR;
        #endif
        #if UNITY_ANY_INSTANCING_ENABLED
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
        #endif
    };
    struct Varyings
    {
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float4 positionCS : SV_POSITION;
        #endif
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float4 texCoord0;
        #endif
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float4 color;
        #endif
        #if UNITY_ANY_INSTANCING_ENABLED
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float4 uv0;
        #endif
    };
    struct VertexDescriptionInputs
    {
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float3 ObjectSpaceNormal;
        #endif
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float3 ObjectSpaceTangent;
        #endif
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float3 ObjectSpacePosition;
        #endif
    };
    struct PackedVaryings
    {
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float4 positionCS : SV_POSITION;
        #endif
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float4 interp0 : TEXCOORD0;
        #endif
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        float4 interp1 : TEXCOORD1;
        #endif
        #if UNITY_ANY_INSTANCING_ENABLED
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
        #endif
    };

        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyzw = input.texCoord0;
        output.interp1.xyzw = input.color;
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
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.texCoord0 = input.interp0.xyzw;
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
    #endif

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 _color01;
float _fillHead;
float _fillTail;
float Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
float4 _MainTex_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

// Graph Functions

void Unity_Clamp_float(float In, float Min, float Max, out float Out)
{
    Out = clamp(In, Min, Max);
}

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_DDXY_float(float In, out float Out)
{
    Out = abs(ddx(In)) + abs(ddy(In));
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Subtract_float4(float4 A, float4 B, out float4 Out)
{
    Out = A - B;
}

void Unity_Add_float4(float4 A, float4 B, out float4 Out)
{
    Out = A + B;
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    //rotation matrix
    Rotation = Rotation * (3.1415926f / 180.0f);
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;

    Out = UV;
}

void Unity_Branch_float(float Predicate, float True, float False, out float Out)
{
    Out = Predicate ? True : False;
}

void Unity_Floor_float(float In, out float Out)
{
    Out = floor(In);
}

struct Bindings_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30
{
    half4 uv0;
};

void SG_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30(float Vector1_aaadcc587b384367b08909fb525fb26d, float Vector1_3e1c52735b2e4d27a6e7bf56ebb50e52, float Boolean_af78085877e144a8b493499b7e0f1c46, Bindings_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30 IN, out float4 Out_1)
{
    float _Property_137f50ba36ae418a929202d699bc499e_Out_0 = Vector1_3e1c52735b2e4d27a6e7bf56ebb50e52;
    float _Property_cf25c1bf1067470b95529f4d38f87846_Out_0 = Boolean_af78085877e144a8b493499b7e0f1c46;
    float _Property_63367afbdb2d448eb8aff4a786030cf0_Out_0 = Vector1_aaadcc587b384367b08909fb525fb26d;
    float _Add_24434867cef447769badb4bb3879b3b3_Out_2;
    Unity_Add_float(_Property_63367afbdb2d448eb8aff4a786030cf0_Out_0, 0.5, _Add_24434867cef447769badb4bb3879b3b3_Out_2);
    float2 _Rotate_6c4160bf69a94b7d8e745a7ef35dde6c_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_6c4160bf69a94b7d8e745a7ef35dde6c_Out_3);
    float _Split_8e18d6de9cfa49498f69e811db060b6f_R_1 = _Rotate_6c4160bf69a94b7d8e745a7ef35dde6c_Out_3[0];
    float _Split_8e18d6de9cfa49498f69e811db060b6f_G_2 = _Rotate_6c4160bf69a94b7d8e745a7ef35dde6c_Out_3[1];
    float _Split_8e18d6de9cfa49498f69e811db060b6f_B_3 = 0;
    float _Split_8e18d6de9cfa49498f69e811db060b6f_A_4 = 0;
    float _OneMinus_5fc0773156204c808a58e9c19e812934_Out_1;
    Unity_OneMinus_float(_Split_8e18d6de9cfa49498f69e811db060b6f_R_1, _OneMinus_5fc0773156204c808a58e9c19e812934_Out_1);
    float2 _Vector2_f6556f360a284dbab1d148851cdff07d_Out_0 = float2(_OneMinus_5fc0773156204c808a58e9c19e812934_Out_1, _Split_8e18d6de9cfa49498f69e811db060b6f_G_2);
    float2 _PolarCoordinates_75b99fc82bd84e6a884dddd32e22933e_Out_4;
    Unity_PolarCoordinates_float(_Vector2_f6556f360a284dbab1d148851cdff07d_Out_0, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_75b99fc82bd84e6a884dddd32e22933e_Out_4);
    float _Split_bf54a59e944c4401bfb24aa58d76e76d_R_1 = _PolarCoordinates_75b99fc82bd84e6a884dddd32e22933e_Out_4[0];
    float _Split_bf54a59e944c4401bfb24aa58d76e76d_G_2 = _PolarCoordinates_75b99fc82bd84e6a884dddd32e22933e_Out_4[1];
    float _Split_bf54a59e944c4401bfb24aa58d76e76d_B_3 = 0;
    float _Split_bf54a59e944c4401bfb24aa58d76e76d_A_4 = 0;
    float _Add_097bd1547e3047758ec904d203391d0b_Out_2;
    Unity_Add_float(_Add_24434867cef447769badb4bb3879b3b3_Out_2, _Split_bf54a59e944c4401bfb24aa58d76e76d_G_2, _Add_097bd1547e3047758ec904d203391d0b_Out_2);
    float _Subtract_2be3d3659cc24a4693b18e30af9d213f_Out_2;
    Unity_Subtract_float(_Add_24434867cef447769badb4bb3879b3b3_Out_2, _Split_bf54a59e944c4401bfb24aa58d76e76d_G_2, _Subtract_2be3d3659cc24a4693b18e30af9d213f_Out_2);
    float _Branch_9aba1e17f8de4a378062933b4cdf5437_Out_3;
    Unity_Branch_float(_Property_cf25c1bf1067470b95529f4d38f87846_Out_0, _Add_097bd1547e3047758ec904d203391d0b_Out_2, _Subtract_2be3d3659cc24a4693b18e30af9d213f_Out_2, _Branch_9aba1e17f8de4a378062933b4cdf5437_Out_3);
    float _Floor_df80cb102d1a41f8b565c41a47b2449f_Out_1;
    Unity_Floor_float(_Branch_9aba1e17f8de4a378062933b4cdf5437_Out_3, _Floor_df80cb102d1a41f8b565c41a47b2449f_Out_1);
    float _Multiply_7ef19861976b442aaae43e822cb07c40_Out_2;
    Unity_Multiply_float(_Property_137f50ba36ae418a929202d699bc499e_Out_0, _Floor_df80cb102d1a41f8b565c41a47b2449f_Out_1, _Multiply_7ef19861976b442aaae43e822cb07c40_Out_2);
    Out_1 = (_Multiply_7ef19861976b442aaae43e822cb07c40_Out_2.xxxx);
}

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Ellipse_float(float2 UV, float Width, float Height, out float Out)
{
    float d = length((UV * 2 - 1) / float2(Width, Height));
    Out = saturate((1 - d) / fwidth(d));
}

struct Bindings_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6
{
    half4 uv0;
};

void SG_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6(float Vector1_d021bfa5510841c68b0da5eb35c7ef57, float Vector1_001a62ee7d39438b8ddee32fa34d4381, Bindings_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6 IN, out float Out_1)
{
    float2 _Rotate_399a3128b83c4274ab44c43c14ab0b42_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_399a3128b83c4274ab44c43c14ab0b42_Out_3);
    float _Split_c99ae83b143c4a7e91459920f997ddd5_R_1 = _Rotate_399a3128b83c4274ab44c43c14ab0b42_Out_3[0];
    float _Split_c99ae83b143c4a7e91459920f997ddd5_G_2 = _Rotate_399a3128b83c4274ab44c43c14ab0b42_Out_3[1];
    float _Split_c99ae83b143c4a7e91459920f997ddd5_B_3 = 0;
    float _Split_c99ae83b143c4a7e91459920f997ddd5_A_4 = 0;
    float _OneMinus_71f3a86901e74ade9cab784b480a2d19_Out_1;
    Unity_OneMinus_float(_Split_c99ae83b143c4a7e91459920f997ddd5_R_1, _OneMinus_71f3a86901e74ade9cab784b480a2d19_Out_1);
    float2 _Vector2_b7857123941e4b308bcc03df889a8cc0_Out_0 = float2(_OneMinus_71f3a86901e74ade9cab784b480a2d19_Out_1, _Split_c99ae83b143c4a7e91459920f997ddd5_G_2);
    float _Property_a817896828c949108c7a90e03f6d1087_Out_0 = Vector1_001a62ee7d39438b8ddee32fa34d4381;
    float _Multiply_e6786901258743b1ad600175b42585d7_Out_2;
    Unity_Multiply_float(_Property_a817896828c949108c7a90e03f6d1087_Out_0, 360, _Multiply_e6786901258743b1ad600175b42585d7_Out_2);
    float2 _Rotate_7861e59713c84421bf662882f24d3ee7_Out_3;
    Unity_Rotate_Degrees_float(_Vector2_b7857123941e4b308bcc03df889a8cc0_Out_0, float2 (0.5, 0.5), _Multiply_e6786901258743b1ad600175b42585d7_Out_2, _Rotate_7861e59713c84421bf662882f24d3ee7_Out_3);
    float _Property_f58f50f1f6444b6ab7cba6ced2ca75d1_Out_0 = Vector1_d021bfa5510841c68b0da5eb35c7ef57;
    float _Multiply_77728f00d9a74e8b9d190cc8317e7599_Out_2;
    Unity_Multiply_float(_Property_f58f50f1f6444b6ab7cba6ced2ca75d1_Out_0, 0.5, _Multiply_77728f00d9a74e8b9d190cc8317e7599_Out_2);
    float _Multiply_35757b763d77403b82a191b6a91b17e9_Out_2;
    Unity_Multiply_float(_Multiply_77728f00d9a74e8b9d190cc8317e7599_Out_2, -0.5, _Multiply_35757b763d77403b82a191b6a91b17e9_Out_2);
    float _Add_6cb4a86d30b1423e9b6b75ce71a3dcbf_Out_2;
    Unity_Add_float(0.5, _Multiply_35757b763d77403b82a191b6a91b17e9_Out_2, _Add_6cb4a86d30b1423e9b6b75ce71a3dcbf_Out_2);
    float2 _Vector2_67c3058fbedd474980b5c708bba63abf_Out_0 = float2(0, _Add_6cb4a86d30b1423e9b6b75ce71a3dcbf_Out_2);
    float2 _TilingAndOffset_1df6dd5b61a34af79c740672d363c60f_Out_3;
    Unity_TilingAndOffset_float(_Rotate_7861e59713c84421bf662882f24d3ee7_Out_3, float2 (1, 1), _Vector2_67c3058fbedd474980b5c708bba63abf_Out_0, _TilingAndOffset_1df6dd5b61a34af79c740672d363c60f_Out_3);
    float _Ellipse_cd0353e235654f4c8d74a95959501f93_Out_4;
    Unity_Ellipse_float(_TilingAndOffset_1df6dd5b61a34af79c740672d363c60f_Out_3, _Multiply_77728f00d9a74e8b9d190cc8317e7599_Out_2, _Multiply_77728f00d9a74e8b9d190cc8317e7599_Out_2, _Ellipse_cd0353e235654f4c8d74a95959501f93_Out_4);
    Out_1 = _Ellipse_cd0353e235654f4c8d74a95959501f93_Out_4;
}

void Unity_Saturate_float4(float4 In, out float4 Out)
{
    Out = saturate(In);
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
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Property_59090694f6954a52b5e6153e0ee8c1be_Out_0 = _color01;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_59fe34c75d414e64ad726de1e5f63fe1_R_1 = _Property_59090694f6954a52b5e6153e0ee8c1be_Out_0[0];
    float _Split_59fe34c75d414e64ad726de1e5f63fe1_G_2 = _Property_59090694f6954a52b5e6153e0ee8c1be_Out_0[1];
    float _Split_59fe34c75d414e64ad726de1e5f63fe1_B_3 = _Property_59090694f6954a52b5e6153e0ee8c1be_Out_0[2];
    float _Split_59fe34c75d414e64ad726de1e5f63fe1_A_4 = _Property_59090694f6954a52b5e6153e0ee8c1be_Out_0[3];
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_7ff1c03310924ee3836050c91f7b71e3_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_2b45fd59e1d745beaf0ef81a8b18a41c_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Clamp_1d1e5e4c10634750b75bbbd0035a27f5_Out_3;
    Unity_Clamp_float(_Property_7ff1c03310924ee3836050c91f7b71e3_Out_0, _Property_2b45fd59e1d745beaf0ef81a8b18a41c_Out_0, 1, _Clamp_1d1e5e4c10634750b75bbbd0035a27f5_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _PolarCoordinates_841781112cf84bd389805bd6983afcd8_Out_4;
    Unity_PolarCoordinates_float(IN.uv0.xy, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_841781112cf84bd389805bd6983afcd8_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_9bbf972d48324f92ae19d0f282d8e0f0_R_1 = _PolarCoordinates_841781112cf84bd389805bd6983afcd8_Out_4[0];
    float _Split_9bbf972d48324f92ae19d0f282d8e0f0_G_2 = _PolarCoordinates_841781112cf84bd389805bd6983afcd8_Out_4[1];
    float _Split_9bbf972d48324f92ae19d0f282d8e0f0_B_3 = 0;
    float _Split_9bbf972d48324f92ae19d0f282d8e0f0_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Float_cead0e992bc449df8d9dce54181339c6_Out_0 = 1;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_3520cd1cbed7442db074fe761994f38d_Out_0 = Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Subtract_0f65a36c972844a3900a158bb05718ec_Out_2;
    Unity_Subtract_float(_Float_cead0e992bc449df8d9dce54181339c6_Out_0, _Property_3520cd1cbed7442db074fe761994f38d_Out_0, _Subtract_0f65a36c972844a3900a158bb05718ec_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _PolarCoordinates_fa4cbfeb86174bee9fb41d7108aa458f_Out_4;
    Unity_PolarCoordinates_float(IN.uv0.xy, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_fa4cbfeb86174bee9fb41d7108aa458f_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_c24ddeeafb2b43df852a13ffb9baae4f_R_1 = _PolarCoordinates_fa4cbfeb86174bee9fb41d7108aa458f_Out_4[0];
    float _Split_c24ddeeafb2b43df852a13ffb9baae4f_G_2 = _PolarCoordinates_fa4cbfeb86174bee9fb41d7108aa458f_Out_4[1];
    float _Split_c24ddeeafb2b43df852a13ffb9baae4f_B_3 = 0;
    float _Split_c24ddeeafb2b43df852a13ffb9baae4f_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _DDXY_6da06c77dad94e40803df8ba82245303_Out_1;
    Unity_DDXY_float(_Split_c24ddeeafb2b43df852a13ffb9baae4f_R_1, _DDXY_6da06c77dad94e40803df8ba82245303_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2;
    Unity_Multiply_float(_DDXY_6da06c77dad94e40803df8ba82245303_Out_1, 0.5, _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2;
    Unity_Subtract_float4((_Subtract_0f65a36c972844a3900a158bb05718ec_Out_2.xxxx), (_Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2.xxxx), _Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Add_1434307ef67b463a952ae990b52d0f19_Out_2;
    Unity_Add_float4((_Subtract_0f65a36c972844a3900a158bb05718ec_Out_2.xxxx), (_Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2.xxxx), _Add_1434307ef67b463a952ae990b52d0f19_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_441455d41e894365b4544ab412ff17da_Out_0 = float2((_Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2).x, (_Add_1434307ef67b463a952ae990b52d0f19_Out_2).x);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Remap_44ff9bb3e0c44baaab52e11fb31882ad_Out_3;
    Unity_Remap_float(_Split_9bbf972d48324f92ae19d0f282d8e0f0_R_1, _Vector2_441455d41e894365b4544ab412ff17da_Out_0, float2 (0, 1), _Remap_44ff9bb3e0c44baaab52e11fb31882ad_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Saturate_2ce89ed81cfa4d09bbffbd0af1bc9409_Out_1;
    Unity_Saturate_float(_Remap_44ff9bb3e0c44baaab52e11fb31882ad_Out_3, _Saturate_2ce89ed81cfa4d09bbffbd0af1bc9409_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2;
    Unity_Subtract_float4((_Float_cead0e992bc449df8d9dce54181339c6_Out_0.xxxx), (_Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2.xxxx), _Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Add_61ac46313d39426f9d62990bead131b2_Out_2;
    Unity_Add_float4((_Float_cead0e992bc449df8d9dce54181339c6_Out_0.xxxx), (_Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2.xxxx), _Add_61ac46313d39426f9d62990bead131b2_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_6fd7dbbacf17437ba05862d85169dead_Out_0 = float2((_Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2).x, (_Add_61ac46313d39426f9d62990bead131b2_Out_2).x);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Remap_03df3856ba5c44f79b7a13415f56162f_Out_3;
    Unity_Remap_float(_Split_9bbf972d48324f92ae19d0f282d8e0f0_R_1, _Vector2_6fd7dbbacf17437ba05862d85169dead_Out_0, float2 (0, 1), _Remap_03df3856ba5c44f79b7a13415f56162f_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Saturate_5bff97ab53984a47857bc3b35beee2ea_Out_1;
    Unity_Saturate_float(_Remap_03df3856ba5c44f79b7a13415f56162f_Out_3, _Saturate_5bff97ab53984a47857bc3b35beee2ea_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_602397fa45324aa89e7b3f2a2f985338_Out_1;
    Unity_OneMinus_float(_Saturate_5bff97ab53984a47857bc3b35beee2ea_Out_1, _OneMinus_602397fa45324aa89e7b3f2a2f985338_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2;
    Unity_Multiply_float(_Saturate_2ce89ed81cfa4d09bbffbd0af1bc9409_Out_1, _OneMinus_602397fa45324aa89e7b3f2a2f985338_Out_1, _Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    Bindings_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30 _SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262;
    _SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262.uv0 = IN.uv0;
    float4 _SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262_Out_1;
    SG_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30(_Clamp_1d1e5e4c10634750b75bbbd0035a27f5_Out_3, _Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2, 1, _SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262, _SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_a5d5680cb3a94bbf86bdedb7384a8d8a_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_672711a1ef644f24803c57c3177839b3_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Clamp_1b1e0ae952a3401cbd0d0c6d77cb181d_Out_3;
    Unity_Clamp_float(_Property_a5d5680cb3a94bbf86bdedb7384a8d8a_Out_0, 0, _Property_672711a1ef644f24803c57c3177839b3_Out_0, _Clamp_1b1e0ae952a3401cbd0d0c6d77cb181d_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1;
    Unity_OneMinus_float(_Clamp_1b1e0ae952a3401cbd0d0c6d77cb181d_Out_3, _OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    Bindings_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30 _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11;
    _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11.uv0 = IN.uv0;
    float4 _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11_Out_1;
    SG_SGChargeBarFillMasking_862d3a251357eeb4990975ddf0630a30(_OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1, _Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2, 0, _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11, _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2;
    Unity_Multiply_float(_SGChargeBarFillMasking_b4fa74a9347244ab8961fb96f1975262_Out_1, _SGChargeBarFillMasking_1aa102a0524747de9ca143c68f7c0f11_Out_1, _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_5468e17a0c1f4b3e8c6ba37d7812cc28_Out_0 = Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_5c413e8f88bb4463a98c98139a9312a9_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_cd882648aaa447d898c35d6ca457b138_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Clamp_8e127e83ba944d0e82ed5247cfde69fc_Out_3;
    Unity_Clamp_float(_Property_5c413e8f88bb4463a98c98139a9312a9_Out_0, _Property_cd882648aaa447d898c35d6ca457b138_Out_0, 1, _Clamp_8e127e83ba944d0e82ed5247cfde69fc_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    Bindings_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6 _SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6;
    _SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6.uv0 = IN.uv0;
    float _SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6_Out_1;
    SG_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6(_Property_5468e17a0c1f4b3e8c6ba37d7812cc28_Out_0, _Clamp_8e127e83ba944d0e82ed5247cfde69fc_Out_3, _SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6, _SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Add_9afa425d6b4a4484b424ad62e647beac_Out_2;
    Unity_Add_float4(_Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2, (_SGChargeBarRoundCorner_2e3a661ab0f541bda9fdb34c9064a3c6_Out_1.xxxx), _Add_9afa425d6b4a4484b424ad62e647beac_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_b709b96791b94b8b9ff4dd5b2d8f9b5a_Out_0 = Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_d3c5646ff5704d228e6a26b2d3f98752_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_21976e95748248d39a26b4d2639a344b_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Clamp_0751c1483d864290943e5ff2f83c0582_Out_3;
    Unity_Clamp_float(_Property_d3c5646ff5704d228e6a26b2d3f98752_Out_0, 0, _Property_21976e95748248d39a26b4d2639a344b_Out_0, _Clamp_0751c1483d864290943e5ff2f83c0582_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    Bindings_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6 _SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b;
    _SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b.uv0 = IN.uv0;
    float _SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b_Out_1;
    SG_SGChargeBarRoundCorner_14512e9ccf11f3e48acef40a0dc3e8d6(_Property_b709b96791b94b8b9ff4dd5b2d8f9b5a_Out_0, _Clamp_0751c1483d864290943e5ff2f83c0582_Out_3, _SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b, _SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2;
    Unity_Add_float4(_Add_9afa425d6b4a4484b424ad62e647beac_Out_2, (_SGChargeBarRoundCorner_4497b1b03f1f45fb9ca8546d67396b5b_Out_1.xxxx), _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #if defined(ENUM_173FCA8E617C4725B6502C6FBE756CEC_SHARP)
    float4 _RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0 = _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2;
    #else
    float4 _RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0 = _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2;
    #endif
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Saturate_737696eb34c241d6b897441762787779_Out_1;
    Unity_Saturate_float4(_RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0, _Saturate_737696eb34c241d6b897441762787779_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    UnityTexture2D _Property_92b3b90fb1c84c859c075f236c70e399_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0 = SAMPLE_TEXTURE2D(_Property_92b3b90fb1c84c859c075f236c70e399_Out_0.tex, _Property_92b3b90fb1c84c859c075f236c70e399_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_db4059a956484545adfbe855ead6e884_R_4 = _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0.r;
    float _SampleTexture2D_db4059a956484545adfbe855ead6e884_G_5 = _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0.g;
    float _SampleTexture2D_db4059a956484545adfbe855ead6e884_B_6 = _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0.b;
    float _SampleTexture2D_db4059a956484545adfbe855ead6e884_A_7 = _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0.a;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Multiply_83aae9860a96466b9d32a13ab9801b27_Out_2;
    Unity_Multiply_float(_Saturate_737696eb34c241d6b897441762787779_Out_1, _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0, _Multiply_83aae9860a96466b9d32a13ab9801b27_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float4 _Multiply_095c15108eac4b039bd50b69d7042144_Out_2;
    Unity_Multiply_float((_Split_59fe34c75d414e64ad726de1e5f63fe1_A_4.xxxx), _Multiply_83aae9860a96466b9d32a13ab9801b27_Out_2, _Multiply_095c15108eac4b039bd50b69d7042144_Out_2);
    #endif
    surface.BaseColor = (_Property_59090694f6954a52b5e6153e0ee8c1be_Out_0.xyz);
    surface.Alpha = (_Multiply_095c15108eac4b039bd50b69d7042144_Out_2).x;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

#if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
output.ObjectSpaceNormal = input.normalOS;
#endif

#if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
output.ObjectSpaceTangent = input.tangentOS.xyz;
#endif

#if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
output.ObjectSpacePosition = input.positionOS;
#endif


    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





#if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
output.uv0 = input.texCoord0;
#endif

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
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SpriteUnlitPass.hlsl"

    ENDHLSL
}
        }
            FallBack "Hidden/Shader Graph/FallbackError"
}