Shader "Unlit/S_UIRadialRainbow"
{
    Properties
    {
        [MainTexture][NoScaleOffset] _Mask("Mask", 2D) = "white" {}
        _FrameThickness("FrameThickness", Vector) = (1.1, 1.1, 0, 0)
        _InnerColor("InnerColor", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_Ramp("Ramp", 2D) = "white" {}
        _Speed("Speed", Vector) = (0, 0, 0, 0)
        _Rotation("Rotation", Float) = 0
        _Color01("Color01", Color) = (1, 0, 0.3212681, 1)
        _Color02("Color02", Color) = (0.07497215, 0, 1, 1)
        _Color03("Color03", Color) = (0, 1, 0.6875515, 1)
        _Color04("Color04", Color) = (1, 0.8461649, 0, 1)
        _Color05("Color05", Color) = (1, 0.8461649, 0, 1)
        _Color06("Color06", Color) = (1, 0.8461649, 0, 1)
        _Color07("Color07", Color) = (1, 0.8461649, 0, 1)
        _Color08("Color08", Color) = (1, 0.8461649, 0, 1)
        _GradientPositions01("GradientPositions01", Vector) = (0, 0.142, 0.285, 0.428)
        _GradientPositions02("GradientPositions02", Vector) = (0.571, 0.714, 0.857, 1)
        _ColorAmount("ColorAmount", Int) = 4
        _UseTexture("UseTexture", Int) = 0
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

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
            Name "Pass"
            Tags
            {
            // LightMode: <None>
        }

        // Render State
        Cull Back
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
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
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
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float4 texCoord0;
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
        float4 uv0;
        float4 VertexColor;
        float3 TimeParameters;
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
        float4 interp0 : TEXCOORD0;
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

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 _Mask_TexelSize;
float2 _FrameThickness;
float4 _InnerColor;
float4 _Ramp_TexelSize;
float2 _Speed;
float _Rotation;
float4 _Color01;
float4 _Color02;
float4 _Color03;
float4 _Color04;
float4 _Color05;
float4 _Color06;
float4 _Color07;
float4 _Color08;
float4 _GradientPositions01;
float4 _GradientPositions02;
float _ColorAmount;
float _UseTexture;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_Mask);
SAMPLER(sampler_Mask);
TEXTURE2D(_Ramp);
SAMPLER(sampler_Ramp);

// Graph Functions

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Step_float(float Edge, float In, out float Out)
{
    Out = step(Edge, In);
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
{
    Out = lerp(A, B, T);
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

void Unity_Lerp_float2(float2 A, float2 B, float2 T, out float2 Out)
{
    Out = lerp(A, B, T);
}

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
{
    Out = smoothstep(Edge1, Edge2, In);
}

struct Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8
{
};

void SG_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8(float4 Color_3db9350e3e7b40a9bc94f9bbca3a3dd6, float4 Color_1, float4 Color_2, float4 Color_3, float4 Color_4, float4 Color_5, float4 Color_6, float4 Color_7, float4 Vector4_0fca494385ab49d08ce1afc653231f44, float4 Vector4_b4f550cbc1ab45e4b200ea689ba2ec50, float Vector1_77a87e2cc8354a469656f2490398cbad, float2 Vector2_b8a1e0cfbb9149e2bdbc25f3ed561ed5, float2 Vector2_12b56547bcf74355a02d20edb4e2c65f, float2 Vector2_b7de4c69caf443168a9e324784340590, float Vector1_efdf863ee8144f3388cc694918a1b6a2, float Vector1_2ae5d9c7d38740aaa4f7b3d9b7b8c480, Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8 IN, out float4 New_0)
{
    float4 _Property_0a2ff2d2e0394e6eaa898052f87ade27_Out_0 = Color_3db9350e3e7b40a9bc94f9bbca3a3dd6;
    float _Property_2f000554a80149969b3c2860c7084231_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_34dbfcfac9644430809d822d16b6c8ba_Out_2;
    Unity_Step_float(1, _Property_2f000554a80149969b3c2860c7084231_Out_0, _Step_34dbfcfac9644430809d822d16b6c8ba_Out_2);
    float4 _Lerp_ed5a66c8649c4639b1aa0da3a11443df_Out_3;
    Unity_Lerp_float4(_Property_0a2ff2d2e0394e6eaa898052f87ade27_Out_0, _Property_0a2ff2d2e0394e6eaa898052f87ade27_Out_0, (_Step_34dbfcfac9644430809d822d16b6c8ba_Out_2.xxxx), _Lerp_ed5a66c8649c4639b1aa0da3a11443df_Out_3);
    float4 _Property_84f97c52665241c780616b8eec11f2ae_Out_0 = Color_3db9350e3e7b40a9bc94f9bbca3a3dd6;
    float4 _Property_9454a3be96394af29077dcda6c48b597_Out_0 = Color_1;
    float4 _Property_483545d18d5f4539b8f54af022aad178_Out_0 = Vector4_0fca494385ab49d08ce1afc653231f44;
    float _Split_bbbe946faeaa43d28c1c50df81b68d3c_R_1 = _Property_483545d18d5f4539b8f54af022aad178_Out_0[0];
    float _Split_bbbe946faeaa43d28c1c50df81b68d3c_G_2 = _Property_483545d18d5f4539b8f54af022aad178_Out_0[1];
    float _Split_bbbe946faeaa43d28c1c50df81b68d3c_B_3 = _Property_483545d18d5f4539b8f54af022aad178_Out_0[2];
    float _Split_bbbe946faeaa43d28c1c50df81b68d3c_A_4 = _Property_483545d18d5f4539b8f54af022aad178_Out_0[3];
    float2 _Property_ad3f1fcc64fb45249c867a76aaf1da44_Out_0 = Vector2_b8a1e0cfbb9149e2bdbc25f3ed561ed5;
    float2 _Property_ad537c25d88e41139999d006d26d0370_Out_0 = Vector2_12b56547bcf74355a02d20edb4e2c65f;
    float2 _Property_090b63313a3349e284ff5644f05d984d_Out_0 = Vector2_b7de4c69caf443168a9e324784340590;
    float2 _TilingAndOffset_97a1f781dd834eb383791e511321508a_Out_3;
    Unity_TilingAndOffset_float(_Property_ad3f1fcc64fb45249c867a76aaf1da44_Out_0, _Property_ad537c25d88e41139999d006d26d0370_Out_0, _Property_090b63313a3349e284ff5644f05d984d_Out_0, _TilingAndOffset_97a1f781dd834eb383791e511321508a_Out_3);
    float _Property_48d884c264af4804896219d23c110a3b_Out_0 = Vector1_77a87e2cc8354a469656f2490398cbad;
    float2 _Rotate_2bc00d9a15414809ae9553e2a0b634f4_Out_3;
    Unity_Rotate_Degrees_float(_TilingAndOffset_97a1f781dd834eb383791e511321508a_Out_3, float2 (0.5, 0.5), _Property_48d884c264af4804896219d23c110a3b_Out_0, _Rotate_2bc00d9a15414809ae9553e2a0b634f4_Out_3);
    float2 _Fraction_9f0094596cdd414eaded7feed610f446_Out_1;
    Unity_Fraction_float2(_Rotate_2bc00d9a15414809ae9553e2a0b634f4_Out_3, _Fraction_9f0094596cdd414eaded7feed610f446_Out_1);
    float _Property_0e46c85ca817426e8465d2ab85a7f0bc_Out_0 = Vector1_2ae5d9c7d38740aaa4f7b3d9b7b8c480;
    float _Step_ed7719dad07543dab0a5dfc0df8d0816_Out_2;
    Unity_Step_float(1, _Property_0e46c85ca817426e8465d2ab85a7f0bc_Out_0, _Step_ed7719dad07543dab0a5dfc0df8d0816_Out_2);
    float2 _Lerp_4eca02bf050343f3b8f6ca42123798b7_Out_3;
    Unity_Lerp_float2(_Rotate_2bc00d9a15414809ae9553e2a0b634f4_Out_3, _Fraction_9f0094596cdd414eaded7feed610f446_Out_1, (_Step_ed7719dad07543dab0a5dfc0df8d0816_Out_2.xx), _Lerp_4eca02bf050343f3b8f6ca42123798b7_Out_3);
    float2 _Property_ca17b2636f2041c081b20e948a6c2783_Out_0 = Vector2_b8a1e0cfbb9149e2bdbc25f3ed561ed5;
    float _Property_9ec4920518dc4ac88dab6808516a93d6_Out_0 = Vector1_77a87e2cc8354a469656f2490398cbad;
    float2 _Rotate_7b9f51cb8ce8446cbb28ad60ee67e06c_Out_3;
    Unity_Rotate_Degrees_float(_Property_ca17b2636f2041c081b20e948a6c2783_Out_0, float2 (0.5, 0.5), _Property_9ec4920518dc4ac88dab6808516a93d6_Out_0, _Rotate_7b9f51cb8ce8446cbb28ad60ee67e06c_Out_3);
    float2 _PolarCoordinates_8e0d41b0dcf542c8a3fe3d08467b50a2_Out_4;
    Unity_PolarCoordinates_float(_Rotate_7b9f51cb8ce8446cbb28ad60ee67e06c_Out_3, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_8e0d41b0dcf542c8a3fe3d08467b50a2_Out_4);
    float _Split_314599f8f9fd4e12a7ca0f35b26432a2_R_1 = _PolarCoordinates_8e0d41b0dcf542c8a3fe3d08467b50a2_Out_4[0];
    float _Split_314599f8f9fd4e12a7ca0f35b26432a2_G_2 = _PolarCoordinates_8e0d41b0dcf542c8a3fe3d08467b50a2_Out_4[1];
    float _Split_314599f8f9fd4e12a7ca0f35b26432a2_B_3 = 0;
    float _Split_314599f8f9fd4e12a7ca0f35b26432a2_A_4 = 0;
    float _Add_6e6adbb1d35749caa29050ca0de195f1_Out_2;
    Unity_Add_float(_Split_314599f8f9fd4e12a7ca0f35b26432a2_G_2, 0.5, _Add_6e6adbb1d35749caa29050ca0de195f1_Out_2);
    float _Property_1fdab43338484183a632dddd987cfe68_Out_0 = Vector1_2ae5d9c7d38740aaa4f7b3d9b7b8c480;
    float _Step_613aa770562443b3881b56b4ef4de08c_Out_2;
    Unity_Step_float(2, _Property_1fdab43338484183a632dddd987cfe68_Out_0, _Step_613aa770562443b3881b56b4ef4de08c_Out_2);
    float2 _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3;
    Unity_Lerp_float2(_Lerp_4eca02bf050343f3b8f6ca42123798b7_Out_3, (_Add_6e6adbb1d35749caa29050ca0de195f1_Out_2.xx), (_Step_613aa770562443b3881b56b4ef4de08c_Out_2.xx), _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3);
    float _Split_68901e4b7cf84413b35e585c3b5dfeda_R_1 = _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3[0];
    float _Split_68901e4b7cf84413b35e585c3b5dfeda_G_2 = _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3[1];
    float _Split_68901e4b7cf84413b35e585c3b5dfeda_B_3 = 0;
    float _Split_68901e4b7cf84413b35e585c3b5dfeda_A_4 = 0;
    float _Smoothstep_eee463b2abdd424789d45e126a902e48_Out_3;
    Unity_Smoothstep_float(_Split_bbbe946faeaa43d28c1c50df81b68d3c_R_1, _Split_bbbe946faeaa43d28c1c50df81b68d3c_G_2, _Split_68901e4b7cf84413b35e585c3b5dfeda_R_1, _Smoothstep_eee463b2abdd424789d45e126a902e48_Out_3);
    float4 _Lerp_9fc6b45377ad49b4892b2f9259187522_Out_3;
    Unity_Lerp_float4(_Property_84f97c52665241c780616b8eec11f2ae_Out_0, _Property_9454a3be96394af29077dcda6c48b597_Out_0, (_Smoothstep_eee463b2abdd424789d45e126a902e48_Out_3.xxxx), _Lerp_9fc6b45377ad49b4892b2f9259187522_Out_3);
    float _Property_941c36c120ff4f889e5b4d20a73f4060_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_44788e99719149ac98a5937b33c39218_Out_2;
    Unity_Step_float(2, _Property_941c36c120ff4f889e5b4d20a73f4060_Out_0, _Step_44788e99719149ac98a5937b33c39218_Out_2);
    float4 _Lerp_4fd34c2c3d9042a1a5feca25a9be6143_Out_3;
    Unity_Lerp_float4(_Lerp_ed5a66c8649c4639b1aa0da3a11443df_Out_3, _Lerp_9fc6b45377ad49b4892b2f9259187522_Out_3, (_Step_44788e99719149ac98a5937b33c39218_Out_2.xxxx), _Lerp_4fd34c2c3d9042a1a5feca25a9be6143_Out_3);
    float4 _Property_8b146fd01b384c4abfffe2f7ed408b47_Out_0 = Color_2;
    float4 _Property_a662a466cb5f490db51ceb78e55ccb33_Out_0 = Vector4_0fca494385ab49d08ce1afc653231f44;
    float _Split_fcd288f8b5a04b249ee16a9a90c1c71e_R_1 = _Property_a662a466cb5f490db51ceb78e55ccb33_Out_0[0];
    float _Split_fcd288f8b5a04b249ee16a9a90c1c71e_G_2 = _Property_a662a466cb5f490db51ceb78e55ccb33_Out_0[1];
    float _Split_fcd288f8b5a04b249ee16a9a90c1c71e_B_3 = _Property_a662a466cb5f490db51ceb78e55ccb33_Out_0[2];
    float _Split_fcd288f8b5a04b249ee16a9a90c1c71e_A_4 = _Property_a662a466cb5f490db51ceb78e55ccb33_Out_0[3];
    float _Smoothstep_179e98e09e2b4ae18fdcb07ce83826b8_Out_3;
    Unity_Smoothstep_float(_Split_fcd288f8b5a04b249ee16a9a90c1c71e_G_2, _Split_fcd288f8b5a04b249ee16a9a90c1c71e_B_3, _Split_68901e4b7cf84413b35e585c3b5dfeda_R_1, _Smoothstep_179e98e09e2b4ae18fdcb07ce83826b8_Out_3);
    float4 _Lerp_d3b70e4665bf426f8697ce619c142dea_Out_3;
    Unity_Lerp_float4(_Lerp_9fc6b45377ad49b4892b2f9259187522_Out_3, _Property_8b146fd01b384c4abfffe2f7ed408b47_Out_0, (_Smoothstep_179e98e09e2b4ae18fdcb07ce83826b8_Out_3.xxxx), _Lerp_d3b70e4665bf426f8697ce619c142dea_Out_3);
    float _Property_243cf601982e457f9db996297da9d14d_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_f4aac0d29dbf40999a65f3f19e4b2703_Out_2;
    Unity_Step_float(3, _Property_243cf601982e457f9db996297da9d14d_Out_0, _Step_f4aac0d29dbf40999a65f3f19e4b2703_Out_2);
    float4 _Lerp_71c282caf046490a98bb8f1f27e6e7af_Out_3;
    Unity_Lerp_float4(_Lerp_4fd34c2c3d9042a1a5feca25a9be6143_Out_3, _Lerp_d3b70e4665bf426f8697ce619c142dea_Out_3, (_Step_f4aac0d29dbf40999a65f3f19e4b2703_Out_2.xxxx), _Lerp_71c282caf046490a98bb8f1f27e6e7af_Out_3);
    float4 _Property_0fbf3a53751f42e7a9032d5d26420305_Out_0 = Color_3;
    float4 _Property_0272811edb63424eb3f51e7d67dbd582_Out_0 = Vector4_0fca494385ab49d08ce1afc653231f44;
    float _Split_b493ab7d09924c51a6f398821ce96506_R_1 = _Property_0272811edb63424eb3f51e7d67dbd582_Out_0[0];
    float _Split_b493ab7d09924c51a6f398821ce96506_G_2 = _Property_0272811edb63424eb3f51e7d67dbd582_Out_0[1];
    float _Split_b493ab7d09924c51a6f398821ce96506_B_3 = _Property_0272811edb63424eb3f51e7d67dbd582_Out_0[2];
    float _Split_b493ab7d09924c51a6f398821ce96506_A_4 = _Property_0272811edb63424eb3f51e7d67dbd582_Out_0[3];
    float4 _Property_a527f1410869489daf4b4fa407a8455a_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Split_14bf9198190e4cac8bb3b3e5a2b49f61_R_1 = _Property_a527f1410869489daf4b4fa407a8455a_Out_0[0];
    float _Split_14bf9198190e4cac8bb3b3e5a2b49f61_G_2 = _Property_a527f1410869489daf4b4fa407a8455a_Out_0[1];
    float _Split_14bf9198190e4cac8bb3b3e5a2b49f61_B_3 = _Property_a527f1410869489daf4b4fa407a8455a_Out_0[2];
    float _Split_14bf9198190e4cac8bb3b3e5a2b49f61_A_4 = _Property_a527f1410869489daf4b4fa407a8455a_Out_0[3];
    float _Smoothstep_02476caa111d444a89c16f1730e29d7e_Out_3;
    Unity_Smoothstep_float(_Split_b493ab7d09924c51a6f398821ce96506_B_3, _Split_14bf9198190e4cac8bb3b3e5a2b49f61_R_1, _Split_68901e4b7cf84413b35e585c3b5dfeda_R_1, _Smoothstep_02476caa111d444a89c16f1730e29d7e_Out_3);
    float4 _Lerp_f0b78e026da145ed889819a552450c5d_Out_3;
    Unity_Lerp_float4(_Lerp_d3b70e4665bf426f8697ce619c142dea_Out_3, _Property_0fbf3a53751f42e7a9032d5d26420305_Out_0, (_Smoothstep_02476caa111d444a89c16f1730e29d7e_Out_3.xxxx), _Lerp_f0b78e026da145ed889819a552450c5d_Out_3);
    float _Property_fdd70a63f09744249d417aed85264db8_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_286ab944f8714242a5021077f54722f3_Out_2;
    Unity_Step_float(4, _Property_fdd70a63f09744249d417aed85264db8_Out_0, _Step_286ab944f8714242a5021077f54722f3_Out_2);
    float4 _Lerp_17bb08ea8ac341d59f7a078f7b6a217d_Out_3;
    Unity_Lerp_float4(_Lerp_71c282caf046490a98bb8f1f27e6e7af_Out_3, _Lerp_f0b78e026da145ed889819a552450c5d_Out_3, (_Step_286ab944f8714242a5021077f54722f3_Out_2.xxxx), _Lerp_17bb08ea8ac341d59f7a078f7b6a217d_Out_3);
    float4 _Property_e4fb59e181f644d5a5ac4e1e73368f1d_Out_0 = Color_4;
    float4 _Property_8d14c91b46634566bb9975d55f7e0dee_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Split_7fac29de024c416b8ae8aee298b54791_R_1 = _Property_8d14c91b46634566bb9975d55f7e0dee_Out_0[0];
    float _Split_7fac29de024c416b8ae8aee298b54791_G_2 = _Property_8d14c91b46634566bb9975d55f7e0dee_Out_0[1];
    float _Split_7fac29de024c416b8ae8aee298b54791_B_3 = _Property_8d14c91b46634566bb9975d55f7e0dee_Out_0[2];
    float _Split_7fac29de024c416b8ae8aee298b54791_A_4 = _Property_8d14c91b46634566bb9975d55f7e0dee_Out_0[3];
    float _Split_88d34486db7c46c5a684abe326569216_R_1 = _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3[0];
    float _Split_88d34486db7c46c5a684abe326569216_G_2 = _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3[1];
    float _Split_88d34486db7c46c5a684abe326569216_B_3 = 0;
    float _Split_88d34486db7c46c5a684abe326569216_A_4 = 0;
    float _Smoothstep_128a8117ae1e460898903021ccc2dfa0_Out_3;
    Unity_Smoothstep_float(_Split_7fac29de024c416b8ae8aee298b54791_R_1, _Split_7fac29de024c416b8ae8aee298b54791_G_2, _Split_88d34486db7c46c5a684abe326569216_R_1, _Smoothstep_128a8117ae1e460898903021ccc2dfa0_Out_3);
    float4 _Lerp_51cf8dc392dd47a4ab660dcd1eff31ab_Out_3;
    Unity_Lerp_float4(_Lerp_f0b78e026da145ed889819a552450c5d_Out_3, _Property_e4fb59e181f644d5a5ac4e1e73368f1d_Out_0, (_Smoothstep_128a8117ae1e460898903021ccc2dfa0_Out_3.xxxx), _Lerp_51cf8dc392dd47a4ab660dcd1eff31ab_Out_3);
    float _Property_ca74c91638434ca291a3a7fe32a595ff_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_66027715693145a89aabf0e4acc647e4_Out_2;
    Unity_Step_float(5, _Property_ca74c91638434ca291a3a7fe32a595ff_Out_0, _Step_66027715693145a89aabf0e4acc647e4_Out_2);
    float4 _Lerp_6fbac211a45b483fabc3d6e0630625f9_Out_3;
    Unity_Lerp_float4(_Lerp_17bb08ea8ac341d59f7a078f7b6a217d_Out_3, _Lerp_51cf8dc392dd47a4ab660dcd1eff31ab_Out_3, (_Step_66027715693145a89aabf0e4acc647e4_Out_2.xxxx), _Lerp_6fbac211a45b483fabc3d6e0630625f9_Out_3);
    float4 _Property_e36a889e969b4e518b8410bbb2676253_Out_0 = Color_5;
    float4 _Property_06ce1893a6d14cb590ae16f6112171c1_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Split_e76f8b076b754ff3bb75a09da5b62a57_R_1 = _Property_06ce1893a6d14cb590ae16f6112171c1_Out_0[0];
    float _Split_e76f8b076b754ff3bb75a09da5b62a57_G_2 = _Property_06ce1893a6d14cb590ae16f6112171c1_Out_0[1];
    float _Split_e76f8b076b754ff3bb75a09da5b62a57_B_3 = _Property_06ce1893a6d14cb590ae16f6112171c1_Out_0[2];
    float _Split_e76f8b076b754ff3bb75a09da5b62a57_A_4 = _Property_06ce1893a6d14cb590ae16f6112171c1_Out_0[3];
    float _Smoothstep_fe26619796b14054958a264244ecf7f1_Out_3;
    Unity_Smoothstep_float(_Split_e76f8b076b754ff3bb75a09da5b62a57_G_2, _Split_e76f8b076b754ff3bb75a09da5b62a57_B_3, _Split_88d34486db7c46c5a684abe326569216_R_1, _Smoothstep_fe26619796b14054958a264244ecf7f1_Out_3);
    float4 _Lerp_32aa8bcc9c07432e93253e8aef66a5cf_Out_3;
    Unity_Lerp_float4(_Lerp_51cf8dc392dd47a4ab660dcd1eff31ab_Out_3, _Property_e36a889e969b4e518b8410bbb2676253_Out_0, (_Smoothstep_fe26619796b14054958a264244ecf7f1_Out_3.xxxx), _Lerp_32aa8bcc9c07432e93253e8aef66a5cf_Out_3);
    float _Property_1d66f04933ec4271927572c9ff65c09d_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_6a7883714df4493db50acf9252f7f475_Out_2;
    Unity_Step_float(6, _Property_1d66f04933ec4271927572c9ff65c09d_Out_0, _Step_6a7883714df4493db50acf9252f7f475_Out_2);
    float4 _Lerp_0c5530a0dce7482cb05b188833564fa5_Out_3;
    Unity_Lerp_float4(_Lerp_6fbac211a45b483fabc3d6e0630625f9_Out_3, _Lerp_32aa8bcc9c07432e93253e8aef66a5cf_Out_3, (_Step_6a7883714df4493db50acf9252f7f475_Out_2.xxxx), _Lerp_0c5530a0dce7482cb05b188833564fa5_Out_3);
    float4 _Property_d71b8b7b341244c48eaef4b76e736359_Out_0 = Color_6;
    float4 _Property_8a8eb33ecd8b4ab799a00038def60c9d_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Split_0237b9f8004b44f8ac9c4dbdd329b38e_R_1 = _Property_8a8eb33ecd8b4ab799a00038def60c9d_Out_0[0];
    float _Split_0237b9f8004b44f8ac9c4dbdd329b38e_G_2 = _Property_8a8eb33ecd8b4ab799a00038def60c9d_Out_0[1];
    float _Split_0237b9f8004b44f8ac9c4dbdd329b38e_B_3 = _Property_8a8eb33ecd8b4ab799a00038def60c9d_Out_0[2];
    float _Split_0237b9f8004b44f8ac9c4dbdd329b38e_A_4 = _Property_8a8eb33ecd8b4ab799a00038def60c9d_Out_0[3];
    float _Smoothstep_43382a8072774027b21eac2c4aeed755_Out_3;
    Unity_Smoothstep_float(_Split_0237b9f8004b44f8ac9c4dbdd329b38e_B_3, _Split_0237b9f8004b44f8ac9c4dbdd329b38e_A_4, _Split_88d34486db7c46c5a684abe326569216_R_1, _Smoothstep_43382a8072774027b21eac2c4aeed755_Out_3);
    float4 _Lerp_1b8ecaa0e924471fa9e1a0a3ba4838c4_Out_3;
    Unity_Lerp_float4(_Lerp_32aa8bcc9c07432e93253e8aef66a5cf_Out_3, _Property_d71b8b7b341244c48eaef4b76e736359_Out_0, (_Smoothstep_43382a8072774027b21eac2c4aeed755_Out_3.xxxx), _Lerp_1b8ecaa0e924471fa9e1a0a3ba4838c4_Out_3);
    float _Property_bbfd9f6f601747cea47993e59b316c97_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_947780af629a4c0ab06c6c45c337df33_Out_2;
    Unity_Step_float(7, _Property_bbfd9f6f601747cea47993e59b316c97_Out_0, _Step_947780af629a4c0ab06c6c45c337df33_Out_2);
    float4 _Lerp_7bce97d8215b46629bbf3a38c2beddb7_Out_3;
    Unity_Lerp_float4(_Lerp_0c5530a0dce7482cb05b188833564fa5_Out_3, _Lerp_1b8ecaa0e924471fa9e1a0a3ba4838c4_Out_3, (_Step_947780af629a4c0ab06c6c45c337df33_Out_2.xxxx), _Lerp_7bce97d8215b46629bbf3a38c2beddb7_Out_3);
    float4 _Property_c712201a00494d7cbd895c3d3d68d291_Out_0 = Color_7;
    float4 _Property_409fc273f2a34f28b79fa46bd2435e60_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Split_21fef25b64c445aba19ccfc7a0994588_R_1 = _Property_409fc273f2a34f28b79fa46bd2435e60_Out_0[0];
    float _Split_21fef25b64c445aba19ccfc7a0994588_G_2 = _Property_409fc273f2a34f28b79fa46bd2435e60_Out_0[1];
    float _Split_21fef25b64c445aba19ccfc7a0994588_B_3 = _Property_409fc273f2a34f28b79fa46bd2435e60_Out_0[2];
    float _Split_21fef25b64c445aba19ccfc7a0994588_A_4 = _Property_409fc273f2a34f28b79fa46bd2435e60_Out_0[3];
    float _Smoothstep_502377937d454e3ab4554ffad30c9224_Out_3;
    Unity_Smoothstep_float(_Split_21fef25b64c445aba19ccfc7a0994588_A_4, 1, _Split_88d34486db7c46c5a684abe326569216_R_1, _Smoothstep_502377937d454e3ab4554ffad30c9224_Out_3);
    float4 _Lerp_ecf7b0bdc01c4655ad7653e86a99daeb_Out_3;
    Unity_Lerp_float4(_Lerp_1b8ecaa0e924471fa9e1a0a3ba4838c4_Out_3, _Property_c712201a00494d7cbd895c3d3d68d291_Out_0, (_Smoothstep_502377937d454e3ab4554ffad30c9224_Out_3.xxxx), _Lerp_ecf7b0bdc01c4655ad7653e86a99daeb_Out_3);
    float _Property_1ab23f4faf804ab69914ea928afcf93f_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_3f385fe3ad654ec6a8dc138c386126aa_Out_2;
    Unity_Step_float(8, _Property_1ab23f4faf804ab69914ea928afcf93f_Out_0, _Step_3f385fe3ad654ec6a8dc138c386126aa_Out_2);
    float4 _Lerp_1ee4399599984c90ac29f61fc35002a4_Out_3;
    Unity_Lerp_float4(_Lerp_7bce97d8215b46629bbf3a38c2beddb7_Out_3, _Lerp_ecf7b0bdc01c4655ad7653e86a99daeb_Out_3, (_Step_3f385fe3ad654ec6a8dc138c386126aa_Out_2.xxxx), _Lerp_1ee4399599984c90ac29f61fc35002a4_Out_3);
    New_0 = _Lerp_1ee4399599984c90ac29f61fc35002a4_Out_3;
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Rectangle_float(float2 UV, float Width, float Height, out float Out)
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Saturate_float(float In, out float Out)
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
    float4 _Property_62526134cac94aba8676fa0c84f65879_Out_0 = _Color01;
    float4 _Property_477c79e0faaa496a9d4034743291c1c7_Out_0 = _Color02;
    float4 _Property_1fb3b68d47474407b72b3d8a315d1cc4_Out_0 = _Color03;
    float4 _Property_04b94cc94aa0408394f439a856b76d2f_Out_0 = _Color04;
    float4 _Property_c09af223cc0c44199c05d4eae5dd54d4_Out_0 = _Color05;
    float4 _Property_606267184abd47cc9372ac82d23feac0_Out_0 = _Color06;
    float4 _Property_cefebafe8ee040fe97095a640012909f_Out_0 = _Color07;
    float4 _Property_ea9c784caa8d49ce941e70b361c08a49_Out_0 = _Color08;
    float4 _Property_ad79e18fc85a434c8f39d21002055d5a_Out_0 = _GradientPositions01;
    float4 _Property_553e23acff2c4b0c90c412e9d9111757_Out_0 = _GradientPositions02;
    float _Property_d59bb8c26b614802828bdea11252c07a_Out_0 = _Rotation;
    float2 _Property_5eb266a27fbe4041be0f29be934026b4_Out_0 = _Speed;
    float _Split_07b28d6fd17c43b488c30fb147361a80_R_1 = _Property_5eb266a27fbe4041be0f29be934026b4_Out_0[0];
    float _Split_07b28d6fd17c43b488c30fb147361a80_G_2 = _Property_5eb266a27fbe4041be0f29be934026b4_Out_0[1];
    float _Split_07b28d6fd17c43b488c30fb147361a80_B_3 = 0;
    float _Split_07b28d6fd17c43b488c30fb147361a80_A_4 = 0;
    float _Multiply_5c32e6daa86b46c0bba26bc503d416b0_Out_2;
    Unity_Multiply_float(IN.TimeParameters.x, _Split_07b28d6fd17c43b488c30fb147361a80_R_1, _Multiply_5c32e6daa86b46c0bba26bc503d416b0_Out_2);
    float _Step_630aaa5c94dd49dfb60135d02b258d18_Out_2;
    Unity_Step_float(0.01, _Multiply_5c32e6daa86b46c0bba26bc503d416b0_Out_2, _Step_630aaa5c94dd49dfb60135d02b258d18_Out_2);
    float _Lerp_9b90fb8914db4cf5a7bbd5ccc546fff0_Out_3;
    Unity_Lerp_float(_Property_d59bb8c26b614802828bdea11252c07a_Out_0, _Multiply_5c32e6daa86b46c0bba26bc503d416b0_Out_2, _Step_630aaa5c94dd49dfb60135d02b258d18_Out_2, _Lerp_9b90fb8914db4cf5a7bbd5ccc546fff0_Out_3);
    float4 _UV_0904f158ff864e0e9e93bab44e83817a_Out_0 = IN.uv0;
    float2 _TilingAndOffset_f0ce4c837b1845848d33befb6a72fce9_Out_3;
    Unity_TilingAndOffset_float((_UV_0904f158ff864e0e9e93bab44e83817a_Out_0.xy), float2 (1, 1), float2 (0, 0), _TilingAndOffset_f0ce4c837b1845848d33befb6a72fce9_Out_3);
    float2 _Fraction_1f89fae6b5604b85bf2eac4cea72f0a0_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_f0ce4c837b1845848d33befb6a72fce9_Out_3, _Fraction_1f89fae6b5604b85bf2eac4cea72f0a0_Out_1);
    float _Property_b61779cde42b4fbea3e551a4ee5d73d0_Out_0 = _ColorAmount;
    Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8 _SGCustomGradient_cfbd3021c3c542249af6bf8136dee314;
    float4 _SGCustomGradient_cfbd3021c3c542249af6bf8136dee314_New_0;
    SG_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8(_Property_62526134cac94aba8676fa0c84f65879_Out_0, _Property_477c79e0faaa496a9d4034743291c1c7_Out_0, _Property_1fb3b68d47474407b72b3d8a315d1cc4_Out_0, _Property_04b94cc94aa0408394f439a856b76d2f_Out_0, _Property_c09af223cc0c44199c05d4eae5dd54d4_Out_0, _Property_606267184abd47cc9372ac82d23feac0_Out_0, _Property_cefebafe8ee040fe97095a640012909f_Out_0, _Property_ea9c784caa8d49ce941e70b361c08a49_Out_0, _Property_ad79e18fc85a434c8f39d21002055d5a_Out_0, _Property_553e23acff2c4b0c90c412e9d9111757_Out_0, _Lerp_9b90fb8914db4cf5a7bbd5ccc546fff0_Out_3, _Fraction_1f89fae6b5604b85bf2eac4cea72f0a0_Out_1, float2 (1, 1), float2 (0, 0), _Property_b61779cde42b4fbea3e551a4ee5d73d0_Out_0, 2, _SGCustomGradient_cfbd3021c3c542249af6bf8136dee314, _SGCustomGradient_cfbd3021c3c542249af6bf8136dee314_New_0);
    UnityTexture2D _Property_a7311642d00b42959535635b88082c56_Out_0 = UnityBuildTexture2DStructNoScale(_Ramp);
    float2 _Property_7ac0b13b0f0842d2909e2e9216c02e87_Out_0 = _Speed;
    float2 _Multiply_24a2ec0cc7214ffb9a9527125b870090_Out_2;
    Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_7ac0b13b0f0842d2909e2e9216c02e87_Out_0, _Multiply_24a2ec0cc7214ffb9a9527125b870090_Out_2);
    float2 _Rotate_685c277188004fd5b23e44106bc9f428_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), (_Multiply_24a2ec0cc7214ffb9a9527125b870090_Out_2).x, _Rotate_685c277188004fd5b23e44106bc9f428_Out_3);
    float _Split_f7289cbf88a842a8b501a90bcd9d778b_R_1 = _Rotate_685c277188004fd5b23e44106bc9f428_Out_3[0];
    float _Split_f7289cbf88a842a8b501a90bcd9d778b_G_2 = _Rotate_685c277188004fd5b23e44106bc9f428_Out_3[1];
    float _Split_f7289cbf88a842a8b501a90bcd9d778b_B_3 = 0;
    float _Split_f7289cbf88a842a8b501a90bcd9d778b_A_4 = 0;
    float _OneMinus_e75135fc95b24bd5b87939861efd9952_Out_1;
    Unity_OneMinus_float(_Split_f7289cbf88a842a8b501a90bcd9d778b_R_1, _OneMinus_e75135fc95b24bd5b87939861efd9952_Out_1);
    float2 _Vector2_5bd1b402e1fe4e0baa1389a6b2b11699_Out_0 = float2(_OneMinus_e75135fc95b24bd5b87939861efd9952_Out_1, _Split_f7289cbf88a842a8b501a90bcd9d778b_G_2);
    float2 _PolarCoordinates_cb6915ba7093498dbd2001d96c8fb897_Out_4;
    Unity_PolarCoordinates_float(_Vector2_5bd1b402e1fe4e0baa1389a6b2b11699_Out_0, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_cb6915ba7093498dbd2001d96c8fb897_Out_4);
    float _Split_02d6f31be8694684858cd192b4bacaba_R_1 = _PolarCoordinates_cb6915ba7093498dbd2001d96c8fb897_Out_4[0];
    float _Split_02d6f31be8694684858cd192b4bacaba_G_2 = _PolarCoordinates_cb6915ba7093498dbd2001d96c8fb897_Out_4[1];
    float _Split_02d6f31be8694684858cd192b4bacaba_B_3 = 0;
    float _Split_02d6f31be8694684858cd192b4bacaba_A_4 = 0;
    float _Add_8b231e94209049c2bdd53f03228a0295_Out_2;
    Unity_Add_float(_Split_02d6f31be8694684858cd192b4bacaba_G_2, 0.5, _Add_8b231e94209049c2bdd53f03228a0295_Out_2);
    float4 _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0 = SAMPLE_TEXTURE2D(_Property_a7311642d00b42959535635b88082c56_Out_0.tex, _Property_a7311642d00b42959535635b88082c56_Out_0.samplerstate, (_Add_8b231e94209049c2bdd53f03228a0295_Out_2.xx));
    float _SampleTexture2D_11b9032f513a4306885ed14965c973e6_R_4 = _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0.r;
    float _SampleTexture2D_11b9032f513a4306885ed14965c973e6_G_5 = _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0.g;
    float _SampleTexture2D_11b9032f513a4306885ed14965c973e6_B_6 = _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0.b;
    float _SampleTexture2D_11b9032f513a4306885ed14965c973e6_A_7 = _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0.a;
    float _Property_ca5c7c3b7eb9490994ba68e23ee87a43_Out_0 = _UseTexture;
    float _Step_a2759376e99046afa588bee996210bcc_Out_2;
    Unity_Step_float(1, _Property_ca5c7c3b7eb9490994ba68e23ee87a43_Out_0, _Step_a2759376e99046afa588bee996210bcc_Out_2);
    float4 _Lerp_c0403dafde8747ee931186268ba1dc3d_Out_3;
    Unity_Lerp_float4(_SGCustomGradient_cfbd3021c3c542249af6bf8136dee314_New_0, _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0, (_Step_a2759376e99046afa588bee996210bcc_Out_2.xxxx), _Lerp_c0403dafde8747ee931186268ba1dc3d_Out_3);
    float4 _Property_aa0f73601f7648859cb2d7c603bae46f_Out_0 = _InnerColor;
    UnityTexture2D _Property_9b9b783269f94e6f8030cd2a50295a79_Out_0 = UnityBuildTexture2DStructNoScale(_Mask);
    float2 _Property_7085c65f55ea492888a28fad61369a22_Out_0 = _FrameThickness;
    float _Split_796152b39d5f4b8c8a5e4b92fb1ecf75_R_1 = _Property_7085c65f55ea492888a28fad61369a22_Out_0[0];
    float _Split_796152b39d5f4b8c8a5e4b92fb1ecf75_G_2 = _Property_7085c65f55ea492888a28fad61369a22_Out_0[1];
    float _Split_796152b39d5f4b8c8a5e4b92fb1ecf75_B_3 = 0;
    float _Split_796152b39d5f4b8c8a5e4b92fb1ecf75_A_4 = 0;
    float _Multiply_1529b3b184234cefaf414ade0331fe28_Out_2;
    Unity_Multiply_float(_Split_796152b39d5f4b8c8a5e4b92fb1ecf75_R_1, -1, _Multiply_1529b3b184234cefaf414ade0331fe28_Out_2);
    float _Divide_59c865b4a9f540b8b9f681d5bfe52c58_Out_2;
    Unity_Divide_float(_Multiply_1529b3b184234cefaf414ade0331fe28_Out_2, 2, _Divide_59c865b4a9f540b8b9f681d5bfe52c58_Out_2);
    float _Add_3aeab0f33bb74db0bb3d28dce9a1b54c_Out_2;
    Unity_Add_float(_Divide_59c865b4a9f540b8b9f681d5bfe52c58_Out_2, 0.5, _Add_3aeab0f33bb74db0bb3d28dce9a1b54c_Out_2);
    float _Multiply_fcbb3fe044714a07b58a58e20005f0be_Out_2;
    Unity_Multiply_float(_Split_796152b39d5f4b8c8a5e4b92fb1ecf75_G_2, -1, _Multiply_fcbb3fe044714a07b58a58e20005f0be_Out_2);
    float _Divide_4e648c33f8d3465984eff65445c0c781_Out_2;
    Unity_Divide_float(_Multiply_fcbb3fe044714a07b58a58e20005f0be_Out_2, 2, _Divide_4e648c33f8d3465984eff65445c0c781_Out_2);
    float _Add_c8db7f8193fd481aa5cc0fdf106594f4_Out_2;
    Unity_Add_float(_Divide_4e648c33f8d3465984eff65445c0c781_Out_2, 0.5, _Add_c8db7f8193fd481aa5cc0fdf106594f4_Out_2);
    float2 _Vector2_9164475dd9ae4d46b7fb285d59e06fbb_Out_0 = float2(_Add_3aeab0f33bb74db0bb3d28dce9a1b54c_Out_2, _Add_c8db7f8193fd481aa5cc0fdf106594f4_Out_2);
    float2 _TilingAndOffset_616af8dba9f542008493dceec7d691d4_Out_3;
    Unity_TilingAndOffset_float(IN.uv0.xy, _Property_7085c65f55ea492888a28fad61369a22_Out_0, _Vector2_9164475dd9ae4d46b7fb285d59e06fbb_Out_0, _TilingAndOffset_616af8dba9f542008493dceec7d691d4_Out_3);
    float4 _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9b9b783269f94e6f8030cd2a50295a79_Out_0.tex, _Property_9b9b783269f94e6f8030cd2a50295a79_Out_0.samplerstate, _TilingAndOffset_616af8dba9f542008493dceec7d691d4_Out_3);
    float _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_R_4 = _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_RGBA_0.r;
    float _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_G_5 = _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_RGBA_0.g;
    float _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_B_6 = _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_RGBA_0.b;
    float _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_A_7 = _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_RGBA_0.a;
    float _Rectangle_5e295f8971fe43fbb322860a074e782a_Out_3;
    Unity_Rectangle_float(_TilingAndOffset_616af8dba9f542008493dceec7d691d4_Out_3, 1, 1, _Rectangle_5e295f8971fe43fbb322860a074e782a_Out_3);
    float _OneMinus_11f4203469d343218c733f1ea805f74b_Out_1;
    Unity_OneMinus_float(_Rectangle_5e295f8971fe43fbb322860a074e782a_Out_3, _OneMinus_11f4203469d343218c733f1ea805f74b_Out_1);
    float _Subtract_2be32d5243794e1d87bb3f964e6f8f8f_Out_2;
    Unity_Subtract_float(_SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_A_7, _OneMinus_11f4203469d343218c733f1ea805f74b_Out_1, _Subtract_2be32d5243794e1d87bb3f964e6f8f8f_Out_2);
    float _Saturate_ee82bf3d98954a25a875b6e0e174efc6_Out_1;
    Unity_Saturate_float(_Subtract_2be32d5243794e1d87bb3f964e6f8f8f_Out_2, _Saturate_ee82bf3d98954a25a875b6e0e174efc6_Out_1);
    float4 _Lerp_29d871897c174ca6a675b71ba58ca50f_Out_3;
    Unity_Lerp_float4(_Lerp_c0403dafde8747ee931186268ba1dc3d_Out_3, _Property_aa0f73601f7648859cb2d7c603bae46f_Out_0, (_Saturate_ee82bf3d98954a25a875b6e0e174efc6_Out_1.xxxx), _Lerp_29d871897c174ca6a675b71ba58ca50f_Out_3);
    UnityTexture2D _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0 = UnityBuildTexture2DStructNoScale(_Mask);
    float4 _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0 = SAMPLE_TEXTURE2D(_Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.tex, _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_R_4 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.r;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_G_5 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.g;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_B_6 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.b;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.a;
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_R_1 = IN.VertexColor[0];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_G_2 = IN.VertexColor[1];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_B_3 = IN.VertexColor[2];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4 = IN.VertexColor[3];
    float _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    Unity_Multiply_float(_SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7, _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4, _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2);
    surface.BaseColor = (_Lerp_29d871897c174ca6a675b71ba58ca50f_Out_3.xyz);
    surface.Alpha = _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
    output.VertexColor = input.color;
    output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
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
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
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
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float4 texCoord0;
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
        float4 uv0;
        float4 VertexColor;
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
        float4 interp0 : TEXCOORD0;
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

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 _Mask_TexelSize;
float2 _FrameThickness;
float4 _InnerColor;
float4 _Ramp_TexelSize;
float2 _Speed;
float _Rotation;
float4 _Color01;
float4 _Color02;
float4 _Color03;
float4 _Color04;
float4 _Color05;
float4 _Color06;
float4 _Color07;
float4 _Color08;
float4 _GradientPositions01;
float4 _GradientPositions02;
float _ColorAmount;
float _UseTexture;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_Mask);
SAMPLER(sampler_Mask);
TEXTURE2D(_Ramp);
SAMPLER(sampler_Ramp);

// Graph Functions

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    UnityTexture2D _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0 = UnityBuildTexture2DStructNoScale(_Mask);
    float4 _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0 = SAMPLE_TEXTURE2D(_Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.tex, _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_R_4 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.r;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_G_5 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.g;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_B_6 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.b;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.a;
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_R_1 = IN.VertexColor[0];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_G_2 = IN.VertexColor[1];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_B_3 = IN.VertexColor[2];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4 = IN.VertexColor[3];
    float _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    Unity_Multiply_float(_SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7, _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4, _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2);
    surface.Alpha = _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
    output.VertexColor = input.color;
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
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
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
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float4 texCoord0;
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
        float4 uv0;
        float4 VertexColor;
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
        float4 interp0 : TEXCOORD0;
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

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 _Mask_TexelSize;
float2 _FrameThickness;
float4 _InnerColor;
float4 _Ramp_TexelSize;
float2 _Speed;
float _Rotation;
float4 _Color01;
float4 _Color02;
float4 _Color03;
float4 _Color04;
float4 _Color05;
float4 _Color06;
float4 _Color07;
float4 _Color08;
float4 _GradientPositions01;
float4 _GradientPositions02;
float _ColorAmount;
float _UseTexture;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_Mask);
SAMPLER(sampler_Mask);
TEXTURE2D(_Ramp);
SAMPLER(sampler_Ramp);

// Graph Functions

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    UnityTexture2D _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0 = UnityBuildTexture2DStructNoScale(_Mask);
    float4 _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0 = SAMPLE_TEXTURE2D(_Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.tex, _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_R_4 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.r;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_G_5 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.g;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_B_6 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.b;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.a;
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_R_1 = IN.VertexColor[0];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_G_2 = IN.VertexColor[1];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_B_3 = IN.VertexColor[2];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4 = IN.VertexColor[3];
    float _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    Unity_Multiply_float(_SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7, _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4, _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2);
    surface.Alpha = _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
    output.VertexColor = input.color;
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
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue" = "Transparent"
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
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
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
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float4 texCoord0;
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
        float4 uv0;
        float4 VertexColor;
        float3 TimeParameters;
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
        float4 interp0 : TEXCOORD0;
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

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 _Mask_TexelSize;
float2 _FrameThickness;
float4 _InnerColor;
float4 _Ramp_TexelSize;
float2 _Speed;
float _Rotation;
float4 _Color01;
float4 _Color02;
float4 _Color03;
float4 _Color04;
float4 _Color05;
float4 _Color06;
float4 _Color07;
float4 _Color08;
float4 _GradientPositions01;
float4 _GradientPositions02;
float _ColorAmount;
float _UseTexture;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_Mask);
SAMPLER(sampler_Mask);
TEXTURE2D(_Ramp);
SAMPLER(sampler_Ramp);

// Graph Functions

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Step_float(float Edge, float In, out float Out)
{
    Out = step(Edge, In);
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
{
    Out = lerp(A, B, T);
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

void Unity_Lerp_float2(float2 A, float2 B, float2 T, out float2 Out)
{
    Out = lerp(A, B, T);
}

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
{
    Out = smoothstep(Edge1, Edge2, In);
}

struct Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8
{
};

void SG_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8(float4 Color_3db9350e3e7b40a9bc94f9bbca3a3dd6, float4 Color_1, float4 Color_2, float4 Color_3, float4 Color_4, float4 Color_5, float4 Color_6, float4 Color_7, float4 Vector4_0fca494385ab49d08ce1afc653231f44, float4 Vector4_b4f550cbc1ab45e4b200ea689ba2ec50, float Vector1_77a87e2cc8354a469656f2490398cbad, float2 Vector2_b8a1e0cfbb9149e2bdbc25f3ed561ed5, float2 Vector2_12b56547bcf74355a02d20edb4e2c65f, float2 Vector2_b7de4c69caf443168a9e324784340590, float Vector1_efdf863ee8144f3388cc694918a1b6a2, float Vector1_2ae5d9c7d38740aaa4f7b3d9b7b8c480, Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8 IN, out float4 New_0)
{
    float4 _Property_0a2ff2d2e0394e6eaa898052f87ade27_Out_0 = Color_3db9350e3e7b40a9bc94f9bbca3a3dd6;
    float _Property_2f000554a80149969b3c2860c7084231_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_34dbfcfac9644430809d822d16b6c8ba_Out_2;
    Unity_Step_float(1, _Property_2f000554a80149969b3c2860c7084231_Out_0, _Step_34dbfcfac9644430809d822d16b6c8ba_Out_2);
    float4 _Lerp_ed5a66c8649c4639b1aa0da3a11443df_Out_3;
    Unity_Lerp_float4(_Property_0a2ff2d2e0394e6eaa898052f87ade27_Out_0, _Property_0a2ff2d2e0394e6eaa898052f87ade27_Out_0, (_Step_34dbfcfac9644430809d822d16b6c8ba_Out_2.xxxx), _Lerp_ed5a66c8649c4639b1aa0da3a11443df_Out_3);
    float4 _Property_84f97c52665241c780616b8eec11f2ae_Out_0 = Color_3db9350e3e7b40a9bc94f9bbca3a3dd6;
    float4 _Property_9454a3be96394af29077dcda6c48b597_Out_0 = Color_1;
    float4 _Property_483545d18d5f4539b8f54af022aad178_Out_0 = Vector4_0fca494385ab49d08ce1afc653231f44;
    float _Split_bbbe946faeaa43d28c1c50df81b68d3c_R_1 = _Property_483545d18d5f4539b8f54af022aad178_Out_0[0];
    float _Split_bbbe946faeaa43d28c1c50df81b68d3c_G_2 = _Property_483545d18d5f4539b8f54af022aad178_Out_0[1];
    float _Split_bbbe946faeaa43d28c1c50df81b68d3c_B_3 = _Property_483545d18d5f4539b8f54af022aad178_Out_0[2];
    float _Split_bbbe946faeaa43d28c1c50df81b68d3c_A_4 = _Property_483545d18d5f4539b8f54af022aad178_Out_0[3];
    float2 _Property_ad3f1fcc64fb45249c867a76aaf1da44_Out_0 = Vector2_b8a1e0cfbb9149e2bdbc25f3ed561ed5;
    float2 _Property_ad537c25d88e41139999d006d26d0370_Out_0 = Vector2_12b56547bcf74355a02d20edb4e2c65f;
    float2 _Property_090b63313a3349e284ff5644f05d984d_Out_0 = Vector2_b7de4c69caf443168a9e324784340590;
    float2 _TilingAndOffset_97a1f781dd834eb383791e511321508a_Out_3;
    Unity_TilingAndOffset_float(_Property_ad3f1fcc64fb45249c867a76aaf1da44_Out_0, _Property_ad537c25d88e41139999d006d26d0370_Out_0, _Property_090b63313a3349e284ff5644f05d984d_Out_0, _TilingAndOffset_97a1f781dd834eb383791e511321508a_Out_3);
    float _Property_48d884c264af4804896219d23c110a3b_Out_0 = Vector1_77a87e2cc8354a469656f2490398cbad;
    float2 _Rotate_2bc00d9a15414809ae9553e2a0b634f4_Out_3;
    Unity_Rotate_Degrees_float(_TilingAndOffset_97a1f781dd834eb383791e511321508a_Out_3, float2 (0.5, 0.5), _Property_48d884c264af4804896219d23c110a3b_Out_0, _Rotate_2bc00d9a15414809ae9553e2a0b634f4_Out_3);
    float2 _Fraction_9f0094596cdd414eaded7feed610f446_Out_1;
    Unity_Fraction_float2(_Rotate_2bc00d9a15414809ae9553e2a0b634f4_Out_3, _Fraction_9f0094596cdd414eaded7feed610f446_Out_1);
    float _Property_0e46c85ca817426e8465d2ab85a7f0bc_Out_0 = Vector1_2ae5d9c7d38740aaa4f7b3d9b7b8c480;
    float _Step_ed7719dad07543dab0a5dfc0df8d0816_Out_2;
    Unity_Step_float(1, _Property_0e46c85ca817426e8465d2ab85a7f0bc_Out_0, _Step_ed7719dad07543dab0a5dfc0df8d0816_Out_2);
    float2 _Lerp_4eca02bf050343f3b8f6ca42123798b7_Out_3;
    Unity_Lerp_float2(_Rotate_2bc00d9a15414809ae9553e2a0b634f4_Out_3, _Fraction_9f0094596cdd414eaded7feed610f446_Out_1, (_Step_ed7719dad07543dab0a5dfc0df8d0816_Out_2.xx), _Lerp_4eca02bf050343f3b8f6ca42123798b7_Out_3);
    float2 _Property_ca17b2636f2041c081b20e948a6c2783_Out_0 = Vector2_b8a1e0cfbb9149e2bdbc25f3ed561ed5;
    float _Property_9ec4920518dc4ac88dab6808516a93d6_Out_0 = Vector1_77a87e2cc8354a469656f2490398cbad;
    float2 _Rotate_7b9f51cb8ce8446cbb28ad60ee67e06c_Out_3;
    Unity_Rotate_Degrees_float(_Property_ca17b2636f2041c081b20e948a6c2783_Out_0, float2 (0.5, 0.5), _Property_9ec4920518dc4ac88dab6808516a93d6_Out_0, _Rotate_7b9f51cb8ce8446cbb28ad60ee67e06c_Out_3);
    float2 _PolarCoordinates_8e0d41b0dcf542c8a3fe3d08467b50a2_Out_4;
    Unity_PolarCoordinates_float(_Rotate_7b9f51cb8ce8446cbb28ad60ee67e06c_Out_3, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_8e0d41b0dcf542c8a3fe3d08467b50a2_Out_4);
    float _Split_314599f8f9fd4e12a7ca0f35b26432a2_R_1 = _PolarCoordinates_8e0d41b0dcf542c8a3fe3d08467b50a2_Out_4[0];
    float _Split_314599f8f9fd4e12a7ca0f35b26432a2_G_2 = _PolarCoordinates_8e0d41b0dcf542c8a3fe3d08467b50a2_Out_4[1];
    float _Split_314599f8f9fd4e12a7ca0f35b26432a2_B_3 = 0;
    float _Split_314599f8f9fd4e12a7ca0f35b26432a2_A_4 = 0;
    float _Add_6e6adbb1d35749caa29050ca0de195f1_Out_2;
    Unity_Add_float(_Split_314599f8f9fd4e12a7ca0f35b26432a2_G_2, 0.5, _Add_6e6adbb1d35749caa29050ca0de195f1_Out_2);
    float _Property_1fdab43338484183a632dddd987cfe68_Out_0 = Vector1_2ae5d9c7d38740aaa4f7b3d9b7b8c480;
    float _Step_613aa770562443b3881b56b4ef4de08c_Out_2;
    Unity_Step_float(2, _Property_1fdab43338484183a632dddd987cfe68_Out_0, _Step_613aa770562443b3881b56b4ef4de08c_Out_2);
    float2 _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3;
    Unity_Lerp_float2(_Lerp_4eca02bf050343f3b8f6ca42123798b7_Out_3, (_Add_6e6adbb1d35749caa29050ca0de195f1_Out_2.xx), (_Step_613aa770562443b3881b56b4ef4de08c_Out_2.xx), _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3);
    float _Split_68901e4b7cf84413b35e585c3b5dfeda_R_1 = _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3[0];
    float _Split_68901e4b7cf84413b35e585c3b5dfeda_G_2 = _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3[1];
    float _Split_68901e4b7cf84413b35e585c3b5dfeda_B_3 = 0;
    float _Split_68901e4b7cf84413b35e585c3b5dfeda_A_4 = 0;
    float _Smoothstep_eee463b2abdd424789d45e126a902e48_Out_3;
    Unity_Smoothstep_float(_Split_bbbe946faeaa43d28c1c50df81b68d3c_R_1, _Split_bbbe946faeaa43d28c1c50df81b68d3c_G_2, _Split_68901e4b7cf84413b35e585c3b5dfeda_R_1, _Smoothstep_eee463b2abdd424789d45e126a902e48_Out_3);
    float4 _Lerp_9fc6b45377ad49b4892b2f9259187522_Out_3;
    Unity_Lerp_float4(_Property_84f97c52665241c780616b8eec11f2ae_Out_0, _Property_9454a3be96394af29077dcda6c48b597_Out_0, (_Smoothstep_eee463b2abdd424789d45e126a902e48_Out_3.xxxx), _Lerp_9fc6b45377ad49b4892b2f9259187522_Out_3);
    float _Property_941c36c120ff4f889e5b4d20a73f4060_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_44788e99719149ac98a5937b33c39218_Out_2;
    Unity_Step_float(2, _Property_941c36c120ff4f889e5b4d20a73f4060_Out_0, _Step_44788e99719149ac98a5937b33c39218_Out_2);
    float4 _Lerp_4fd34c2c3d9042a1a5feca25a9be6143_Out_3;
    Unity_Lerp_float4(_Lerp_ed5a66c8649c4639b1aa0da3a11443df_Out_3, _Lerp_9fc6b45377ad49b4892b2f9259187522_Out_3, (_Step_44788e99719149ac98a5937b33c39218_Out_2.xxxx), _Lerp_4fd34c2c3d9042a1a5feca25a9be6143_Out_3);
    float4 _Property_8b146fd01b384c4abfffe2f7ed408b47_Out_0 = Color_2;
    float4 _Property_a662a466cb5f490db51ceb78e55ccb33_Out_0 = Vector4_0fca494385ab49d08ce1afc653231f44;
    float _Split_fcd288f8b5a04b249ee16a9a90c1c71e_R_1 = _Property_a662a466cb5f490db51ceb78e55ccb33_Out_0[0];
    float _Split_fcd288f8b5a04b249ee16a9a90c1c71e_G_2 = _Property_a662a466cb5f490db51ceb78e55ccb33_Out_0[1];
    float _Split_fcd288f8b5a04b249ee16a9a90c1c71e_B_3 = _Property_a662a466cb5f490db51ceb78e55ccb33_Out_0[2];
    float _Split_fcd288f8b5a04b249ee16a9a90c1c71e_A_4 = _Property_a662a466cb5f490db51ceb78e55ccb33_Out_0[3];
    float _Smoothstep_179e98e09e2b4ae18fdcb07ce83826b8_Out_3;
    Unity_Smoothstep_float(_Split_fcd288f8b5a04b249ee16a9a90c1c71e_G_2, _Split_fcd288f8b5a04b249ee16a9a90c1c71e_B_3, _Split_68901e4b7cf84413b35e585c3b5dfeda_R_1, _Smoothstep_179e98e09e2b4ae18fdcb07ce83826b8_Out_3);
    float4 _Lerp_d3b70e4665bf426f8697ce619c142dea_Out_3;
    Unity_Lerp_float4(_Lerp_9fc6b45377ad49b4892b2f9259187522_Out_3, _Property_8b146fd01b384c4abfffe2f7ed408b47_Out_0, (_Smoothstep_179e98e09e2b4ae18fdcb07ce83826b8_Out_3.xxxx), _Lerp_d3b70e4665bf426f8697ce619c142dea_Out_3);
    float _Property_243cf601982e457f9db996297da9d14d_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_f4aac0d29dbf40999a65f3f19e4b2703_Out_2;
    Unity_Step_float(3, _Property_243cf601982e457f9db996297da9d14d_Out_0, _Step_f4aac0d29dbf40999a65f3f19e4b2703_Out_2);
    float4 _Lerp_71c282caf046490a98bb8f1f27e6e7af_Out_3;
    Unity_Lerp_float4(_Lerp_4fd34c2c3d9042a1a5feca25a9be6143_Out_3, _Lerp_d3b70e4665bf426f8697ce619c142dea_Out_3, (_Step_f4aac0d29dbf40999a65f3f19e4b2703_Out_2.xxxx), _Lerp_71c282caf046490a98bb8f1f27e6e7af_Out_3);
    float4 _Property_0fbf3a53751f42e7a9032d5d26420305_Out_0 = Color_3;
    float4 _Property_0272811edb63424eb3f51e7d67dbd582_Out_0 = Vector4_0fca494385ab49d08ce1afc653231f44;
    float _Split_b493ab7d09924c51a6f398821ce96506_R_1 = _Property_0272811edb63424eb3f51e7d67dbd582_Out_0[0];
    float _Split_b493ab7d09924c51a6f398821ce96506_G_2 = _Property_0272811edb63424eb3f51e7d67dbd582_Out_0[1];
    float _Split_b493ab7d09924c51a6f398821ce96506_B_3 = _Property_0272811edb63424eb3f51e7d67dbd582_Out_0[2];
    float _Split_b493ab7d09924c51a6f398821ce96506_A_4 = _Property_0272811edb63424eb3f51e7d67dbd582_Out_0[3];
    float4 _Property_a527f1410869489daf4b4fa407a8455a_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Split_14bf9198190e4cac8bb3b3e5a2b49f61_R_1 = _Property_a527f1410869489daf4b4fa407a8455a_Out_0[0];
    float _Split_14bf9198190e4cac8bb3b3e5a2b49f61_G_2 = _Property_a527f1410869489daf4b4fa407a8455a_Out_0[1];
    float _Split_14bf9198190e4cac8bb3b3e5a2b49f61_B_3 = _Property_a527f1410869489daf4b4fa407a8455a_Out_0[2];
    float _Split_14bf9198190e4cac8bb3b3e5a2b49f61_A_4 = _Property_a527f1410869489daf4b4fa407a8455a_Out_0[3];
    float _Smoothstep_02476caa111d444a89c16f1730e29d7e_Out_3;
    Unity_Smoothstep_float(_Split_b493ab7d09924c51a6f398821ce96506_B_3, _Split_14bf9198190e4cac8bb3b3e5a2b49f61_R_1, _Split_68901e4b7cf84413b35e585c3b5dfeda_R_1, _Smoothstep_02476caa111d444a89c16f1730e29d7e_Out_3);
    float4 _Lerp_f0b78e026da145ed889819a552450c5d_Out_3;
    Unity_Lerp_float4(_Lerp_d3b70e4665bf426f8697ce619c142dea_Out_3, _Property_0fbf3a53751f42e7a9032d5d26420305_Out_0, (_Smoothstep_02476caa111d444a89c16f1730e29d7e_Out_3.xxxx), _Lerp_f0b78e026da145ed889819a552450c5d_Out_3);
    float _Property_fdd70a63f09744249d417aed85264db8_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_286ab944f8714242a5021077f54722f3_Out_2;
    Unity_Step_float(4, _Property_fdd70a63f09744249d417aed85264db8_Out_0, _Step_286ab944f8714242a5021077f54722f3_Out_2);
    float4 _Lerp_17bb08ea8ac341d59f7a078f7b6a217d_Out_3;
    Unity_Lerp_float4(_Lerp_71c282caf046490a98bb8f1f27e6e7af_Out_3, _Lerp_f0b78e026da145ed889819a552450c5d_Out_3, (_Step_286ab944f8714242a5021077f54722f3_Out_2.xxxx), _Lerp_17bb08ea8ac341d59f7a078f7b6a217d_Out_3);
    float4 _Property_e4fb59e181f644d5a5ac4e1e73368f1d_Out_0 = Color_4;
    float4 _Property_8d14c91b46634566bb9975d55f7e0dee_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Split_7fac29de024c416b8ae8aee298b54791_R_1 = _Property_8d14c91b46634566bb9975d55f7e0dee_Out_0[0];
    float _Split_7fac29de024c416b8ae8aee298b54791_G_2 = _Property_8d14c91b46634566bb9975d55f7e0dee_Out_0[1];
    float _Split_7fac29de024c416b8ae8aee298b54791_B_3 = _Property_8d14c91b46634566bb9975d55f7e0dee_Out_0[2];
    float _Split_7fac29de024c416b8ae8aee298b54791_A_4 = _Property_8d14c91b46634566bb9975d55f7e0dee_Out_0[3];
    float _Split_88d34486db7c46c5a684abe326569216_R_1 = _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3[0];
    float _Split_88d34486db7c46c5a684abe326569216_G_2 = _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3[1];
    float _Split_88d34486db7c46c5a684abe326569216_B_3 = 0;
    float _Split_88d34486db7c46c5a684abe326569216_A_4 = 0;
    float _Smoothstep_128a8117ae1e460898903021ccc2dfa0_Out_3;
    Unity_Smoothstep_float(_Split_7fac29de024c416b8ae8aee298b54791_R_1, _Split_7fac29de024c416b8ae8aee298b54791_G_2, _Split_88d34486db7c46c5a684abe326569216_R_1, _Smoothstep_128a8117ae1e460898903021ccc2dfa0_Out_3);
    float4 _Lerp_51cf8dc392dd47a4ab660dcd1eff31ab_Out_3;
    Unity_Lerp_float4(_Lerp_f0b78e026da145ed889819a552450c5d_Out_3, _Property_e4fb59e181f644d5a5ac4e1e73368f1d_Out_0, (_Smoothstep_128a8117ae1e460898903021ccc2dfa0_Out_3.xxxx), _Lerp_51cf8dc392dd47a4ab660dcd1eff31ab_Out_3);
    float _Property_ca74c91638434ca291a3a7fe32a595ff_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_66027715693145a89aabf0e4acc647e4_Out_2;
    Unity_Step_float(5, _Property_ca74c91638434ca291a3a7fe32a595ff_Out_0, _Step_66027715693145a89aabf0e4acc647e4_Out_2);
    float4 _Lerp_6fbac211a45b483fabc3d6e0630625f9_Out_3;
    Unity_Lerp_float4(_Lerp_17bb08ea8ac341d59f7a078f7b6a217d_Out_3, _Lerp_51cf8dc392dd47a4ab660dcd1eff31ab_Out_3, (_Step_66027715693145a89aabf0e4acc647e4_Out_2.xxxx), _Lerp_6fbac211a45b483fabc3d6e0630625f9_Out_3);
    float4 _Property_e36a889e969b4e518b8410bbb2676253_Out_0 = Color_5;
    float4 _Property_06ce1893a6d14cb590ae16f6112171c1_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Split_e76f8b076b754ff3bb75a09da5b62a57_R_1 = _Property_06ce1893a6d14cb590ae16f6112171c1_Out_0[0];
    float _Split_e76f8b076b754ff3bb75a09da5b62a57_G_2 = _Property_06ce1893a6d14cb590ae16f6112171c1_Out_0[1];
    float _Split_e76f8b076b754ff3bb75a09da5b62a57_B_3 = _Property_06ce1893a6d14cb590ae16f6112171c1_Out_0[2];
    float _Split_e76f8b076b754ff3bb75a09da5b62a57_A_4 = _Property_06ce1893a6d14cb590ae16f6112171c1_Out_0[3];
    float _Smoothstep_fe26619796b14054958a264244ecf7f1_Out_3;
    Unity_Smoothstep_float(_Split_e76f8b076b754ff3bb75a09da5b62a57_G_2, _Split_e76f8b076b754ff3bb75a09da5b62a57_B_3, _Split_88d34486db7c46c5a684abe326569216_R_1, _Smoothstep_fe26619796b14054958a264244ecf7f1_Out_3);
    float4 _Lerp_32aa8bcc9c07432e93253e8aef66a5cf_Out_3;
    Unity_Lerp_float4(_Lerp_51cf8dc392dd47a4ab660dcd1eff31ab_Out_3, _Property_e36a889e969b4e518b8410bbb2676253_Out_0, (_Smoothstep_fe26619796b14054958a264244ecf7f1_Out_3.xxxx), _Lerp_32aa8bcc9c07432e93253e8aef66a5cf_Out_3);
    float _Property_1d66f04933ec4271927572c9ff65c09d_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_6a7883714df4493db50acf9252f7f475_Out_2;
    Unity_Step_float(6, _Property_1d66f04933ec4271927572c9ff65c09d_Out_0, _Step_6a7883714df4493db50acf9252f7f475_Out_2);
    float4 _Lerp_0c5530a0dce7482cb05b188833564fa5_Out_3;
    Unity_Lerp_float4(_Lerp_6fbac211a45b483fabc3d6e0630625f9_Out_3, _Lerp_32aa8bcc9c07432e93253e8aef66a5cf_Out_3, (_Step_6a7883714df4493db50acf9252f7f475_Out_2.xxxx), _Lerp_0c5530a0dce7482cb05b188833564fa5_Out_3);
    float4 _Property_d71b8b7b341244c48eaef4b76e736359_Out_0 = Color_6;
    float4 _Property_8a8eb33ecd8b4ab799a00038def60c9d_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Split_0237b9f8004b44f8ac9c4dbdd329b38e_R_1 = _Property_8a8eb33ecd8b4ab799a00038def60c9d_Out_0[0];
    float _Split_0237b9f8004b44f8ac9c4dbdd329b38e_G_2 = _Property_8a8eb33ecd8b4ab799a00038def60c9d_Out_0[1];
    float _Split_0237b9f8004b44f8ac9c4dbdd329b38e_B_3 = _Property_8a8eb33ecd8b4ab799a00038def60c9d_Out_0[2];
    float _Split_0237b9f8004b44f8ac9c4dbdd329b38e_A_4 = _Property_8a8eb33ecd8b4ab799a00038def60c9d_Out_0[3];
    float _Smoothstep_43382a8072774027b21eac2c4aeed755_Out_3;
    Unity_Smoothstep_float(_Split_0237b9f8004b44f8ac9c4dbdd329b38e_B_3, _Split_0237b9f8004b44f8ac9c4dbdd329b38e_A_4, _Split_88d34486db7c46c5a684abe326569216_R_1, _Smoothstep_43382a8072774027b21eac2c4aeed755_Out_3);
    float4 _Lerp_1b8ecaa0e924471fa9e1a0a3ba4838c4_Out_3;
    Unity_Lerp_float4(_Lerp_32aa8bcc9c07432e93253e8aef66a5cf_Out_3, _Property_d71b8b7b341244c48eaef4b76e736359_Out_0, (_Smoothstep_43382a8072774027b21eac2c4aeed755_Out_3.xxxx), _Lerp_1b8ecaa0e924471fa9e1a0a3ba4838c4_Out_3);
    float _Property_bbfd9f6f601747cea47993e59b316c97_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_947780af629a4c0ab06c6c45c337df33_Out_2;
    Unity_Step_float(7, _Property_bbfd9f6f601747cea47993e59b316c97_Out_0, _Step_947780af629a4c0ab06c6c45c337df33_Out_2);
    float4 _Lerp_7bce97d8215b46629bbf3a38c2beddb7_Out_3;
    Unity_Lerp_float4(_Lerp_0c5530a0dce7482cb05b188833564fa5_Out_3, _Lerp_1b8ecaa0e924471fa9e1a0a3ba4838c4_Out_3, (_Step_947780af629a4c0ab06c6c45c337df33_Out_2.xxxx), _Lerp_7bce97d8215b46629bbf3a38c2beddb7_Out_3);
    float4 _Property_c712201a00494d7cbd895c3d3d68d291_Out_0 = Color_7;
    float4 _Property_409fc273f2a34f28b79fa46bd2435e60_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Split_21fef25b64c445aba19ccfc7a0994588_R_1 = _Property_409fc273f2a34f28b79fa46bd2435e60_Out_0[0];
    float _Split_21fef25b64c445aba19ccfc7a0994588_G_2 = _Property_409fc273f2a34f28b79fa46bd2435e60_Out_0[1];
    float _Split_21fef25b64c445aba19ccfc7a0994588_B_3 = _Property_409fc273f2a34f28b79fa46bd2435e60_Out_0[2];
    float _Split_21fef25b64c445aba19ccfc7a0994588_A_4 = _Property_409fc273f2a34f28b79fa46bd2435e60_Out_0[3];
    float _Smoothstep_502377937d454e3ab4554ffad30c9224_Out_3;
    Unity_Smoothstep_float(_Split_21fef25b64c445aba19ccfc7a0994588_A_4, 1, _Split_88d34486db7c46c5a684abe326569216_R_1, _Smoothstep_502377937d454e3ab4554ffad30c9224_Out_3);
    float4 _Lerp_ecf7b0bdc01c4655ad7653e86a99daeb_Out_3;
    Unity_Lerp_float4(_Lerp_1b8ecaa0e924471fa9e1a0a3ba4838c4_Out_3, _Property_c712201a00494d7cbd895c3d3d68d291_Out_0, (_Smoothstep_502377937d454e3ab4554ffad30c9224_Out_3.xxxx), _Lerp_ecf7b0bdc01c4655ad7653e86a99daeb_Out_3);
    float _Property_1ab23f4faf804ab69914ea928afcf93f_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
    float _Step_3f385fe3ad654ec6a8dc138c386126aa_Out_2;
    Unity_Step_float(8, _Property_1ab23f4faf804ab69914ea928afcf93f_Out_0, _Step_3f385fe3ad654ec6a8dc138c386126aa_Out_2);
    float4 _Lerp_1ee4399599984c90ac29f61fc35002a4_Out_3;
    Unity_Lerp_float4(_Lerp_7bce97d8215b46629bbf3a38c2beddb7_Out_3, _Lerp_ecf7b0bdc01c4655ad7653e86a99daeb_Out_3, (_Step_3f385fe3ad654ec6a8dc138c386126aa_Out_2.xxxx), _Lerp_1ee4399599984c90ac29f61fc35002a4_Out_3);
    New_0 = _Lerp_1ee4399599984c90ac29f61fc35002a4_Out_3;
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Rectangle_float(float2 UV, float Width, float Height, out float Out)
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Saturate_float(float In, out float Out)
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
    float4 _Property_62526134cac94aba8676fa0c84f65879_Out_0 = _Color01;
    float4 _Property_477c79e0faaa496a9d4034743291c1c7_Out_0 = _Color02;
    float4 _Property_1fb3b68d47474407b72b3d8a315d1cc4_Out_0 = _Color03;
    float4 _Property_04b94cc94aa0408394f439a856b76d2f_Out_0 = _Color04;
    float4 _Property_c09af223cc0c44199c05d4eae5dd54d4_Out_0 = _Color05;
    float4 _Property_606267184abd47cc9372ac82d23feac0_Out_0 = _Color06;
    float4 _Property_cefebafe8ee040fe97095a640012909f_Out_0 = _Color07;
    float4 _Property_ea9c784caa8d49ce941e70b361c08a49_Out_0 = _Color08;
    float4 _Property_ad79e18fc85a434c8f39d21002055d5a_Out_0 = _GradientPositions01;
    float4 _Property_553e23acff2c4b0c90c412e9d9111757_Out_0 = _GradientPositions02;
    float _Property_d59bb8c26b614802828bdea11252c07a_Out_0 = _Rotation;
    float2 _Property_5eb266a27fbe4041be0f29be934026b4_Out_0 = _Speed;
    float _Split_07b28d6fd17c43b488c30fb147361a80_R_1 = _Property_5eb266a27fbe4041be0f29be934026b4_Out_0[0];
    float _Split_07b28d6fd17c43b488c30fb147361a80_G_2 = _Property_5eb266a27fbe4041be0f29be934026b4_Out_0[1];
    float _Split_07b28d6fd17c43b488c30fb147361a80_B_3 = 0;
    float _Split_07b28d6fd17c43b488c30fb147361a80_A_4 = 0;
    float _Multiply_5c32e6daa86b46c0bba26bc503d416b0_Out_2;
    Unity_Multiply_float(IN.TimeParameters.x, _Split_07b28d6fd17c43b488c30fb147361a80_R_1, _Multiply_5c32e6daa86b46c0bba26bc503d416b0_Out_2);
    float _Step_630aaa5c94dd49dfb60135d02b258d18_Out_2;
    Unity_Step_float(0.01, _Multiply_5c32e6daa86b46c0bba26bc503d416b0_Out_2, _Step_630aaa5c94dd49dfb60135d02b258d18_Out_2);
    float _Lerp_9b90fb8914db4cf5a7bbd5ccc546fff0_Out_3;
    Unity_Lerp_float(_Property_d59bb8c26b614802828bdea11252c07a_Out_0, _Multiply_5c32e6daa86b46c0bba26bc503d416b0_Out_2, _Step_630aaa5c94dd49dfb60135d02b258d18_Out_2, _Lerp_9b90fb8914db4cf5a7bbd5ccc546fff0_Out_3);
    float4 _UV_0904f158ff864e0e9e93bab44e83817a_Out_0 = IN.uv0;
    float2 _TilingAndOffset_f0ce4c837b1845848d33befb6a72fce9_Out_3;
    Unity_TilingAndOffset_float((_UV_0904f158ff864e0e9e93bab44e83817a_Out_0.xy), float2 (1, 1), float2 (0, 0), _TilingAndOffset_f0ce4c837b1845848d33befb6a72fce9_Out_3);
    float2 _Fraction_1f89fae6b5604b85bf2eac4cea72f0a0_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_f0ce4c837b1845848d33befb6a72fce9_Out_3, _Fraction_1f89fae6b5604b85bf2eac4cea72f0a0_Out_1);
    float _Property_b61779cde42b4fbea3e551a4ee5d73d0_Out_0 = _ColorAmount;
    Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8 _SGCustomGradient_cfbd3021c3c542249af6bf8136dee314;
    float4 _SGCustomGradient_cfbd3021c3c542249af6bf8136dee314_New_0;
    SG_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8(_Property_62526134cac94aba8676fa0c84f65879_Out_0, _Property_477c79e0faaa496a9d4034743291c1c7_Out_0, _Property_1fb3b68d47474407b72b3d8a315d1cc4_Out_0, _Property_04b94cc94aa0408394f439a856b76d2f_Out_0, _Property_c09af223cc0c44199c05d4eae5dd54d4_Out_0, _Property_606267184abd47cc9372ac82d23feac0_Out_0, _Property_cefebafe8ee040fe97095a640012909f_Out_0, _Property_ea9c784caa8d49ce941e70b361c08a49_Out_0, _Property_ad79e18fc85a434c8f39d21002055d5a_Out_0, _Property_553e23acff2c4b0c90c412e9d9111757_Out_0, _Lerp_9b90fb8914db4cf5a7bbd5ccc546fff0_Out_3, _Fraction_1f89fae6b5604b85bf2eac4cea72f0a0_Out_1, float2 (1, 1), float2 (0, 0), _Property_b61779cde42b4fbea3e551a4ee5d73d0_Out_0, 2, _SGCustomGradient_cfbd3021c3c542249af6bf8136dee314, _SGCustomGradient_cfbd3021c3c542249af6bf8136dee314_New_0);
    UnityTexture2D _Property_a7311642d00b42959535635b88082c56_Out_0 = UnityBuildTexture2DStructNoScale(_Ramp);
    float2 _Property_7ac0b13b0f0842d2909e2e9216c02e87_Out_0 = _Speed;
    float2 _Multiply_24a2ec0cc7214ffb9a9527125b870090_Out_2;
    Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_7ac0b13b0f0842d2909e2e9216c02e87_Out_0, _Multiply_24a2ec0cc7214ffb9a9527125b870090_Out_2);
    float2 _Rotate_685c277188004fd5b23e44106bc9f428_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), (_Multiply_24a2ec0cc7214ffb9a9527125b870090_Out_2).x, _Rotate_685c277188004fd5b23e44106bc9f428_Out_3);
    float _Split_f7289cbf88a842a8b501a90bcd9d778b_R_1 = _Rotate_685c277188004fd5b23e44106bc9f428_Out_3[0];
    float _Split_f7289cbf88a842a8b501a90bcd9d778b_G_2 = _Rotate_685c277188004fd5b23e44106bc9f428_Out_3[1];
    float _Split_f7289cbf88a842a8b501a90bcd9d778b_B_3 = 0;
    float _Split_f7289cbf88a842a8b501a90bcd9d778b_A_4 = 0;
    float _OneMinus_e75135fc95b24bd5b87939861efd9952_Out_1;
    Unity_OneMinus_float(_Split_f7289cbf88a842a8b501a90bcd9d778b_R_1, _OneMinus_e75135fc95b24bd5b87939861efd9952_Out_1);
    float2 _Vector2_5bd1b402e1fe4e0baa1389a6b2b11699_Out_0 = float2(_OneMinus_e75135fc95b24bd5b87939861efd9952_Out_1, _Split_f7289cbf88a842a8b501a90bcd9d778b_G_2);
    float2 _PolarCoordinates_cb6915ba7093498dbd2001d96c8fb897_Out_4;
    Unity_PolarCoordinates_float(_Vector2_5bd1b402e1fe4e0baa1389a6b2b11699_Out_0, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_cb6915ba7093498dbd2001d96c8fb897_Out_4);
    float _Split_02d6f31be8694684858cd192b4bacaba_R_1 = _PolarCoordinates_cb6915ba7093498dbd2001d96c8fb897_Out_4[0];
    float _Split_02d6f31be8694684858cd192b4bacaba_G_2 = _PolarCoordinates_cb6915ba7093498dbd2001d96c8fb897_Out_4[1];
    float _Split_02d6f31be8694684858cd192b4bacaba_B_3 = 0;
    float _Split_02d6f31be8694684858cd192b4bacaba_A_4 = 0;
    float _Add_8b231e94209049c2bdd53f03228a0295_Out_2;
    Unity_Add_float(_Split_02d6f31be8694684858cd192b4bacaba_G_2, 0.5, _Add_8b231e94209049c2bdd53f03228a0295_Out_2);
    float4 _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0 = SAMPLE_TEXTURE2D(_Property_a7311642d00b42959535635b88082c56_Out_0.tex, _Property_a7311642d00b42959535635b88082c56_Out_0.samplerstate, (_Add_8b231e94209049c2bdd53f03228a0295_Out_2.xx));
    float _SampleTexture2D_11b9032f513a4306885ed14965c973e6_R_4 = _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0.r;
    float _SampleTexture2D_11b9032f513a4306885ed14965c973e6_G_5 = _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0.g;
    float _SampleTexture2D_11b9032f513a4306885ed14965c973e6_B_6 = _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0.b;
    float _SampleTexture2D_11b9032f513a4306885ed14965c973e6_A_7 = _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0.a;
    float _Property_ca5c7c3b7eb9490994ba68e23ee87a43_Out_0 = _UseTexture;
    float _Step_a2759376e99046afa588bee996210bcc_Out_2;
    Unity_Step_float(1, _Property_ca5c7c3b7eb9490994ba68e23ee87a43_Out_0, _Step_a2759376e99046afa588bee996210bcc_Out_2);
    float4 _Lerp_c0403dafde8747ee931186268ba1dc3d_Out_3;
    Unity_Lerp_float4(_SGCustomGradient_cfbd3021c3c542249af6bf8136dee314_New_0, _SampleTexture2D_11b9032f513a4306885ed14965c973e6_RGBA_0, (_Step_a2759376e99046afa588bee996210bcc_Out_2.xxxx), _Lerp_c0403dafde8747ee931186268ba1dc3d_Out_3);
    float4 _Property_aa0f73601f7648859cb2d7c603bae46f_Out_0 = _InnerColor;
    UnityTexture2D _Property_9b9b783269f94e6f8030cd2a50295a79_Out_0 = UnityBuildTexture2DStructNoScale(_Mask);
    float2 _Property_7085c65f55ea492888a28fad61369a22_Out_0 = _FrameThickness;
    float _Split_796152b39d5f4b8c8a5e4b92fb1ecf75_R_1 = _Property_7085c65f55ea492888a28fad61369a22_Out_0[0];
    float _Split_796152b39d5f4b8c8a5e4b92fb1ecf75_G_2 = _Property_7085c65f55ea492888a28fad61369a22_Out_0[1];
    float _Split_796152b39d5f4b8c8a5e4b92fb1ecf75_B_3 = 0;
    float _Split_796152b39d5f4b8c8a5e4b92fb1ecf75_A_4 = 0;
    float _Multiply_1529b3b184234cefaf414ade0331fe28_Out_2;
    Unity_Multiply_float(_Split_796152b39d5f4b8c8a5e4b92fb1ecf75_R_1, -1, _Multiply_1529b3b184234cefaf414ade0331fe28_Out_2);
    float _Divide_59c865b4a9f540b8b9f681d5bfe52c58_Out_2;
    Unity_Divide_float(_Multiply_1529b3b184234cefaf414ade0331fe28_Out_2, 2, _Divide_59c865b4a9f540b8b9f681d5bfe52c58_Out_2);
    float _Add_3aeab0f33bb74db0bb3d28dce9a1b54c_Out_2;
    Unity_Add_float(_Divide_59c865b4a9f540b8b9f681d5bfe52c58_Out_2, 0.5, _Add_3aeab0f33bb74db0bb3d28dce9a1b54c_Out_2);
    float _Multiply_fcbb3fe044714a07b58a58e20005f0be_Out_2;
    Unity_Multiply_float(_Split_796152b39d5f4b8c8a5e4b92fb1ecf75_G_2, -1, _Multiply_fcbb3fe044714a07b58a58e20005f0be_Out_2);
    float _Divide_4e648c33f8d3465984eff65445c0c781_Out_2;
    Unity_Divide_float(_Multiply_fcbb3fe044714a07b58a58e20005f0be_Out_2, 2, _Divide_4e648c33f8d3465984eff65445c0c781_Out_2);
    float _Add_c8db7f8193fd481aa5cc0fdf106594f4_Out_2;
    Unity_Add_float(_Divide_4e648c33f8d3465984eff65445c0c781_Out_2, 0.5, _Add_c8db7f8193fd481aa5cc0fdf106594f4_Out_2);
    float2 _Vector2_9164475dd9ae4d46b7fb285d59e06fbb_Out_0 = float2(_Add_3aeab0f33bb74db0bb3d28dce9a1b54c_Out_2, _Add_c8db7f8193fd481aa5cc0fdf106594f4_Out_2);
    float2 _TilingAndOffset_616af8dba9f542008493dceec7d691d4_Out_3;
    Unity_TilingAndOffset_float(IN.uv0.xy, _Property_7085c65f55ea492888a28fad61369a22_Out_0, _Vector2_9164475dd9ae4d46b7fb285d59e06fbb_Out_0, _TilingAndOffset_616af8dba9f542008493dceec7d691d4_Out_3);
    float4 _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9b9b783269f94e6f8030cd2a50295a79_Out_0.tex, _Property_9b9b783269f94e6f8030cd2a50295a79_Out_0.samplerstate, _TilingAndOffset_616af8dba9f542008493dceec7d691d4_Out_3);
    float _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_R_4 = _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_RGBA_0.r;
    float _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_G_5 = _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_RGBA_0.g;
    float _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_B_6 = _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_RGBA_0.b;
    float _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_A_7 = _SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_RGBA_0.a;
    float _Rectangle_5e295f8971fe43fbb322860a074e782a_Out_3;
    Unity_Rectangle_float(_TilingAndOffset_616af8dba9f542008493dceec7d691d4_Out_3, 1, 1, _Rectangle_5e295f8971fe43fbb322860a074e782a_Out_3);
    float _OneMinus_11f4203469d343218c733f1ea805f74b_Out_1;
    Unity_OneMinus_float(_Rectangle_5e295f8971fe43fbb322860a074e782a_Out_3, _OneMinus_11f4203469d343218c733f1ea805f74b_Out_1);
    float _Subtract_2be32d5243794e1d87bb3f964e6f8f8f_Out_2;
    Unity_Subtract_float(_SampleTexture2D_1832d5f57cea4aa3b02108ccd69c0ac7_A_7, _OneMinus_11f4203469d343218c733f1ea805f74b_Out_1, _Subtract_2be32d5243794e1d87bb3f964e6f8f8f_Out_2);
    float _Saturate_ee82bf3d98954a25a875b6e0e174efc6_Out_1;
    Unity_Saturate_float(_Subtract_2be32d5243794e1d87bb3f964e6f8f8f_Out_2, _Saturate_ee82bf3d98954a25a875b6e0e174efc6_Out_1);
    float4 _Lerp_29d871897c174ca6a675b71ba58ca50f_Out_3;
    Unity_Lerp_float4(_Lerp_c0403dafde8747ee931186268ba1dc3d_Out_3, _Property_aa0f73601f7648859cb2d7c603bae46f_Out_0, (_Saturate_ee82bf3d98954a25a875b6e0e174efc6_Out_1.xxxx), _Lerp_29d871897c174ca6a675b71ba58ca50f_Out_3);
    UnityTexture2D _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0 = UnityBuildTexture2DStructNoScale(_Mask);
    float4 _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0 = SAMPLE_TEXTURE2D(_Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.tex, _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_R_4 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.r;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_G_5 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.g;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_B_6 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.b;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.a;
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_R_1 = IN.VertexColor[0];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_G_2 = IN.VertexColor[1];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_B_3 = IN.VertexColor[2];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4 = IN.VertexColor[3];
    float _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    Unity_Multiply_float(_SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7, _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4, _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2);
    surface.BaseColor = (_Lerp_29d871897c174ca6a675b71ba58ca50f_Out_3.xyz);
    surface.Alpha = _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
    output.VertexColor = input.color;
    output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
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
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
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
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float4 texCoord0;
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
        float4 uv0;
        float4 VertexColor;
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
        float4 interp0 : TEXCOORD0;
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

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 _Mask_TexelSize;
float2 _FrameThickness;
float4 _InnerColor;
float4 _Ramp_TexelSize;
float2 _Speed;
float _Rotation;
float4 _Color01;
float4 _Color02;
float4 _Color03;
float4 _Color04;
float4 _Color05;
float4 _Color06;
float4 _Color07;
float4 _Color08;
float4 _GradientPositions01;
float4 _GradientPositions02;
float _ColorAmount;
float _UseTexture;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_Mask);
SAMPLER(sampler_Mask);
TEXTURE2D(_Ramp);
SAMPLER(sampler_Ramp);

// Graph Functions

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    UnityTexture2D _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0 = UnityBuildTexture2DStructNoScale(_Mask);
    float4 _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0 = SAMPLE_TEXTURE2D(_Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.tex, _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_R_4 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.r;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_G_5 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.g;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_B_6 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.b;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.a;
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_R_1 = IN.VertexColor[0];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_G_2 = IN.VertexColor[1];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_B_3 = IN.VertexColor[2];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4 = IN.VertexColor[3];
    float _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    Unity_Multiply_float(_SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7, _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4, _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2);
    surface.Alpha = _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
    output.VertexColor = input.color;
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
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
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
        float4 color : COLOR;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float4 texCoord0;
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
        float4 uv0;
        float4 VertexColor;
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
        float4 interp0 : TEXCOORD0;
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

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 _Mask_TexelSize;
float2 _FrameThickness;
float4 _InnerColor;
float4 _Ramp_TexelSize;
float2 _Speed;
float _Rotation;
float4 _Color01;
float4 _Color02;
float4 _Color03;
float4 _Color04;
float4 _Color05;
float4 _Color06;
float4 _Color07;
float4 _Color08;
float4 _GradientPositions01;
float4 _GradientPositions02;
float _ColorAmount;
float _UseTexture;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_Mask);
SAMPLER(sampler_Mask);
TEXTURE2D(_Ramp);
SAMPLER(sampler_Ramp);

// Graph Functions

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    UnityTexture2D _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0 = UnityBuildTexture2DStructNoScale(_Mask);
    float4 _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0 = SAMPLE_TEXTURE2D(_Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.tex, _Property_7f6d693f30b8401cbb77910cdd171aa2_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_R_4 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.r;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_G_5 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.g;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_B_6 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.b;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.a;
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_R_1 = IN.VertexColor[0];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_G_2 = IN.VertexColor[1];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_B_3 = IN.VertexColor[2];
    float _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4 = IN.VertexColor[3];
    float _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    Unity_Multiply_float(_SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7, _Split_7596bf6ad75e4ef8ae642726b36ed96e_A_4, _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2);
    surface.Alpha = _Multiply_506687ea9f724d0399df6222ffc0e448_Out_2;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
    output.VertexColor = input.color;
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