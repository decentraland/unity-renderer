Shader "Custom/S_FinalMap"
{
    Properties
    {
        [NoScaleOffset] _MainTex("MainTex", 2D) = "white" {}
        [NoScaleOffset]_Map("Map", 2D) = "white" {}
        [NoScaleOffset]_EstateIDMap("EstateIDMap", 2D) = "white" {}
        _SizeOfTexture("SizeOfTexture", Vector) = (512, 512, 0, 0)
        _MousePosition("MousePosition", Vector) = (0, 0, 0, 0)
        _Zoom("Zoom", Float) = 3.75
        _GridThickness("GridThickness", Range(0.0001, 10)) = 1
        _SobelGridOffset("SobelGridOffset", Float) = 2000
        _GridOffset("GridOffset", Float) = 0.9
        _GridColor("GridColor", Color) = (0, 0, 0, 0)
        _GridOpacity("GridOpacity", Float) = 0.05
        [HDR]_HighlightColor("HighlightColor", Color) = (0, 0.6943347, 1, 1)
        _ColorInnerHighlight("ColorInnerHighlight", Color) = (1, 1, 1, 1)
        _HighlightThickness("HighlightThickness", Float) = 2
        _HighlightRotation("HighlightRotation", Float) = 10
        _Color01("ColorPlaza", Color) = (1, 0, 0, 0)
        _Color02("ColorEstates", Color) = (0.03901875, 1, 0, 0)
        _Color03("ColorStreets", Color) = (0, 0.2896385, 1, 0)
        _Color04("ColorBase", Color) = (0.2189539, 0.1096921, 0.2735849, 0)
        _Color05("ColorBackground", Color) = (0.2189539, 0.1096921, 0.2735849, 0)
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
        ZTest[unity_GUIZTestMode] //ZTest LEqual
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
        #define _AlphaClip 1
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float2 _MousePosition;
float _Zoom;
float _GridThickness;
float _SobelGridOffset;
float _GridOffset;
float4 _GridColor;
float _GridOpacity;
float4 _HighlightColor;
float4 _ColorInnerHighlight;
float _HighlightThickness;
float _HighlightRotation;
float4 _Color01;
float4 _Color02;
float4 _Color03;
float4 _Color04;
float4 _Color05;
CBUFFER_END

// Object and Global properties
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);

// Graph Functions

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

// 7b6d5a90df0cb86d20ecea9cb96d928e
#include "Assets/Rendering/Map/V5/MapV5.hlsl"

void Unity_Divide_float2(float2 A, float2 B, out float2 Out)
{
    Out = A / B;
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_SampleGradient_float(Gradient Gradient, float Time, out float4 Out)
{
    float3 color = Gradient.colors[0].rgb;
    [unroll]
    for (int c = 1; c < 8; c++)
    {
        float colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
        color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
    }
#ifndef UNITY_COLORSPACE_GAMMA
    color = SRGBToLinear(color);
#endif
    float alpha = Gradient.alphas[0].x;
    [unroll]
    for (int a = 1; a < 8; a++)
    {
        float alphaPos = saturate((Time - Gradient.alphas[a - 1].y) / (Gradient.alphas[a].y - Gradient.alphas[a - 1].y)) * step(a, Gradient.alphasLength - 1);
        alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
    }
    Out = float4(color, alpha);
}

struct Bindings_SGMapHighlightGradient_e18ba2c84b9936946bedc864ac8082fc
{
    half4 uv0;
    float3 TimeParameters;
};

void SG_SGMapHighlightGradient_e18ba2c84b9936946bedc864ac8082fc(float Vector1_361221063f124ccab2ad3ec68e1f5d56, float Vector1_238d8cc490904e7288f485eda9e42fb6, float2 Vector2_925ecca986df430e90199f0d05779aec, float2 Vector2_10475809fb3d4358aef5f163b670c39e, Bindings_SGMapHighlightGradient_e18ba2c84b9936946bedc864ac8082fc IN, out float4 Out_1)
{
    Gradient _Gradient_210c51b3f7b04c4897eadb906d891900_Out_0 = NewGradient(0, 4, 2, float4(0.4386287, 0, 1, 0),float4(1, 0.6190026, 0, 0.3329976),float4(1, 0, 0.02109909, 0.6659952),float4(0.4386287, 0, 1, 1),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0));
    float2 _Property_d796eb97e5d44414a83d93ff09d72dbb_Out_0 = Vector2_10475809fb3d4358aef5f163b670c39e;
    float2 _Add_47d9b3c4d3c64310b77647fc2b95c116_Out_2;
    Unity_Add_float2(_Property_d796eb97e5d44414a83d93ff09d72dbb_Out_0, float2(0.5, 0.5), _Add_47d9b3c4d3c64310b77647fc2b95c116_Out_2);
    float2 _Property_e98b0650494a43f285e3946aa2b1c325_Out_0 = Vector2_925ecca986df430e90199f0d05779aec;
    float2 _Divide_b3a94e8cf3914b498c267f96727b882a_Out_2;
    Unity_Divide_float2(_Property_e98b0650494a43f285e3946aa2b1c325_Out_0, float2(2, 2), _Divide_b3a94e8cf3914b498c267f96727b882a_Out_2);
    float2 _Add_aae49534148d4f798348a8e4d522323d_Out_2;
    Unity_Add_float2(_Add_47d9b3c4d3c64310b77647fc2b95c116_Out_2, _Divide_b3a94e8cf3914b498c267f96727b882a_Out_2, _Add_aae49534148d4f798348a8e4d522323d_Out_2);
    float2 _Property_802580ab3476494ca0a9d21aebe48bc3_Out_0 = Vector2_925ecca986df430e90199f0d05779aec;
    float2 _Divide_ebbc05a3ceac4c7a823a98f6d33f48ea_Out_2;
    Unity_Divide_float2(_Add_aae49534148d4f798348a8e4d522323d_Out_2, _Property_802580ab3476494ca0a9d21aebe48bc3_Out_0, _Divide_ebbc05a3ceac4c7a823a98f6d33f48ea_Out_2);
    float _Property_f9dc7e6926834a45ad16cff551756ff9_Out_0 = Vector1_361221063f124ccab2ad3ec68e1f5d56;
    float _Multiply_88775923f8e84251bee82776718bd4e5_Out_2;
    Unity_Multiply_float(IN.TimeParameters.x, _Property_f9dc7e6926834a45ad16cff551756ff9_Out_0, _Multiply_88775923f8e84251bee82776718bd4e5_Out_2);
    float2 _Rotate_c6747b48584b422a994eda42cc60c2f9_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, _Divide_ebbc05a3ceac4c7a823a98f6d33f48ea_Out_2, _Multiply_88775923f8e84251bee82776718bd4e5_Out_2, _Rotate_c6747b48584b422a994eda42cc60c2f9_Out_3);
    float2 _PolarCoordinates_184b089217934ac883f9473201fdc826_Out_4;
    Unity_PolarCoordinates_float(_Rotate_c6747b48584b422a994eda42cc60c2f9_Out_3, _Divide_ebbc05a3ceac4c7a823a98f6d33f48ea_Out_2, 1, 1, _PolarCoordinates_184b089217934ac883f9473201fdc826_Out_4);
    float2 _Add_8ffb3795e959460687a5ab4edefccffa_Out_2;
    Unity_Add_float2(_PolarCoordinates_184b089217934ac883f9473201fdc826_Out_4, float2(0, 0.5), _Add_8ffb3795e959460687a5ab4edefccffa_Out_2);
    float _Split_f7eeea1d2bbf4aa6b1bf45ceedd89f96_R_1 = _Add_8ffb3795e959460687a5ab4edefccffa_Out_2[0];
    float _Split_f7eeea1d2bbf4aa6b1bf45ceedd89f96_G_2 = _Add_8ffb3795e959460687a5ab4edefccffa_Out_2[1];
    float _Split_f7eeea1d2bbf4aa6b1bf45ceedd89f96_B_3 = 0;
    float _Split_f7eeea1d2bbf4aa6b1bf45ceedd89f96_A_4 = 0;
    float4 _SampleGradient_0a5ac9ebeb1140d3954920b890a2c657_Out_2;
    Unity_SampleGradient_float(_Gradient_210c51b3f7b04c4897eadb906d891900_Out_0, _Split_f7eeea1d2bbf4aa6b1bf45ceedd89f96_G_2, _SampleGradient_0a5ac9ebeb1140d3954920b890a2c657_Out_2);
    Out_1 = _SampleGradient_0a5ac9ebeb1140d3954920b890a2c657_Out_2;
}

// 18883b1713183b9c39d0c16e2b5ea8f4
#include "Assets/Rendering/Map/V5/MapMerger.hlsl"

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
    UnityTexture2D _Property_759d2c340f054d1299007d7a8cae23fb_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_95fd9c9218994a71a3cf1dad8aebcec6_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float _Property_a548e3af294344bca54802afb4da1a23_Out_0 = _Zoom;
    float2 _Property_b03f33d008b842f4a1c178522202a87b_Out_0 = _SizeOfTexture;
    float _Property_d5e42179367c4c9191717a3ef64bd5e9_Out_0 = _GridThickness;
    float _Property_4508fa000c0243ec932c7501be41deb9_Out_0 = _SobelGridOffset;
    float _Property_9073ba7f820f4e59af356e0a00a0ea93_Out_0 = _GridOffset;
    float2 _Property_f7aa9ab32e8b4a7290f6118745c60c75_Out_0 = _MousePosition;
    float2 _Add_eb53b4f0961d4f449a77e74cea421ef4_Out_2;
    Unity_Add_float2(_Property_f7aa9ab32e8b4a7290f6118745c60c75_Out_0, float2(0, -1), _Add_eb53b4f0961d4f449a77e74cea421ef4_Out_2);
    float _Property_8dca6d41008d4c2aa9e02ef61ebd39a5_Out_0 = _HighlightThickness;
    float4 _UV_429c8fd82d9e4da78daca218cd5f5a21_Out_0 = IN.uv0;
    float4 _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Color_8;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineIntense_15;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineFade_20;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineInner_23;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Highlight_14;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightInner_17;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightMid_18;
    Main_float(_Property_759d2c340f054d1299007d7a8cae23fb_Out_0, _Property_95fd9c9218994a71a3cf1dad8aebcec6_Out_0, 1, _Property_a548e3af294344bca54802afb4da1a23_Out_0, _Property_b03f33d008b842f4a1c178522202a87b_Out_0, _Property_d5e42179367c4c9191717a3ef64bd5e9_Out_0, _Property_4508fa000c0243ec932c7501be41deb9_Out_0, _Property_9073ba7f820f4e59af356e0a00a0ea93_Out_0, _Add_eb53b4f0961d4f449a77e74cea421ef4_Out_2, _Property_8dca6d41008d4c2aa9e02ef61ebd39a5_Out_0, (_UV_429c8fd82d9e4da78daca218cd5f5a21_Out_0.xy), _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Color_8, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineIntense_15, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineFade_20, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineInner_23, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Highlight_14, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightInner_17, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightMid_18);
    float _Property_79c88caa89af4e338f7e7ea68f2abfe9_Out_0 = _GridOpacity;
    float4 _Property_6cb9215f47264aec8fe98d47f9d40752_Out_0 = _Color01;
    float4 _Property_6c88746bf77347e5beaee0e6c4b6197c_Out_0 = _Color02;
    float4 _Property_3fadd20be5ce4fcab164168d541cd78d_Out_0 = _Color03;
    float4 _Property_d1772ed172fb487cb538ed8d98b457f8_Out_0 = _Color04;
    float4 _Property_75b273d862db40aaa1f6f6608ba0a65c_Out_0 = _Color05;
    float4 _Property_46c9cb585089445aafc6df15820ec63e_Out_0 = _GridColor;
    float4 _Property_75bcf7573198493baa092f11db5a8497_Out_0 = IsGammaSpace() ? LinearToSRGB(_HighlightColor) : _HighlightColor;
    float4 _Property_9a6afe13de3a4f8083bb32c9ce9bb179_Out_0 = _ColorInnerHighlight;
    float _Property_41ad390fd0064d389b70dac20b88d515_Out_0 = _HighlightRotation;
    float _Property_993280fd61374b1dbefbcf0eb378acc3_Out_0 = _Zoom;
    float2 _Property_31469bbf929f4dbb8254243fa27b3ffe_Out_0 = _SizeOfTexture;
    float2 _Property_77f71ca158c94f94934c3d74b6046b4b_Out_0 = _MousePosition;
    Bindings_SGMapHighlightGradient_e18ba2c84b9936946bedc864ac8082fc _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3;
    _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3.uv0 = IN.uv0;
    _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3.TimeParameters = IN.TimeParameters;
    float4 _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3_Out_1;
    SG_SGMapHighlightGradient_e18ba2c84b9936946bedc864ac8082fc(_Property_41ad390fd0064d389b70dac20b88d515_Out_0, _Property_993280fd61374b1dbefbcf0eb378acc3_Out_0, _Property_31469bbf929f4dbb8254243fa27b3ffe_Out_0, _Property_77f71ca158c94f94934c3d74b6046b4b_Out_0, _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3, _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3_Out_1);
    float4 _MapMergerCustomFunction_8772f0022d8040d2a5e4220da6fdae78_Out_0;
    MapMerger_float(_MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Color_8, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineIntense_15, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineFade_20, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Highlight_14, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightInner_17, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightMid_18, _Property_79c88caa89af4e338f7e7ea68f2abfe9_Out_0, _Property_6cb9215f47264aec8fe98d47f9d40752_Out_0, _Property_6c88746bf77347e5beaee0e6c4b6197c_Out_0, _Property_3fadd20be5ce4fcab164168d541cd78d_Out_0, _Property_d1772ed172fb487cb538ed8d98b457f8_Out_0, _Property_75b273d862db40aaa1f6f6608ba0a65c_Out_0, _Property_46c9cb585089445aafc6df15820ec63e_Out_0, _Property_75bcf7573198493baa092f11db5a8497_Out_0, _Property_9a6afe13de3a4f8083bb32c9ce9bb179_Out_0, _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3_Out_1, _MapMergerCustomFunction_8772f0022d8040d2a5e4220da6fdae78_Out_0);
    surface.BaseColor = (_MapMergerCustomFunction_8772f0022d8040d2a5e4220da6fdae78_Out_0.xyz);
    surface.Alpha = 1;
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
        ZTest[unity_GUIZTestMode] //ZTest LEqual
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
        #define _AlphaClip 1
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
float4 _MainTex_TexelSize;
float4 _Map_TexelSize;
float4 _EstateIDMap_TexelSize;
float2 _SizeOfTexture;
float2 _MousePosition;
float _Zoom;
float _GridThickness;
float _SobelGridOffset;
float _GridOffset;
float4 _GridColor;
float _GridOpacity;
float4 _HighlightColor;
float4 _ColorInnerHighlight;
float _HighlightThickness;
float _HighlightRotation;
float4 _Color01;
float4 _Color02;
float4 _Color03;
float4 _Color04;
float4 _Color05;
CBUFFER_END

// Object and Global properties
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_Map);
SAMPLER(sampler_Map);
TEXTURE2D(_EstateIDMap);
SAMPLER(sampler_EstateIDMap);

// Graph Functions

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

// 7b6d5a90df0cb86d20ecea9cb96d928e
#include "Assets/Rendering/Map/V5/MapV5.hlsl"

void Unity_Divide_float2(float2 A, float2 B, out float2 Out)
{
    Out = A / B;
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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

void Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale, out float2 Out)
{
    float2 delta = UV - Center;
    float radius = length(delta) * 2 * RadialScale;
    float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
    Out = float2(radius, angle);
}

void Unity_SampleGradient_float(Gradient Gradient, float Time, out float4 Out)
{
    float3 color = Gradient.colors[0].rgb;
    [unroll]
    for (int c = 1; c < 8; c++)
    {
        float colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
        color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
    }
#ifndef UNITY_COLORSPACE_GAMMA
    color = SRGBToLinear(color);
#endif
    float alpha = Gradient.alphas[0].x;
    [unroll]
    for (int a = 1; a < 8; a++)
    {
        float alphaPos = saturate((Time - Gradient.alphas[a - 1].y) / (Gradient.alphas[a].y - Gradient.alphas[a - 1].y)) * step(a, Gradient.alphasLength - 1);
        alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
    }
    Out = float4(color, alpha);
}

struct Bindings_SGMapHighlightGradient_e18ba2c84b9936946bedc864ac8082fc
{
    half4 uv0;
    float3 TimeParameters;
};

void SG_SGMapHighlightGradient_e18ba2c84b9936946bedc864ac8082fc(float Vector1_361221063f124ccab2ad3ec68e1f5d56, float Vector1_238d8cc490904e7288f485eda9e42fb6, float2 Vector2_925ecca986df430e90199f0d05779aec, float2 Vector2_10475809fb3d4358aef5f163b670c39e, Bindings_SGMapHighlightGradient_e18ba2c84b9936946bedc864ac8082fc IN, out float4 Out_1)
{
    Gradient _Gradient_210c51b3f7b04c4897eadb906d891900_Out_0 = NewGradient(0, 4, 2, float4(0.4386287, 0, 1, 0),float4(1, 0.6190026, 0, 0.3329976),float4(1, 0, 0.02109909, 0.6659952),float4(0.4386287, 0, 1, 1),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0),float4(0, 0, 0, 0), float2(1, 0),float2(1, 1),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0),float2(0, 0));
    float2 _Property_d796eb97e5d44414a83d93ff09d72dbb_Out_0 = Vector2_10475809fb3d4358aef5f163b670c39e;
    float2 _Add_47d9b3c4d3c64310b77647fc2b95c116_Out_2;
    Unity_Add_float2(_Property_d796eb97e5d44414a83d93ff09d72dbb_Out_0, float2(0.5, 0.5), _Add_47d9b3c4d3c64310b77647fc2b95c116_Out_2);
    float2 _Property_e98b0650494a43f285e3946aa2b1c325_Out_0 = Vector2_925ecca986df430e90199f0d05779aec;
    float2 _Divide_b3a94e8cf3914b498c267f96727b882a_Out_2;
    Unity_Divide_float2(_Property_e98b0650494a43f285e3946aa2b1c325_Out_0, float2(2, 2), _Divide_b3a94e8cf3914b498c267f96727b882a_Out_2);
    float2 _Add_aae49534148d4f798348a8e4d522323d_Out_2;
    Unity_Add_float2(_Add_47d9b3c4d3c64310b77647fc2b95c116_Out_2, _Divide_b3a94e8cf3914b498c267f96727b882a_Out_2, _Add_aae49534148d4f798348a8e4d522323d_Out_2);
    float2 _Property_802580ab3476494ca0a9d21aebe48bc3_Out_0 = Vector2_925ecca986df430e90199f0d05779aec;
    float2 _Divide_ebbc05a3ceac4c7a823a98f6d33f48ea_Out_2;
    Unity_Divide_float2(_Add_aae49534148d4f798348a8e4d522323d_Out_2, _Property_802580ab3476494ca0a9d21aebe48bc3_Out_0, _Divide_ebbc05a3ceac4c7a823a98f6d33f48ea_Out_2);
    float _Property_f9dc7e6926834a45ad16cff551756ff9_Out_0 = Vector1_361221063f124ccab2ad3ec68e1f5d56;
    float _Multiply_88775923f8e84251bee82776718bd4e5_Out_2;
    Unity_Multiply_float(IN.TimeParameters.x, _Property_f9dc7e6926834a45ad16cff551756ff9_Out_0, _Multiply_88775923f8e84251bee82776718bd4e5_Out_2);
    float2 _Rotate_c6747b48584b422a994eda42cc60c2f9_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, _Divide_ebbc05a3ceac4c7a823a98f6d33f48ea_Out_2, _Multiply_88775923f8e84251bee82776718bd4e5_Out_2, _Rotate_c6747b48584b422a994eda42cc60c2f9_Out_3);
    float2 _PolarCoordinates_184b089217934ac883f9473201fdc826_Out_4;
    Unity_PolarCoordinates_float(_Rotate_c6747b48584b422a994eda42cc60c2f9_Out_3, _Divide_ebbc05a3ceac4c7a823a98f6d33f48ea_Out_2, 1, 1, _PolarCoordinates_184b089217934ac883f9473201fdc826_Out_4);
    float2 _Add_8ffb3795e959460687a5ab4edefccffa_Out_2;
    Unity_Add_float2(_PolarCoordinates_184b089217934ac883f9473201fdc826_Out_4, float2(0, 0.5), _Add_8ffb3795e959460687a5ab4edefccffa_Out_2);
    float _Split_f7eeea1d2bbf4aa6b1bf45ceedd89f96_R_1 = _Add_8ffb3795e959460687a5ab4edefccffa_Out_2[0];
    float _Split_f7eeea1d2bbf4aa6b1bf45ceedd89f96_G_2 = _Add_8ffb3795e959460687a5ab4edefccffa_Out_2[1];
    float _Split_f7eeea1d2bbf4aa6b1bf45ceedd89f96_B_3 = 0;
    float _Split_f7eeea1d2bbf4aa6b1bf45ceedd89f96_A_4 = 0;
    float4 _SampleGradient_0a5ac9ebeb1140d3954920b890a2c657_Out_2;
    Unity_SampleGradient_float(_Gradient_210c51b3f7b04c4897eadb906d891900_Out_0, _Split_f7eeea1d2bbf4aa6b1bf45ceedd89f96_G_2, _SampleGradient_0a5ac9ebeb1140d3954920b890a2c657_Out_2);
    Out_1 = _SampleGradient_0a5ac9ebeb1140d3954920b890a2c657_Out_2;
}

// 18883b1713183b9c39d0c16e2b5ea8f4
#include "Assets/Rendering/Map/V5/MapMerger.hlsl"

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
    UnityTexture2D _Property_759d2c340f054d1299007d7a8cae23fb_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_95fd9c9218994a71a3cf1dad8aebcec6_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float _Property_a548e3af294344bca54802afb4da1a23_Out_0 = _Zoom;
    float2 _Property_b03f33d008b842f4a1c178522202a87b_Out_0 = _SizeOfTexture;
    float _Property_d5e42179367c4c9191717a3ef64bd5e9_Out_0 = _GridThickness;
    float _Property_4508fa000c0243ec932c7501be41deb9_Out_0 = _SobelGridOffset;
    float _Property_9073ba7f820f4e59af356e0a00a0ea93_Out_0 = _GridOffset;
    float2 _Property_f7aa9ab32e8b4a7290f6118745c60c75_Out_0 = _MousePosition;
    float2 _Add_eb53b4f0961d4f449a77e74cea421ef4_Out_2;
    Unity_Add_float2(_Property_f7aa9ab32e8b4a7290f6118745c60c75_Out_0, float2(0, -1), _Add_eb53b4f0961d4f449a77e74cea421ef4_Out_2);
    float _Property_8dca6d41008d4c2aa9e02ef61ebd39a5_Out_0 = _HighlightThickness;
    float4 _UV_429c8fd82d9e4da78daca218cd5f5a21_Out_0 = IN.uv0;
    float4 _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Color_8;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineIntense_15;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineFade_20;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineInner_23;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Highlight_14;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightInner_17;
    float _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightMid_18;
    Main_float(_Property_759d2c340f054d1299007d7a8cae23fb_Out_0, _Property_95fd9c9218994a71a3cf1dad8aebcec6_Out_0, 1, _Property_a548e3af294344bca54802afb4da1a23_Out_0, _Property_b03f33d008b842f4a1c178522202a87b_Out_0, _Property_d5e42179367c4c9191717a3ef64bd5e9_Out_0, _Property_4508fa000c0243ec932c7501be41deb9_Out_0, _Property_9073ba7f820f4e59af356e0a00a0ea93_Out_0, _Add_eb53b4f0961d4f449a77e74cea421ef4_Out_2, _Property_8dca6d41008d4c2aa9e02ef61ebd39a5_Out_0, (_UV_429c8fd82d9e4da78daca218cd5f5a21_Out_0.xy), _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Color_8, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineIntense_15, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineFade_20, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineInner_23, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Highlight_14, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightInner_17, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightMid_18);
    float _Property_79c88caa89af4e338f7e7ea68f2abfe9_Out_0 = _GridOpacity;
    float4 _Property_6cb9215f47264aec8fe98d47f9d40752_Out_0 = _Color01;
    float4 _Property_6c88746bf77347e5beaee0e6c4b6197c_Out_0 = _Color02;
    float4 _Property_3fadd20be5ce4fcab164168d541cd78d_Out_0 = _Color03;
    float4 _Property_d1772ed172fb487cb538ed8d98b457f8_Out_0 = _Color04;
    float4 _Property_75b273d862db40aaa1f6f6608ba0a65c_Out_0 = _Color05;
    float4 _Property_46c9cb585089445aafc6df15820ec63e_Out_0 = _GridColor;
    float4 _Property_75bcf7573198493baa092f11db5a8497_Out_0 = IsGammaSpace() ? LinearToSRGB(_HighlightColor) : _HighlightColor;
    float4 _Property_9a6afe13de3a4f8083bb32c9ce9bb179_Out_0 = _ColorInnerHighlight;
    float _Property_41ad390fd0064d389b70dac20b88d515_Out_0 = _HighlightRotation;
    float _Property_993280fd61374b1dbefbcf0eb378acc3_Out_0 = _Zoom;
    float2 _Property_31469bbf929f4dbb8254243fa27b3ffe_Out_0 = _SizeOfTexture;
    float2 _Property_77f71ca158c94f94934c3d74b6046b4b_Out_0 = _MousePosition;
    Bindings_SGMapHighlightGradient_e18ba2c84b9936946bedc864ac8082fc _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3;
    _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3.uv0 = IN.uv0;
    _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3.TimeParameters = IN.TimeParameters;
    float4 _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3_Out_1;
    SG_SGMapHighlightGradient_e18ba2c84b9936946bedc864ac8082fc(_Property_41ad390fd0064d389b70dac20b88d515_Out_0, _Property_993280fd61374b1dbefbcf0eb378acc3_Out_0, _Property_31469bbf929f4dbb8254243fa27b3ffe_Out_0, _Property_77f71ca158c94f94934c3d74b6046b4b_Out_0, _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3, _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3_Out_1);
    float4 _MapMergerCustomFunction_8772f0022d8040d2a5e4220da6fdae78_Out_0;
    MapMerger_float(_MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Color_8, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineIntense_15, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_OutlineFade_20, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_Highlight_14, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightInner_17, _MainCustomFunction_9c0b53cee7d14c63aadc52a2e1fca416_HighlightMid_18, _Property_79c88caa89af4e338f7e7ea68f2abfe9_Out_0, _Property_6cb9215f47264aec8fe98d47f9d40752_Out_0, _Property_6c88746bf77347e5beaee0e6c4b6197c_Out_0, _Property_3fadd20be5ce4fcab164168d541cd78d_Out_0, _Property_d1772ed172fb487cb538ed8d98b457f8_Out_0, _Property_75b273d862db40aaa1f6f6608ba0a65c_Out_0, _Property_46c9cb585089445aafc6df15820ec63e_Out_0, _Property_75bcf7573198493baa092f11db5a8497_Out_0, _Property_9a6afe13de3a4f8083bb32c9ce9bb179_Out_0, _SGMapHighlightGradient_84530ebe7f5246b3a1bd45c752596ed3_Out_1, _MapMergerCustomFunction_8772f0022d8040d2a5e4220da6fdae78_Out_0);
    surface.BaseColor = (_MapMergerCustomFunction_8772f0022d8040d2a5e4220da6fdae78_Out_0.xyz);
    surface.Alpha = 1;
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