Shader "Custom/S_InfiniteFloor" 
{
    Properties
    {
        [NoScaleOffset] _MainTex("MainTex", 2D) = "white" {}
        [NoScaleOffset]_Map("Map", 2D) = "white" {}
        [NoScaleOffset]_EstateIDMap("EstateIDMap", 2D) = "white" {}
        _SizeOfTexture("SizeOfTexture", Vector) = (512, 512, 0, 0)
        _Zoom("Zoom", Float) = 3.75
        _GridThickness("GridThickness", Range(0.0001, 10)) = 1
        _GridOffset("GridOffset", Float) = 1
        _ThicknessOffset("ThicknessOffset", Float) = 0
        _ColorGrid("ColorGrid", Color) = (0, 0, 0, 0)
        _ColorPlazas("ColorPlazas", Color) = (0.1686275, 0.4352941, 0.2156863, 1)
        _ColorDistricts("ColorDistricts", Color) = (0.1372549, 0.6784314, 0.6039216, 1)
        _ColorStreets("ColorStreets", Color) = (0.4627451, 0.4941176, 0.4862745, 1)
        _ColorParcels("ColorParcels", Color) = (0.2627451, 0.427451, 0.2901961, 1)
        _ColorOwnedParcels("ColorOwnedParcels", Color) = (0.5294118, 0.4666667, 0.1686275, 1)
        _ColorEmpty("ColorEmpty", Color) = (0, 0.1019608, 0.03137255, 1)
        [NoScaleOffset]_GrassTexture("GrassTexture", 2D) = "white" {}
        _GrassScale("GrassScale", Float) = 1
        _OwnedVariationRange("OwnedVariationRange", Vector) = (0.75, 1.75, 0, 0)
        _UnownedVariationRange("UnownedVariationRange", Vector) = (0.05, 1.75, 0, 0)
        _GrassGridTiling("GrassGridTiling", Float) = 16
        _GrassGridThickness("GrassGridThickness", Float) = 0.02
        _GrassGridVariationFrequency("GrassGridVariationFrequency", Float) = 4
        _GrassGridThicknessVariation("GrassGridThicknessVariation", Float) = 1.1
        _GrassGridIntenseFade("GrassGridIntenseFade", Float) = 2
        _GrassGridFarFade("GrassGridFarFade", Float) = 25
        _GrassGridFadePosition("GrassGridFadePosition", Vector) = (0, 0, 0, 0)
        _GrassGridColor("GrassGridColor", Color) = (0, 1, 0.7757626, 1)
        [NoScaleOffset]_RoadTexture("RoadTexture", 2D) = "white" {}
        _RoadScale("RoadScale", Float) = 1
        _RoadFade("RoadFade", Float) = 100
        _Smoothness("Smoothness", Range(0, 1)) = 0
        _Metallic("Metallic", Range(0, 1)) = 0
        _PlayerPosition("PlayerPosition", Vector) = (0, 0, 0, 0)
        _FogFade("FogFade", Float) = 1
        _FogIntensity("FogIntensity", Range(0, 1)) = 1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
        SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue" = "Geometry"
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
    Blend One Zero
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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
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
        float4 uv0 : TEXCOORD0;
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
        float4 texCoord0;
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
        float3 ObjectSpacePosition;
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
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float4 interp3 : TEXCOORD3;
        float3 interp4 : TEXCOORD4;
        #if defined(LIGHTMAP_ON)
        float2 interp5 : TEXCOORD5;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 interp6 : TEXCOORD6;
        #endif
        float4 interp7 : TEXCOORD7;
        float4 interp8 : TEXCOORD8;
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
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyzw = input.texCoord0;
        output.interp4.xyz = input.viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        output.interp5.xy = input.lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.interp6.xyz = input.sh;
        #endif
        output.interp7.xyzw = input.fogFactorAndVertexLight;
        output.interp8.xyzw = input.shadowCoord;
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
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.texCoord0 = input.interp3.xyzw;
        output.viewDirectionWS = input.interp4.xyz;
        #if defined(LIGHTMAP_ON)
        output.lightmapUV = input.interp5.xy;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.sh = input.interp6.xyz;
        #endif
        output.fogFactorAndVertexLight = input.interp7.xyzw;
        output.shadowCoord = input.interp8.xyzw;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

struct Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b
{
    float3 WorldSpacePosition;
    half4 uv0;
};

void SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(float2 Vector2_6bff7006be6546f1a2eccc78e58e6232, float Vector1_e86c202cda73418eae7d4a134b98a195, Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b IN, out float2 UV_1, out float2 WorldUV_3, out float Zoom_2)
{
    float4 _UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0 = IN.uv0;
    float _Split_259410824e3c4f498de1eef89dacf280_R_1 = IN.WorldSpacePosition[0];
    float _Split_259410824e3c4f498de1eef89dacf280_G_2 = IN.WorldSpacePosition[1];
    float _Split_259410824e3c4f498de1eef89dacf280_B_3 = IN.WorldSpacePosition[2];
    float _Split_259410824e3c4f498de1eef89dacf280_A_4 = 0;
    float2 _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0 = float2(_Split_259410824e3c4f498de1eef89dacf280_R_1, _Split_259410824e3c4f498de1eef89dacf280_B_3);
    float _Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0 = Vector1_e86c202cda73418eae7d4a134b98a195;
    float _Split_70c1c438ae374dec9f011f8d2999c80e_R_1 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[0];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_G_2 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[1];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_B_3 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[2];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_A_4 = 0;
    float _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
    Unity_Divide_float(_Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0, _Split_70c1c438ae374dec9f011f8d2999c80e_R_1, _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2);
    UV_1 = (_UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0.xy);
    WorldUV_3 = _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0;
    Zoom_2 = _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
}

// 7b6d5a90df0cb86d20ecea9cb96d928e
#include "Assets/Rendering/InfiniteFloor/MapV5.hlsl"

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Comparison_NotEqual_float(float A, float B, out float Out)
{
    Out = A != B ? 1 : 0;
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Floor_float2(float2 In, out float2 Out)
{
    Out = floor(In);
}


inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}


inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}


inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = Unity_SimpleNoise_RandomValue_float(c0);
    float r1 = Unity_SimpleNoise_RandomValue_float(c1);
    float r2 = Unity_SimpleNoise_RandomValue_float(c2);
    float r3 = Unity_SimpleNoise_RandomValue_float(c3);

    float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
    float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
    float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
    return t;
}

void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3 - 0));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3 - 1));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3 - 2));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    Out = t;
}

void Unity_Branch_float(float Predicate, float True, float False, out float Out)
{
    Out = Predicate ? True : False;
}

struct Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841
{
};

void SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(float2 Vector2_cd614e04b07b47eb95a2e5e3ffa41872, float Vector1_355229664685490fa0fc11fe1a97899f, float2 Vector2_1721f8718e464df9a2a9bd7239c50524, UnityTexture2D Texture2D_405855aa0a514baaa11320593c2f07c1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03, Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 IN, out float Mixed_1, out float Owned_2)
{
    UnityTexture2D _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0 = Texture2D_405855aa0a514baaa11320593c2f07c1;
    float2 _Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0 = Vector2_cd614e04b07b47eb95a2e5e3ffa41872;
    float _Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0 = 1;
    float _Property_06d14f9324724405bca2e16df40bef40_Out_0 = Vector1_355229664685490fa0fc11fe1a97899f;
    float _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2;
    Unity_Divide_float(_Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0, _Property_06d14f9324724405bca2e16df40bef40_Out_0, _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2);
    float _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2;
    Unity_Multiply_float(_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2, -1, _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2);
    float _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2;
    Unity_Divide_float(_Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2, 2, _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2);
    float _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2;
    Unity_Add_float(_Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2, 0.5, _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2);
    float2 _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3;
    Unity_TilingAndOffset_float(_Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0, (_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2.xx), (_Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2.xx), _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float4 _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.tex, _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.samplerstate, _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_R_4 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.r;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.g;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.b;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_A_7 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.a;
    float _Add_c7db3142e97045b2877ef4033d663af0_Out_2;
    Unity_Add_float(_SampleTexture2D_108c05205d884e9298d8d59122709828_R_4, _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5, _Add_c7db3142e97045b2877ef4033d663af0_Out_2);
    float _Add_da555a32eb354a279399036fba5f852b_Out_2;
    Unity_Add_float(_Add_c7db3142e97045b2877ef4033d663af0_Out_2, _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6, _Add_da555a32eb354a279399036fba5f852b_Out_2);
    float _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2);
    float2 _Property_49ef008f07834aba983ae50300e94c82_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1;
    float _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3;
    Unity_Remap_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, float2 (0, 1), _Property_49ef008f07834aba983ae50300e94c82_Out_0, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3);
    float2 _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0 = Vector2_1721f8718e464df9a2a9bd7239c50524;
    float2 _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3, _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0, float2 (0, 0), _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3);
    float2 _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1;
    Unity_Floor_float2(_TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3, _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1);
    float _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2;
    Unity_SimpleNoise_float(_Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1, 150, _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2);
    float2 _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03;
    float _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3;
    Unity_Remap_float(_SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2, float2 (0, 1), _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3);
    float _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Unity_Branch_float(_Comparison_9f9bfac15975447a9c81341908b981a0_Out_2, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3, _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3);
    float _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2);
    float _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
    Unity_Branch_float(_Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2, 1, 0, _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3);
    Mixed_1 = _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Owned_2 = _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

struct Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e
{
};

void SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(float2 Vector2_a43b69bd63e74b8899664790c597f8c6, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c, float Vector1_8be634a8378a4521b7522d631008fc39, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c_1, float Vector1_1, float Vector1_78a75bc300dd47ee83f8fbd9e84a0cad, float2 Vector2_f975587bc79d4eadbd0807b55a090f9d, float Vector1_e4cafe8cd46043f0ae5392b59d6b03fe, float2 Vector2_7389d8e6e0014c32be011a6864268e6a, float2 Vector2_cf1b396af9b54596bc8052bf3fe215fb, Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e IN, out float Grass_1, out float Road_2)
{
    UnityTexture2D _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c;
    float2 _Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0 = Vector1_8be634a8378a4521b7522d631008fc39;
    float _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2;
    Unity_Divide_float(1, _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0, _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2);
    float _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2;
    Unity_Multiply_float(_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2, -1, _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2);
    float _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2;
    Unity_Divide_float(_Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2, 2, _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2);
    float _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2;
    Unity_Add_float(_Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2, 0.5, _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2);
    float2 _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3;
    Unity_TilingAndOffset_float(_Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0, (_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2.xx), (_Add_e636f222615a4d5b80b5dc3743ef5097_Out_2.xx), _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float4 _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.tex, _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.samplerstate, _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.r;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_G_5 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.g;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_B_6 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.b;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_A_7 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.a;
    UnityTexture2D _Property_acfdee93b05546369a691885f7d8fc49_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c_1;
    float2 _Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float2 _Property_622a1d2b345749f7af37dbeb28c9856a_Out_0 = Vector2_f975587bc79d4eadbd0807b55a090f9d;
    float _Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0 = 1;
    float _Property_9655382379e942478a21d79fef207bdd_Out_0 = Vector1_78a75bc300dd47ee83f8fbd9e84a0cad;
    float _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2;
    Unity_Divide_float(_Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0, _Property_9655382379e942478a21d79fef207bdd_Out_0, _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2);
    float2 _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2;
    Unity_Multiply_float(_Property_622a1d2b345749f7af37dbeb28c9856a_Out_0, (_Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2.xx), _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2);
    float _Split_92aaba0363214ad786eeca836e64e191_R_1 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[0];
    float _Split_92aaba0363214ad786eeca836e64e191_G_2 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[1];
    float _Split_92aaba0363214ad786eeca836e64e191_B_3 = 0;
    float _Split_92aaba0363214ad786eeca836e64e191_A_4 = 0;
    float _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2;
    Unity_Multiply_float(_Split_92aaba0363214ad786eeca836e64e191_R_1, -1, _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2);
    float _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2;
    Unity_Divide_float(_Multiply_426451ec50dc4f3b80866858356f7c82_Out_2, 2, _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2);
    float _Add_1e90aeca170749febce3402fc85db207_Out_2;
    Unity_Add_float(_Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2, 0.5, _Add_1e90aeca170749febce3402fc85db207_Out_2);
    float2 _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3;
    Unity_TilingAndOffset_float(_Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0, (_Split_92aaba0363214ad786eeca836e64e191_R_1.xx), (_Add_1e90aeca170749febce3402fc85db207_Out_2.xx), _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3);
    float2 _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3, _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1);
    float _Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0 = Vector1_1;
    float _Multiply_3e0c255ae161401f94116d1116002307_Out_2;
    Unity_Multiply_float(_Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0, 10000, _Multiply_3e0c255ae161401f94116d1116002307_Out_2);
    float2 _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3;
    Unity_TilingAndOffset_float(_Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1, (_Multiply_3e0c255ae161401f94116d1116002307_Out_2.xx), float2 (0, 0), _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float4 _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0 = SAMPLE_TEXTURE2D(_Property_acfdee93b05546369a691885f7d8fc49_Out_0.tex, _Property_acfdee93b05546369a691885f7d8fc49_Out_0.samplerstate, _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.r;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_G_5 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.g;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_B_6 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.b;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_A_7 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.a;
    float2 _Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0 = Vector2_cf1b396af9b54596bc8052bf3fe215fb;
    float2 _Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0 = Vector2_7389d8e6e0014c32be011a6864268e6a;
    float2 _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2;
    Unity_Multiply_float(_Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0, float2(-1, -1), _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2);
    float2 _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2;
    Unity_Add_float2(_Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0, _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2, _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2);
    float _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0 = Vector1_e4cafe8cd46043f0ae5392b59d6b03fe;
    float _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2;
    Unity_Divide_float(10, _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0, _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2);
    float2 _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4;
    Unity_PolarCoordinates_float(_Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2, float2 (0.5, 0.5), _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2, 1, _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4);
    float _Split_f3387914733a4792b78907a73c898380_R_1 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[0];
    float _Split_f3387914733a4792b78907a73c898380_G_2 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[1];
    float _Split_f3387914733a4792b78907a73c898380_B_3 = 0;
    float _Split_f3387914733a4792b78907a73c898380_A_4 = 0;
    float _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1;
    Unity_Saturate_float(_Split_f3387914733a4792b78907a73c898380_R_1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1);
    float _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
    Unity_Lerp_float(_SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4, 1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1, _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3);
    Grass_1 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4;
    Road_2 = _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
}

struct Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a
{
};

void SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(float2 Vector2_311ffee78d314f71a9463e39924ea623, float2 Vector2_a57b68e1b4044834933fd8337f0a0577, float Vector1_7284deecf5d9431d92fc35a123337ff4, float Vector1_ab2c4cd721534cf4a387156d51a1fed9, Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a IN, out float Out_1)
{
    float2 _Property_70523c283f40499f89e4f7748deff77e_Out_0 = Vector2_311ffee78d314f71a9463e39924ea623;
    float2 _Property_f28b80022c3246688280e0762030829b_Out_0 = Vector2_a57b68e1b4044834933fd8337f0a0577;
    float2 _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2;
    Unity_Multiply_float(_Property_f28b80022c3246688280e0762030829b_Out_0, float2(-1, -1), _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2);
    float2 _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2;
    Unity_Add_float2(_Property_70523c283f40499f89e4f7748deff77e_Out_0, _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2, _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2);
    float _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0 = Vector1_7284deecf5d9431d92fc35a123337ff4;
    float _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2;
    Unity_Divide_float(1, _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0, _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2);
    float2 _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4;
    Unity_PolarCoordinates_float(_Add_e90ad347cd4b42c3963540725f4e79d9_Out_2, float2 (0.5, 0.5), _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2, 1, _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4);
    float _Split_904e58337bbe428998ef573899b98f55_R_1 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[0];
    float _Split_904e58337bbe428998ef573899b98f55_G_2 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[1];
    float _Split_904e58337bbe428998ef573899b98f55_B_3 = 0;
    float _Split_904e58337bbe428998ef573899b98f55_A_4 = 0;
    float _Property_f8541835e99e409989806d7eff9d13e8_Out_0 = Vector1_ab2c4cd721534cf4a387156d51a1fed9;
    float _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2;
    Unity_Multiply_float(_Split_904e58337bbe428998ef573899b98f55_R_1, _Property_f8541835e99e409989806d7eff9d13e8_Out_0, _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2);
    float _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
    Unity_Saturate_float(_Multiply_a77283d5783542b596ccaa11bb712b63_Out_2, _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1);
    Out_1 = _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Rectangle_float(float2 UV, float Width, float Height, out float Out)
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

struct Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c
{
};

void SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(float2 Vector2_e01f7c264af944fdb8bcea2d35ae3001, float2 Vector2_622ed4642c4445d194b78ad6759b208d, float2 Vector2_575137a8a58748d1a0e062a00216bbe5, float Vector1_23353b8652e043faab2f58b3964e3f17, float Vector1_5c017085898d45e48611a2e9ace96469, float Vector1_f8f78e1de998447c949d6ce599a31355, float4 Vector4_5fb32e510cd648f8b219982d0bc6426a, float Vector1_eb4dbe959ea64ae896f61f72a5d275d0, float Vector1_2b42c6accf5149c3929d5731c737ba7c, float2 Vector2_2c998556cbda461d8a0b69199046f9f5, float Vector1_f62ba0f4717b42c1b7e03ce424479587, float Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d, float Vector1_63cdfc1b7ebd4084b00ceb9b109e3919, Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c IN, out float GrassGrid_1)
{
    float2 _Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0 = Vector2_622ed4642c4445d194b78ad6759b208d;
    float2 _Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0 = Vector2_2c998556cbda461d8a0b69199046f9f5;
    float2 _Multiply_d348b89a76874839babedba1f8d3296d_Out_2;
    Unity_Multiply_float(_Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0, float2(-1, -1), _Multiply_d348b89a76874839babedba1f8d3296d_Out_2);
    float2 _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2;
    Unity_Add_float2(_Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0, _Multiply_d348b89a76874839babedba1f8d3296d_Out_2, _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2);
    float _Property_eb9ff929e1204221bca1c31925f600b7_Out_0 = Vector1_2b42c6accf5149c3929d5731c737ba7c;
    float _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2;
    Unity_Divide_float(10, _Property_eb9ff929e1204221bca1c31925f600b7_Out_0, _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2);
    float2 _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2, 1, _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4);
    float _Split_2e7cca56ed8b4f69890662df97d724ba_R_1 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[0];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_G_2 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[1];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_B_3 = 0;
    float _Split_2e7cca56ed8b4f69890662df97d724ba_A_4 = 0;
    float _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1;
    Unity_OneMinus_float(_Split_2e7cca56ed8b4f69890662df97d724ba_R_1, _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1);
    float _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1;
    Unity_Saturate_float(_OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1, _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1);
    float _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0 = Vector1_eb4dbe959ea64ae896f61f72a5d275d0;
    float _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2;
    Unity_Divide_float(10, _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0, _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2);
    float2 _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2, 1, _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4);
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[0];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_G_2 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[1];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_B_3 = 0;
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_A_4 = 0;
    float _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1;
    Unity_OneMinus_float(_Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1, _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1);
    float _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1;
    Unity_Saturate_float(_OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1, _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1);
    float2 _Property_8c07974a4edd4dc89101134d98954a00_Out_0 = Vector2_e01f7c264af944fdb8bcea2d35ae3001;
    float _Float_93afd7af653a45a38377067c2d80ab35_Out_0 = 1;
    float _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0 = Vector1_23353b8652e043faab2f58b3964e3f17;
    float _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2;
    Unity_Divide_float(_Float_93afd7af653a45a38377067c2d80ab35_Out_0, _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0, _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2);
    float _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2;
    Unity_Multiply_float(_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2, -1, _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2);
    float _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2;
    Unity_Divide_float(_Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2, 2, _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2);
    float _Add_bc17962f1e2d49fca18bb00e85478880_Out_2;
    Unity_Add_float(_Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2, 0.5, _Add_bc17962f1e2d49fca18bb00e85478880_Out_2);
    float2 _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3;
    Unity_TilingAndOffset_float(_Property_8c07974a4edd4dc89101134d98954a00_Out_0, (_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2.xx), (_Add_bc17962f1e2d49fca18bb00e85478880_Out_2.xx), _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3);
    float2 _Property_deac93b8878546c8a300d7352a631a26_Out_0 = Vector2_575137a8a58748d1a0e062a00216bbe5;
    float2 _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3, _Property_deac93b8878546c8a300d7352a631a26_Out_0, float2 (0, 0), _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3);
    float2 _Fraction_a4f4615406494c08b0082401f60051c2_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3, _Fraction_a4f4615406494c08b0082401f60051c2_Out_1);
    float _Property_0081be27ffb041f4ac12f66f0dc62624_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float2 _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Property_0081be27ffb041f4ac12f66f0dc62624_Out_0.xx), float2 (0, 0), _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3);
    float2 _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3, _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1);
    float _Property_d89b62d779e54332979e2425f6cd1857_Out_0 = Vector1_f8f78e1de998447c949d6ce599a31355;
    float _Add_1feb1662c2884dffad05bb44310c6586_Out_2;
    Unity_Add_float(_Property_d89b62d779e54332979e2425f6cd1857_Out_0, 1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2);
    float _Divide_43532697f4454e47a4092b408ec0ff25_Out_2;
    Unity_Divide_float(1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2);
    float _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3;
    Unity_Rectangle_float(_Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3);
    float _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1;
    Unity_OneMinus_float(_Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3, _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1);
    float _Property_43f57c55e694444e9bf9aeb01760d823_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_853fa90d1d314da390af1a6f21f72298_Out_2;
    Unity_Divide_float(_Property_43f57c55e694444e9bf9aeb01760d823_Out_0, _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0, _Divide_853fa90d1d314da390af1a6f21f72298_Out_2);
    float2 _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Divide_853fa90d1d314da390af1a6f21f72298_Out_2.xx), float2 (0, 0), _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3);
    float2 _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3, _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1);
    float _Property_5236eaaed502435380fd9232ca3f5a7b_Out_0 = Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d;
    float _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_479d5e5f38be495d982aed56501420aa_Out_2;
    Unity_Divide_float(_Property_5236eaaed502435380fd9232ca3f5a7b_Out_0, _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0, _Divide_479d5e5f38be495d982aed56501420aa_Out_2);
    float _Add_eae9a749d00c4960aea43a9320fead3e_Out_2;
    Unity_Add_float(_Divide_479d5e5f38be495d982aed56501420aa_Out_2, 1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2);
    float _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2;
    Unity_Divide_float(1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2);
    float _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3;
    Unity_Rectangle_float(_Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3);
    float _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1;
    Unity_OneMinus_float(_Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1);
    float _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2;
    Unity_Add_float(_OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1, _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2);
    float _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1;
    Unity_Saturate_float(_Add_2b900f3ab1b44858bf5696180eac62e6_Out_2, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1);
    float _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2;
    Unity_Multiply_float(_Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2);
    float4 _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0 = Vector4_5fb32e510cd648f8b219982d0bc6426a;
    float _Split_d97e936de3fa453e9725f0c2256e5eac_R_1 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[0];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_G_2 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[1];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_B_3 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[2];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_A_4 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[3];
    float _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2;
    Unity_Multiply_float(_Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Split_d97e936de3fa453e9725f0c2256e5eac_A_4, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2);
    float _Add_9af69bb68e3044b29a3495457a20582c_Out_2;
    Unity_Add_float(_Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2, _Add_9af69bb68e3044b29a3495457a20582c_Out_2);
    float _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
    Unity_Multiply_float(_Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1, _Add_9af69bb68e3044b29a3495457a20582c_Out_2, _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2);
    GrassGrid_1 = _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
}

void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
{
    SHADERGRAPH_FOG(Position, Color, Density);
}

// 98116c2c658709e5fcb200b1ae28460e
#include "Assets/Rendering/InfiniteFloor/InfiniteFloorMerger.hlsl"

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_e943bfd340cf4709a76ba852685dbf55_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_c294f42edfdb40c18d1605395ee9f835_Out_0 = _PlayerPosition;
    float _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0 = _Zoom;
    Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.WorldSpacePosition = IN.WorldSpacePosition;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.uv0 = IN.uv0;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3;
    float _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2;
    SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(_Property_c294f42edfdb40c18d1605395ee9f835_Out_0, _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2);
    float2 _Property_edde0349814e405a8f77a67715a72a11_Out_0 = _SizeOfTexture;
    float _Property_c4b62049d152467eb90794c337831029_Out_0 = _GridThickness;
    float _Property_038d46b4121440948e0171cd5c26d417_Out_0 = _ThicknessOffset;
    float _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0 = _GridOffset;
    float4 _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18;
    Main_float(_Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0, _Property_e943bfd340cf4709a76ba852685dbf55_Out_0, 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_edde0349814e405a8f77a67715a72a11_Out_0, _Property_c4b62049d152467eb90794c337831029_Out_0, _Property_038d46b4121440948e0171cd5c26d417_Out_0, _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0, float2 (0, 0), 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18);
    float2 _Property_81c877f57fbd4119951d21e6a05f9536_Out_0 = _SizeOfTexture;
    UnityTexture2D _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0 = _OwnedVariationRange;
    float2 _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0 = _UnownedVariationRange;
    Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2;
    SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_81c877f57fbd4119951d21e6a05f9536_Out_0, _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0, _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0, _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2);
    UnityTexture2D _Property_03a2f987be36438cb0f8886babf33c96_Out_0 = UnityBuildTexture2DStructNoScale(_GrassTexture);
    float _Property_3ea8b576653841a7a5a603ef1f120469_Out_0 = _GrassScale;
    UnityTexture2D _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0 = UnityBuildTexture2DStructNoScale(_RoadTexture);
    float _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0 = _RoadScale;
    float _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0 = _Zoom;
    float2 _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0 = _SizeOfTexture;
    float _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0 = _RoadFade;
    float2 _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0 = _PlayerPosition;
    Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2;
    SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _Property_03a2f987be36438cb0f8886babf33c96_Out_0, _Property_3ea8b576653841a7a5a603ef1f120469_Out_0, _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0, _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0, _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0, _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0, _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0, _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2);
    float2 _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0 = _SizeOfTexture;
    float _Property_039b41a22968494fb95b7756f155d828_Out_0 = _GrassGridTiling;
    float _Property_98eedb86fc5b4145bc7b65222fede898_Out_0 = _GrassGridThickness;
    float4 _Property_da5db702a8724d9f9a82e143a886ee60_Out_0 = _GrassGridColor;
    float _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0 = _GrassGridIntenseFade;
    float _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0 = _GrassGridFarFade;
    float2 _Property_1ec68bba28374a39850d7800d8f1d362_Out_0 = _PlayerPosition;
    float _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0 = _GrassGridVariationFrequency;
    float _Property_4dcfad1507d94638b52b49967893b527_Out_0 = _GrassGridThicknessVariation;
    float2 _Property_6a4b9239d1bd424885c4b7027264b775_Out_0 = _PlayerPosition;
    float _Property_0d3761446a804744bb350177ec5d239a_Out_0 = _FogFade;
    float _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0 = _FogIntensity;
    Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af;
    float _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1;
    SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6a4b9239d1bd424885c4b7027264b775_Out_0, _Property_0d3761446a804744bb350177ec5d239a_Out_0, _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1);
    Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850;
    float _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1;
    SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_039b41a22968494fb95b7756f155d828_Out_0, _Property_98eedb86fc5b4145bc7b65222fede898_Out_0, _Property_da5db702a8724d9f9a82e143a886ee60_Out_0, _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0, _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0, _Property_1ec68bba28374a39850d7800d8f1d362_Out_0, _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0, _Property_4dcfad1507d94638b52b49967893b527_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1);
    float4 _Property_00719080222e437aa9abf5da2dd48a70_Out_0 = _ColorGrid;
    float4 _Property_118255dd5c8c455ab153d511ba1fc031_Out_0 = _ColorPlazas;
    float4 _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0 = _ColorDistricts;
    float4 _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0 = _ColorStreets;
    float4 _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0 = _ColorParcels;
    float4 _Property_6a1f7cc7467741628211b53b8709021d_Out_0 = _ColorOwnedParcels;
    float4 _Property_9d59173f2b1a48428823e35074ce62c5_Out_0 = _ColorEmpty;
    float4 _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0 = _GrassGridColor;
    float4 _Fog_caf07e8785584760b79500664df1fc44_Color_0;
    float _Fog_caf07e8785584760b79500664df1fc44_Density_1;
    Unity_Fog_float(_Fog_caf07e8785584760b79500664df1fc44_Color_0, _Fog_caf07e8785584760b79500664df1fc44_Density_1, IN.ObjectSpacePosition);
    float4 _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0;
    Merger_float(_MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _Property_00719080222e437aa9abf5da2dd48a70_Out_0, _Property_118255dd5c8c455ab153d511ba1fc031_Out_0, _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0, _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0, _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0, _Property_6a1f7cc7467741628211b53b8709021d_Out_0, _Property_9d59173f2b1a48428823e35074ce62c5_Out_0, _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0, _Fog_caf07e8785584760b79500664df1fc44_Color_0, _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0);
    float _Property_b872305fbdab44078bf3b70b9f9de114_Out_0 = _Metallic;
    float _Property_de71bcc7d06c4da3bea536b766bc403a_Out_0 = _Smoothness;
    surface.BaseColor = (_MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0.xyz);
    surface.NormalTS = IN.TangentSpaceNormal;
    surface.Emission = float3(0, 0, 0);
    surface.Metallic = _Property_b872305fbdab44078bf3b70b9f9de114_Out_0;
    surface.Smoothness = _Property_de71bcc7d06c4da3bea536b766bc403a_Out_0;
    surface.Occlusion = 1;
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



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
    output.uv0 = input.texCoord0;
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
    Blend One Zero
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
        #pragma multi_compile _ LIGHTMAP_ON
    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
    #pragma multi_compile _ _SHADOWS_SOFT
    #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
    #pragma multi_compile _ _GBUFFER_NORMALS_OCT
        // GraphKeywords: <None>

        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_GBUFFER
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
        float4 uv0 : TEXCOORD0;
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
        float4 texCoord0;
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
        float3 ObjectSpacePosition;
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
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float4 interp3 : TEXCOORD3;
        float3 interp4 : TEXCOORD4;
        #if defined(LIGHTMAP_ON)
        float2 interp5 : TEXCOORD5;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 interp6 : TEXCOORD6;
        #endif
        float4 interp7 : TEXCOORD7;
        float4 interp8 : TEXCOORD8;
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
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyzw = input.texCoord0;
        output.interp4.xyz = input.viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        output.interp5.xy = input.lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.interp6.xyz = input.sh;
        #endif
        output.interp7.xyzw = input.fogFactorAndVertexLight;
        output.interp8.xyzw = input.shadowCoord;
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
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.texCoord0 = input.interp3.xyzw;
        output.viewDirectionWS = input.interp4.xyz;
        #if defined(LIGHTMAP_ON)
        output.lightmapUV = input.interp5.xy;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.sh = input.interp6.xyz;
        #endif
        output.fogFactorAndVertexLight = input.interp7.xyzw;
        output.shadowCoord = input.interp8.xyzw;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

struct Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b
{
    float3 WorldSpacePosition;
    half4 uv0;
};

void SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(float2 Vector2_6bff7006be6546f1a2eccc78e58e6232, float Vector1_e86c202cda73418eae7d4a134b98a195, Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b IN, out float2 UV_1, out float2 WorldUV_3, out float Zoom_2)
{
    float4 _UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0 = IN.uv0;
    float _Split_259410824e3c4f498de1eef89dacf280_R_1 = IN.WorldSpacePosition[0];
    float _Split_259410824e3c4f498de1eef89dacf280_G_2 = IN.WorldSpacePosition[1];
    float _Split_259410824e3c4f498de1eef89dacf280_B_3 = IN.WorldSpacePosition[2];
    float _Split_259410824e3c4f498de1eef89dacf280_A_4 = 0;
    float2 _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0 = float2(_Split_259410824e3c4f498de1eef89dacf280_R_1, _Split_259410824e3c4f498de1eef89dacf280_B_3);
    float _Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0 = Vector1_e86c202cda73418eae7d4a134b98a195;
    float _Split_70c1c438ae374dec9f011f8d2999c80e_R_1 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[0];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_G_2 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[1];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_B_3 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[2];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_A_4 = 0;
    float _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
    Unity_Divide_float(_Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0, _Split_70c1c438ae374dec9f011f8d2999c80e_R_1, _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2);
    UV_1 = (_UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0.xy);
    WorldUV_3 = _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0;
    Zoom_2 = _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
}

// 7b6d5a90df0cb86d20ecea9cb96d928e
#include "Assets/Rendering/InfiniteFloor/MapV5.hlsl"

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Comparison_NotEqual_float(float A, float B, out float Out)
{
    Out = A != B ? 1 : 0;
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Floor_float2(float2 In, out float2 Out)
{
    Out = floor(In);
}


inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}


inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}


inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = Unity_SimpleNoise_RandomValue_float(c0);
    float r1 = Unity_SimpleNoise_RandomValue_float(c1);
    float r2 = Unity_SimpleNoise_RandomValue_float(c2);
    float r3 = Unity_SimpleNoise_RandomValue_float(c3);

    float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
    float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
    float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
    return t;
}

void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3 - 0));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3 - 1));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3 - 2));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    Out = t;
}

void Unity_Branch_float(float Predicate, float True, float False, out float Out)
{
    Out = Predicate ? True : False;
}

struct Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841
{
};

void SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(float2 Vector2_cd614e04b07b47eb95a2e5e3ffa41872, float Vector1_355229664685490fa0fc11fe1a97899f, float2 Vector2_1721f8718e464df9a2a9bd7239c50524, UnityTexture2D Texture2D_405855aa0a514baaa11320593c2f07c1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03, Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 IN, out float Mixed_1, out float Owned_2)
{
    UnityTexture2D _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0 = Texture2D_405855aa0a514baaa11320593c2f07c1;
    float2 _Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0 = Vector2_cd614e04b07b47eb95a2e5e3ffa41872;
    float _Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0 = 1;
    float _Property_06d14f9324724405bca2e16df40bef40_Out_0 = Vector1_355229664685490fa0fc11fe1a97899f;
    float _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2;
    Unity_Divide_float(_Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0, _Property_06d14f9324724405bca2e16df40bef40_Out_0, _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2);
    float _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2;
    Unity_Multiply_float(_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2, -1, _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2);
    float _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2;
    Unity_Divide_float(_Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2, 2, _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2);
    float _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2;
    Unity_Add_float(_Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2, 0.5, _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2);
    float2 _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3;
    Unity_TilingAndOffset_float(_Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0, (_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2.xx), (_Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2.xx), _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float4 _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.tex, _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.samplerstate, _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_R_4 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.r;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.g;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.b;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_A_7 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.a;
    float _Add_c7db3142e97045b2877ef4033d663af0_Out_2;
    Unity_Add_float(_SampleTexture2D_108c05205d884e9298d8d59122709828_R_4, _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5, _Add_c7db3142e97045b2877ef4033d663af0_Out_2);
    float _Add_da555a32eb354a279399036fba5f852b_Out_2;
    Unity_Add_float(_Add_c7db3142e97045b2877ef4033d663af0_Out_2, _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6, _Add_da555a32eb354a279399036fba5f852b_Out_2);
    float _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2);
    float2 _Property_49ef008f07834aba983ae50300e94c82_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1;
    float _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3;
    Unity_Remap_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, float2 (0, 1), _Property_49ef008f07834aba983ae50300e94c82_Out_0, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3);
    float2 _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0 = Vector2_1721f8718e464df9a2a9bd7239c50524;
    float2 _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3, _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0, float2 (0, 0), _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3);
    float2 _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1;
    Unity_Floor_float2(_TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3, _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1);
    float _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2;
    Unity_SimpleNoise_float(_Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1, 150, _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2);
    float2 _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03;
    float _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3;
    Unity_Remap_float(_SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2, float2 (0, 1), _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3);
    float _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Unity_Branch_float(_Comparison_9f9bfac15975447a9c81341908b981a0_Out_2, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3, _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3);
    float _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2);
    float _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
    Unity_Branch_float(_Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2, 1, 0, _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3);
    Mixed_1 = _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Owned_2 = _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

struct Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e
{
};

void SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(float2 Vector2_a43b69bd63e74b8899664790c597f8c6, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c, float Vector1_8be634a8378a4521b7522d631008fc39, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c_1, float Vector1_1, float Vector1_78a75bc300dd47ee83f8fbd9e84a0cad, float2 Vector2_f975587bc79d4eadbd0807b55a090f9d, float Vector1_e4cafe8cd46043f0ae5392b59d6b03fe, float2 Vector2_7389d8e6e0014c32be011a6864268e6a, float2 Vector2_cf1b396af9b54596bc8052bf3fe215fb, Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e IN, out float Grass_1, out float Road_2)
{
    UnityTexture2D _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c;
    float2 _Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0 = Vector1_8be634a8378a4521b7522d631008fc39;
    float _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2;
    Unity_Divide_float(1, _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0, _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2);
    float _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2;
    Unity_Multiply_float(_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2, -1, _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2);
    float _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2;
    Unity_Divide_float(_Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2, 2, _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2);
    float _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2;
    Unity_Add_float(_Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2, 0.5, _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2);
    float2 _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3;
    Unity_TilingAndOffset_float(_Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0, (_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2.xx), (_Add_e636f222615a4d5b80b5dc3743ef5097_Out_2.xx), _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float4 _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.tex, _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.samplerstate, _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.r;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_G_5 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.g;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_B_6 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.b;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_A_7 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.a;
    UnityTexture2D _Property_acfdee93b05546369a691885f7d8fc49_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c_1;
    float2 _Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float2 _Property_622a1d2b345749f7af37dbeb28c9856a_Out_0 = Vector2_f975587bc79d4eadbd0807b55a090f9d;
    float _Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0 = 1;
    float _Property_9655382379e942478a21d79fef207bdd_Out_0 = Vector1_78a75bc300dd47ee83f8fbd9e84a0cad;
    float _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2;
    Unity_Divide_float(_Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0, _Property_9655382379e942478a21d79fef207bdd_Out_0, _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2);
    float2 _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2;
    Unity_Multiply_float(_Property_622a1d2b345749f7af37dbeb28c9856a_Out_0, (_Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2.xx), _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2);
    float _Split_92aaba0363214ad786eeca836e64e191_R_1 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[0];
    float _Split_92aaba0363214ad786eeca836e64e191_G_2 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[1];
    float _Split_92aaba0363214ad786eeca836e64e191_B_3 = 0;
    float _Split_92aaba0363214ad786eeca836e64e191_A_4 = 0;
    float _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2;
    Unity_Multiply_float(_Split_92aaba0363214ad786eeca836e64e191_R_1, -1, _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2);
    float _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2;
    Unity_Divide_float(_Multiply_426451ec50dc4f3b80866858356f7c82_Out_2, 2, _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2);
    float _Add_1e90aeca170749febce3402fc85db207_Out_2;
    Unity_Add_float(_Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2, 0.5, _Add_1e90aeca170749febce3402fc85db207_Out_2);
    float2 _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3;
    Unity_TilingAndOffset_float(_Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0, (_Split_92aaba0363214ad786eeca836e64e191_R_1.xx), (_Add_1e90aeca170749febce3402fc85db207_Out_2.xx), _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3);
    float2 _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3, _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1);
    float _Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0 = Vector1_1;
    float _Multiply_3e0c255ae161401f94116d1116002307_Out_2;
    Unity_Multiply_float(_Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0, 10000, _Multiply_3e0c255ae161401f94116d1116002307_Out_2);
    float2 _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3;
    Unity_TilingAndOffset_float(_Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1, (_Multiply_3e0c255ae161401f94116d1116002307_Out_2.xx), float2 (0, 0), _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float4 _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0 = SAMPLE_TEXTURE2D(_Property_acfdee93b05546369a691885f7d8fc49_Out_0.tex, _Property_acfdee93b05546369a691885f7d8fc49_Out_0.samplerstate, _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.r;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_G_5 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.g;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_B_6 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.b;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_A_7 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.a;
    float2 _Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0 = Vector2_cf1b396af9b54596bc8052bf3fe215fb;
    float2 _Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0 = Vector2_7389d8e6e0014c32be011a6864268e6a;
    float2 _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2;
    Unity_Multiply_float(_Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0, float2(-1, -1), _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2);
    float2 _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2;
    Unity_Add_float2(_Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0, _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2, _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2);
    float _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0 = Vector1_e4cafe8cd46043f0ae5392b59d6b03fe;
    float _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2;
    Unity_Divide_float(10, _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0, _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2);
    float2 _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4;
    Unity_PolarCoordinates_float(_Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2, float2 (0.5, 0.5), _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2, 1, _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4);
    float _Split_f3387914733a4792b78907a73c898380_R_1 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[0];
    float _Split_f3387914733a4792b78907a73c898380_G_2 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[1];
    float _Split_f3387914733a4792b78907a73c898380_B_3 = 0;
    float _Split_f3387914733a4792b78907a73c898380_A_4 = 0;
    float _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1;
    Unity_Saturate_float(_Split_f3387914733a4792b78907a73c898380_R_1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1);
    float _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
    Unity_Lerp_float(_SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4, 1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1, _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3);
    Grass_1 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4;
    Road_2 = _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
}

struct Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a
{
};

void SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(float2 Vector2_311ffee78d314f71a9463e39924ea623, float2 Vector2_a57b68e1b4044834933fd8337f0a0577, float Vector1_7284deecf5d9431d92fc35a123337ff4, float Vector1_ab2c4cd721534cf4a387156d51a1fed9, Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a IN, out float Out_1)
{
    float2 _Property_70523c283f40499f89e4f7748deff77e_Out_0 = Vector2_311ffee78d314f71a9463e39924ea623;
    float2 _Property_f28b80022c3246688280e0762030829b_Out_0 = Vector2_a57b68e1b4044834933fd8337f0a0577;
    float2 _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2;
    Unity_Multiply_float(_Property_f28b80022c3246688280e0762030829b_Out_0, float2(-1, -1), _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2);
    float2 _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2;
    Unity_Add_float2(_Property_70523c283f40499f89e4f7748deff77e_Out_0, _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2, _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2);
    float _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0 = Vector1_7284deecf5d9431d92fc35a123337ff4;
    float _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2;
    Unity_Divide_float(1, _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0, _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2);
    float2 _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4;
    Unity_PolarCoordinates_float(_Add_e90ad347cd4b42c3963540725f4e79d9_Out_2, float2 (0.5, 0.5), _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2, 1, _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4);
    float _Split_904e58337bbe428998ef573899b98f55_R_1 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[0];
    float _Split_904e58337bbe428998ef573899b98f55_G_2 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[1];
    float _Split_904e58337bbe428998ef573899b98f55_B_3 = 0;
    float _Split_904e58337bbe428998ef573899b98f55_A_4 = 0;
    float _Property_f8541835e99e409989806d7eff9d13e8_Out_0 = Vector1_ab2c4cd721534cf4a387156d51a1fed9;
    float _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2;
    Unity_Multiply_float(_Split_904e58337bbe428998ef573899b98f55_R_1, _Property_f8541835e99e409989806d7eff9d13e8_Out_0, _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2);
    float _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
    Unity_Saturate_float(_Multiply_a77283d5783542b596ccaa11bb712b63_Out_2, _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1);
    Out_1 = _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Rectangle_float(float2 UV, float Width, float Height, out float Out)
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

struct Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c
{
};

void SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(float2 Vector2_e01f7c264af944fdb8bcea2d35ae3001, float2 Vector2_622ed4642c4445d194b78ad6759b208d, float2 Vector2_575137a8a58748d1a0e062a00216bbe5, float Vector1_23353b8652e043faab2f58b3964e3f17, float Vector1_5c017085898d45e48611a2e9ace96469, float Vector1_f8f78e1de998447c949d6ce599a31355, float4 Vector4_5fb32e510cd648f8b219982d0bc6426a, float Vector1_eb4dbe959ea64ae896f61f72a5d275d0, float Vector1_2b42c6accf5149c3929d5731c737ba7c, float2 Vector2_2c998556cbda461d8a0b69199046f9f5, float Vector1_f62ba0f4717b42c1b7e03ce424479587, float Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d, float Vector1_63cdfc1b7ebd4084b00ceb9b109e3919, Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c IN, out float GrassGrid_1)
{
    float2 _Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0 = Vector2_622ed4642c4445d194b78ad6759b208d;
    float2 _Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0 = Vector2_2c998556cbda461d8a0b69199046f9f5;
    float2 _Multiply_d348b89a76874839babedba1f8d3296d_Out_2;
    Unity_Multiply_float(_Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0, float2(-1, -1), _Multiply_d348b89a76874839babedba1f8d3296d_Out_2);
    float2 _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2;
    Unity_Add_float2(_Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0, _Multiply_d348b89a76874839babedba1f8d3296d_Out_2, _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2);
    float _Property_eb9ff929e1204221bca1c31925f600b7_Out_0 = Vector1_2b42c6accf5149c3929d5731c737ba7c;
    float _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2;
    Unity_Divide_float(10, _Property_eb9ff929e1204221bca1c31925f600b7_Out_0, _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2);
    float2 _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2, 1, _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4);
    float _Split_2e7cca56ed8b4f69890662df97d724ba_R_1 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[0];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_G_2 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[1];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_B_3 = 0;
    float _Split_2e7cca56ed8b4f69890662df97d724ba_A_4 = 0;
    float _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1;
    Unity_OneMinus_float(_Split_2e7cca56ed8b4f69890662df97d724ba_R_1, _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1);
    float _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1;
    Unity_Saturate_float(_OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1, _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1);
    float _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0 = Vector1_eb4dbe959ea64ae896f61f72a5d275d0;
    float _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2;
    Unity_Divide_float(10, _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0, _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2);
    float2 _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2, 1, _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4);
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[0];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_G_2 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[1];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_B_3 = 0;
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_A_4 = 0;
    float _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1;
    Unity_OneMinus_float(_Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1, _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1);
    float _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1;
    Unity_Saturate_float(_OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1, _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1);
    float2 _Property_8c07974a4edd4dc89101134d98954a00_Out_0 = Vector2_e01f7c264af944fdb8bcea2d35ae3001;
    float _Float_93afd7af653a45a38377067c2d80ab35_Out_0 = 1;
    float _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0 = Vector1_23353b8652e043faab2f58b3964e3f17;
    float _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2;
    Unity_Divide_float(_Float_93afd7af653a45a38377067c2d80ab35_Out_0, _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0, _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2);
    float _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2;
    Unity_Multiply_float(_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2, -1, _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2);
    float _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2;
    Unity_Divide_float(_Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2, 2, _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2);
    float _Add_bc17962f1e2d49fca18bb00e85478880_Out_2;
    Unity_Add_float(_Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2, 0.5, _Add_bc17962f1e2d49fca18bb00e85478880_Out_2);
    float2 _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3;
    Unity_TilingAndOffset_float(_Property_8c07974a4edd4dc89101134d98954a00_Out_0, (_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2.xx), (_Add_bc17962f1e2d49fca18bb00e85478880_Out_2.xx), _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3);
    float2 _Property_deac93b8878546c8a300d7352a631a26_Out_0 = Vector2_575137a8a58748d1a0e062a00216bbe5;
    float2 _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3, _Property_deac93b8878546c8a300d7352a631a26_Out_0, float2 (0, 0), _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3);
    float2 _Fraction_a4f4615406494c08b0082401f60051c2_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3, _Fraction_a4f4615406494c08b0082401f60051c2_Out_1);
    float _Property_0081be27ffb041f4ac12f66f0dc62624_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float2 _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Property_0081be27ffb041f4ac12f66f0dc62624_Out_0.xx), float2 (0, 0), _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3);
    float2 _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3, _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1);
    float _Property_d89b62d779e54332979e2425f6cd1857_Out_0 = Vector1_f8f78e1de998447c949d6ce599a31355;
    float _Add_1feb1662c2884dffad05bb44310c6586_Out_2;
    Unity_Add_float(_Property_d89b62d779e54332979e2425f6cd1857_Out_0, 1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2);
    float _Divide_43532697f4454e47a4092b408ec0ff25_Out_2;
    Unity_Divide_float(1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2);
    float _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3;
    Unity_Rectangle_float(_Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3);
    float _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1;
    Unity_OneMinus_float(_Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3, _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1);
    float _Property_43f57c55e694444e9bf9aeb01760d823_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_853fa90d1d314da390af1a6f21f72298_Out_2;
    Unity_Divide_float(_Property_43f57c55e694444e9bf9aeb01760d823_Out_0, _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0, _Divide_853fa90d1d314da390af1a6f21f72298_Out_2);
    float2 _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Divide_853fa90d1d314da390af1a6f21f72298_Out_2.xx), float2 (0, 0), _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3);
    float2 _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3, _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1);
    float _Property_5236eaaed502435380fd9232ca3f5a7b_Out_0 = Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d;
    float _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_479d5e5f38be495d982aed56501420aa_Out_2;
    Unity_Divide_float(_Property_5236eaaed502435380fd9232ca3f5a7b_Out_0, _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0, _Divide_479d5e5f38be495d982aed56501420aa_Out_2);
    float _Add_eae9a749d00c4960aea43a9320fead3e_Out_2;
    Unity_Add_float(_Divide_479d5e5f38be495d982aed56501420aa_Out_2, 1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2);
    float _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2;
    Unity_Divide_float(1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2);
    float _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3;
    Unity_Rectangle_float(_Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3);
    float _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1;
    Unity_OneMinus_float(_Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1);
    float _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2;
    Unity_Add_float(_OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1, _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2);
    float _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1;
    Unity_Saturate_float(_Add_2b900f3ab1b44858bf5696180eac62e6_Out_2, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1);
    float _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2;
    Unity_Multiply_float(_Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2);
    float4 _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0 = Vector4_5fb32e510cd648f8b219982d0bc6426a;
    float _Split_d97e936de3fa453e9725f0c2256e5eac_R_1 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[0];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_G_2 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[1];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_B_3 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[2];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_A_4 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[3];
    float _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2;
    Unity_Multiply_float(_Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Split_d97e936de3fa453e9725f0c2256e5eac_A_4, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2);
    float _Add_9af69bb68e3044b29a3495457a20582c_Out_2;
    Unity_Add_float(_Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2, _Add_9af69bb68e3044b29a3495457a20582c_Out_2);
    float _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
    Unity_Multiply_float(_Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1, _Add_9af69bb68e3044b29a3495457a20582c_Out_2, _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2);
    GrassGrid_1 = _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
}

void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
{
    SHADERGRAPH_FOG(Position, Color, Density);
}

// 98116c2c658709e5fcb200b1ae28460e
#include "Assets/Rendering/InfiniteFloor/InfiniteFloorMerger.hlsl"

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_e943bfd340cf4709a76ba852685dbf55_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_c294f42edfdb40c18d1605395ee9f835_Out_0 = _PlayerPosition;
    float _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0 = _Zoom;
    Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.WorldSpacePosition = IN.WorldSpacePosition;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.uv0 = IN.uv0;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3;
    float _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2;
    SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(_Property_c294f42edfdb40c18d1605395ee9f835_Out_0, _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2);
    float2 _Property_edde0349814e405a8f77a67715a72a11_Out_0 = _SizeOfTexture;
    float _Property_c4b62049d152467eb90794c337831029_Out_0 = _GridThickness;
    float _Property_038d46b4121440948e0171cd5c26d417_Out_0 = _ThicknessOffset;
    float _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0 = _GridOffset;
    float4 _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18;
    Main_float(_Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0, _Property_e943bfd340cf4709a76ba852685dbf55_Out_0, 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_edde0349814e405a8f77a67715a72a11_Out_0, _Property_c4b62049d152467eb90794c337831029_Out_0, _Property_038d46b4121440948e0171cd5c26d417_Out_0, _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0, float2 (0, 0), 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18);
    float2 _Property_81c877f57fbd4119951d21e6a05f9536_Out_0 = _SizeOfTexture;
    UnityTexture2D _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0 = _OwnedVariationRange;
    float2 _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0 = _UnownedVariationRange;
    Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2;
    SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_81c877f57fbd4119951d21e6a05f9536_Out_0, _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0, _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0, _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2);
    UnityTexture2D _Property_03a2f987be36438cb0f8886babf33c96_Out_0 = UnityBuildTexture2DStructNoScale(_GrassTexture);
    float _Property_3ea8b576653841a7a5a603ef1f120469_Out_0 = _GrassScale;
    UnityTexture2D _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0 = UnityBuildTexture2DStructNoScale(_RoadTexture);
    float _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0 = _RoadScale;
    float _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0 = _Zoom;
    float2 _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0 = _SizeOfTexture;
    float _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0 = _RoadFade;
    float2 _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0 = _PlayerPosition;
    Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2;
    SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _Property_03a2f987be36438cb0f8886babf33c96_Out_0, _Property_3ea8b576653841a7a5a603ef1f120469_Out_0, _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0, _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0, _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0, _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0, _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0, _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2);
    float2 _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0 = _SizeOfTexture;
    float _Property_039b41a22968494fb95b7756f155d828_Out_0 = _GrassGridTiling;
    float _Property_98eedb86fc5b4145bc7b65222fede898_Out_0 = _GrassGridThickness;
    float4 _Property_da5db702a8724d9f9a82e143a886ee60_Out_0 = _GrassGridColor;
    float _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0 = _GrassGridIntenseFade;
    float _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0 = _GrassGridFarFade;
    float2 _Property_1ec68bba28374a39850d7800d8f1d362_Out_0 = _PlayerPosition;
    float _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0 = _GrassGridVariationFrequency;
    float _Property_4dcfad1507d94638b52b49967893b527_Out_0 = _GrassGridThicknessVariation;
    float2 _Property_6a4b9239d1bd424885c4b7027264b775_Out_0 = _PlayerPosition;
    float _Property_0d3761446a804744bb350177ec5d239a_Out_0 = _FogFade;
    float _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0 = _FogIntensity;
    Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af;
    float _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1;
    SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6a4b9239d1bd424885c4b7027264b775_Out_0, _Property_0d3761446a804744bb350177ec5d239a_Out_0, _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1);
    Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850;
    float _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1;
    SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_039b41a22968494fb95b7756f155d828_Out_0, _Property_98eedb86fc5b4145bc7b65222fede898_Out_0, _Property_da5db702a8724d9f9a82e143a886ee60_Out_0, _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0, _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0, _Property_1ec68bba28374a39850d7800d8f1d362_Out_0, _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0, _Property_4dcfad1507d94638b52b49967893b527_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1);
    float4 _Property_00719080222e437aa9abf5da2dd48a70_Out_0 = _ColorGrid;
    float4 _Property_118255dd5c8c455ab153d511ba1fc031_Out_0 = _ColorPlazas;
    float4 _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0 = _ColorDistricts;
    float4 _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0 = _ColorStreets;
    float4 _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0 = _ColorParcels;
    float4 _Property_6a1f7cc7467741628211b53b8709021d_Out_0 = _ColorOwnedParcels;
    float4 _Property_9d59173f2b1a48428823e35074ce62c5_Out_0 = _ColorEmpty;
    float4 _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0 = _GrassGridColor;
    float4 _Fog_caf07e8785584760b79500664df1fc44_Color_0;
    float _Fog_caf07e8785584760b79500664df1fc44_Density_1;
    Unity_Fog_float(_Fog_caf07e8785584760b79500664df1fc44_Color_0, _Fog_caf07e8785584760b79500664df1fc44_Density_1, IN.ObjectSpacePosition);
    float4 _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0;
    Merger_float(_MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _Property_00719080222e437aa9abf5da2dd48a70_Out_0, _Property_118255dd5c8c455ab153d511ba1fc031_Out_0, _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0, _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0, _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0, _Property_6a1f7cc7467741628211b53b8709021d_Out_0, _Property_9d59173f2b1a48428823e35074ce62c5_Out_0, _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0, _Fog_caf07e8785584760b79500664df1fc44_Color_0, _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0);
    float _Property_b872305fbdab44078bf3b70b9f9de114_Out_0 = _Metallic;
    float _Property_de71bcc7d06c4da3bea536b766bc403a_Out_0 = _Smoothness;
    surface.BaseColor = (_MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0.xyz);
    surface.NormalTS = IN.TangentSpaceNormal;
    surface.Emission = float3(0, 0, 0);
    surface.Metallic = _Property_b872305fbdab44078bf3b70b9f9de114_Out_0;
    surface.Smoothness = _Property_de71bcc7d06c4da3bea536b766bc403a_Out_0;
    surface.Occlusion = 1;
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



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
    output.uv0 = input.texCoord0;
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
    Blend One Zero
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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
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
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions
// GraphFunctions: <None>

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
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
    Blend One Zero
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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
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
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions
// GraphFunctions: <None>

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
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
    Blend One Zero
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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
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

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.normalWS;
        output.interp1.xyzw = input.tangentWS;
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
        output.normalWS = input.interp0.xyz;
        output.tangentWS = input.interp1.xyzw;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions
// GraphFunctions: <None>

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    surface.NormalTS = IN.TangentSpaceNormal;
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



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
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
        float4 uv0 : TEXCOORD0;
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
        float3 ObjectSpacePosition;
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

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.texCoord0;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

struct Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b
{
    float3 WorldSpacePosition;
    half4 uv0;
};

void SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(float2 Vector2_6bff7006be6546f1a2eccc78e58e6232, float Vector1_e86c202cda73418eae7d4a134b98a195, Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b IN, out float2 UV_1, out float2 WorldUV_3, out float Zoom_2)
{
    float4 _UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0 = IN.uv0;
    float _Split_259410824e3c4f498de1eef89dacf280_R_1 = IN.WorldSpacePosition[0];
    float _Split_259410824e3c4f498de1eef89dacf280_G_2 = IN.WorldSpacePosition[1];
    float _Split_259410824e3c4f498de1eef89dacf280_B_3 = IN.WorldSpacePosition[2];
    float _Split_259410824e3c4f498de1eef89dacf280_A_4 = 0;
    float2 _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0 = float2(_Split_259410824e3c4f498de1eef89dacf280_R_1, _Split_259410824e3c4f498de1eef89dacf280_B_3);
    float _Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0 = Vector1_e86c202cda73418eae7d4a134b98a195;
    float _Split_70c1c438ae374dec9f011f8d2999c80e_R_1 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[0];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_G_2 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[1];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_B_3 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[2];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_A_4 = 0;
    float _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
    Unity_Divide_float(_Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0, _Split_70c1c438ae374dec9f011f8d2999c80e_R_1, _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2);
    UV_1 = (_UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0.xy);
    WorldUV_3 = _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0;
    Zoom_2 = _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
}

// 7b6d5a90df0cb86d20ecea9cb96d928e
#include "Assets/Rendering/InfiniteFloor/MapV5.hlsl"

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Comparison_NotEqual_float(float A, float B, out float Out)
{
    Out = A != B ? 1 : 0;
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Floor_float2(float2 In, out float2 Out)
{
    Out = floor(In);
}


inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}


inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}


inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = Unity_SimpleNoise_RandomValue_float(c0);
    float r1 = Unity_SimpleNoise_RandomValue_float(c1);
    float r2 = Unity_SimpleNoise_RandomValue_float(c2);
    float r3 = Unity_SimpleNoise_RandomValue_float(c3);

    float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
    float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
    float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
    return t;
}

void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3 - 0));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3 - 1));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3 - 2));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    Out = t;
}

void Unity_Branch_float(float Predicate, float True, float False, out float Out)
{
    Out = Predicate ? True : False;
}

struct Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841
{
};

void SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(float2 Vector2_cd614e04b07b47eb95a2e5e3ffa41872, float Vector1_355229664685490fa0fc11fe1a97899f, float2 Vector2_1721f8718e464df9a2a9bd7239c50524, UnityTexture2D Texture2D_405855aa0a514baaa11320593c2f07c1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03, Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 IN, out float Mixed_1, out float Owned_2)
{
    UnityTexture2D _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0 = Texture2D_405855aa0a514baaa11320593c2f07c1;
    float2 _Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0 = Vector2_cd614e04b07b47eb95a2e5e3ffa41872;
    float _Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0 = 1;
    float _Property_06d14f9324724405bca2e16df40bef40_Out_0 = Vector1_355229664685490fa0fc11fe1a97899f;
    float _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2;
    Unity_Divide_float(_Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0, _Property_06d14f9324724405bca2e16df40bef40_Out_0, _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2);
    float _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2;
    Unity_Multiply_float(_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2, -1, _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2);
    float _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2;
    Unity_Divide_float(_Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2, 2, _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2);
    float _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2;
    Unity_Add_float(_Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2, 0.5, _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2);
    float2 _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3;
    Unity_TilingAndOffset_float(_Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0, (_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2.xx), (_Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2.xx), _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float4 _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.tex, _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.samplerstate, _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_R_4 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.r;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.g;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.b;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_A_7 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.a;
    float _Add_c7db3142e97045b2877ef4033d663af0_Out_2;
    Unity_Add_float(_SampleTexture2D_108c05205d884e9298d8d59122709828_R_4, _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5, _Add_c7db3142e97045b2877ef4033d663af0_Out_2);
    float _Add_da555a32eb354a279399036fba5f852b_Out_2;
    Unity_Add_float(_Add_c7db3142e97045b2877ef4033d663af0_Out_2, _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6, _Add_da555a32eb354a279399036fba5f852b_Out_2);
    float _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2);
    float2 _Property_49ef008f07834aba983ae50300e94c82_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1;
    float _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3;
    Unity_Remap_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, float2 (0, 1), _Property_49ef008f07834aba983ae50300e94c82_Out_0, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3);
    float2 _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0 = Vector2_1721f8718e464df9a2a9bd7239c50524;
    float2 _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3, _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0, float2 (0, 0), _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3);
    float2 _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1;
    Unity_Floor_float2(_TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3, _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1);
    float _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2;
    Unity_SimpleNoise_float(_Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1, 150, _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2);
    float2 _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03;
    float _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3;
    Unity_Remap_float(_SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2, float2 (0, 1), _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3);
    float _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Unity_Branch_float(_Comparison_9f9bfac15975447a9c81341908b981a0_Out_2, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3, _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3);
    float _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2);
    float _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
    Unity_Branch_float(_Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2, 1, 0, _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3);
    Mixed_1 = _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Owned_2 = _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

struct Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e
{
};

void SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(float2 Vector2_a43b69bd63e74b8899664790c597f8c6, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c, float Vector1_8be634a8378a4521b7522d631008fc39, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c_1, float Vector1_1, float Vector1_78a75bc300dd47ee83f8fbd9e84a0cad, float2 Vector2_f975587bc79d4eadbd0807b55a090f9d, float Vector1_e4cafe8cd46043f0ae5392b59d6b03fe, float2 Vector2_7389d8e6e0014c32be011a6864268e6a, float2 Vector2_cf1b396af9b54596bc8052bf3fe215fb, Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e IN, out float Grass_1, out float Road_2)
{
    UnityTexture2D _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c;
    float2 _Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0 = Vector1_8be634a8378a4521b7522d631008fc39;
    float _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2;
    Unity_Divide_float(1, _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0, _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2);
    float _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2;
    Unity_Multiply_float(_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2, -1, _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2);
    float _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2;
    Unity_Divide_float(_Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2, 2, _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2);
    float _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2;
    Unity_Add_float(_Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2, 0.5, _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2);
    float2 _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3;
    Unity_TilingAndOffset_float(_Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0, (_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2.xx), (_Add_e636f222615a4d5b80b5dc3743ef5097_Out_2.xx), _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float4 _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.tex, _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.samplerstate, _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.r;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_G_5 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.g;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_B_6 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.b;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_A_7 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.a;
    UnityTexture2D _Property_acfdee93b05546369a691885f7d8fc49_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c_1;
    float2 _Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float2 _Property_622a1d2b345749f7af37dbeb28c9856a_Out_0 = Vector2_f975587bc79d4eadbd0807b55a090f9d;
    float _Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0 = 1;
    float _Property_9655382379e942478a21d79fef207bdd_Out_0 = Vector1_78a75bc300dd47ee83f8fbd9e84a0cad;
    float _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2;
    Unity_Divide_float(_Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0, _Property_9655382379e942478a21d79fef207bdd_Out_0, _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2);
    float2 _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2;
    Unity_Multiply_float(_Property_622a1d2b345749f7af37dbeb28c9856a_Out_0, (_Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2.xx), _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2);
    float _Split_92aaba0363214ad786eeca836e64e191_R_1 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[0];
    float _Split_92aaba0363214ad786eeca836e64e191_G_2 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[1];
    float _Split_92aaba0363214ad786eeca836e64e191_B_3 = 0;
    float _Split_92aaba0363214ad786eeca836e64e191_A_4 = 0;
    float _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2;
    Unity_Multiply_float(_Split_92aaba0363214ad786eeca836e64e191_R_1, -1, _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2);
    float _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2;
    Unity_Divide_float(_Multiply_426451ec50dc4f3b80866858356f7c82_Out_2, 2, _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2);
    float _Add_1e90aeca170749febce3402fc85db207_Out_2;
    Unity_Add_float(_Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2, 0.5, _Add_1e90aeca170749febce3402fc85db207_Out_2);
    float2 _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3;
    Unity_TilingAndOffset_float(_Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0, (_Split_92aaba0363214ad786eeca836e64e191_R_1.xx), (_Add_1e90aeca170749febce3402fc85db207_Out_2.xx), _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3);
    float2 _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3, _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1);
    float _Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0 = Vector1_1;
    float _Multiply_3e0c255ae161401f94116d1116002307_Out_2;
    Unity_Multiply_float(_Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0, 10000, _Multiply_3e0c255ae161401f94116d1116002307_Out_2);
    float2 _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3;
    Unity_TilingAndOffset_float(_Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1, (_Multiply_3e0c255ae161401f94116d1116002307_Out_2.xx), float2 (0, 0), _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float4 _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0 = SAMPLE_TEXTURE2D(_Property_acfdee93b05546369a691885f7d8fc49_Out_0.tex, _Property_acfdee93b05546369a691885f7d8fc49_Out_0.samplerstate, _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.r;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_G_5 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.g;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_B_6 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.b;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_A_7 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.a;
    float2 _Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0 = Vector2_cf1b396af9b54596bc8052bf3fe215fb;
    float2 _Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0 = Vector2_7389d8e6e0014c32be011a6864268e6a;
    float2 _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2;
    Unity_Multiply_float(_Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0, float2(-1, -1), _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2);
    float2 _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2;
    Unity_Add_float2(_Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0, _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2, _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2);
    float _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0 = Vector1_e4cafe8cd46043f0ae5392b59d6b03fe;
    float _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2;
    Unity_Divide_float(10, _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0, _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2);
    float2 _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4;
    Unity_PolarCoordinates_float(_Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2, float2 (0.5, 0.5), _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2, 1, _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4);
    float _Split_f3387914733a4792b78907a73c898380_R_1 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[0];
    float _Split_f3387914733a4792b78907a73c898380_G_2 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[1];
    float _Split_f3387914733a4792b78907a73c898380_B_3 = 0;
    float _Split_f3387914733a4792b78907a73c898380_A_4 = 0;
    float _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1;
    Unity_Saturate_float(_Split_f3387914733a4792b78907a73c898380_R_1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1);
    float _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
    Unity_Lerp_float(_SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4, 1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1, _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3);
    Grass_1 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4;
    Road_2 = _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
}

struct Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a
{
};

void SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(float2 Vector2_311ffee78d314f71a9463e39924ea623, float2 Vector2_a57b68e1b4044834933fd8337f0a0577, float Vector1_7284deecf5d9431d92fc35a123337ff4, float Vector1_ab2c4cd721534cf4a387156d51a1fed9, Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a IN, out float Out_1)
{
    float2 _Property_70523c283f40499f89e4f7748deff77e_Out_0 = Vector2_311ffee78d314f71a9463e39924ea623;
    float2 _Property_f28b80022c3246688280e0762030829b_Out_0 = Vector2_a57b68e1b4044834933fd8337f0a0577;
    float2 _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2;
    Unity_Multiply_float(_Property_f28b80022c3246688280e0762030829b_Out_0, float2(-1, -1), _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2);
    float2 _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2;
    Unity_Add_float2(_Property_70523c283f40499f89e4f7748deff77e_Out_0, _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2, _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2);
    float _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0 = Vector1_7284deecf5d9431d92fc35a123337ff4;
    float _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2;
    Unity_Divide_float(1, _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0, _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2);
    float2 _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4;
    Unity_PolarCoordinates_float(_Add_e90ad347cd4b42c3963540725f4e79d9_Out_2, float2 (0.5, 0.5), _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2, 1, _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4);
    float _Split_904e58337bbe428998ef573899b98f55_R_1 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[0];
    float _Split_904e58337bbe428998ef573899b98f55_G_2 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[1];
    float _Split_904e58337bbe428998ef573899b98f55_B_3 = 0;
    float _Split_904e58337bbe428998ef573899b98f55_A_4 = 0;
    float _Property_f8541835e99e409989806d7eff9d13e8_Out_0 = Vector1_ab2c4cd721534cf4a387156d51a1fed9;
    float _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2;
    Unity_Multiply_float(_Split_904e58337bbe428998ef573899b98f55_R_1, _Property_f8541835e99e409989806d7eff9d13e8_Out_0, _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2);
    float _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
    Unity_Saturate_float(_Multiply_a77283d5783542b596ccaa11bb712b63_Out_2, _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1);
    Out_1 = _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Rectangle_float(float2 UV, float Width, float Height, out float Out)
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

struct Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c
{
};

void SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(float2 Vector2_e01f7c264af944fdb8bcea2d35ae3001, float2 Vector2_622ed4642c4445d194b78ad6759b208d, float2 Vector2_575137a8a58748d1a0e062a00216bbe5, float Vector1_23353b8652e043faab2f58b3964e3f17, float Vector1_5c017085898d45e48611a2e9ace96469, float Vector1_f8f78e1de998447c949d6ce599a31355, float4 Vector4_5fb32e510cd648f8b219982d0bc6426a, float Vector1_eb4dbe959ea64ae896f61f72a5d275d0, float Vector1_2b42c6accf5149c3929d5731c737ba7c, float2 Vector2_2c998556cbda461d8a0b69199046f9f5, float Vector1_f62ba0f4717b42c1b7e03ce424479587, float Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d, float Vector1_63cdfc1b7ebd4084b00ceb9b109e3919, Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c IN, out float GrassGrid_1)
{
    float2 _Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0 = Vector2_622ed4642c4445d194b78ad6759b208d;
    float2 _Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0 = Vector2_2c998556cbda461d8a0b69199046f9f5;
    float2 _Multiply_d348b89a76874839babedba1f8d3296d_Out_2;
    Unity_Multiply_float(_Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0, float2(-1, -1), _Multiply_d348b89a76874839babedba1f8d3296d_Out_2);
    float2 _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2;
    Unity_Add_float2(_Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0, _Multiply_d348b89a76874839babedba1f8d3296d_Out_2, _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2);
    float _Property_eb9ff929e1204221bca1c31925f600b7_Out_0 = Vector1_2b42c6accf5149c3929d5731c737ba7c;
    float _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2;
    Unity_Divide_float(10, _Property_eb9ff929e1204221bca1c31925f600b7_Out_0, _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2);
    float2 _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2, 1, _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4);
    float _Split_2e7cca56ed8b4f69890662df97d724ba_R_1 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[0];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_G_2 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[1];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_B_3 = 0;
    float _Split_2e7cca56ed8b4f69890662df97d724ba_A_4 = 0;
    float _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1;
    Unity_OneMinus_float(_Split_2e7cca56ed8b4f69890662df97d724ba_R_1, _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1);
    float _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1;
    Unity_Saturate_float(_OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1, _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1);
    float _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0 = Vector1_eb4dbe959ea64ae896f61f72a5d275d0;
    float _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2;
    Unity_Divide_float(10, _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0, _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2);
    float2 _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2, 1, _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4);
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[0];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_G_2 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[1];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_B_3 = 0;
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_A_4 = 0;
    float _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1;
    Unity_OneMinus_float(_Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1, _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1);
    float _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1;
    Unity_Saturate_float(_OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1, _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1);
    float2 _Property_8c07974a4edd4dc89101134d98954a00_Out_0 = Vector2_e01f7c264af944fdb8bcea2d35ae3001;
    float _Float_93afd7af653a45a38377067c2d80ab35_Out_0 = 1;
    float _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0 = Vector1_23353b8652e043faab2f58b3964e3f17;
    float _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2;
    Unity_Divide_float(_Float_93afd7af653a45a38377067c2d80ab35_Out_0, _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0, _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2);
    float _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2;
    Unity_Multiply_float(_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2, -1, _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2);
    float _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2;
    Unity_Divide_float(_Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2, 2, _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2);
    float _Add_bc17962f1e2d49fca18bb00e85478880_Out_2;
    Unity_Add_float(_Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2, 0.5, _Add_bc17962f1e2d49fca18bb00e85478880_Out_2);
    float2 _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3;
    Unity_TilingAndOffset_float(_Property_8c07974a4edd4dc89101134d98954a00_Out_0, (_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2.xx), (_Add_bc17962f1e2d49fca18bb00e85478880_Out_2.xx), _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3);
    float2 _Property_deac93b8878546c8a300d7352a631a26_Out_0 = Vector2_575137a8a58748d1a0e062a00216bbe5;
    float2 _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3, _Property_deac93b8878546c8a300d7352a631a26_Out_0, float2 (0, 0), _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3);
    float2 _Fraction_a4f4615406494c08b0082401f60051c2_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3, _Fraction_a4f4615406494c08b0082401f60051c2_Out_1);
    float _Property_0081be27ffb041f4ac12f66f0dc62624_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float2 _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Property_0081be27ffb041f4ac12f66f0dc62624_Out_0.xx), float2 (0, 0), _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3);
    float2 _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3, _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1);
    float _Property_d89b62d779e54332979e2425f6cd1857_Out_0 = Vector1_f8f78e1de998447c949d6ce599a31355;
    float _Add_1feb1662c2884dffad05bb44310c6586_Out_2;
    Unity_Add_float(_Property_d89b62d779e54332979e2425f6cd1857_Out_0, 1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2);
    float _Divide_43532697f4454e47a4092b408ec0ff25_Out_2;
    Unity_Divide_float(1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2);
    float _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3;
    Unity_Rectangle_float(_Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3);
    float _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1;
    Unity_OneMinus_float(_Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3, _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1);
    float _Property_43f57c55e694444e9bf9aeb01760d823_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_853fa90d1d314da390af1a6f21f72298_Out_2;
    Unity_Divide_float(_Property_43f57c55e694444e9bf9aeb01760d823_Out_0, _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0, _Divide_853fa90d1d314da390af1a6f21f72298_Out_2);
    float2 _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Divide_853fa90d1d314da390af1a6f21f72298_Out_2.xx), float2 (0, 0), _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3);
    float2 _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3, _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1);
    float _Property_5236eaaed502435380fd9232ca3f5a7b_Out_0 = Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d;
    float _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_479d5e5f38be495d982aed56501420aa_Out_2;
    Unity_Divide_float(_Property_5236eaaed502435380fd9232ca3f5a7b_Out_0, _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0, _Divide_479d5e5f38be495d982aed56501420aa_Out_2);
    float _Add_eae9a749d00c4960aea43a9320fead3e_Out_2;
    Unity_Add_float(_Divide_479d5e5f38be495d982aed56501420aa_Out_2, 1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2);
    float _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2;
    Unity_Divide_float(1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2);
    float _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3;
    Unity_Rectangle_float(_Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3);
    float _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1;
    Unity_OneMinus_float(_Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1);
    float _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2;
    Unity_Add_float(_OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1, _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2);
    float _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1;
    Unity_Saturate_float(_Add_2b900f3ab1b44858bf5696180eac62e6_Out_2, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1);
    float _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2;
    Unity_Multiply_float(_Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2);
    float4 _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0 = Vector4_5fb32e510cd648f8b219982d0bc6426a;
    float _Split_d97e936de3fa453e9725f0c2256e5eac_R_1 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[0];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_G_2 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[1];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_B_3 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[2];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_A_4 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[3];
    float _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2;
    Unity_Multiply_float(_Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Split_d97e936de3fa453e9725f0c2256e5eac_A_4, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2);
    float _Add_9af69bb68e3044b29a3495457a20582c_Out_2;
    Unity_Add_float(_Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2, _Add_9af69bb68e3044b29a3495457a20582c_Out_2);
    float _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
    Unity_Multiply_float(_Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1, _Add_9af69bb68e3044b29a3495457a20582c_Out_2, _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2);
    GrassGrid_1 = _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
}

void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
{
    SHADERGRAPH_FOG(Position, Color, Density);
}

// 98116c2c658709e5fcb200b1ae28460e
#include "Assets/Rendering/InfiniteFloor/InfiniteFloorMerger.hlsl"

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_e943bfd340cf4709a76ba852685dbf55_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_c294f42edfdb40c18d1605395ee9f835_Out_0 = _PlayerPosition;
    float _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0 = _Zoom;
    Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.WorldSpacePosition = IN.WorldSpacePosition;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.uv0 = IN.uv0;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3;
    float _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2;
    SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(_Property_c294f42edfdb40c18d1605395ee9f835_Out_0, _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2);
    float2 _Property_edde0349814e405a8f77a67715a72a11_Out_0 = _SizeOfTexture;
    float _Property_c4b62049d152467eb90794c337831029_Out_0 = _GridThickness;
    float _Property_038d46b4121440948e0171cd5c26d417_Out_0 = _ThicknessOffset;
    float _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0 = _GridOffset;
    float4 _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18;
    Main_float(_Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0, _Property_e943bfd340cf4709a76ba852685dbf55_Out_0, 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_edde0349814e405a8f77a67715a72a11_Out_0, _Property_c4b62049d152467eb90794c337831029_Out_0, _Property_038d46b4121440948e0171cd5c26d417_Out_0, _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0, float2 (0, 0), 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18);
    float2 _Property_81c877f57fbd4119951d21e6a05f9536_Out_0 = _SizeOfTexture;
    UnityTexture2D _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0 = _OwnedVariationRange;
    float2 _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0 = _UnownedVariationRange;
    Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2;
    SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_81c877f57fbd4119951d21e6a05f9536_Out_0, _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0, _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0, _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2);
    UnityTexture2D _Property_03a2f987be36438cb0f8886babf33c96_Out_0 = UnityBuildTexture2DStructNoScale(_GrassTexture);
    float _Property_3ea8b576653841a7a5a603ef1f120469_Out_0 = _GrassScale;
    UnityTexture2D _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0 = UnityBuildTexture2DStructNoScale(_RoadTexture);
    float _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0 = _RoadScale;
    float _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0 = _Zoom;
    float2 _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0 = _SizeOfTexture;
    float _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0 = _RoadFade;
    float2 _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0 = _PlayerPosition;
    Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2;
    SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _Property_03a2f987be36438cb0f8886babf33c96_Out_0, _Property_3ea8b576653841a7a5a603ef1f120469_Out_0, _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0, _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0, _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0, _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0, _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0, _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2);
    float2 _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0 = _SizeOfTexture;
    float _Property_039b41a22968494fb95b7756f155d828_Out_0 = _GrassGridTiling;
    float _Property_98eedb86fc5b4145bc7b65222fede898_Out_0 = _GrassGridThickness;
    float4 _Property_da5db702a8724d9f9a82e143a886ee60_Out_0 = _GrassGridColor;
    float _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0 = _GrassGridIntenseFade;
    float _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0 = _GrassGridFarFade;
    float2 _Property_1ec68bba28374a39850d7800d8f1d362_Out_0 = _PlayerPosition;
    float _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0 = _GrassGridVariationFrequency;
    float _Property_4dcfad1507d94638b52b49967893b527_Out_0 = _GrassGridThicknessVariation;
    float2 _Property_6a4b9239d1bd424885c4b7027264b775_Out_0 = _PlayerPosition;
    float _Property_0d3761446a804744bb350177ec5d239a_Out_0 = _FogFade;
    float _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0 = _FogIntensity;
    Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af;
    float _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1;
    SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6a4b9239d1bd424885c4b7027264b775_Out_0, _Property_0d3761446a804744bb350177ec5d239a_Out_0, _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1);
    Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850;
    float _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1;
    SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_039b41a22968494fb95b7756f155d828_Out_0, _Property_98eedb86fc5b4145bc7b65222fede898_Out_0, _Property_da5db702a8724d9f9a82e143a886ee60_Out_0, _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0, _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0, _Property_1ec68bba28374a39850d7800d8f1d362_Out_0, _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0, _Property_4dcfad1507d94638b52b49967893b527_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1);
    float4 _Property_00719080222e437aa9abf5da2dd48a70_Out_0 = _ColorGrid;
    float4 _Property_118255dd5c8c455ab153d511ba1fc031_Out_0 = _ColorPlazas;
    float4 _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0 = _ColorDistricts;
    float4 _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0 = _ColorStreets;
    float4 _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0 = _ColorParcels;
    float4 _Property_6a1f7cc7467741628211b53b8709021d_Out_0 = _ColorOwnedParcels;
    float4 _Property_9d59173f2b1a48428823e35074ce62c5_Out_0 = _ColorEmpty;
    float4 _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0 = _GrassGridColor;
    float4 _Fog_caf07e8785584760b79500664df1fc44_Color_0;
    float _Fog_caf07e8785584760b79500664df1fc44_Density_1;
    Unity_Fog_float(_Fog_caf07e8785584760b79500664df1fc44_Color_0, _Fog_caf07e8785584760b79500664df1fc44_Density_1, IN.ObjectSpacePosition);
    float4 _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0;
    Merger_float(_MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _Property_00719080222e437aa9abf5da2dd48a70_Out_0, _Property_118255dd5c8c455ab153d511ba1fc031_Out_0, _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0, _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0, _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0, _Property_6a1f7cc7467741628211b53b8709021d_Out_0, _Property_9d59173f2b1a48428823e35074ce62c5_Out_0, _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0, _Fog_caf07e8785584760b79500664df1fc44_Color_0, _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0);
    surface.BaseColor = (_MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0.xyz);
    surface.Emission = float3(0, 0, 0);
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





    output.WorldSpacePosition = input.positionWS;
    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
    output.uv0 = input.texCoord0;
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
    Blend One Zero
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
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
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
        float3 ObjectSpacePosition;
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

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.texCoord0;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

struct Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b
{
    float3 WorldSpacePosition;
    half4 uv0;
};

void SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(float2 Vector2_6bff7006be6546f1a2eccc78e58e6232, float Vector1_e86c202cda73418eae7d4a134b98a195, Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b IN, out float2 UV_1, out float2 WorldUV_3, out float Zoom_2)
{
    float4 _UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0 = IN.uv0;
    float _Split_259410824e3c4f498de1eef89dacf280_R_1 = IN.WorldSpacePosition[0];
    float _Split_259410824e3c4f498de1eef89dacf280_G_2 = IN.WorldSpacePosition[1];
    float _Split_259410824e3c4f498de1eef89dacf280_B_3 = IN.WorldSpacePosition[2];
    float _Split_259410824e3c4f498de1eef89dacf280_A_4 = 0;
    float2 _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0 = float2(_Split_259410824e3c4f498de1eef89dacf280_R_1, _Split_259410824e3c4f498de1eef89dacf280_B_3);
    float _Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0 = Vector1_e86c202cda73418eae7d4a134b98a195;
    float _Split_70c1c438ae374dec9f011f8d2999c80e_R_1 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[0];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_G_2 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[1];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_B_3 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[2];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_A_4 = 0;
    float _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
    Unity_Divide_float(_Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0, _Split_70c1c438ae374dec9f011f8d2999c80e_R_1, _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2);
    UV_1 = (_UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0.xy);
    WorldUV_3 = _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0;
    Zoom_2 = _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
}

// 7b6d5a90df0cb86d20ecea9cb96d928e
#include "Assets/Rendering/InfiniteFloor/MapV5.hlsl"

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Comparison_NotEqual_float(float A, float B, out float Out)
{
    Out = A != B ? 1 : 0;
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Floor_float2(float2 In, out float2 Out)
{
    Out = floor(In);
}


inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}


inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}


inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = Unity_SimpleNoise_RandomValue_float(c0);
    float r1 = Unity_SimpleNoise_RandomValue_float(c1);
    float r2 = Unity_SimpleNoise_RandomValue_float(c2);
    float r3 = Unity_SimpleNoise_RandomValue_float(c3);

    float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
    float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
    float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
    return t;
}

void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3 - 0));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3 - 1));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3 - 2));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    Out = t;
}

void Unity_Branch_float(float Predicate, float True, float False, out float Out)
{
    Out = Predicate ? True : False;
}

struct Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841
{
};

void SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(float2 Vector2_cd614e04b07b47eb95a2e5e3ffa41872, float Vector1_355229664685490fa0fc11fe1a97899f, float2 Vector2_1721f8718e464df9a2a9bd7239c50524, UnityTexture2D Texture2D_405855aa0a514baaa11320593c2f07c1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03, Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 IN, out float Mixed_1, out float Owned_2)
{
    UnityTexture2D _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0 = Texture2D_405855aa0a514baaa11320593c2f07c1;
    float2 _Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0 = Vector2_cd614e04b07b47eb95a2e5e3ffa41872;
    float _Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0 = 1;
    float _Property_06d14f9324724405bca2e16df40bef40_Out_0 = Vector1_355229664685490fa0fc11fe1a97899f;
    float _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2;
    Unity_Divide_float(_Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0, _Property_06d14f9324724405bca2e16df40bef40_Out_0, _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2);
    float _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2;
    Unity_Multiply_float(_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2, -1, _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2);
    float _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2;
    Unity_Divide_float(_Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2, 2, _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2);
    float _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2;
    Unity_Add_float(_Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2, 0.5, _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2);
    float2 _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3;
    Unity_TilingAndOffset_float(_Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0, (_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2.xx), (_Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2.xx), _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float4 _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.tex, _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.samplerstate, _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_R_4 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.r;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.g;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.b;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_A_7 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.a;
    float _Add_c7db3142e97045b2877ef4033d663af0_Out_2;
    Unity_Add_float(_SampleTexture2D_108c05205d884e9298d8d59122709828_R_4, _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5, _Add_c7db3142e97045b2877ef4033d663af0_Out_2);
    float _Add_da555a32eb354a279399036fba5f852b_Out_2;
    Unity_Add_float(_Add_c7db3142e97045b2877ef4033d663af0_Out_2, _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6, _Add_da555a32eb354a279399036fba5f852b_Out_2);
    float _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2);
    float2 _Property_49ef008f07834aba983ae50300e94c82_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1;
    float _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3;
    Unity_Remap_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, float2 (0, 1), _Property_49ef008f07834aba983ae50300e94c82_Out_0, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3);
    float2 _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0 = Vector2_1721f8718e464df9a2a9bd7239c50524;
    float2 _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3, _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0, float2 (0, 0), _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3);
    float2 _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1;
    Unity_Floor_float2(_TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3, _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1);
    float _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2;
    Unity_SimpleNoise_float(_Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1, 150, _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2);
    float2 _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03;
    float _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3;
    Unity_Remap_float(_SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2, float2 (0, 1), _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3);
    float _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Unity_Branch_float(_Comparison_9f9bfac15975447a9c81341908b981a0_Out_2, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3, _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3);
    float _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2);
    float _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
    Unity_Branch_float(_Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2, 1, 0, _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3);
    Mixed_1 = _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Owned_2 = _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

struct Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e
{
};

void SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(float2 Vector2_a43b69bd63e74b8899664790c597f8c6, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c, float Vector1_8be634a8378a4521b7522d631008fc39, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c_1, float Vector1_1, float Vector1_78a75bc300dd47ee83f8fbd9e84a0cad, float2 Vector2_f975587bc79d4eadbd0807b55a090f9d, float Vector1_e4cafe8cd46043f0ae5392b59d6b03fe, float2 Vector2_7389d8e6e0014c32be011a6864268e6a, float2 Vector2_cf1b396af9b54596bc8052bf3fe215fb, Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e IN, out float Grass_1, out float Road_2)
{
    UnityTexture2D _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c;
    float2 _Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0 = Vector1_8be634a8378a4521b7522d631008fc39;
    float _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2;
    Unity_Divide_float(1, _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0, _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2);
    float _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2;
    Unity_Multiply_float(_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2, -1, _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2);
    float _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2;
    Unity_Divide_float(_Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2, 2, _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2);
    float _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2;
    Unity_Add_float(_Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2, 0.5, _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2);
    float2 _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3;
    Unity_TilingAndOffset_float(_Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0, (_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2.xx), (_Add_e636f222615a4d5b80b5dc3743ef5097_Out_2.xx), _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float4 _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.tex, _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.samplerstate, _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.r;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_G_5 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.g;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_B_6 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.b;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_A_7 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.a;
    UnityTexture2D _Property_acfdee93b05546369a691885f7d8fc49_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c_1;
    float2 _Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float2 _Property_622a1d2b345749f7af37dbeb28c9856a_Out_0 = Vector2_f975587bc79d4eadbd0807b55a090f9d;
    float _Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0 = 1;
    float _Property_9655382379e942478a21d79fef207bdd_Out_0 = Vector1_78a75bc300dd47ee83f8fbd9e84a0cad;
    float _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2;
    Unity_Divide_float(_Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0, _Property_9655382379e942478a21d79fef207bdd_Out_0, _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2);
    float2 _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2;
    Unity_Multiply_float(_Property_622a1d2b345749f7af37dbeb28c9856a_Out_0, (_Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2.xx), _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2);
    float _Split_92aaba0363214ad786eeca836e64e191_R_1 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[0];
    float _Split_92aaba0363214ad786eeca836e64e191_G_2 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[1];
    float _Split_92aaba0363214ad786eeca836e64e191_B_3 = 0;
    float _Split_92aaba0363214ad786eeca836e64e191_A_4 = 0;
    float _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2;
    Unity_Multiply_float(_Split_92aaba0363214ad786eeca836e64e191_R_1, -1, _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2);
    float _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2;
    Unity_Divide_float(_Multiply_426451ec50dc4f3b80866858356f7c82_Out_2, 2, _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2);
    float _Add_1e90aeca170749febce3402fc85db207_Out_2;
    Unity_Add_float(_Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2, 0.5, _Add_1e90aeca170749febce3402fc85db207_Out_2);
    float2 _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3;
    Unity_TilingAndOffset_float(_Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0, (_Split_92aaba0363214ad786eeca836e64e191_R_1.xx), (_Add_1e90aeca170749febce3402fc85db207_Out_2.xx), _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3);
    float2 _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3, _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1);
    float _Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0 = Vector1_1;
    float _Multiply_3e0c255ae161401f94116d1116002307_Out_2;
    Unity_Multiply_float(_Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0, 10000, _Multiply_3e0c255ae161401f94116d1116002307_Out_2);
    float2 _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3;
    Unity_TilingAndOffset_float(_Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1, (_Multiply_3e0c255ae161401f94116d1116002307_Out_2.xx), float2 (0, 0), _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float4 _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0 = SAMPLE_TEXTURE2D(_Property_acfdee93b05546369a691885f7d8fc49_Out_0.tex, _Property_acfdee93b05546369a691885f7d8fc49_Out_0.samplerstate, _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.r;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_G_5 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.g;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_B_6 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.b;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_A_7 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.a;
    float2 _Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0 = Vector2_cf1b396af9b54596bc8052bf3fe215fb;
    float2 _Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0 = Vector2_7389d8e6e0014c32be011a6864268e6a;
    float2 _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2;
    Unity_Multiply_float(_Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0, float2(-1, -1), _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2);
    float2 _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2;
    Unity_Add_float2(_Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0, _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2, _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2);
    float _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0 = Vector1_e4cafe8cd46043f0ae5392b59d6b03fe;
    float _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2;
    Unity_Divide_float(10, _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0, _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2);
    float2 _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4;
    Unity_PolarCoordinates_float(_Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2, float2 (0.5, 0.5), _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2, 1, _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4);
    float _Split_f3387914733a4792b78907a73c898380_R_1 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[0];
    float _Split_f3387914733a4792b78907a73c898380_G_2 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[1];
    float _Split_f3387914733a4792b78907a73c898380_B_3 = 0;
    float _Split_f3387914733a4792b78907a73c898380_A_4 = 0;
    float _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1;
    Unity_Saturate_float(_Split_f3387914733a4792b78907a73c898380_R_1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1);
    float _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
    Unity_Lerp_float(_SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4, 1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1, _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3);
    Grass_1 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4;
    Road_2 = _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
}

struct Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a
{
};

void SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(float2 Vector2_311ffee78d314f71a9463e39924ea623, float2 Vector2_a57b68e1b4044834933fd8337f0a0577, float Vector1_7284deecf5d9431d92fc35a123337ff4, float Vector1_ab2c4cd721534cf4a387156d51a1fed9, Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a IN, out float Out_1)
{
    float2 _Property_70523c283f40499f89e4f7748deff77e_Out_0 = Vector2_311ffee78d314f71a9463e39924ea623;
    float2 _Property_f28b80022c3246688280e0762030829b_Out_0 = Vector2_a57b68e1b4044834933fd8337f0a0577;
    float2 _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2;
    Unity_Multiply_float(_Property_f28b80022c3246688280e0762030829b_Out_0, float2(-1, -1), _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2);
    float2 _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2;
    Unity_Add_float2(_Property_70523c283f40499f89e4f7748deff77e_Out_0, _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2, _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2);
    float _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0 = Vector1_7284deecf5d9431d92fc35a123337ff4;
    float _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2;
    Unity_Divide_float(1, _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0, _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2);
    float2 _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4;
    Unity_PolarCoordinates_float(_Add_e90ad347cd4b42c3963540725f4e79d9_Out_2, float2 (0.5, 0.5), _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2, 1, _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4);
    float _Split_904e58337bbe428998ef573899b98f55_R_1 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[0];
    float _Split_904e58337bbe428998ef573899b98f55_G_2 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[1];
    float _Split_904e58337bbe428998ef573899b98f55_B_3 = 0;
    float _Split_904e58337bbe428998ef573899b98f55_A_4 = 0;
    float _Property_f8541835e99e409989806d7eff9d13e8_Out_0 = Vector1_ab2c4cd721534cf4a387156d51a1fed9;
    float _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2;
    Unity_Multiply_float(_Split_904e58337bbe428998ef573899b98f55_R_1, _Property_f8541835e99e409989806d7eff9d13e8_Out_0, _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2);
    float _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
    Unity_Saturate_float(_Multiply_a77283d5783542b596ccaa11bb712b63_Out_2, _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1);
    Out_1 = _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Rectangle_float(float2 UV, float Width, float Height, out float Out)
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

struct Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c
{
};

void SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(float2 Vector2_e01f7c264af944fdb8bcea2d35ae3001, float2 Vector2_622ed4642c4445d194b78ad6759b208d, float2 Vector2_575137a8a58748d1a0e062a00216bbe5, float Vector1_23353b8652e043faab2f58b3964e3f17, float Vector1_5c017085898d45e48611a2e9ace96469, float Vector1_f8f78e1de998447c949d6ce599a31355, float4 Vector4_5fb32e510cd648f8b219982d0bc6426a, float Vector1_eb4dbe959ea64ae896f61f72a5d275d0, float Vector1_2b42c6accf5149c3929d5731c737ba7c, float2 Vector2_2c998556cbda461d8a0b69199046f9f5, float Vector1_f62ba0f4717b42c1b7e03ce424479587, float Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d, float Vector1_63cdfc1b7ebd4084b00ceb9b109e3919, Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c IN, out float GrassGrid_1)
{
    float2 _Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0 = Vector2_622ed4642c4445d194b78ad6759b208d;
    float2 _Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0 = Vector2_2c998556cbda461d8a0b69199046f9f5;
    float2 _Multiply_d348b89a76874839babedba1f8d3296d_Out_2;
    Unity_Multiply_float(_Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0, float2(-1, -1), _Multiply_d348b89a76874839babedba1f8d3296d_Out_2);
    float2 _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2;
    Unity_Add_float2(_Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0, _Multiply_d348b89a76874839babedba1f8d3296d_Out_2, _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2);
    float _Property_eb9ff929e1204221bca1c31925f600b7_Out_0 = Vector1_2b42c6accf5149c3929d5731c737ba7c;
    float _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2;
    Unity_Divide_float(10, _Property_eb9ff929e1204221bca1c31925f600b7_Out_0, _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2);
    float2 _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2, 1, _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4);
    float _Split_2e7cca56ed8b4f69890662df97d724ba_R_1 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[0];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_G_2 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[1];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_B_3 = 0;
    float _Split_2e7cca56ed8b4f69890662df97d724ba_A_4 = 0;
    float _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1;
    Unity_OneMinus_float(_Split_2e7cca56ed8b4f69890662df97d724ba_R_1, _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1);
    float _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1;
    Unity_Saturate_float(_OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1, _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1);
    float _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0 = Vector1_eb4dbe959ea64ae896f61f72a5d275d0;
    float _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2;
    Unity_Divide_float(10, _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0, _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2);
    float2 _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2, 1, _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4);
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[0];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_G_2 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[1];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_B_3 = 0;
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_A_4 = 0;
    float _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1;
    Unity_OneMinus_float(_Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1, _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1);
    float _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1;
    Unity_Saturate_float(_OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1, _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1);
    float2 _Property_8c07974a4edd4dc89101134d98954a00_Out_0 = Vector2_e01f7c264af944fdb8bcea2d35ae3001;
    float _Float_93afd7af653a45a38377067c2d80ab35_Out_0 = 1;
    float _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0 = Vector1_23353b8652e043faab2f58b3964e3f17;
    float _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2;
    Unity_Divide_float(_Float_93afd7af653a45a38377067c2d80ab35_Out_0, _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0, _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2);
    float _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2;
    Unity_Multiply_float(_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2, -1, _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2);
    float _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2;
    Unity_Divide_float(_Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2, 2, _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2);
    float _Add_bc17962f1e2d49fca18bb00e85478880_Out_2;
    Unity_Add_float(_Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2, 0.5, _Add_bc17962f1e2d49fca18bb00e85478880_Out_2);
    float2 _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3;
    Unity_TilingAndOffset_float(_Property_8c07974a4edd4dc89101134d98954a00_Out_0, (_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2.xx), (_Add_bc17962f1e2d49fca18bb00e85478880_Out_2.xx), _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3);
    float2 _Property_deac93b8878546c8a300d7352a631a26_Out_0 = Vector2_575137a8a58748d1a0e062a00216bbe5;
    float2 _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3, _Property_deac93b8878546c8a300d7352a631a26_Out_0, float2 (0, 0), _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3);
    float2 _Fraction_a4f4615406494c08b0082401f60051c2_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3, _Fraction_a4f4615406494c08b0082401f60051c2_Out_1);
    float _Property_0081be27ffb041f4ac12f66f0dc62624_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float2 _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Property_0081be27ffb041f4ac12f66f0dc62624_Out_0.xx), float2 (0, 0), _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3);
    float2 _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3, _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1);
    float _Property_d89b62d779e54332979e2425f6cd1857_Out_0 = Vector1_f8f78e1de998447c949d6ce599a31355;
    float _Add_1feb1662c2884dffad05bb44310c6586_Out_2;
    Unity_Add_float(_Property_d89b62d779e54332979e2425f6cd1857_Out_0, 1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2);
    float _Divide_43532697f4454e47a4092b408ec0ff25_Out_2;
    Unity_Divide_float(1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2);
    float _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3;
    Unity_Rectangle_float(_Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3);
    float _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1;
    Unity_OneMinus_float(_Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3, _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1);
    float _Property_43f57c55e694444e9bf9aeb01760d823_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_853fa90d1d314da390af1a6f21f72298_Out_2;
    Unity_Divide_float(_Property_43f57c55e694444e9bf9aeb01760d823_Out_0, _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0, _Divide_853fa90d1d314da390af1a6f21f72298_Out_2);
    float2 _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Divide_853fa90d1d314da390af1a6f21f72298_Out_2.xx), float2 (0, 0), _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3);
    float2 _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3, _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1);
    float _Property_5236eaaed502435380fd9232ca3f5a7b_Out_0 = Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d;
    float _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_479d5e5f38be495d982aed56501420aa_Out_2;
    Unity_Divide_float(_Property_5236eaaed502435380fd9232ca3f5a7b_Out_0, _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0, _Divide_479d5e5f38be495d982aed56501420aa_Out_2);
    float _Add_eae9a749d00c4960aea43a9320fead3e_Out_2;
    Unity_Add_float(_Divide_479d5e5f38be495d982aed56501420aa_Out_2, 1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2);
    float _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2;
    Unity_Divide_float(1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2);
    float _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3;
    Unity_Rectangle_float(_Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3);
    float _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1;
    Unity_OneMinus_float(_Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1);
    float _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2;
    Unity_Add_float(_OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1, _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2);
    float _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1;
    Unity_Saturate_float(_Add_2b900f3ab1b44858bf5696180eac62e6_Out_2, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1);
    float _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2;
    Unity_Multiply_float(_Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2);
    float4 _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0 = Vector4_5fb32e510cd648f8b219982d0bc6426a;
    float _Split_d97e936de3fa453e9725f0c2256e5eac_R_1 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[0];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_G_2 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[1];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_B_3 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[2];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_A_4 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[3];
    float _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2;
    Unity_Multiply_float(_Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Split_d97e936de3fa453e9725f0c2256e5eac_A_4, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2);
    float _Add_9af69bb68e3044b29a3495457a20582c_Out_2;
    Unity_Add_float(_Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2, _Add_9af69bb68e3044b29a3495457a20582c_Out_2);
    float _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
    Unity_Multiply_float(_Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1, _Add_9af69bb68e3044b29a3495457a20582c_Out_2, _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2);
    GrassGrid_1 = _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
}

void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
{
    SHADERGRAPH_FOG(Position, Color, Density);
}

// 98116c2c658709e5fcb200b1ae28460e
#include "Assets/Rendering/InfiniteFloor/InfiniteFloorMerger.hlsl"

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_e943bfd340cf4709a76ba852685dbf55_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_c294f42edfdb40c18d1605395ee9f835_Out_0 = _PlayerPosition;
    float _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0 = _Zoom;
    Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.WorldSpacePosition = IN.WorldSpacePosition;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.uv0 = IN.uv0;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3;
    float _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2;
    SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(_Property_c294f42edfdb40c18d1605395ee9f835_Out_0, _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2);
    float2 _Property_edde0349814e405a8f77a67715a72a11_Out_0 = _SizeOfTexture;
    float _Property_c4b62049d152467eb90794c337831029_Out_0 = _GridThickness;
    float _Property_038d46b4121440948e0171cd5c26d417_Out_0 = _ThicknessOffset;
    float _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0 = _GridOffset;
    float4 _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18;
    Main_float(_Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0, _Property_e943bfd340cf4709a76ba852685dbf55_Out_0, 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_edde0349814e405a8f77a67715a72a11_Out_0, _Property_c4b62049d152467eb90794c337831029_Out_0, _Property_038d46b4121440948e0171cd5c26d417_Out_0, _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0, float2 (0, 0), 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18);
    float2 _Property_81c877f57fbd4119951d21e6a05f9536_Out_0 = _SizeOfTexture;
    UnityTexture2D _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0 = _OwnedVariationRange;
    float2 _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0 = _UnownedVariationRange;
    Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2;
    SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_81c877f57fbd4119951d21e6a05f9536_Out_0, _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0, _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0, _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2);
    UnityTexture2D _Property_03a2f987be36438cb0f8886babf33c96_Out_0 = UnityBuildTexture2DStructNoScale(_GrassTexture);
    float _Property_3ea8b576653841a7a5a603ef1f120469_Out_0 = _GrassScale;
    UnityTexture2D _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0 = UnityBuildTexture2DStructNoScale(_RoadTexture);
    float _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0 = _RoadScale;
    float _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0 = _Zoom;
    float2 _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0 = _SizeOfTexture;
    float _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0 = _RoadFade;
    float2 _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0 = _PlayerPosition;
    Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2;
    SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _Property_03a2f987be36438cb0f8886babf33c96_Out_0, _Property_3ea8b576653841a7a5a603ef1f120469_Out_0, _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0, _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0, _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0, _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0, _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0, _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2);
    float2 _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0 = _SizeOfTexture;
    float _Property_039b41a22968494fb95b7756f155d828_Out_0 = _GrassGridTiling;
    float _Property_98eedb86fc5b4145bc7b65222fede898_Out_0 = _GrassGridThickness;
    float4 _Property_da5db702a8724d9f9a82e143a886ee60_Out_0 = _GrassGridColor;
    float _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0 = _GrassGridIntenseFade;
    float _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0 = _GrassGridFarFade;
    float2 _Property_1ec68bba28374a39850d7800d8f1d362_Out_0 = _PlayerPosition;
    float _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0 = _GrassGridVariationFrequency;
    float _Property_4dcfad1507d94638b52b49967893b527_Out_0 = _GrassGridThicknessVariation;
    float2 _Property_6a4b9239d1bd424885c4b7027264b775_Out_0 = _PlayerPosition;
    float _Property_0d3761446a804744bb350177ec5d239a_Out_0 = _FogFade;
    float _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0 = _FogIntensity;
    Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af;
    float _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1;
    SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6a4b9239d1bd424885c4b7027264b775_Out_0, _Property_0d3761446a804744bb350177ec5d239a_Out_0, _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1);
    Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850;
    float _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1;
    SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_039b41a22968494fb95b7756f155d828_Out_0, _Property_98eedb86fc5b4145bc7b65222fede898_Out_0, _Property_da5db702a8724d9f9a82e143a886ee60_Out_0, _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0, _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0, _Property_1ec68bba28374a39850d7800d8f1d362_Out_0, _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0, _Property_4dcfad1507d94638b52b49967893b527_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1);
    float4 _Property_00719080222e437aa9abf5da2dd48a70_Out_0 = _ColorGrid;
    float4 _Property_118255dd5c8c455ab153d511ba1fc031_Out_0 = _ColorPlazas;
    float4 _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0 = _ColorDistricts;
    float4 _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0 = _ColorStreets;
    float4 _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0 = _ColorParcels;
    float4 _Property_6a1f7cc7467741628211b53b8709021d_Out_0 = _ColorOwnedParcels;
    float4 _Property_9d59173f2b1a48428823e35074ce62c5_Out_0 = _ColorEmpty;
    float4 _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0 = _GrassGridColor;
    float4 _Fog_caf07e8785584760b79500664df1fc44_Color_0;
    float _Fog_caf07e8785584760b79500664df1fc44_Density_1;
    Unity_Fog_float(_Fog_caf07e8785584760b79500664df1fc44_Color_0, _Fog_caf07e8785584760b79500664df1fc44_Density_1, IN.ObjectSpacePosition);
    float4 _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0;
    Merger_float(_MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _Property_00719080222e437aa9abf5da2dd48a70_Out_0, _Property_118255dd5c8c455ab153d511ba1fc031_Out_0, _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0, _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0, _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0, _Property_6a1f7cc7467741628211b53b8709021d_Out_0, _Property_9d59173f2b1a48428823e35074ce62c5_Out_0, _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0, _Fog_caf07e8785584760b79500664df1fc44_Color_0, _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0);
    surface.BaseColor = (_MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0.xyz);
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





    output.WorldSpacePosition = input.positionWS;
    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
    output.uv0 = input.texCoord0;
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
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue" = "Geometry"
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
    Blend One Zero
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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
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
        float4 uv0 : TEXCOORD0;
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
        float4 texCoord0;
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
        float3 ObjectSpacePosition;
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
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float4 interp3 : TEXCOORD3;
        float3 interp4 : TEXCOORD4;
        #if defined(LIGHTMAP_ON)
        float2 interp5 : TEXCOORD5;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 interp6 : TEXCOORD6;
        #endif
        float4 interp7 : TEXCOORD7;
        float4 interp8 : TEXCOORD8;
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
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyzw = input.texCoord0;
        output.interp4.xyz = input.viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        output.interp5.xy = input.lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.interp6.xyz = input.sh;
        #endif
        output.interp7.xyzw = input.fogFactorAndVertexLight;
        output.interp8.xyzw = input.shadowCoord;
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
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.texCoord0 = input.interp3.xyzw;
        output.viewDirectionWS = input.interp4.xyz;
        #if defined(LIGHTMAP_ON)
        output.lightmapUV = input.interp5.xy;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.sh = input.interp6.xyz;
        #endif
        output.fogFactorAndVertexLight = input.interp7.xyzw;
        output.shadowCoord = input.interp8.xyzw;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

struct Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b
{
    float3 WorldSpacePosition;
    half4 uv0;
};

void SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(float2 Vector2_6bff7006be6546f1a2eccc78e58e6232, float Vector1_e86c202cda73418eae7d4a134b98a195, Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b IN, out float2 UV_1, out float2 WorldUV_3, out float Zoom_2)
{
    float4 _UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0 = IN.uv0;
    float _Split_259410824e3c4f498de1eef89dacf280_R_1 = IN.WorldSpacePosition[0];
    float _Split_259410824e3c4f498de1eef89dacf280_G_2 = IN.WorldSpacePosition[1];
    float _Split_259410824e3c4f498de1eef89dacf280_B_3 = IN.WorldSpacePosition[2];
    float _Split_259410824e3c4f498de1eef89dacf280_A_4 = 0;
    float2 _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0 = float2(_Split_259410824e3c4f498de1eef89dacf280_R_1, _Split_259410824e3c4f498de1eef89dacf280_B_3);
    float _Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0 = Vector1_e86c202cda73418eae7d4a134b98a195;
    float _Split_70c1c438ae374dec9f011f8d2999c80e_R_1 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[0];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_G_2 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[1];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_B_3 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[2];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_A_4 = 0;
    float _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
    Unity_Divide_float(_Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0, _Split_70c1c438ae374dec9f011f8d2999c80e_R_1, _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2);
    UV_1 = (_UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0.xy);
    WorldUV_3 = _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0;
    Zoom_2 = _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
}

// 7b6d5a90df0cb86d20ecea9cb96d928e
#include "Assets/Rendering/InfiniteFloor/MapV5.hlsl"

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Comparison_NotEqual_float(float A, float B, out float Out)
{
    Out = A != B ? 1 : 0;
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Floor_float2(float2 In, out float2 Out)
{
    Out = floor(In);
}


inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}


inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}


inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = Unity_SimpleNoise_RandomValue_float(c0);
    float r1 = Unity_SimpleNoise_RandomValue_float(c1);
    float r2 = Unity_SimpleNoise_RandomValue_float(c2);
    float r3 = Unity_SimpleNoise_RandomValue_float(c3);

    float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
    float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
    float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
    return t;
}

void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3 - 0));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3 - 1));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3 - 2));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    Out = t;
}

void Unity_Branch_float(float Predicate, float True, float False, out float Out)
{
    Out = Predicate ? True : False;
}

struct Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841
{
};

void SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(float2 Vector2_cd614e04b07b47eb95a2e5e3ffa41872, float Vector1_355229664685490fa0fc11fe1a97899f, float2 Vector2_1721f8718e464df9a2a9bd7239c50524, UnityTexture2D Texture2D_405855aa0a514baaa11320593c2f07c1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03, Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 IN, out float Mixed_1, out float Owned_2)
{
    UnityTexture2D _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0 = Texture2D_405855aa0a514baaa11320593c2f07c1;
    float2 _Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0 = Vector2_cd614e04b07b47eb95a2e5e3ffa41872;
    float _Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0 = 1;
    float _Property_06d14f9324724405bca2e16df40bef40_Out_0 = Vector1_355229664685490fa0fc11fe1a97899f;
    float _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2;
    Unity_Divide_float(_Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0, _Property_06d14f9324724405bca2e16df40bef40_Out_0, _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2);
    float _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2;
    Unity_Multiply_float(_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2, -1, _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2);
    float _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2;
    Unity_Divide_float(_Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2, 2, _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2);
    float _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2;
    Unity_Add_float(_Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2, 0.5, _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2);
    float2 _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3;
    Unity_TilingAndOffset_float(_Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0, (_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2.xx), (_Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2.xx), _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float4 _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.tex, _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.samplerstate, _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_R_4 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.r;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.g;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.b;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_A_7 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.a;
    float _Add_c7db3142e97045b2877ef4033d663af0_Out_2;
    Unity_Add_float(_SampleTexture2D_108c05205d884e9298d8d59122709828_R_4, _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5, _Add_c7db3142e97045b2877ef4033d663af0_Out_2);
    float _Add_da555a32eb354a279399036fba5f852b_Out_2;
    Unity_Add_float(_Add_c7db3142e97045b2877ef4033d663af0_Out_2, _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6, _Add_da555a32eb354a279399036fba5f852b_Out_2);
    float _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2);
    float2 _Property_49ef008f07834aba983ae50300e94c82_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1;
    float _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3;
    Unity_Remap_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, float2 (0, 1), _Property_49ef008f07834aba983ae50300e94c82_Out_0, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3);
    float2 _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0 = Vector2_1721f8718e464df9a2a9bd7239c50524;
    float2 _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3, _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0, float2 (0, 0), _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3);
    float2 _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1;
    Unity_Floor_float2(_TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3, _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1);
    float _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2;
    Unity_SimpleNoise_float(_Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1, 150, _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2);
    float2 _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03;
    float _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3;
    Unity_Remap_float(_SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2, float2 (0, 1), _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3);
    float _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Unity_Branch_float(_Comparison_9f9bfac15975447a9c81341908b981a0_Out_2, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3, _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3);
    float _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2);
    float _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
    Unity_Branch_float(_Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2, 1, 0, _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3);
    Mixed_1 = _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Owned_2 = _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

struct Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e
{
};

void SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(float2 Vector2_a43b69bd63e74b8899664790c597f8c6, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c, float Vector1_8be634a8378a4521b7522d631008fc39, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c_1, float Vector1_1, float Vector1_78a75bc300dd47ee83f8fbd9e84a0cad, float2 Vector2_f975587bc79d4eadbd0807b55a090f9d, float Vector1_e4cafe8cd46043f0ae5392b59d6b03fe, float2 Vector2_7389d8e6e0014c32be011a6864268e6a, float2 Vector2_cf1b396af9b54596bc8052bf3fe215fb, Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e IN, out float Grass_1, out float Road_2)
{
    UnityTexture2D _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c;
    float2 _Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0 = Vector1_8be634a8378a4521b7522d631008fc39;
    float _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2;
    Unity_Divide_float(1, _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0, _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2);
    float _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2;
    Unity_Multiply_float(_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2, -1, _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2);
    float _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2;
    Unity_Divide_float(_Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2, 2, _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2);
    float _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2;
    Unity_Add_float(_Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2, 0.5, _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2);
    float2 _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3;
    Unity_TilingAndOffset_float(_Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0, (_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2.xx), (_Add_e636f222615a4d5b80b5dc3743ef5097_Out_2.xx), _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float4 _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.tex, _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.samplerstate, _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.r;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_G_5 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.g;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_B_6 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.b;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_A_7 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.a;
    UnityTexture2D _Property_acfdee93b05546369a691885f7d8fc49_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c_1;
    float2 _Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float2 _Property_622a1d2b345749f7af37dbeb28c9856a_Out_0 = Vector2_f975587bc79d4eadbd0807b55a090f9d;
    float _Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0 = 1;
    float _Property_9655382379e942478a21d79fef207bdd_Out_0 = Vector1_78a75bc300dd47ee83f8fbd9e84a0cad;
    float _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2;
    Unity_Divide_float(_Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0, _Property_9655382379e942478a21d79fef207bdd_Out_0, _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2);
    float2 _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2;
    Unity_Multiply_float(_Property_622a1d2b345749f7af37dbeb28c9856a_Out_0, (_Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2.xx), _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2);
    float _Split_92aaba0363214ad786eeca836e64e191_R_1 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[0];
    float _Split_92aaba0363214ad786eeca836e64e191_G_2 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[1];
    float _Split_92aaba0363214ad786eeca836e64e191_B_3 = 0;
    float _Split_92aaba0363214ad786eeca836e64e191_A_4 = 0;
    float _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2;
    Unity_Multiply_float(_Split_92aaba0363214ad786eeca836e64e191_R_1, -1, _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2);
    float _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2;
    Unity_Divide_float(_Multiply_426451ec50dc4f3b80866858356f7c82_Out_2, 2, _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2);
    float _Add_1e90aeca170749febce3402fc85db207_Out_2;
    Unity_Add_float(_Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2, 0.5, _Add_1e90aeca170749febce3402fc85db207_Out_2);
    float2 _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3;
    Unity_TilingAndOffset_float(_Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0, (_Split_92aaba0363214ad786eeca836e64e191_R_1.xx), (_Add_1e90aeca170749febce3402fc85db207_Out_2.xx), _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3);
    float2 _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3, _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1);
    float _Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0 = Vector1_1;
    float _Multiply_3e0c255ae161401f94116d1116002307_Out_2;
    Unity_Multiply_float(_Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0, 10000, _Multiply_3e0c255ae161401f94116d1116002307_Out_2);
    float2 _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3;
    Unity_TilingAndOffset_float(_Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1, (_Multiply_3e0c255ae161401f94116d1116002307_Out_2.xx), float2 (0, 0), _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float4 _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0 = SAMPLE_TEXTURE2D(_Property_acfdee93b05546369a691885f7d8fc49_Out_0.tex, _Property_acfdee93b05546369a691885f7d8fc49_Out_0.samplerstate, _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.r;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_G_5 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.g;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_B_6 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.b;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_A_7 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.a;
    float2 _Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0 = Vector2_cf1b396af9b54596bc8052bf3fe215fb;
    float2 _Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0 = Vector2_7389d8e6e0014c32be011a6864268e6a;
    float2 _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2;
    Unity_Multiply_float(_Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0, float2(-1, -1), _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2);
    float2 _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2;
    Unity_Add_float2(_Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0, _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2, _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2);
    float _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0 = Vector1_e4cafe8cd46043f0ae5392b59d6b03fe;
    float _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2;
    Unity_Divide_float(10, _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0, _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2);
    float2 _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4;
    Unity_PolarCoordinates_float(_Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2, float2 (0.5, 0.5), _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2, 1, _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4);
    float _Split_f3387914733a4792b78907a73c898380_R_1 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[0];
    float _Split_f3387914733a4792b78907a73c898380_G_2 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[1];
    float _Split_f3387914733a4792b78907a73c898380_B_3 = 0;
    float _Split_f3387914733a4792b78907a73c898380_A_4 = 0;
    float _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1;
    Unity_Saturate_float(_Split_f3387914733a4792b78907a73c898380_R_1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1);
    float _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
    Unity_Lerp_float(_SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4, 1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1, _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3);
    Grass_1 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4;
    Road_2 = _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
}

struct Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a
{
};

void SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(float2 Vector2_311ffee78d314f71a9463e39924ea623, float2 Vector2_a57b68e1b4044834933fd8337f0a0577, float Vector1_7284deecf5d9431d92fc35a123337ff4, float Vector1_ab2c4cd721534cf4a387156d51a1fed9, Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a IN, out float Out_1)
{
    float2 _Property_70523c283f40499f89e4f7748deff77e_Out_0 = Vector2_311ffee78d314f71a9463e39924ea623;
    float2 _Property_f28b80022c3246688280e0762030829b_Out_0 = Vector2_a57b68e1b4044834933fd8337f0a0577;
    float2 _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2;
    Unity_Multiply_float(_Property_f28b80022c3246688280e0762030829b_Out_0, float2(-1, -1), _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2);
    float2 _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2;
    Unity_Add_float2(_Property_70523c283f40499f89e4f7748deff77e_Out_0, _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2, _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2);
    float _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0 = Vector1_7284deecf5d9431d92fc35a123337ff4;
    float _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2;
    Unity_Divide_float(1, _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0, _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2);
    float2 _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4;
    Unity_PolarCoordinates_float(_Add_e90ad347cd4b42c3963540725f4e79d9_Out_2, float2 (0.5, 0.5), _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2, 1, _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4);
    float _Split_904e58337bbe428998ef573899b98f55_R_1 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[0];
    float _Split_904e58337bbe428998ef573899b98f55_G_2 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[1];
    float _Split_904e58337bbe428998ef573899b98f55_B_3 = 0;
    float _Split_904e58337bbe428998ef573899b98f55_A_4 = 0;
    float _Property_f8541835e99e409989806d7eff9d13e8_Out_0 = Vector1_ab2c4cd721534cf4a387156d51a1fed9;
    float _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2;
    Unity_Multiply_float(_Split_904e58337bbe428998ef573899b98f55_R_1, _Property_f8541835e99e409989806d7eff9d13e8_Out_0, _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2);
    float _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
    Unity_Saturate_float(_Multiply_a77283d5783542b596ccaa11bb712b63_Out_2, _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1);
    Out_1 = _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Rectangle_float(float2 UV, float Width, float Height, out float Out)
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

struct Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c
{
};

void SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(float2 Vector2_e01f7c264af944fdb8bcea2d35ae3001, float2 Vector2_622ed4642c4445d194b78ad6759b208d, float2 Vector2_575137a8a58748d1a0e062a00216bbe5, float Vector1_23353b8652e043faab2f58b3964e3f17, float Vector1_5c017085898d45e48611a2e9ace96469, float Vector1_f8f78e1de998447c949d6ce599a31355, float4 Vector4_5fb32e510cd648f8b219982d0bc6426a, float Vector1_eb4dbe959ea64ae896f61f72a5d275d0, float Vector1_2b42c6accf5149c3929d5731c737ba7c, float2 Vector2_2c998556cbda461d8a0b69199046f9f5, float Vector1_f62ba0f4717b42c1b7e03ce424479587, float Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d, float Vector1_63cdfc1b7ebd4084b00ceb9b109e3919, Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c IN, out float GrassGrid_1)
{
    float2 _Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0 = Vector2_622ed4642c4445d194b78ad6759b208d;
    float2 _Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0 = Vector2_2c998556cbda461d8a0b69199046f9f5;
    float2 _Multiply_d348b89a76874839babedba1f8d3296d_Out_2;
    Unity_Multiply_float(_Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0, float2(-1, -1), _Multiply_d348b89a76874839babedba1f8d3296d_Out_2);
    float2 _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2;
    Unity_Add_float2(_Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0, _Multiply_d348b89a76874839babedba1f8d3296d_Out_2, _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2);
    float _Property_eb9ff929e1204221bca1c31925f600b7_Out_0 = Vector1_2b42c6accf5149c3929d5731c737ba7c;
    float _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2;
    Unity_Divide_float(10, _Property_eb9ff929e1204221bca1c31925f600b7_Out_0, _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2);
    float2 _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2, 1, _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4);
    float _Split_2e7cca56ed8b4f69890662df97d724ba_R_1 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[0];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_G_2 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[1];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_B_3 = 0;
    float _Split_2e7cca56ed8b4f69890662df97d724ba_A_4 = 0;
    float _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1;
    Unity_OneMinus_float(_Split_2e7cca56ed8b4f69890662df97d724ba_R_1, _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1);
    float _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1;
    Unity_Saturate_float(_OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1, _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1);
    float _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0 = Vector1_eb4dbe959ea64ae896f61f72a5d275d0;
    float _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2;
    Unity_Divide_float(10, _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0, _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2);
    float2 _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2, 1, _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4);
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[0];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_G_2 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[1];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_B_3 = 0;
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_A_4 = 0;
    float _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1;
    Unity_OneMinus_float(_Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1, _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1);
    float _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1;
    Unity_Saturate_float(_OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1, _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1);
    float2 _Property_8c07974a4edd4dc89101134d98954a00_Out_0 = Vector2_e01f7c264af944fdb8bcea2d35ae3001;
    float _Float_93afd7af653a45a38377067c2d80ab35_Out_0 = 1;
    float _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0 = Vector1_23353b8652e043faab2f58b3964e3f17;
    float _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2;
    Unity_Divide_float(_Float_93afd7af653a45a38377067c2d80ab35_Out_0, _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0, _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2);
    float _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2;
    Unity_Multiply_float(_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2, -1, _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2);
    float _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2;
    Unity_Divide_float(_Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2, 2, _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2);
    float _Add_bc17962f1e2d49fca18bb00e85478880_Out_2;
    Unity_Add_float(_Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2, 0.5, _Add_bc17962f1e2d49fca18bb00e85478880_Out_2);
    float2 _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3;
    Unity_TilingAndOffset_float(_Property_8c07974a4edd4dc89101134d98954a00_Out_0, (_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2.xx), (_Add_bc17962f1e2d49fca18bb00e85478880_Out_2.xx), _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3);
    float2 _Property_deac93b8878546c8a300d7352a631a26_Out_0 = Vector2_575137a8a58748d1a0e062a00216bbe5;
    float2 _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3, _Property_deac93b8878546c8a300d7352a631a26_Out_0, float2 (0, 0), _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3);
    float2 _Fraction_a4f4615406494c08b0082401f60051c2_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3, _Fraction_a4f4615406494c08b0082401f60051c2_Out_1);
    float _Property_0081be27ffb041f4ac12f66f0dc62624_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float2 _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Property_0081be27ffb041f4ac12f66f0dc62624_Out_0.xx), float2 (0, 0), _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3);
    float2 _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3, _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1);
    float _Property_d89b62d779e54332979e2425f6cd1857_Out_0 = Vector1_f8f78e1de998447c949d6ce599a31355;
    float _Add_1feb1662c2884dffad05bb44310c6586_Out_2;
    Unity_Add_float(_Property_d89b62d779e54332979e2425f6cd1857_Out_0, 1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2);
    float _Divide_43532697f4454e47a4092b408ec0ff25_Out_2;
    Unity_Divide_float(1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2);
    float _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3;
    Unity_Rectangle_float(_Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3);
    float _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1;
    Unity_OneMinus_float(_Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3, _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1);
    float _Property_43f57c55e694444e9bf9aeb01760d823_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_853fa90d1d314da390af1a6f21f72298_Out_2;
    Unity_Divide_float(_Property_43f57c55e694444e9bf9aeb01760d823_Out_0, _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0, _Divide_853fa90d1d314da390af1a6f21f72298_Out_2);
    float2 _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Divide_853fa90d1d314da390af1a6f21f72298_Out_2.xx), float2 (0, 0), _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3);
    float2 _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3, _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1);
    float _Property_5236eaaed502435380fd9232ca3f5a7b_Out_0 = Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d;
    float _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_479d5e5f38be495d982aed56501420aa_Out_2;
    Unity_Divide_float(_Property_5236eaaed502435380fd9232ca3f5a7b_Out_0, _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0, _Divide_479d5e5f38be495d982aed56501420aa_Out_2);
    float _Add_eae9a749d00c4960aea43a9320fead3e_Out_2;
    Unity_Add_float(_Divide_479d5e5f38be495d982aed56501420aa_Out_2, 1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2);
    float _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2;
    Unity_Divide_float(1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2);
    float _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3;
    Unity_Rectangle_float(_Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3);
    float _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1;
    Unity_OneMinus_float(_Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1);
    float _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2;
    Unity_Add_float(_OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1, _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2);
    float _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1;
    Unity_Saturate_float(_Add_2b900f3ab1b44858bf5696180eac62e6_Out_2, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1);
    float _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2;
    Unity_Multiply_float(_Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2);
    float4 _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0 = Vector4_5fb32e510cd648f8b219982d0bc6426a;
    float _Split_d97e936de3fa453e9725f0c2256e5eac_R_1 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[0];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_G_2 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[1];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_B_3 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[2];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_A_4 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[3];
    float _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2;
    Unity_Multiply_float(_Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Split_d97e936de3fa453e9725f0c2256e5eac_A_4, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2);
    float _Add_9af69bb68e3044b29a3495457a20582c_Out_2;
    Unity_Add_float(_Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2, _Add_9af69bb68e3044b29a3495457a20582c_Out_2);
    float _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
    Unity_Multiply_float(_Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1, _Add_9af69bb68e3044b29a3495457a20582c_Out_2, _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2);
    GrassGrid_1 = _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
}

void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
{
    SHADERGRAPH_FOG(Position, Color, Density);
}

// 98116c2c658709e5fcb200b1ae28460e
#include "Assets/Rendering/InfiniteFloor/InfiniteFloorMerger.hlsl"

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_e943bfd340cf4709a76ba852685dbf55_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_c294f42edfdb40c18d1605395ee9f835_Out_0 = _PlayerPosition;
    float _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0 = _Zoom;
    Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.WorldSpacePosition = IN.WorldSpacePosition;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.uv0 = IN.uv0;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3;
    float _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2;
    SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(_Property_c294f42edfdb40c18d1605395ee9f835_Out_0, _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2);
    float2 _Property_edde0349814e405a8f77a67715a72a11_Out_0 = _SizeOfTexture;
    float _Property_c4b62049d152467eb90794c337831029_Out_0 = _GridThickness;
    float _Property_038d46b4121440948e0171cd5c26d417_Out_0 = _ThicknessOffset;
    float _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0 = _GridOffset;
    float4 _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18;
    Main_float(_Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0, _Property_e943bfd340cf4709a76ba852685dbf55_Out_0, 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_edde0349814e405a8f77a67715a72a11_Out_0, _Property_c4b62049d152467eb90794c337831029_Out_0, _Property_038d46b4121440948e0171cd5c26d417_Out_0, _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0, float2 (0, 0), 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18);
    float2 _Property_81c877f57fbd4119951d21e6a05f9536_Out_0 = _SizeOfTexture;
    UnityTexture2D _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0 = _OwnedVariationRange;
    float2 _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0 = _UnownedVariationRange;
    Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2;
    SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_81c877f57fbd4119951d21e6a05f9536_Out_0, _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0, _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0, _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2);
    UnityTexture2D _Property_03a2f987be36438cb0f8886babf33c96_Out_0 = UnityBuildTexture2DStructNoScale(_GrassTexture);
    float _Property_3ea8b576653841a7a5a603ef1f120469_Out_0 = _GrassScale;
    UnityTexture2D _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0 = UnityBuildTexture2DStructNoScale(_RoadTexture);
    float _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0 = _RoadScale;
    float _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0 = _Zoom;
    float2 _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0 = _SizeOfTexture;
    float _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0 = _RoadFade;
    float2 _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0 = _PlayerPosition;
    Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2;
    SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _Property_03a2f987be36438cb0f8886babf33c96_Out_0, _Property_3ea8b576653841a7a5a603ef1f120469_Out_0, _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0, _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0, _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0, _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0, _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0, _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2);
    float2 _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0 = _SizeOfTexture;
    float _Property_039b41a22968494fb95b7756f155d828_Out_0 = _GrassGridTiling;
    float _Property_98eedb86fc5b4145bc7b65222fede898_Out_0 = _GrassGridThickness;
    float4 _Property_da5db702a8724d9f9a82e143a886ee60_Out_0 = _GrassGridColor;
    float _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0 = _GrassGridIntenseFade;
    float _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0 = _GrassGridFarFade;
    float2 _Property_1ec68bba28374a39850d7800d8f1d362_Out_0 = _PlayerPosition;
    float _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0 = _GrassGridVariationFrequency;
    float _Property_4dcfad1507d94638b52b49967893b527_Out_0 = _GrassGridThicknessVariation;
    float2 _Property_6a4b9239d1bd424885c4b7027264b775_Out_0 = _PlayerPosition;
    float _Property_0d3761446a804744bb350177ec5d239a_Out_0 = _FogFade;
    float _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0 = _FogIntensity;
    Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af;
    float _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1;
    SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6a4b9239d1bd424885c4b7027264b775_Out_0, _Property_0d3761446a804744bb350177ec5d239a_Out_0, _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1);
    Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850;
    float _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1;
    SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_039b41a22968494fb95b7756f155d828_Out_0, _Property_98eedb86fc5b4145bc7b65222fede898_Out_0, _Property_da5db702a8724d9f9a82e143a886ee60_Out_0, _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0, _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0, _Property_1ec68bba28374a39850d7800d8f1d362_Out_0, _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0, _Property_4dcfad1507d94638b52b49967893b527_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1);
    float4 _Property_00719080222e437aa9abf5da2dd48a70_Out_0 = _ColorGrid;
    float4 _Property_118255dd5c8c455ab153d511ba1fc031_Out_0 = _ColorPlazas;
    float4 _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0 = _ColorDistricts;
    float4 _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0 = _ColorStreets;
    float4 _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0 = _ColorParcels;
    float4 _Property_6a1f7cc7467741628211b53b8709021d_Out_0 = _ColorOwnedParcels;
    float4 _Property_9d59173f2b1a48428823e35074ce62c5_Out_0 = _ColorEmpty;
    float4 _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0 = _GrassGridColor;
    float4 _Fog_caf07e8785584760b79500664df1fc44_Color_0;
    float _Fog_caf07e8785584760b79500664df1fc44_Density_1;
    Unity_Fog_float(_Fog_caf07e8785584760b79500664df1fc44_Color_0, _Fog_caf07e8785584760b79500664df1fc44_Density_1, IN.ObjectSpacePosition);
    float4 _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0;
    Merger_float(_MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _Property_00719080222e437aa9abf5da2dd48a70_Out_0, _Property_118255dd5c8c455ab153d511ba1fc031_Out_0, _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0, _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0, _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0, _Property_6a1f7cc7467741628211b53b8709021d_Out_0, _Property_9d59173f2b1a48428823e35074ce62c5_Out_0, _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0, _Fog_caf07e8785584760b79500664df1fc44_Color_0, _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0);
    float _Property_b872305fbdab44078bf3b70b9f9de114_Out_0 = _Metallic;
    float _Property_de71bcc7d06c4da3bea536b766bc403a_Out_0 = _Smoothness;
    surface.BaseColor = (_MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0.xyz);
    surface.NormalTS = IN.TangentSpaceNormal;
    surface.Emission = float3(0, 0, 0);
    surface.Metallic = _Property_b872305fbdab44078bf3b70b9f9de114_Out_0;
    surface.Smoothness = _Property_de71bcc7d06c4da3bea536b766bc403a_Out_0;
    surface.Occlusion = 1;
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



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
    output.uv0 = input.texCoord0;
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
    Blend One Zero
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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
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
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions
// GraphFunctions: <None>

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
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
    Blend One Zero
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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
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
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions
// GraphFunctions: <None>

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
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
    Blend One Zero
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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
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

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.normalWS;
        output.interp1.xyzw = input.tangentWS;
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
        output.normalWS = input.interp0.xyz;
        output.tangentWS = input.interp1.xyzw;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions
// GraphFunctions: <None>

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    surface.NormalTS = IN.TangentSpaceNormal;
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



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
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
        float4 uv0 : TEXCOORD0;
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
        float3 ObjectSpacePosition;
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

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.texCoord0;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

struct Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b
{
    float3 WorldSpacePosition;
    half4 uv0;
};

void SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(float2 Vector2_6bff7006be6546f1a2eccc78e58e6232, float Vector1_e86c202cda73418eae7d4a134b98a195, Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b IN, out float2 UV_1, out float2 WorldUV_3, out float Zoom_2)
{
    float4 _UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0 = IN.uv0;
    float _Split_259410824e3c4f498de1eef89dacf280_R_1 = IN.WorldSpacePosition[0];
    float _Split_259410824e3c4f498de1eef89dacf280_G_2 = IN.WorldSpacePosition[1];
    float _Split_259410824e3c4f498de1eef89dacf280_B_3 = IN.WorldSpacePosition[2];
    float _Split_259410824e3c4f498de1eef89dacf280_A_4 = 0;
    float2 _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0 = float2(_Split_259410824e3c4f498de1eef89dacf280_R_1, _Split_259410824e3c4f498de1eef89dacf280_B_3);
    float _Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0 = Vector1_e86c202cda73418eae7d4a134b98a195;
    float _Split_70c1c438ae374dec9f011f8d2999c80e_R_1 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[0];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_G_2 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[1];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_B_3 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[2];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_A_4 = 0;
    float _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
    Unity_Divide_float(_Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0, _Split_70c1c438ae374dec9f011f8d2999c80e_R_1, _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2);
    UV_1 = (_UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0.xy);
    WorldUV_3 = _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0;
    Zoom_2 = _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
}

// 7b6d5a90df0cb86d20ecea9cb96d928e
#include "Assets/Rendering/InfiniteFloor/MapV5.hlsl"

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Comparison_NotEqual_float(float A, float B, out float Out)
{
    Out = A != B ? 1 : 0;
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Floor_float2(float2 In, out float2 Out)
{
    Out = floor(In);
}


inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}


inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}


inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = Unity_SimpleNoise_RandomValue_float(c0);
    float r1 = Unity_SimpleNoise_RandomValue_float(c1);
    float r2 = Unity_SimpleNoise_RandomValue_float(c2);
    float r3 = Unity_SimpleNoise_RandomValue_float(c3);

    float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
    float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
    float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
    return t;
}

void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3 - 0));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3 - 1));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3 - 2));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    Out = t;
}

void Unity_Branch_float(float Predicate, float True, float False, out float Out)
{
    Out = Predicate ? True : False;
}

struct Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841
{
};

void SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(float2 Vector2_cd614e04b07b47eb95a2e5e3ffa41872, float Vector1_355229664685490fa0fc11fe1a97899f, float2 Vector2_1721f8718e464df9a2a9bd7239c50524, UnityTexture2D Texture2D_405855aa0a514baaa11320593c2f07c1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03, Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 IN, out float Mixed_1, out float Owned_2)
{
    UnityTexture2D _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0 = Texture2D_405855aa0a514baaa11320593c2f07c1;
    float2 _Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0 = Vector2_cd614e04b07b47eb95a2e5e3ffa41872;
    float _Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0 = 1;
    float _Property_06d14f9324724405bca2e16df40bef40_Out_0 = Vector1_355229664685490fa0fc11fe1a97899f;
    float _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2;
    Unity_Divide_float(_Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0, _Property_06d14f9324724405bca2e16df40bef40_Out_0, _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2);
    float _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2;
    Unity_Multiply_float(_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2, -1, _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2);
    float _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2;
    Unity_Divide_float(_Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2, 2, _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2);
    float _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2;
    Unity_Add_float(_Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2, 0.5, _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2);
    float2 _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3;
    Unity_TilingAndOffset_float(_Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0, (_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2.xx), (_Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2.xx), _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float4 _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.tex, _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.samplerstate, _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_R_4 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.r;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.g;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.b;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_A_7 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.a;
    float _Add_c7db3142e97045b2877ef4033d663af0_Out_2;
    Unity_Add_float(_SampleTexture2D_108c05205d884e9298d8d59122709828_R_4, _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5, _Add_c7db3142e97045b2877ef4033d663af0_Out_2);
    float _Add_da555a32eb354a279399036fba5f852b_Out_2;
    Unity_Add_float(_Add_c7db3142e97045b2877ef4033d663af0_Out_2, _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6, _Add_da555a32eb354a279399036fba5f852b_Out_2);
    float _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2);
    float2 _Property_49ef008f07834aba983ae50300e94c82_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1;
    float _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3;
    Unity_Remap_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, float2 (0, 1), _Property_49ef008f07834aba983ae50300e94c82_Out_0, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3);
    float2 _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0 = Vector2_1721f8718e464df9a2a9bd7239c50524;
    float2 _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3, _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0, float2 (0, 0), _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3);
    float2 _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1;
    Unity_Floor_float2(_TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3, _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1);
    float _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2;
    Unity_SimpleNoise_float(_Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1, 150, _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2);
    float2 _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03;
    float _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3;
    Unity_Remap_float(_SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2, float2 (0, 1), _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3);
    float _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Unity_Branch_float(_Comparison_9f9bfac15975447a9c81341908b981a0_Out_2, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3, _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3);
    float _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2);
    float _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
    Unity_Branch_float(_Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2, 1, 0, _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3);
    Mixed_1 = _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Owned_2 = _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

struct Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e
{
};

void SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(float2 Vector2_a43b69bd63e74b8899664790c597f8c6, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c, float Vector1_8be634a8378a4521b7522d631008fc39, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c_1, float Vector1_1, float Vector1_78a75bc300dd47ee83f8fbd9e84a0cad, float2 Vector2_f975587bc79d4eadbd0807b55a090f9d, float Vector1_e4cafe8cd46043f0ae5392b59d6b03fe, float2 Vector2_7389d8e6e0014c32be011a6864268e6a, float2 Vector2_cf1b396af9b54596bc8052bf3fe215fb, Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e IN, out float Grass_1, out float Road_2)
{
    UnityTexture2D _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c;
    float2 _Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0 = Vector1_8be634a8378a4521b7522d631008fc39;
    float _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2;
    Unity_Divide_float(1, _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0, _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2);
    float _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2;
    Unity_Multiply_float(_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2, -1, _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2);
    float _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2;
    Unity_Divide_float(_Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2, 2, _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2);
    float _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2;
    Unity_Add_float(_Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2, 0.5, _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2);
    float2 _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3;
    Unity_TilingAndOffset_float(_Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0, (_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2.xx), (_Add_e636f222615a4d5b80b5dc3743ef5097_Out_2.xx), _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float4 _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.tex, _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.samplerstate, _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.r;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_G_5 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.g;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_B_6 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.b;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_A_7 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.a;
    UnityTexture2D _Property_acfdee93b05546369a691885f7d8fc49_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c_1;
    float2 _Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float2 _Property_622a1d2b345749f7af37dbeb28c9856a_Out_0 = Vector2_f975587bc79d4eadbd0807b55a090f9d;
    float _Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0 = 1;
    float _Property_9655382379e942478a21d79fef207bdd_Out_0 = Vector1_78a75bc300dd47ee83f8fbd9e84a0cad;
    float _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2;
    Unity_Divide_float(_Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0, _Property_9655382379e942478a21d79fef207bdd_Out_0, _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2);
    float2 _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2;
    Unity_Multiply_float(_Property_622a1d2b345749f7af37dbeb28c9856a_Out_0, (_Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2.xx), _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2);
    float _Split_92aaba0363214ad786eeca836e64e191_R_1 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[0];
    float _Split_92aaba0363214ad786eeca836e64e191_G_2 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[1];
    float _Split_92aaba0363214ad786eeca836e64e191_B_3 = 0;
    float _Split_92aaba0363214ad786eeca836e64e191_A_4 = 0;
    float _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2;
    Unity_Multiply_float(_Split_92aaba0363214ad786eeca836e64e191_R_1, -1, _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2);
    float _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2;
    Unity_Divide_float(_Multiply_426451ec50dc4f3b80866858356f7c82_Out_2, 2, _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2);
    float _Add_1e90aeca170749febce3402fc85db207_Out_2;
    Unity_Add_float(_Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2, 0.5, _Add_1e90aeca170749febce3402fc85db207_Out_2);
    float2 _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3;
    Unity_TilingAndOffset_float(_Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0, (_Split_92aaba0363214ad786eeca836e64e191_R_1.xx), (_Add_1e90aeca170749febce3402fc85db207_Out_2.xx), _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3);
    float2 _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3, _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1);
    float _Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0 = Vector1_1;
    float _Multiply_3e0c255ae161401f94116d1116002307_Out_2;
    Unity_Multiply_float(_Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0, 10000, _Multiply_3e0c255ae161401f94116d1116002307_Out_2);
    float2 _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3;
    Unity_TilingAndOffset_float(_Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1, (_Multiply_3e0c255ae161401f94116d1116002307_Out_2.xx), float2 (0, 0), _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float4 _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0 = SAMPLE_TEXTURE2D(_Property_acfdee93b05546369a691885f7d8fc49_Out_0.tex, _Property_acfdee93b05546369a691885f7d8fc49_Out_0.samplerstate, _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.r;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_G_5 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.g;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_B_6 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.b;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_A_7 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.a;
    float2 _Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0 = Vector2_cf1b396af9b54596bc8052bf3fe215fb;
    float2 _Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0 = Vector2_7389d8e6e0014c32be011a6864268e6a;
    float2 _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2;
    Unity_Multiply_float(_Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0, float2(-1, -1), _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2);
    float2 _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2;
    Unity_Add_float2(_Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0, _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2, _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2);
    float _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0 = Vector1_e4cafe8cd46043f0ae5392b59d6b03fe;
    float _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2;
    Unity_Divide_float(10, _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0, _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2);
    float2 _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4;
    Unity_PolarCoordinates_float(_Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2, float2 (0.5, 0.5), _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2, 1, _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4);
    float _Split_f3387914733a4792b78907a73c898380_R_1 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[0];
    float _Split_f3387914733a4792b78907a73c898380_G_2 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[1];
    float _Split_f3387914733a4792b78907a73c898380_B_3 = 0;
    float _Split_f3387914733a4792b78907a73c898380_A_4 = 0;
    float _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1;
    Unity_Saturate_float(_Split_f3387914733a4792b78907a73c898380_R_1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1);
    float _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
    Unity_Lerp_float(_SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4, 1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1, _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3);
    Grass_1 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4;
    Road_2 = _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
}

struct Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a
{
};

void SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(float2 Vector2_311ffee78d314f71a9463e39924ea623, float2 Vector2_a57b68e1b4044834933fd8337f0a0577, float Vector1_7284deecf5d9431d92fc35a123337ff4, float Vector1_ab2c4cd721534cf4a387156d51a1fed9, Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a IN, out float Out_1)
{
    float2 _Property_70523c283f40499f89e4f7748deff77e_Out_0 = Vector2_311ffee78d314f71a9463e39924ea623;
    float2 _Property_f28b80022c3246688280e0762030829b_Out_0 = Vector2_a57b68e1b4044834933fd8337f0a0577;
    float2 _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2;
    Unity_Multiply_float(_Property_f28b80022c3246688280e0762030829b_Out_0, float2(-1, -1), _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2);
    float2 _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2;
    Unity_Add_float2(_Property_70523c283f40499f89e4f7748deff77e_Out_0, _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2, _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2);
    float _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0 = Vector1_7284deecf5d9431d92fc35a123337ff4;
    float _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2;
    Unity_Divide_float(1, _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0, _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2);
    float2 _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4;
    Unity_PolarCoordinates_float(_Add_e90ad347cd4b42c3963540725f4e79d9_Out_2, float2 (0.5, 0.5), _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2, 1, _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4);
    float _Split_904e58337bbe428998ef573899b98f55_R_1 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[0];
    float _Split_904e58337bbe428998ef573899b98f55_G_2 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[1];
    float _Split_904e58337bbe428998ef573899b98f55_B_3 = 0;
    float _Split_904e58337bbe428998ef573899b98f55_A_4 = 0;
    float _Property_f8541835e99e409989806d7eff9d13e8_Out_0 = Vector1_ab2c4cd721534cf4a387156d51a1fed9;
    float _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2;
    Unity_Multiply_float(_Split_904e58337bbe428998ef573899b98f55_R_1, _Property_f8541835e99e409989806d7eff9d13e8_Out_0, _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2);
    float _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
    Unity_Saturate_float(_Multiply_a77283d5783542b596ccaa11bb712b63_Out_2, _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1);
    Out_1 = _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Rectangle_float(float2 UV, float Width, float Height, out float Out)
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

struct Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c
{
};

void SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(float2 Vector2_e01f7c264af944fdb8bcea2d35ae3001, float2 Vector2_622ed4642c4445d194b78ad6759b208d, float2 Vector2_575137a8a58748d1a0e062a00216bbe5, float Vector1_23353b8652e043faab2f58b3964e3f17, float Vector1_5c017085898d45e48611a2e9ace96469, float Vector1_f8f78e1de998447c949d6ce599a31355, float4 Vector4_5fb32e510cd648f8b219982d0bc6426a, float Vector1_eb4dbe959ea64ae896f61f72a5d275d0, float Vector1_2b42c6accf5149c3929d5731c737ba7c, float2 Vector2_2c998556cbda461d8a0b69199046f9f5, float Vector1_f62ba0f4717b42c1b7e03ce424479587, float Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d, float Vector1_63cdfc1b7ebd4084b00ceb9b109e3919, Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c IN, out float GrassGrid_1)
{
    float2 _Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0 = Vector2_622ed4642c4445d194b78ad6759b208d;
    float2 _Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0 = Vector2_2c998556cbda461d8a0b69199046f9f5;
    float2 _Multiply_d348b89a76874839babedba1f8d3296d_Out_2;
    Unity_Multiply_float(_Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0, float2(-1, -1), _Multiply_d348b89a76874839babedba1f8d3296d_Out_2);
    float2 _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2;
    Unity_Add_float2(_Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0, _Multiply_d348b89a76874839babedba1f8d3296d_Out_2, _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2);
    float _Property_eb9ff929e1204221bca1c31925f600b7_Out_0 = Vector1_2b42c6accf5149c3929d5731c737ba7c;
    float _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2;
    Unity_Divide_float(10, _Property_eb9ff929e1204221bca1c31925f600b7_Out_0, _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2);
    float2 _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2, 1, _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4);
    float _Split_2e7cca56ed8b4f69890662df97d724ba_R_1 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[0];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_G_2 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[1];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_B_3 = 0;
    float _Split_2e7cca56ed8b4f69890662df97d724ba_A_4 = 0;
    float _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1;
    Unity_OneMinus_float(_Split_2e7cca56ed8b4f69890662df97d724ba_R_1, _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1);
    float _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1;
    Unity_Saturate_float(_OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1, _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1);
    float _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0 = Vector1_eb4dbe959ea64ae896f61f72a5d275d0;
    float _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2;
    Unity_Divide_float(10, _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0, _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2);
    float2 _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2, 1, _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4);
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[0];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_G_2 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[1];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_B_3 = 0;
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_A_4 = 0;
    float _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1;
    Unity_OneMinus_float(_Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1, _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1);
    float _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1;
    Unity_Saturate_float(_OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1, _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1);
    float2 _Property_8c07974a4edd4dc89101134d98954a00_Out_0 = Vector2_e01f7c264af944fdb8bcea2d35ae3001;
    float _Float_93afd7af653a45a38377067c2d80ab35_Out_0 = 1;
    float _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0 = Vector1_23353b8652e043faab2f58b3964e3f17;
    float _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2;
    Unity_Divide_float(_Float_93afd7af653a45a38377067c2d80ab35_Out_0, _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0, _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2);
    float _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2;
    Unity_Multiply_float(_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2, -1, _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2);
    float _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2;
    Unity_Divide_float(_Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2, 2, _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2);
    float _Add_bc17962f1e2d49fca18bb00e85478880_Out_2;
    Unity_Add_float(_Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2, 0.5, _Add_bc17962f1e2d49fca18bb00e85478880_Out_2);
    float2 _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3;
    Unity_TilingAndOffset_float(_Property_8c07974a4edd4dc89101134d98954a00_Out_0, (_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2.xx), (_Add_bc17962f1e2d49fca18bb00e85478880_Out_2.xx), _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3);
    float2 _Property_deac93b8878546c8a300d7352a631a26_Out_0 = Vector2_575137a8a58748d1a0e062a00216bbe5;
    float2 _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3, _Property_deac93b8878546c8a300d7352a631a26_Out_0, float2 (0, 0), _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3);
    float2 _Fraction_a4f4615406494c08b0082401f60051c2_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3, _Fraction_a4f4615406494c08b0082401f60051c2_Out_1);
    float _Property_0081be27ffb041f4ac12f66f0dc62624_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float2 _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Property_0081be27ffb041f4ac12f66f0dc62624_Out_0.xx), float2 (0, 0), _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3);
    float2 _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3, _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1);
    float _Property_d89b62d779e54332979e2425f6cd1857_Out_0 = Vector1_f8f78e1de998447c949d6ce599a31355;
    float _Add_1feb1662c2884dffad05bb44310c6586_Out_2;
    Unity_Add_float(_Property_d89b62d779e54332979e2425f6cd1857_Out_0, 1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2);
    float _Divide_43532697f4454e47a4092b408ec0ff25_Out_2;
    Unity_Divide_float(1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2);
    float _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3;
    Unity_Rectangle_float(_Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3);
    float _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1;
    Unity_OneMinus_float(_Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3, _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1);
    float _Property_43f57c55e694444e9bf9aeb01760d823_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_853fa90d1d314da390af1a6f21f72298_Out_2;
    Unity_Divide_float(_Property_43f57c55e694444e9bf9aeb01760d823_Out_0, _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0, _Divide_853fa90d1d314da390af1a6f21f72298_Out_2);
    float2 _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Divide_853fa90d1d314da390af1a6f21f72298_Out_2.xx), float2 (0, 0), _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3);
    float2 _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3, _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1);
    float _Property_5236eaaed502435380fd9232ca3f5a7b_Out_0 = Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d;
    float _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_479d5e5f38be495d982aed56501420aa_Out_2;
    Unity_Divide_float(_Property_5236eaaed502435380fd9232ca3f5a7b_Out_0, _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0, _Divide_479d5e5f38be495d982aed56501420aa_Out_2);
    float _Add_eae9a749d00c4960aea43a9320fead3e_Out_2;
    Unity_Add_float(_Divide_479d5e5f38be495d982aed56501420aa_Out_2, 1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2);
    float _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2;
    Unity_Divide_float(1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2);
    float _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3;
    Unity_Rectangle_float(_Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3);
    float _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1;
    Unity_OneMinus_float(_Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1);
    float _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2;
    Unity_Add_float(_OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1, _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2);
    float _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1;
    Unity_Saturate_float(_Add_2b900f3ab1b44858bf5696180eac62e6_Out_2, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1);
    float _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2;
    Unity_Multiply_float(_Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2);
    float4 _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0 = Vector4_5fb32e510cd648f8b219982d0bc6426a;
    float _Split_d97e936de3fa453e9725f0c2256e5eac_R_1 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[0];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_G_2 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[1];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_B_3 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[2];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_A_4 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[3];
    float _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2;
    Unity_Multiply_float(_Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Split_d97e936de3fa453e9725f0c2256e5eac_A_4, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2);
    float _Add_9af69bb68e3044b29a3495457a20582c_Out_2;
    Unity_Add_float(_Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2, _Add_9af69bb68e3044b29a3495457a20582c_Out_2);
    float _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
    Unity_Multiply_float(_Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1, _Add_9af69bb68e3044b29a3495457a20582c_Out_2, _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2);
    GrassGrid_1 = _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
}

void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
{
    SHADERGRAPH_FOG(Position, Color, Density);
}

// 98116c2c658709e5fcb200b1ae28460e
#include "Assets/Rendering/InfiniteFloor/InfiniteFloorMerger.hlsl"

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_e943bfd340cf4709a76ba852685dbf55_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_c294f42edfdb40c18d1605395ee9f835_Out_0 = _PlayerPosition;
    float _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0 = _Zoom;
    Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.WorldSpacePosition = IN.WorldSpacePosition;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.uv0 = IN.uv0;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3;
    float _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2;
    SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(_Property_c294f42edfdb40c18d1605395ee9f835_Out_0, _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2);
    float2 _Property_edde0349814e405a8f77a67715a72a11_Out_0 = _SizeOfTexture;
    float _Property_c4b62049d152467eb90794c337831029_Out_0 = _GridThickness;
    float _Property_038d46b4121440948e0171cd5c26d417_Out_0 = _ThicknessOffset;
    float _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0 = _GridOffset;
    float4 _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18;
    Main_float(_Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0, _Property_e943bfd340cf4709a76ba852685dbf55_Out_0, 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_edde0349814e405a8f77a67715a72a11_Out_0, _Property_c4b62049d152467eb90794c337831029_Out_0, _Property_038d46b4121440948e0171cd5c26d417_Out_0, _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0, float2 (0, 0), 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18);
    float2 _Property_81c877f57fbd4119951d21e6a05f9536_Out_0 = _SizeOfTexture;
    UnityTexture2D _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0 = _OwnedVariationRange;
    float2 _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0 = _UnownedVariationRange;
    Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2;
    SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_81c877f57fbd4119951d21e6a05f9536_Out_0, _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0, _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0, _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2);
    UnityTexture2D _Property_03a2f987be36438cb0f8886babf33c96_Out_0 = UnityBuildTexture2DStructNoScale(_GrassTexture);
    float _Property_3ea8b576653841a7a5a603ef1f120469_Out_0 = _GrassScale;
    UnityTexture2D _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0 = UnityBuildTexture2DStructNoScale(_RoadTexture);
    float _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0 = _RoadScale;
    float _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0 = _Zoom;
    float2 _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0 = _SizeOfTexture;
    float _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0 = _RoadFade;
    float2 _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0 = _PlayerPosition;
    Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2;
    SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _Property_03a2f987be36438cb0f8886babf33c96_Out_0, _Property_3ea8b576653841a7a5a603ef1f120469_Out_0, _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0, _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0, _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0, _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0, _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0, _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2);
    float2 _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0 = _SizeOfTexture;
    float _Property_039b41a22968494fb95b7756f155d828_Out_0 = _GrassGridTiling;
    float _Property_98eedb86fc5b4145bc7b65222fede898_Out_0 = _GrassGridThickness;
    float4 _Property_da5db702a8724d9f9a82e143a886ee60_Out_0 = _GrassGridColor;
    float _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0 = _GrassGridIntenseFade;
    float _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0 = _GrassGridFarFade;
    float2 _Property_1ec68bba28374a39850d7800d8f1d362_Out_0 = _PlayerPosition;
    float _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0 = _GrassGridVariationFrequency;
    float _Property_4dcfad1507d94638b52b49967893b527_Out_0 = _GrassGridThicknessVariation;
    float2 _Property_6a4b9239d1bd424885c4b7027264b775_Out_0 = _PlayerPosition;
    float _Property_0d3761446a804744bb350177ec5d239a_Out_0 = _FogFade;
    float _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0 = _FogIntensity;
    Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af;
    float _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1;
    SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6a4b9239d1bd424885c4b7027264b775_Out_0, _Property_0d3761446a804744bb350177ec5d239a_Out_0, _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1);
    Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850;
    float _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1;
    SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_039b41a22968494fb95b7756f155d828_Out_0, _Property_98eedb86fc5b4145bc7b65222fede898_Out_0, _Property_da5db702a8724d9f9a82e143a886ee60_Out_0, _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0, _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0, _Property_1ec68bba28374a39850d7800d8f1d362_Out_0, _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0, _Property_4dcfad1507d94638b52b49967893b527_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1);
    float4 _Property_00719080222e437aa9abf5da2dd48a70_Out_0 = _ColorGrid;
    float4 _Property_118255dd5c8c455ab153d511ba1fc031_Out_0 = _ColorPlazas;
    float4 _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0 = _ColorDistricts;
    float4 _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0 = _ColorStreets;
    float4 _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0 = _ColorParcels;
    float4 _Property_6a1f7cc7467741628211b53b8709021d_Out_0 = _ColorOwnedParcels;
    float4 _Property_9d59173f2b1a48428823e35074ce62c5_Out_0 = _ColorEmpty;
    float4 _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0 = _GrassGridColor;
    float4 _Fog_caf07e8785584760b79500664df1fc44_Color_0;
    float _Fog_caf07e8785584760b79500664df1fc44_Density_1;
    Unity_Fog_float(_Fog_caf07e8785584760b79500664df1fc44_Color_0, _Fog_caf07e8785584760b79500664df1fc44_Density_1, IN.ObjectSpacePosition);
    float4 _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0;
    Merger_float(_MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _Property_00719080222e437aa9abf5da2dd48a70_Out_0, _Property_118255dd5c8c455ab153d511ba1fc031_Out_0, _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0, _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0, _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0, _Property_6a1f7cc7467741628211b53b8709021d_Out_0, _Property_9d59173f2b1a48428823e35074ce62c5_Out_0, _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0, _Fog_caf07e8785584760b79500664df1fc44_Color_0, _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0);
    surface.BaseColor = (_MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0.xyz);
    surface.Emission = float3(0, 0, 0);
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





    output.WorldSpacePosition = input.positionWS;
    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
    output.uv0 = input.texCoord0;
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
    Blend One Zero
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
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
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
        float3 ObjectSpacePosition;
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

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyzw = input.texCoord0;
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float _Zoom;
float _GridThickness;
float _GridOffset;
float _ThicknessOffset;
float4 _ColorGrid;
float4 _ColorPlazas;
float4 _ColorDistricts;
float4 _ColorStreets;
float4 _ColorParcels;
float4 _ColorOwnedParcels;
float4 _ColorEmpty;
float4 _GrassTexture_TexelSize;
float _GrassScale;
float2 _OwnedVariationRange;
float2 _UnownedVariationRange;
float _GrassGridTiling;
float _GrassGridThickness;
float _GrassGridVariationFrequency;
float _GrassGridThicknessVariation;
float _GrassGridIntenseFade;
float _GrassGridFarFade;
float2 _GrassGridFadePosition;
float4 _GrassGridColor;
float4 _RoadTexture_TexelSize;
float _RoadScale;
float _RoadFade;
float _Smoothness;
float _Metallic;
float2 _PlayerPosition;
float _FogFade;
float _FogIntensity;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);
TEXTURE2D(_GrassTexture);
SAMPLER(sampler_GrassTexture);
TEXTURE2D(_RoadTexture);
SAMPLER(sampler_RoadTexture);

// Graph Functions

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

struct Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b
{
    float3 WorldSpacePosition;
    half4 uv0;
};

void SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(float2 Vector2_6bff7006be6546f1a2eccc78e58e6232, float Vector1_e86c202cda73418eae7d4a134b98a195, Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b IN, out float2 UV_1, out float2 WorldUV_3, out float Zoom_2)
{
    float4 _UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0 = IN.uv0;
    float _Split_259410824e3c4f498de1eef89dacf280_R_1 = IN.WorldSpacePosition[0];
    float _Split_259410824e3c4f498de1eef89dacf280_G_2 = IN.WorldSpacePosition[1];
    float _Split_259410824e3c4f498de1eef89dacf280_B_3 = IN.WorldSpacePosition[2];
    float _Split_259410824e3c4f498de1eef89dacf280_A_4 = 0;
    float2 _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0 = float2(_Split_259410824e3c4f498de1eef89dacf280_R_1, _Split_259410824e3c4f498de1eef89dacf280_B_3);
    float _Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0 = Vector1_e86c202cda73418eae7d4a134b98a195;
    float _Split_70c1c438ae374dec9f011f8d2999c80e_R_1 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[0];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_G_2 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[1];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_B_3 = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)))[2];
    float _Split_70c1c438ae374dec9f011f8d2999c80e_A_4 = 0;
    float _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
    Unity_Divide_float(_Property_f780fb4de0a74463bb8bd3eb3d8f4563_Out_0, _Split_70c1c438ae374dec9f011f8d2999c80e_R_1, _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2);
    UV_1 = (_UV_a109b41d29c247eb88f929d56aa4fa7f_Out_0.xy);
    WorldUV_3 = _Vector2_662d8f08a64040e6bbd3ce6d415bba81_Out_0;
    Zoom_2 = _Divide_e8ad11c79d4b490b9cd5c40f3138d36a_Out_2;
}

// 7b6d5a90df0cb86d20ecea9cb96d928e
#include "Assets/Rendering/InfiniteFloor/MapV5.hlsl"

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Comparison_NotEqual_float(float A, float B, out float Out)
{
    Out = A != B ? 1 : 0;
}

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_Floor_float2(float2 In, out float2 Out)
{
    Out = floor(In);
}


inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}


inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}


inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = Unity_SimpleNoise_RandomValue_float(c0);
    float r1 = Unity_SimpleNoise_RandomValue_float(c1);
    float r2 = Unity_SimpleNoise_RandomValue_float(c2);
    float r3 = Unity_SimpleNoise_RandomValue_float(c3);

    float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
    float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
    float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
    return t;
}

void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3 - 0));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3 - 1));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3 - 2));
    t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    Out = t;
}

void Unity_Branch_float(float Predicate, float True, float False, out float Out)
{
    Out = Predicate ? True : False;
}

struct Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841
{
};

void SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(float2 Vector2_cd614e04b07b47eb95a2e5e3ffa41872, float Vector1_355229664685490fa0fc11fe1a97899f, float2 Vector2_1721f8718e464df9a2a9bd7239c50524, UnityTexture2D Texture2D_405855aa0a514baaa11320593c2f07c1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1, float2 Vector2_a2bcdf183ce44ee0a06e5ee37040af03, Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 IN, out float Mixed_1, out float Owned_2)
{
    UnityTexture2D _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0 = Texture2D_405855aa0a514baaa11320593c2f07c1;
    float2 _Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0 = Vector2_cd614e04b07b47eb95a2e5e3ffa41872;
    float _Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0 = 1;
    float _Property_06d14f9324724405bca2e16df40bef40_Out_0 = Vector1_355229664685490fa0fc11fe1a97899f;
    float _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2;
    Unity_Divide_float(_Float_5b7818daca7e4ccd8b77f6f91fbb7168_Out_0, _Property_06d14f9324724405bca2e16df40bef40_Out_0, _Divide_b41312ee15a644afa35f79f03d44c56c_Out_2);
    float _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2;
    Unity_Multiply_float(_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2, -1, _Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2);
    float _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2;
    Unity_Divide_float(_Multiply_8adc606342bd48a6b82e1f318f7d16fa_Out_2, 2, _Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2);
    float _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2;
    Unity_Add_float(_Divide_ff5f71d37f5e4eda8a41a8a782f5b237_Out_2, 0.5, _Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2);
    float2 _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3;
    Unity_TilingAndOffset_float(_Property_f7560b4c61a846bb9a9dd782ec1eae9b_Out_0, (_Divide_b41312ee15a644afa35f79f03d44c56c_Out_2.xx), (_Add_d4e9f8d03289498591f2da3dc11ca28b_Out_2.xx), _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float4 _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0 = SAMPLE_TEXTURE2D(_Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.tex, _Property_9ec0518079c648e08cd3235ecc89eb4c_Out_0.samplerstate, _TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3);
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_R_4 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.r;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.g;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.b;
    float _SampleTexture2D_108c05205d884e9298d8d59122709828_A_7 = _SampleTexture2D_108c05205d884e9298d8d59122709828_RGBA_0.a;
    float _Add_c7db3142e97045b2877ef4033d663af0_Out_2;
    Unity_Add_float(_SampleTexture2D_108c05205d884e9298d8d59122709828_R_4, _SampleTexture2D_108c05205d884e9298d8d59122709828_G_5, _Add_c7db3142e97045b2877ef4033d663af0_Out_2);
    float _Add_da555a32eb354a279399036fba5f852b_Out_2;
    Unity_Add_float(_Add_c7db3142e97045b2877ef4033d663af0_Out_2, _SampleTexture2D_108c05205d884e9298d8d59122709828_B_6, _Add_da555a32eb354a279399036fba5f852b_Out_2);
    float _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_9f9bfac15975447a9c81341908b981a0_Out_2);
    float2 _Property_49ef008f07834aba983ae50300e94c82_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03_1;
    float _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3;
    Unity_Remap_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, float2 (0, 1), _Property_49ef008f07834aba983ae50300e94c82_Out_0, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3);
    float2 _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0 = Vector2_1721f8718e464df9a2a9bd7239c50524;
    float2 _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_22b01d2127ef49ec898001b6871be91f_Out_3, _Property_bcd9267271c346b4a1c28f1a2fc4ec70_Out_0, float2 (0, 0), _TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3);
    float2 _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1;
    Unity_Floor_float2(_TilingAndOffset_17b1f0527e1b44119918fc3bb6b0bb32_Out_3, _Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1);
    float _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2;
    Unity_SimpleNoise_float(_Floor_7a5be3430da543e7b69a6adbdccbcbf8_Out_1, 150, _SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2);
    float2 _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0 = Vector2_a2bcdf183ce44ee0a06e5ee37040af03;
    float _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3;
    Unity_Remap_float(_SimpleNoise_3d3960e4e49d4369a723cbf287e940d1_Out_2, float2 (0, 1), _Property_c4deda37651342b3ad0a3da57d391f7e_Out_0, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3);
    float _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Unity_Branch_float(_Comparison_9f9bfac15975447a9c81341908b981a0_Out_2, _Remap_e47046e6e65f45ffb670a9c2632dce4a_Out_3, _Remap_0bea52121d004ec088ba32dff0aa7608_Out_3, _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3);
    float _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2;
    Unity_Comparison_NotEqual_float(_Add_da555a32eb354a279399036fba5f852b_Out_2, 0, _Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2);
    float _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
    Unity_Branch_float(_Comparison_85fe4c509c52437aaf0de1f5d4451c02_Out_2, 1, 0, _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3);
    Mixed_1 = _Branch_ed811547b40c484b914bf9ef6a4687b5_Out_3;
    Owned_2 = _Branch_af87ffb2e2db4acc909a6995a1cc4bfc_Out_3;
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Fraction_float2(float2 In, out float2 Out)
{
    Out = frac(In);
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

struct Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e
{
};

void SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(float2 Vector2_a43b69bd63e74b8899664790c597f8c6, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c, float Vector1_8be634a8378a4521b7522d631008fc39, UnityTexture2D Texture2D_d62eae330bc04650b9938434081cf58c_1, float Vector1_1, float Vector1_78a75bc300dd47ee83f8fbd9e84a0cad, float2 Vector2_f975587bc79d4eadbd0807b55a090f9d, float Vector1_e4cafe8cd46043f0ae5392b59d6b03fe, float2 Vector2_7389d8e6e0014c32be011a6864268e6a, float2 Vector2_cf1b396af9b54596bc8052bf3fe215fb, Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e IN, out float Grass_1, out float Road_2)
{
    UnityTexture2D _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c;
    float2 _Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0 = Vector1_8be634a8378a4521b7522d631008fc39;
    float _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2;
    Unity_Divide_float(1, _Property_5556a37a3d9c46fa9305c0292e78a38c_Out_0, _Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2);
    float _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2;
    Unity_Multiply_float(_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2, -1, _Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2);
    float _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2;
    Unity_Divide_float(_Multiply_90b5b3d95b034a98b7f6a7ebeb811d1c_Out_2, 2, _Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2);
    float _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2;
    Unity_Add_float(_Divide_b66bf0ac730b46f1a2ddbfe3bb52e2ff_Out_2, 0.5, _Add_e636f222615a4d5b80b5dc3743ef5097_Out_2);
    float2 _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3;
    Unity_TilingAndOffset_float(_Property_7499b88678ad4a1b9ba21aee14479b4d_Out_0, (_Divide_b799ecc3984d451d884bf2ebfa4280de_Out_2.xx), (_Add_e636f222615a4d5b80b5dc3743ef5097_Out_2.xx), _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float4 _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0 = SAMPLE_TEXTURE2D(_Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.tex, _Property_96acefdbb4cb45b1a8c08421eaf40edd_Out_0.samplerstate, _TilingAndOffset_3fe861ebd3fc413c8a8035265c6beb9e_Out_3);
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.r;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_G_5 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.g;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_B_6 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.b;
    float _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_A_7 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_RGBA_0.a;
    UnityTexture2D _Property_acfdee93b05546369a691885f7d8fc49_Out_0 = Texture2D_d62eae330bc04650b9938434081cf58c_1;
    float2 _Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0 = Vector2_a43b69bd63e74b8899664790c597f8c6;
    float2 _Property_622a1d2b345749f7af37dbeb28c9856a_Out_0 = Vector2_f975587bc79d4eadbd0807b55a090f9d;
    float _Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0 = 1;
    float _Property_9655382379e942478a21d79fef207bdd_Out_0 = Vector1_78a75bc300dd47ee83f8fbd9e84a0cad;
    float _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2;
    Unity_Divide_float(_Float_617766fbd4a64ea6af6c3110d479ed6e_Out_0, _Property_9655382379e942478a21d79fef207bdd_Out_0, _Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2);
    float2 _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2;
    Unity_Multiply_float(_Property_622a1d2b345749f7af37dbeb28c9856a_Out_0, (_Divide_68dece912bbd4b25b411503ebd55cfc5_Out_2.xx), _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2);
    float _Split_92aaba0363214ad786eeca836e64e191_R_1 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[0];
    float _Split_92aaba0363214ad786eeca836e64e191_G_2 = _Multiply_36d2dec3d12849d6a04e62618c6abfe2_Out_2[1];
    float _Split_92aaba0363214ad786eeca836e64e191_B_3 = 0;
    float _Split_92aaba0363214ad786eeca836e64e191_A_4 = 0;
    float _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2;
    Unity_Multiply_float(_Split_92aaba0363214ad786eeca836e64e191_R_1, -1, _Multiply_426451ec50dc4f3b80866858356f7c82_Out_2);
    float _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2;
    Unity_Divide_float(_Multiply_426451ec50dc4f3b80866858356f7c82_Out_2, 2, _Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2);
    float _Add_1e90aeca170749febce3402fc85db207_Out_2;
    Unity_Add_float(_Divide_20b8e6433c0c4bb586bfc10bac094087_Out_2, 0.5, _Add_1e90aeca170749febce3402fc85db207_Out_2);
    float2 _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3;
    Unity_TilingAndOffset_float(_Property_45ee8d3e24ab47f9a80838eed656b6bd_Out_0, (_Split_92aaba0363214ad786eeca836e64e191_R_1.xx), (_Add_1e90aeca170749febce3402fc85db207_Out_2.xx), _TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3);
    float2 _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_71115c63dcc241069d31f7db96b1be6b_Out_3, _Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1);
    float _Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0 = Vector1_1;
    float _Multiply_3e0c255ae161401f94116d1116002307_Out_2;
    Unity_Multiply_float(_Property_4a5c36ace7674c479fbd914397b6c7ff_Out_0, 10000, _Multiply_3e0c255ae161401f94116d1116002307_Out_2);
    float2 _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3;
    Unity_TilingAndOffset_float(_Fraction_0c89d7770e9a4b0d878b4ab86d86da20_Out_1, (_Multiply_3e0c255ae161401f94116d1116002307_Out_2.xx), float2 (0, 0), _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float4 _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0 = SAMPLE_TEXTURE2D(_Property_acfdee93b05546369a691885f7d8fc49_Out_0.tex, _Property_acfdee93b05546369a691885f7d8fc49_Out_0.samplerstate, _TilingAndOffset_54419e827cc9400797311ef5312663d6_Out_3);
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.r;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_G_5 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.g;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_B_6 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.b;
    float _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_A_7 = _SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_RGBA_0.a;
    float2 _Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0 = Vector2_cf1b396af9b54596bc8052bf3fe215fb;
    float2 _Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0 = Vector2_7389d8e6e0014c32be011a6864268e6a;
    float2 _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2;
    Unity_Multiply_float(_Property_85742fd4a66d4108a5a31fb86cc5929a_Out_0, float2(-1, -1), _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2);
    float2 _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2;
    Unity_Add_float2(_Property_01fcc4fd15554fb68dabb74a05a66a91_Out_0, _Multiply_e4b2caca033a4399b132aaa2714e6eb0_Out_2, _Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2);
    float _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0 = Vector1_e4cafe8cd46043f0ae5392b59d6b03fe;
    float _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2;
    Unity_Divide_float(10, _Property_1a5c11cc39f8477e9ffe0b33092b46a2_Out_0, _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2);
    float2 _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4;
    Unity_PolarCoordinates_float(_Add_95e357a34a8843e9b2cdca8f9b86539e_Out_2, float2 (0.5, 0.5), _Divide_ec0467a7a4704d64b0c4673dade46108_Out_2, 1, _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4);
    float _Split_f3387914733a4792b78907a73c898380_R_1 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[0];
    float _Split_f3387914733a4792b78907a73c898380_G_2 = _PolarCoordinates_ccfc26f734344fef83f4d72b2a2398d3_Out_4[1];
    float _Split_f3387914733a4792b78907a73c898380_B_3 = 0;
    float _Split_f3387914733a4792b78907a73c898380_A_4 = 0;
    float _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1;
    Unity_Saturate_float(_Split_f3387914733a4792b78907a73c898380_R_1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1);
    float _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
    Unity_Lerp_float(_SampleTexture2D_323adb48b37a40d9bd6f56190ccae523_R_4, 1, _Saturate_0a027211636744e89ee7ea5d9329f3d3_Out_1, _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3);
    Grass_1 = _SampleTexture2D_848275d955c947948bfb0a85a0be6dc9_R_4;
    Road_2 = _Lerp_33e77e15fa924e54a8fa663f08dc5993_Out_3;
}

struct Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a
{
};

void SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(float2 Vector2_311ffee78d314f71a9463e39924ea623, float2 Vector2_a57b68e1b4044834933fd8337f0a0577, float Vector1_7284deecf5d9431d92fc35a123337ff4, float Vector1_ab2c4cd721534cf4a387156d51a1fed9, Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a IN, out float Out_1)
{
    float2 _Property_70523c283f40499f89e4f7748deff77e_Out_0 = Vector2_311ffee78d314f71a9463e39924ea623;
    float2 _Property_f28b80022c3246688280e0762030829b_Out_0 = Vector2_a57b68e1b4044834933fd8337f0a0577;
    float2 _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2;
    Unity_Multiply_float(_Property_f28b80022c3246688280e0762030829b_Out_0, float2(-1, -1), _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2);
    float2 _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2;
    Unity_Add_float2(_Property_70523c283f40499f89e4f7748deff77e_Out_0, _Multiply_269adcf2851d402aaa5985aaae79d7a3_Out_2, _Add_e90ad347cd4b42c3963540725f4e79d9_Out_2);
    float _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0 = Vector1_7284deecf5d9431d92fc35a123337ff4;
    float _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2;
    Unity_Divide_float(1, _Property_3ba2cfa823cf437fb838caae47d7a32b_Out_0, _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2);
    float2 _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4;
    Unity_PolarCoordinates_float(_Add_e90ad347cd4b42c3963540725f4e79d9_Out_2, float2 (0.5, 0.5), _Divide_0020e8c897f94e14b92feac87cf71bff_Out_2, 1, _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4);
    float _Split_904e58337bbe428998ef573899b98f55_R_1 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[0];
    float _Split_904e58337bbe428998ef573899b98f55_G_2 = _PolarCoordinates_318c2d38f4814d0fbad42473f2e2c6ab_Out_4[1];
    float _Split_904e58337bbe428998ef573899b98f55_B_3 = 0;
    float _Split_904e58337bbe428998ef573899b98f55_A_4 = 0;
    float _Property_f8541835e99e409989806d7eff9d13e8_Out_0 = Vector1_ab2c4cd721534cf4a387156d51a1fed9;
    float _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2;
    Unity_Multiply_float(_Split_904e58337bbe428998ef573899b98f55_R_1, _Property_f8541835e99e409989806d7eff9d13e8_Out_0, _Multiply_a77283d5783542b596ccaa11bb712b63_Out_2);
    float _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
    Unity_Saturate_float(_Multiply_a77283d5783542b596ccaa11bb712b63_Out_2, _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1);
    Out_1 = _Saturate_9efcbf5d7c8f41d18bf63394c7844835_Out_1;
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Rectangle_float(float2 UV, float Width, float Height, out float Out)
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

struct Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c
{
};

void SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(float2 Vector2_e01f7c264af944fdb8bcea2d35ae3001, float2 Vector2_622ed4642c4445d194b78ad6759b208d, float2 Vector2_575137a8a58748d1a0e062a00216bbe5, float Vector1_23353b8652e043faab2f58b3964e3f17, float Vector1_5c017085898d45e48611a2e9ace96469, float Vector1_f8f78e1de998447c949d6ce599a31355, float4 Vector4_5fb32e510cd648f8b219982d0bc6426a, float Vector1_eb4dbe959ea64ae896f61f72a5d275d0, float Vector1_2b42c6accf5149c3929d5731c737ba7c, float2 Vector2_2c998556cbda461d8a0b69199046f9f5, float Vector1_f62ba0f4717b42c1b7e03ce424479587, float Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d, float Vector1_63cdfc1b7ebd4084b00ceb9b109e3919, Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c IN, out float GrassGrid_1)
{
    float2 _Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0 = Vector2_622ed4642c4445d194b78ad6759b208d;
    float2 _Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0 = Vector2_2c998556cbda461d8a0b69199046f9f5;
    float2 _Multiply_d348b89a76874839babedba1f8d3296d_Out_2;
    Unity_Multiply_float(_Property_1caf66638cbf426e935bd47a7e2bd56f_Out_0, float2(-1, -1), _Multiply_d348b89a76874839babedba1f8d3296d_Out_2);
    float2 _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2;
    Unity_Add_float2(_Property_2db1fe33e3db40ce800d41a68defcd2e_Out_0, _Multiply_d348b89a76874839babedba1f8d3296d_Out_2, _Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2);
    float _Property_eb9ff929e1204221bca1c31925f600b7_Out_0 = Vector1_2b42c6accf5149c3929d5731c737ba7c;
    float _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2;
    Unity_Divide_float(10, _Property_eb9ff929e1204221bca1c31925f600b7_Out_0, _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2);
    float2 _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_19cb569ac9754e22a8246bf4ec2303e2_Out_2, 1, _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4);
    float _Split_2e7cca56ed8b4f69890662df97d724ba_R_1 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[0];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_G_2 = _PolarCoordinates_2000745dd9774a5f9d59b0514a1d4c37_Out_4[1];
    float _Split_2e7cca56ed8b4f69890662df97d724ba_B_3 = 0;
    float _Split_2e7cca56ed8b4f69890662df97d724ba_A_4 = 0;
    float _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1;
    Unity_OneMinus_float(_Split_2e7cca56ed8b4f69890662df97d724ba_R_1, _OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1);
    float _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1;
    Unity_Saturate_float(_OneMinus_d8dcd242c9174a8198950d9eaa370a18_Out_1, _Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1);
    float _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0 = Vector1_eb4dbe959ea64ae896f61f72a5d275d0;
    float _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2;
    Unity_Divide_float(10, _Property_8cb0721852734f0bbaf69514761a7bc5_Out_0, _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2);
    float2 _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4;
    Unity_PolarCoordinates_float(_Add_7dacb8ed05f74d4b9c467d5f3cab50a7_Out_2, float2 (0.5, 0.5), _Divide_e1de24cba5b44f829b2937e98a7f5a69_Out_2, 1, _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4);
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[0];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_G_2 = _PolarCoordinates_70c717be0dda4416a81020a5f6a65e9c_Out_4[1];
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_B_3 = 0;
    float _Split_f954b1dc25a7466084abce0fe3ca0bbc_A_4 = 0;
    float _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1;
    Unity_OneMinus_float(_Split_f954b1dc25a7466084abce0fe3ca0bbc_R_1, _OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1);
    float _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1;
    Unity_Saturate_float(_OneMinus_242f45349bc946e9b88e497abe6d5921_Out_1, _Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1);
    float2 _Property_8c07974a4edd4dc89101134d98954a00_Out_0 = Vector2_e01f7c264af944fdb8bcea2d35ae3001;
    float _Float_93afd7af653a45a38377067c2d80ab35_Out_0 = 1;
    float _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0 = Vector1_23353b8652e043faab2f58b3964e3f17;
    float _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2;
    Unity_Divide_float(_Float_93afd7af653a45a38377067c2d80ab35_Out_0, _Property_121df4362f324d4e9c2c5a573fded3d5_Out_0, _Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2);
    float _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2;
    Unity_Multiply_float(_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2, -1, _Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2);
    float _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2;
    Unity_Divide_float(_Multiply_425cd39f7b8148debb83ff3b5ccf00d8_Out_2, 2, _Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2);
    float _Add_bc17962f1e2d49fca18bb00e85478880_Out_2;
    Unity_Add_float(_Divide_adb65c17d1e34291a0804aab8fc60d95_Out_2, 0.5, _Add_bc17962f1e2d49fca18bb00e85478880_Out_2);
    float2 _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3;
    Unity_TilingAndOffset_float(_Property_8c07974a4edd4dc89101134d98954a00_Out_0, (_Divide_25fb1144298842f280b90abd5ce4ea2b_Out_2.xx), (_Add_bc17962f1e2d49fca18bb00e85478880_Out_2.xx), _TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3);
    float2 _Property_deac93b8878546c8a300d7352a631a26_Out_0 = Vector2_575137a8a58748d1a0e062a00216bbe5;
    float2 _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3;
    Unity_TilingAndOffset_float(_TilingAndOffset_830c84d852434688bd1ff940453866d2_Out_3, _Property_deac93b8878546c8a300d7352a631a26_Out_0, float2 (0, 0), _TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3);
    float2 _Fraction_a4f4615406494c08b0082401f60051c2_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_7fcbaecf1f2d449bba56785cf2870948_Out_3, _Fraction_a4f4615406494c08b0082401f60051c2_Out_1);
    float _Property_0081be27ffb041f4ac12f66f0dc62624_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float2 _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Property_0081be27ffb041f4ac12f66f0dc62624_Out_0.xx), float2 (0, 0), _TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3);
    float2 _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_402cb8a3f7e14efe9bec4fb9d388ef3a_Out_3, _Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1);
    float _Property_d89b62d779e54332979e2425f6cd1857_Out_0 = Vector1_f8f78e1de998447c949d6ce599a31355;
    float _Add_1feb1662c2884dffad05bb44310c6586_Out_2;
    Unity_Add_float(_Property_d89b62d779e54332979e2425f6cd1857_Out_0, 1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2);
    float _Divide_43532697f4454e47a4092b408ec0ff25_Out_2;
    Unity_Divide_float(1, _Add_1feb1662c2884dffad05bb44310c6586_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2);
    float _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3;
    Unity_Rectangle_float(_Fraction_8cafd891631e4ea4a9849fc75450ab1f_Out_1, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Divide_43532697f4454e47a4092b408ec0ff25_Out_2, _Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3);
    float _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1;
    Unity_OneMinus_float(_Rectangle_c344a5e114b9471bb39bacdfafa1a50d_Out_3, _OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1);
    float _Property_43f57c55e694444e9bf9aeb01760d823_Out_0 = Vector1_5c017085898d45e48611a2e9ace96469;
    float _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_853fa90d1d314da390af1a6f21f72298_Out_2;
    Unity_Divide_float(_Property_43f57c55e694444e9bf9aeb01760d823_Out_0, _Property_fcee581d4da24fdb96a8aebcd72a47e1_Out_0, _Divide_853fa90d1d314da390af1a6f21f72298_Out_2);
    float2 _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3;
    Unity_TilingAndOffset_float(_Fraction_a4f4615406494c08b0082401f60051c2_Out_1, (_Divide_853fa90d1d314da390af1a6f21f72298_Out_2.xx), float2 (0, 0), _TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3);
    float2 _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1;
    Unity_Fraction_float2(_TilingAndOffset_2bfa5955c27f4d5a8930a9fbbe67667f_Out_3, _Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1);
    float _Property_5236eaaed502435380fd9232ca3f5a7b_Out_0 = Vector1_c93800ee3a6e4a2da0c62f2a6228ea8d;
    float _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0 = Vector1_f62ba0f4717b42c1b7e03ce424479587;
    float _Divide_479d5e5f38be495d982aed56501420aa_Out_2;
    Unity_Divide_float(_Property_5236eaaed502435380fd9232ca3f5a7b_Out_0, _Property_7cde1b41a1d64077a5a62ca3eee66e65_Out_0, _Divide_479d5e5f38be495d982aed56501420aa_Out_2);
    float _Add_eae9a749d00c4960aea43a9320fead3e_Out_2;
    Unity_Add_float(_Divide_479d5e5f38be495d982aed56501420aa_Out_2, 1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2);
    float _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2;
    Unity_Divide_float(1, _Add_eae9a749d00c4960aea43a9320fead3e_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2);
    float _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3;
    Unity_Rectangle_float(_Fraction_6856d11b8b644e3781b3b8df1489188a_Out_1, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Divide_381af8706fb34f5f84a67ea2dafe5ba8_Out_2, _Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3);
    float _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1;
    Unity_OneMinus_float(_Rectangle_13dfb01bed4e4e44a8b5c07b35f19e79_Out_3, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1);
    float _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2;
    Unity_Add_float(_OneMinus_077dbc84a76140079d9e9b086b01e07d_Out_1, _OneMinus_3cce9956029f46419d9cf4177cf76375_Out_1, _Add_2b900f3ab1b44858bf5696180eac62e6_Out_2);
    float _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1;
    Unity_Saturate_float(_Add_2b900f3ab1b44858bf5696180eac62e6_Out_2, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1);
    float _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2;
    Unity_Multiply_float(_Saturate_ef289898bf06492bbca23caf2c18c0d2_Out_1, _Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2);
    float4 _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0 = Vector4_5fb32e510cd648f8b219982d0bc6426a;
    float _Split_d97e936de3fa453e9725f0c2256e5eac_R_1 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[0];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_G_2 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[1];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_B_3 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[2];
    float _Split_d97e936de3fa453e9725f0c2256e5eac_A_4 = _Property_701774e99e5f40a3a268dd4ed4a649f4_Out_0[3];
    float _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2;
    Unity_Multiply_float(_Saturate_7abd66701cc44d08a7d555a182d9524c_Out_1, _Split_d97e936de3fa453e9725f0c2256e5eac_A_4, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2);
    float _Add_9af69bb68e3044b29a3495457a20582c_Out_2;
    Unity_Add_float(_Multiply_79e9988ec15742549119ad8f3a1b1a5f_Out_2, _Multiply_1dda2aa7bf2c43d0a1fc795034d26df3_Out_2, _Add_9af69bb68e3044b29a3495457a20582c_Out_2);
    float _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
    Unity_Multiply_float(_Saturate_98ef4302d6f242329024c3c0a628f93f_Out_1, _Add_9af69bb68e3044b29a3495457a20582c_Out_2, _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2);
    GrassGrid_1 = _Multiply_dfe1ba2ac92c4a07b960bab95193d207_Out_2;
}

void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
{
    SHADERGRAPH_FOG(Position, Color, Density);
}

// 98116c2c658709e5fcb200b1ae28460e
#include "Assets/Rendering/InfiniteFloor/InfiniteFloorMerger.hlsl"

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
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_e943bfd340cf4709a76ba852685dbf55_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_c294f42edfdb40c18d1605395ee9f835_Out_0 = _PlayerPosition;
    float _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0 = _Zoom;
    Bindings_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.WorldSpacePosition = IN.WorldSpacePosition;
    _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71.uv0 = IN.uv0;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1;
    float2 _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3;
    float _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2;
    SG_SGInfiniteFloorUVZoom_6ee81c35f625acd4b8405b97857eb86b(_Property_c294f42edfdb40c18d1605395ee9f835_Out_0, _Property_c33a86687eb443a7ac9a32fb8ba88ead_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2);
    float2 _Property_edde0349814e405a8f77a67715a72a11_Out_0 = _SizeOfTexture;
    float _Property_c4b62049d152467eb90794c337831029_Out_0 = _GridThickness;
    float _Property_038d46b4121440948e0171cd5c26d417_Out_0 = _ThicknessOffset;
    float _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0 = _GridOffset;
    float4 _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18;
    Main_float(_Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0, _Property_e943bfd340cf4709a76ba852685dbf55_Out_0, 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_edde0349814e405a8f77a67715a72a11_Out_0, _Property_c4b62049d152467eb90794c337831029_Out_0, _Property_038d46b4121440948e0171cd5c26d417_Out_0, _Property_16536b70759f4bf581ea44e8e3c7f4e9_Out_0, float2 (0, 0), 0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineGrid_20, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightInner_17, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_HighlightMid_18);
    float2 _Property_81c877f57fbd4119951d21e6a05f9536_Out_0 = _SizeOfTexture;
    UnityTexture2D _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0 = _OwnedVariationRange;
    float2 _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0 = _UnownedVariationRange;
    Bindings_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841 _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1;
    float _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2;
    SG_SGInfiniteFloorRandomTiling_550efd7b02dd7d64fa2dcf4beb08a841(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_81c877f57fbd4119951d21e6a05f9536_Out_0, _Property_3b4574bd48f4480ca7355ada71ce2431_Out_0, _Property_9c80dbed011a4661b4dc39d24bc003f0_Out_0, _Property_ddec154d4dd846898efbbbd6f1bca98c_Out_0, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2);
    UnityTexture2D _Property_03a2f987be36438cb0f8886babf33c96_Out_0 = UnityBuildTexture2DStructNoScale(_GrassTexture);
    float _Property_3ea8b576653841a7a5a603ef1f120469_Out_0 = _GrassScale;
    UnityTexture2D _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0 = UnityBuildTexture2DStructNoScale(_RoadTexture);
    float _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0 = _RoadScale;
    float _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0 = _Zoom;
    float2 _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0 = _SizeOfTexture;
    float _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0 = _RoadFade;
    float2 _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0 = _PlayerPosition;
    Bindings_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1;
    float _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2;
    SG_SGInfiniteFloorTextures_63fd93f36b589a74ab03010d32b4816e(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _Property_03a2f987be36438cb0f8886babf33c96_Out_0, _Property_3ea8b576653841a7a5a603ef1f120469_Out_0, _Property_fb810e437e704eecbef8ebb2de0b4f87_Out_0, _Property_0232f0c810ad40a48d30cd2b51fe5fdf_Out_0, _Property_6caa425955974bfd86c28c3c1ca33d46_Out_0, _Property_b78f9c5d976c4281958cbf7b418a43a7_Out_0, _Property_b0bea84b765c4ea8b7e39d6456ca430b_Out_0, _Property_bbfe7ca4923b49599e2471ca8eabdabe_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2);
    float2 _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0 = _SizeOfTexture;
    float _Property_039b41a22968494fb95b7756f155d828_Out_0 = _GrassGridTiling;
    float _Property_98eedb86fc5b4145bc7b65222fede898_Out_0 = _GrassGridThickness;
    float4 _Property_da5db702a8724d9f9a82e143a886ee60_Out_0 = _GrassGridColor;
    float _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0 = _GrassGridIntenseFade;
    float _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0 = _GrassGridFarFade;
    float2 _Property_1ec68bba28374a39850d7800d8f1d362_Out_0 = _PlayerPosition;
    float _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0 = _GrassGridVariationFrequency;
    float _Property_4dcfad1507d94638b52b49967893b527_Out_0 = _GrassGridThicknessVariation;
    float2 _Property_6a4b9239d1bd424885c4b7027264b775_Out_0 = _PlayerPosition;
    float _Property_0d3761446a804744bb350177ec5d239a_Out_0 = _FogFade;
    float _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0 = _FogIntensity;
    Bindings_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af;
    float _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1;
    SG_SGInfiniteFloorFog_e920b4f99adf91644a34c593aebc814a(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6a4b9239d1bd424885c4b7027264b775_Out_0, _Property_0d3761446a804744bb350177ec5d239a_Out_0, _Property_bb89e5ccabcb4357bc314a157bca2114_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1);
    Bindings_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850;
    float _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1;
    SG_SGInfiniteFloorGrassGrid_e02aaeaeb785f7c40952cad0ab2a961c(_SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_UV_1, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_WorldUV_3, _Property_6e6d5f35495f480d98315e5b25970b6b_Out_0, _SGInfiniteFloorUVZoom_ecd7cad0aa684eeba0ea7a49fefc4e71_Zoom_2, _Property_039b41a22968494fb95b7756f155d828_Out_0, _Property_98eedb86fc5b4145bc7b65222fede898_Out_0, _Property_da5db702a8724d9f9a82e143a886ee60_Out_0, _Property_0f7cd9fac6a443adbd0aedde52094771_Out_0, _Property_3bc3521f3344491fa2c03051f50bfb1e_Out_0, _Property_1ec68bba28374a39850d7800d8f1d362_Out_0, _Property_ad4336f464c246cda74fc94a312e4fa7_Out_0, _Property_4dcfad1507d94638b52b49967893b527_Out_0, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1);
    float4 _Property_00719080222e437aa9abf5da2dd48a70_Out_0 = _ColorGrid;
    float4 _Property_118255dd5c8c455ab153d511ba1fc031_Out_0 = _ColorPlazas;
    float4 _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0 = _ColorDistricts;
    float4 _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0 = _ColorStreets;
    float4 _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0 = _ColorParcels;
    float4 _Property_6a1f7cc7467741628211b53b8709021d_Out_0 = _ColorOwnedParcels;
    float4 _Property_9d59173f2b1a48428823e35074ce62c5_Out_0 = _ColorEmpty;
    float4 _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0 = _GrassGridColor;
    float4 _Fog_caf07e8785584760b79500664df1fc44_Color_0;
    float _Fog_caf07e8785584760b79500664df1fc44_Density_1;
    Unity_Fog_float(_Fog_caf07e8785584760b79500664df1fc44_Color_0, _Fog_caf07e8785584760b79500664df1fc44_Density_1, IN.ObjectSpacePosition);
    float4 _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0;
    Merger_float(_MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineSobel_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_OutlineInner_22, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Mixed_1, _SGInfiniteFloorRandomTiling_48db84b6e62945b9b8036dc18bd48e2b_Owned_2, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Grass_1, _SGInfiniteFloorTextures_ade6262db40e4a86af0fa20271a4d4ac_Road_2, _SGInfiniteFloorGrassGrid_98502873c2c74e2f83619e0ef808f850_GrassGrid_1, _SGInfiniteFloorFog_e91dc499f9dd4d5ea7fbaf64da5d89af_Out_1, _Property_00719080222e437aa9abf5da2dd48a70_Out_0, _Property_118255dd5c8c455ab153d511ba1fc031_Out_0, _Property_ea3a4525b26042e282e0d5dcec3efb89_Out_0, _Property_b4c2fd41f9674a7d9da551d4157f191a_Out_0, _Property_772bc9abe9c94089af81a6648cd34b1b_Out_0, _Property_6a1f7cc7467741628211b53b8709021d_Out_0, _Property_9d59173f2b1a48428823e35074ce62c5_Out_0, _Property_04d7af1c04904489b3c3443e9c433a5f_Out_0, _Fog_caf07e8785584760b79500664df1fc44_Color_0, _MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0);
    surface.BaseColor = (_MergerCustomFunction_10a3a34cea6d41debf50150c101b0cf2_Out_0.xyz);
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





    output.WorldSpacePosition = input.positionWS;
    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
    output.uv0 = input.texCoord0;
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