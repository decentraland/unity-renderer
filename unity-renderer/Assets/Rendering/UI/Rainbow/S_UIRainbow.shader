Shader "Unlit/S_UIRainbow"
{
    Properties
    {
        [NoScaleOffset] _Mask("Mask", 2D) = "white" {}
        [NoScaleOffset]_Ramp("Ramp", 2D) = "white" {}
        _Fill("Fill", Range(0, 1)) = 1
        _Speed("Speed", Vector) = (0, 0, 0, 0)
        _Rotation("Rotation", Float) = 0
        _Color01("Color01", Color) = (1, 0, 0.3212681, 1)
        _Color02("Color02", Color) = (0.07497215, 0, 1, 1)
        _Color03("Color03", Color) = (0, 1, 0.6875515, 1)
        _Color04("Color04", Color) = (1, 0, 0.3215686, 1)
        _Color05("Color05", Color) = (0.3667524, 1, 0, 1)
        _Color06("Color06", Color) = (1, 0, 0.3399687, 1)
        _Color07("Color07", Color) = (1, 0.4366035, 0, 1)
        _Color08("Color08", Color) = (1, 0.8461649, 0, 1)
        _GradientPositions01("GradientPositions01", Vector) = (0, 0.4, 0.5, 0.98)
        _GradientPositions02("GradientPositions02", Vector) = (0.571, 0.714, 0.857, 1)
        _ColorAmount("ColorAmount", Int) = 4
        _UseTexture("UseTexture", Int) = 0
        _GradientMode("GradientMode", Float) = 0
        _FillDirection("FillDirection", Float) = 0
        [NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}
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
float4 _Ramp_TexelSize;
float _Fill;
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
float _GradientMode;
float _FillDirection;
float4 _MainTex_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_Mask);
SAMPLER(sampler_Mask);
TEXTURE2D(_Ramp);
SAMPLER(sampler_Ramp);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
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

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Step_float(float Edge, float In, out float Out)
{
    Out = step(Edge, In);
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

// a89ee8db4fc65ba871c645adea7088a7
#include "Assets/Rendering/UI/Rainbow/CustomGradient.hlsl"

struct Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8
{
};

void SG_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8(float4 Color_3db9350e3e7b40a9bc94f9bbca3a3dd6, float4 Color_1, float4 Color_2, float4 Color_3, float4 Color_4, float4 Color_5, float4 Color_6, float4 Color_7, float4 Vector4_0fca494385ab49d08ce1afc653231f44, float4 Vector4_b4f550cbc1ab45e4b200ea689ba2ec50, float Vector1_77a87e2cc8354a469656f2490398cbad, float2 Vector2_b8a1e0cfbb9149e2bdbc25f3ed561ed5, float2 Vector2_12b56547bcf74355a02d20edb4e2c65f, float2 Vector2_b7de4c69caf443168a9e324784340590, float Vector1_efdf863ee8144f3388cc694918a1b6a2, float Vector1_2ae5d9c7d38740aaa4f7b3d9b7b8c480, Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8 IN, out float4 New_0)
{
    float4 _Property_b992de34ec03483aae62afaf8d81035f_Out_0 = Color_3db9350e3e7b40a9bc94f9bbca3a3dd6;
    float4 _Property_d13dff627eeb401fa5dee43d7ad56ae0_Out_0 = Color_1;
    float4 _Property_ef681230a5984861ad2df2b6b1a0c187_Out_0 = Color_2;
    float4 _Property_15b7db87469b40ecadb31b8933086732_Out_0 = Color_3;
    float4 _Property_068a81962d08431197a8c5bf042618c8_Out_0 = Color_4;
    float4 _Property_bbf2cc8d24144c0bae15065ad3538ec1_Out_0 = Color_5;
    float4 _Property_b88ec54b08ce400ea74c2ff92fef7f8b_Out_0 = Color_6;
    float4 _Property_1d5fc92b2ce14e5c9447158f50da3289_Out_0 = Color_7;
    float4 _Property_320fcd83a6c747caa0eec2d64355b295_Out_0 = Vector4_0fca494385ab49d08ce1afc653231f44;
    float4 _Property_0a1123e125bf4497889d31acfd46ddda_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Property_7c523a20416c47318b70f09b26e4abfe_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
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
    float4 _SampleGradientCustomFunction_736a22e593224a91a6c829d956178b50_Out_0;
    SampleGradient_float(_Property_b992de34ec03483aae62afaf8d81035f_Out_0, _Property_d13dff627eeb401fa5dee43d7ad56ae0_Out_0, _Property_ef681230a5984861ad2df2b6b1a0c187_Out_0, _Property_15b7db87469b40ecadb31b8933086732_Out_0, _Property_068a81962d08431197a8c5bf042618c8_Out_0, _Property_bbf2cc8d24144c0bae15065ad3538ec1_Out_0, _Property_b88ec54b08ce400ea74c2ff92fef7f8b_Out_0, _Property_1d5fc92b2ce14e5c9447158f50da3289_Out_0, _Property_320fcd83a6c747caa0eec2d64355b295_Out_0, _Property_0a1123e125bf4497889d31acfd46ddda_Out_0, _Property_7c523a20416c47318b70f09b26e4abfe_Out_0, _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3, _SampleGradientCustomFunction_736a22e593224a91a6c829d956178b50_Out_0);
    New_0 = _SampleGradientCustomFunction_736a22e593224a91a6c829d956178b50_Out_0;
}

void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
{
    Out = lerp(A, B, T);
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

// 3f26f3a32894c8cd90039a1c79b4a55a
#include "Assets/Rendering/UI/Rainbow/CustomGradientFill.hlsl"

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
    float4 _Property_dcb46586fadd4dc2bc116e8a16a81e4e_Out_0 = _Color01;
    float4 _Property_c55aac4b1e594335b208c53f6ebdab8d_Out_0 = _Color02;
    float4 _Property_31fd74bfba5c47c5bd718d8a36d356ad_Out_0 = _Color03;
    float4 _Property_fe5ba11ebcd8464cb7a8389438d4241d_Out_0 = _Color04;
    float4 _Property_8cd4eb6b02954f26b4a60236327ff45d_Out_0 = _Color05;
    float4 _Property_20244f23bc1745d88f385b7066ef76af_Out_0 = _Color06;
    float4 _Property_ee01d0a3b2fb412f9df0e25f728775eb_Out_0 = _Color07;
    float4 _Property_f82baa231e61476083e8ec20883e4880_Out_0 = _Color08;
    float4 _Property_345576a774184e23ac56d4cc1c6c94e1_Out_0 = _GradientPositions01;
    float4 _Property_e24a2b974ce04ef69a36e8db9a6a0656_Out_0 = _GradientPositions02;
    float _Property_0e62fc1188ec4ec684340ce36f72967e_Out_0 = _Rotation;
    float4 _UV_751d447d2aff49c79383613184793eaa_Out_0 = IN.uv0;
    float2 _Property_cb54eefaf33d4d2da9aa6c44a4a44a25_Out_0 = _Speed;
    float2 _Multiply_b022e89f3c7b4b2da34d20aaa3990648_Out_2;
    Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_cb54eefaf33d4d2da9aa6c44a4a44a25_Out_0, _Multiply_b022e89f3c7b4b2da34d20aaa3990648_Out_2);
    float _Property_83247cc06db0450aa518444b92db7772_Out_0 = _ColorAmount;
    Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8 _SGCustomGradient_11ee6933b2e54925a65fb2a586e6229f;
    float4 _SGCustomGradient_11ee6933b2e54925a65fb2a586e6229f_New_0;
    SG_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8(_Property_dcb46586fadd4dc2bc116e8a16a81e4e_Out_0, _Property_c55aac4b1e594335b208c53f6ebdab8d_Out_0, _Property_31fd74bfba5c47c5bd718d8a36d356ad_Out_0, _Property_fe5ba11ebcd8464cb7a8389438d4241d_Out_0, _Property_8cd4eb6b02954f26b4a60236327ff45d_Out_0, _Property_20244f23bc1745d88f385b7066ef76af_Out_0, _Property_ee01d0a3b2fb412f9df0e25f728775eb_Out_0, _Property_f82baa231e61476083e8ec20883e4880_Out_0, _Property_345576a774184e23ac56d4cc1c6c94e1_Out_0, _Property_e24a2b974ce04ef69a36e8db9a6a0656_Out_0, _Property_0e62fc1188ec4ec684340ce36f72967e_Out_0, (_UV_751d447d2aff49c79383613184793eaa_Out_0.xy), float2 (1, 1), _Multiply_b022e89f3c7b4b2da34d20aaa3990648_Out_2, _Property_83247cc06db0450aa518444b92db7772_Out_0, 1, _SGCustomGradient_11ee6933b2e54925a65fb2a586e6229f, _SGCustomGradient_11ee6933b2e54925a65fb2a586e6229f_New_0);
    UnityTexture2D _Property_04d0c6d168534ec38a7a2b6f1bcf85d0_Out_0 = UnityBuildTexture2DStructNoScale(_Ramp);
    float2 _TilingAndOffset_a0c2e8b3c22247469d4e7292158a5db8_Out_3;
    Unity_TilingAndOffset_float((_UV_751d447d2aff49c79383613184793eaa_Out_0.xy), float2 (1, 1), _Multiply_b022e89f3c7b4b2da34d20aaa3990648_Out_2, _TilingAndOffset_a0c2e8b3c22247469d4e7292158a5db8_Out_3);
    float4 _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0 = SAMPLE_TEXTURE2D(_Property_04d0c6d168534ec38a7a2b6f1bcf85d0_Out_0.tex, _Property_04d0c6d168534ec38a7a2b6f1bcf85d0_Out_0.samplerstate, _TilingAndOffset_a0c2e8b3c22247469d4e7292158a5db8_Out_3);
    float _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_R_4 = _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0.r;
    float _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_G_5 = _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0.g;
    float _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_B_6 = _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0.b;
    float _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_A_7 = _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0.a;
    float _Property_a7f0dcabc38646f1b4c9f150cfdce451_Out_0 = _UseTexture;
    float _Step_7e913cae8fe44466b0ead5bd4c075e35_Out_2;
    Unity_Step_float(1, _Property_a7f0dcabc38646f1b4c9f150cfdce451_Out_0, _Step_7e913cae8fe44466b0ead5bd4c075e35_Out_2);
    float4 _Lerp_226c2460881d4e9d88c061f3bf6da2b7_Out_3;
    Unity_Lerp_float4(_SGCustomGradient_11ee6933b2e54925a65fb2a586e6229f_New_0, _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0, (_Step_7e913cae8fe44466b0ead5bd4c075e35_Out_2.xxxx), _Lerp_226c2460881d4e9d88c061f3bf6da2b7_Out_3);
    UnityTexture2D _Property_331bce0a859d43c8b4de8c3997988b6b_Out_0 = UnityBuildTexture2DStructNoScale(_Mask);
    float4 _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0 = SAMPLE_TEXTURE2D(_Property_331bce0a859d43c8b4de8c3997988b6b_Out_0.tex, _Property_331bce0a859d43c8b4de8c3997988b6b_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_R_4 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.r;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_G_5 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.g;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_B_6 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.b;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.a;
    float _Split_05d9212f9c5a46bfaa1216c01464649d_R_1 = IN.VertexColor[0];
    float _Split_05d9212f9c5a46bfaa1216c01464649d_G_2 = IN.VertexColor[1];
    float _Split_05d9212f9c5a46bfaa1216c01464649d_B_3 = IN.VertexColor[2];
    float _Split_05d9212f9c5a46bfaa1216c01464649d_A_4 = IN.VertexColor[3];
    float _Multiply_cdf08e66ce82418ab6ea28efe8fae1b3_Out_2;
    Unity_Multiply_float(_SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7, _Split_05d9212f9c5a46bfaa1216c01464649d_A_4, _Multiply_cdf08e66ce82418ab6ea28efe8fae1b3_Out_2);
    float _Property_8c7d1e1a34f043d784e2e1940c241952_Out_0 = _Fill;
    float _Property_8212b593471649d6abc410729b20074a_Out_0 = _FillDirection;
    float4 _UV_778cca2eecd942fd92a102a75cccc785_Out_0 = IN.uv0;
    float _FillCustomFunction_a3c9e80ff4354cbe8e43a7576ac66cf6_Out_2;
    Fill_float(_Property_8c7d1e1a34f043d784e2e1940c241952_Out_0, _Property_8212b593471649d6abc410729b20074a_Out_0, (_UV_778cca2eecd942fd92a102a75cccc785_Out_0.xy), _FillCustomFunction_a3c9e80ff4354cbe8e43a7576ac66cf6_Out_2);
    float _Multiply_5c470095b80346f0a6905e75ebcd9178_Out_2;
    Unity_Multiply_float(_Multiply_cdf08e66ce82418ab6ea28efe8fae1b3_Out_2, _FillCustomFunction_a3c9e80ff4354cbe8e43a7576ac66cf6_Out_2, _Multiply_5c470095b80346f0a6905e75ebcd9178_Out_2);
    surface.BaseColor = (_Lerp_226c2460881d4e9d88c061f3bf6da2b7_Out_3.xyz);
    surface.Alpha = _Multiply_5c470095b80346f0a6905e75ebcd9178_Out_2;
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
float4 _Ramp_TexelSize;
float _Fill;
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
float _GradientMode;
float _FillDirection;
float4 _MainTex_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_Mask);
SAMPLER(sampler_Mask);
TEXTURE2D(_Ramp);
SAMPLER(sampler_Ramp);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
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

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Step_float(float Edge, float In, out float Out)
{
    Out = step(Edge, In);
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

// a89ee8db4fc65ba871c645adea7088a7
#include "Assets/Rendering/UI/Rainbow/CustomGradient.hlsl"

struct Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8
{
};

void SG_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8(float4 Color_3db9350e3e7b40a9bc94f9bbca3a3dd6, float4 Color_1, float4 Color_2, float4 Color_3, float4 Color_4, float4 Color_5, float4 Color_6, float4 Color_7, float4 Vector4_0fca494385ab49d08ce1afc653231f44, float4 Vector4_b4f550cbc1ab45e4b200ea689ba2ec50, float Vector1_77a87e2cc8354a469656f2490398cbad, float2 Vector2_b8a1e0cfbb9149e2bdbc25f3ed561ed5, float2 Vector2_12b56547bcf74355a02d20edb4e2c65f, float2 Vector2_b7de4c69caf443168a9e324784340590, float Vector1_efdf863ee8144f3388cc694918a1b6a2, float Vector1_2ae5d9c7d38740aaa4f7b3d9b7b8c480, Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8 IN, out float4 New_0)
{
    float4 _Property_b992de34ec03483aae62afaf8d81035f_Out_0 = Color_3db9350e3e7b40a9bc94f9bbca3a3dd6;
    float4 _Property_d13dff627eeb401fa5dee43d7ad56ae0_Out_0 = Color_1;
    float4 _Property_ef681230a5984861ad2df2b6b1a0c187_Out_0 = Color_2;
    float4 _Property_15b7db87469b40ecadb31b8933086732_Out_0 = Color_3;
    float4 _Property_068a81962d08431197a8c5bf042618c8_Out_0 = Color_4;
    float4 _Property_bbf2cc8d24144c0bae15065ad3538ec1_Out_0 = Color_5;
    float4 _Property_b88ec54b08ce400ea74c2ff92fef7f8b_Out_0 = Color_6;
    float4 _Property_1d5fc92b2ce14e5c9447158f50da3289_Out_0 = Color_7;
    float4 _Property_320fcd83a6c747caa0eec2d64355b295_Out_0 = Vector4_0fca494385ab49d08ce1afc653231f44;
    float4 _Property_0a1123e125bf4497889d31acfd46ddda_Out_0 = Vector4_b4f550cbc1ab45e4b200ea689ba2ec50;
    float _Property_7c523a20416c47318b70f09b26e4abfe_Out_0 = Vector1_efdf863ee8144f3388cc694918a1b6a2;
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
    float4 _SampleGradientCustomFunction_736a22e593224a91a6c829d956178b50_Out_0;
    SampleGradient_float(_Property_b992de34ec03483aae62afaf8d81035f_Out_0, _Property_d13dff627eeb401fa5dee43d7ad56ae0_Out_0, _Property_ef681230a5984861ad2df2b6b1a0c187_Out_0, _Property_15b7db87469b40ecadb31b8933086732_Out_0, _Property_068a81962d08431197a8c5bf042618c8_Out_0, _Property_bbf2cc8d24144c0bae15065ad3538ec1_Out_0, _Property_b88ec54b08ce400ea74c2ff92fef7f8b_Out_0, _Property_1d5fc92b2ce14e5c9447158f50da3289_Out_0, _Property_320fcd83a6c747caa0eec2d64355b295_Out_0, _Property_0a1123e125bf4497889d31acfd46ddda_Out_0, _Property_7c523a20416c47318b70f09b26e4abfe_Out_0, _Lerp_e17baec2b83942fc8b85b55516ab429e_Out_3, _SampleGradientCustomFunction_736a22e593224a91a6c829d956178b50_Out_0);
    New_0 = _SampleGradientCustomFunction_736a22e593224a91a6c829d956178b50_Out_0;
}

void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
{
    Out = lerp(A, B, T);
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

// 3f26f3a32894c8cd90039a1c79b4a55a
#include "Assets/Rendering/UI/Rainbow/CustomGradientFill.hlsl"

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
    float4 _Property_dcb46586fadd4dc2bc116e8a16a81e4e_Out_0 = _Color01;
    float4 _Property_c55aac4b1e594335b208c53f6ebdab8d_Out_0 = _Color02;
    float4 _Property_31fd74bfba5c47c5bd718d8a36d356ad_Out_0 = _Color03;
    float4 _Property_fe5ba11ebcd8464cb7a8389438d4241d_Out_0 = _Color04;
    float4 _Property_8cd4eb6b02954f26b4a60236327ff45d_Out_0 = _Color05;
    float4 _Property_20244f23bc1745d88f385b7066ef76af_Out_0 = _Color06;
    float4 _Property_ee01d0a3b2fb412f9df0e25f728775eb_Out_0 = _Color07;
    float4 _Property_f82baa231e61476083e8ec20883e4880_Out_0 = _Color08;
    float4 _Property_345576a774184e23ac56d4cc1c6c94e1_Out_0 = _GradientPositions01;
    float4 _Property_e24a2b974ce04ef69a36e8db9a6a0656_Out_0 = _GradientPositions02;
    float _Property_0e62fc1188ec4ec684340ce36f72967e_Out_0 = _Rotation;
    float4 _UV_751d447d2aff49c79383613184793eaa_Out_0 = IN.uv0;
    float2 _Property_cb54eefaf33d4d2da9aa6c44a4a44a25_Out_0 = _Speed;
    float2 _Multiply_b022e89f3c7b4b2da34d20aaa3990648_Out_2;
    Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_cb54eefaf33d4d2da9aa6c44a4a44a25_Out_0, _Multiply_b022e89f3c7b4b2da34d20aaa3990648_Out_2);
    float _Property_83247cc06db0450aa518444b92db7772_Out_0 = _ColorAmount;
    Bindings_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8 _SGCustomGradient_11ee6933b2e54925a65fb2a586e6229f;
    float4 _SGCustomGradient_11ee6933b2e54925a65fb2a586e6229f_New_0;
    SG_SGCustomGradient_32f40084ffeebc24f9332f16b5470ef8(_Property_dcb46586fadd4dc2bc116e8a16a81e4e_Out_0, _Property_c55aac4b1e594335b208c53f6ebdab8d_Out_0, _Property_31fd74bfba5c47c5bd718d8a36d356ad_Out_0, _Property_fe5ba11ebcd8464cb7a8389438d4241d_Out_0, _Property_8cd4eb6b02954f26b4a60236327ff45d_Out_0, _Property_20244f23bc1745d88f385b7066ef76af_Out_0, _Property_ee01d0a3b2fb412f9df0e25f728775eb_Out_0, _Property_f82baa231e61476083e8ec20883e4880_Out_0, _Property_345576a774184e23ac56d4cc1c6c94e1_Out_0, _Property_e24a2b974ce04ef69a36e8db9a6a0656_Out_0, _Property_0e62fc1188ec4ec684340ce36f72967e_Out_0, (_UV_751d447d2aff49c79383613184793eaa_Out_0.xy), float2 (1, 1), _Multiply_b022e89f3c7b4b2da34d20aaa3990648_Out_2, _Property_83247cc06db0450aa518444b92db7772_Out_0, 1, _SGCustomGradient_11ee6933b2e54925a65fb2a586e6229f, _SGCustomGradient_11ee6933b2e54925a65fb2a586e6229f_New_0);
    UnityTexture2D _Property_04d0c6d168534ec38a7a2b6f1bcf85d0_Out_0 = UnityBuildTexture2DStructNoScale(_Ramp);
    float2 _TilingAndOffset_a0c2e8b3c22247469d4e7292158a5db8_Out_3;
    Unity_TilingAndOffset_float((_UV_751d447d2aff49c79383613184793eaa_Out_0.xy), float2 (1, 1), _Multiply_b022e89f3c7b4b2da34d20aaa3990648_Out_2, _TilingAndOffset_a0c2e8b3c22247469d4e7292158a5db8_Out_3);
    float4 _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0 = SAMPLE_TEXTURE2D(_Property_04d0c6d168534ec38a7a2b6f1bcf85d0_Out_0.tex, _Property_04d0c6d168534ec38a7a2b6f1bcf85d0_Out_0.samplerstate, _TilingAndOffset_a0c2e8b3c22247469d4e7292158a5db8_Out_3);
    float _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_R_4 = _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0.r;
    float _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_G_5 = _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0.g;
    float _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_B_6 = _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0.b;
    float _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_A_7 = _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0.a;
    float _Property_a7f0dcabc38646f1b4c9f150cfdce451_Out_0 = _UseTexture;
    float _Step_7e913cae8fe44466b0ead5bd4c075e35_Out_2;
    Unity_Step_float(1, _Property_a7f0dcabc38646f1b4c9f150cfdce451_Out_0, _Step_7e913cae8fe44466b0ead5bd4c075e35_Out_2);
    float4 _Lerp_226c2460881d4e9d88c061f3bf6da2b7_Out_3;
    Unity_Lerp_float4(_SGCustomGradient_11ee6933b2e54925a65fb2a586e6229f_New_0, _SampleTexture2D_36afca2d416f45eaa9abc135c98369db_RGBA_0, (_Step_7e913cae8fe44466b0ead5bd4c075e35_Out_2.xxxx), _Lerp_226c2460881d4e9d88c061f3bf6da2b7_Out_3);
    UnityTexture2D _Property_331bce0a859d43c8b4de8c3997988b6b_Out_0 = UnityBuildTexture2DStructNoScale(_Mask);
    float4 _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0 = SAMPLE_TEXTURE2D(_Property_331bce0a859d43c8b4de8c3997988b6b_Out_0.tex, _Property_331bce0a859d43c8b4de8c3997988b6b_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_R_4 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.r;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_G_5 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.g;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_B_6 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.b;
    float _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7 = _SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_RGBA_0.a;
    float _Split_05d9212f9c5a46bfaa1216c01464649d_R_1 = IN.VertexColor[0];
    float _Split_05d9212f9c5a46bfaa1216c01464649d_G_2 = IN.VertexColor[1];
    float _Split_05d9212f9c5a46bfaa1216c01464649d_B_3 = IN.VertexColor[2];
    float _Split_05d9212f9c5a46bfaa1216c01464649d_A_4 = IN.VertexColor[3];
    float _Multiply_cdf08e66ce82418ab6ea28efe8fae1b3_Out_2;
    Unity_Multiply_float(_SampleTexture2D_3d0e856edd734a90b0fd103edbdaf058_A_7, _Split_05d9212f9c5a46bfaa1216c01464649d_A_4, _Multiply_cdf08e66ce82418ab6ea28efe8fae1b3_Out_2);
    float _Property_8c7d1e1a34f043d784e2e1940c241952_Out_0 = _Fill;
    float _Property_8212b593471649d6abc410729b20074a_Out_0 = _FillDirection;
    float4 _UV_778cca2eecd942fd92a102a75cccc785_Out_0 = IN.uv0;
    float _FillCustomFunction_a3c9e80ff4354cbe8e43a7576ac66cf6_Out_2;
    Fill_float(_Property_8c7d1e1a34f043d784e2e1940c241952_Out_0, _Property_8212b593471649d6abc410729b20074a_Out_0, (_UV_778cca2eecd942fd92a102a75cccc785_Out_0.xy), _FillCustomFunction_a3c9e80ff4354cbe8e43a7576ac66cf6_Out_2);
    float _Multiply_5c470095b80346f0a6905e75ebcd9178_Out_2;
    Unity_Multiply_float(_Multiply_cdf08e66ce82418ab6ea28efe8fae1b3_Out_2, _FillCustomFunction_a3c9e80ff4354cbe8e43a7576ac66cf6_Out_2, _Multiply_5c470095b80346f0a6905e75ebcd9178_Out_2);
    surface.BaseColor = (_Lerp_226c2460881d4e9d88c061f3bf6da2b7_Out_3.xyz);
    surface.Alpha = _Multiply_5c470095b80346f0a6905e75ebcd9178_Out_2;
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
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SpriteUnlitPass.hlsl"

    ENDHLSL
}
    }
        FallBack "Hidden/Shader Graph/FallbackError"
}