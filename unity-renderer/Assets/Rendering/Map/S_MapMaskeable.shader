Shader "Unlit/S_MapMaskeable"
{
    Properties
    {
        [NoScaleOffset] _MainTex("MainTex", 2D) = "white" {}
        [NoScaleOffset]_Map("Map", 2D) = "white" {}
        [NoScaleOffset]_EstateIDMap("EstateIDMap", 2D) = "white" {}
        _SizeOfTexture("SizeOfTexture", Vector) = (512, 512, 0, 0)
        _Resolution("Resolution", Vector) = (1920, 1920, 0, 0)
        _Zoom("Zoom", Float) = 3.75
        _GridThickness("GridThickness", Range(0, 10)) = 1
        _GridColor("GridColor", Color) = (0, 0, 0, 0)
        _Color01("Color01", Color) = (1, 0, 0, 0)
        _Color02("Color02", Color) = (0.03901875, 1, 0, 0)
        _Color03("Color03", Color) = (0, 0.2896385, 1, 0)
        _Color04("Color04", Color) = (0.2189539, 0.1096921, 0.2735849, 0)
        _Color05("Color05", Color) = (0.2189539, 0.1096921, 0.2735849, 0)
        [HDR]_OverlayColor("OverlayColor", Color) = (0, 0.6943347, 1, 1)
        _MousePosition("MousePosition", Vector) = (0, 0, 0, 0)
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
float2 _Resolution;
float _Zoom;
float _GridThickness;
float4 _GridColor;
float4 _Color01;
float4 _Color02;
float4 _Color03;
float4 _Color04;
float4 _Color05;
float4 _OverlayColor;
float2 _MousePosition;
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

// 93f1b5a12dbda84fb8a3942dd5477f86
#include "Assets/Rendering/Map/V5/MapV5.hlsl"

void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
{
    Out = lerp(A, B, T);
}

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
    float3 BaseColor;
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    float4 _Property_9f678e9bf00f4ed6b34d99575970e57e_Out_0 = _Color01;
    float4 _Property_07f58d26e0f34f869946fc9c7068d7ed_Out_0 = _Color02;
    UnityTexture2D _Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_e943bfd340cf4709a76ba852685dbf55_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_98fd361fb8814411aa26fc5b1ea8ec6f_Out_0 = _Resolution;
    float _Property_3f1b5cf8b252493c8e63de84a7cb057d_Out_0 = _Zoom;
    float2 _Property_edde0349814e405a8f77a67715a72a11_Out_0 = _SizeOfTexture;
    float _Property_c4b62049d152467eb90794c337831029_Out_0 = _GridThickness;
    float2 _Property_6e173f8bb5fc495d954d69b84c789f41_Out_0 = _MousePosition;
    float2 _Add_ed3a8efc70be4ed492ffb4db5598643e_Out_2;
    Unity_Add_float2(_Property_6e173f8bb5fc495d954d69b84c789f41_Out_0, float2(0, -1), _Add_ed3a8efc70be4ed492ffb4db5598643e_Out_2);
    float4 _UV_863ce765fc6140529ac9ef62a7a9fb96_Out_0 = IN.uv0;
    float4 _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Outline_15;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14;
    Main_float(_Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0, _Property_e943bfd340cf4709a76ba852685dbf55_Out_0, _Property_98fd361fb8814411aa26fc5b1ea8ec6f_Out_0, _Property_3f1b5cf8b252493c8e63de84a7cb057d_Out_0, _Property_edde0349814e405a8f77a67715a72a11_Out_0, _Property_c4b62049d152467eb90794c337831029_Out_0, _Add_ed3a8efc70be4ed492ffb4db5598643e_Out_2, (_UV_863ce765fc6140529ac9ef62a7a9fb96_Out_0.xy), _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Outline_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14);
    float _Split_87fbd0bb230d4a0784e738b46f97a57c_R_1 = _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8[0];
    float _Split_87fbd0bb230d4a0784e738b46f97a57c_G_2 = _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8[1];
    float _Split_87fbd0bb230d4a0784e738b46f97a57c_B_3 = _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8[2];
    float _Split_87fbd0bb230d4a0784e738b46f97a57c_A_4 = _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8[3];
    float4 _Lerp_9e66aa5a846e4eddbe0e155bea8804f3_Out_3;
    Unity_Lerp_float4(_Property_9f678e9bf00f4ed6b34d99575970e57e_Out_0, _Property_07f58d26e0f34f869946fc9c7068d7ed_Out_0, (_Split_87fbd0bb230d4a0784e738b46f97a57c_R_1.xxxx), _Lerp_9e66aa5a846e4eddbe0e155bea8804f3_Out_3);
    float4 _Property_daf81264874e4526b919bc7f8bc13e47_Out_0 = _Color03;
    float4 _Lerp_2f7f64a6ea45453e970bd48cfecb8e65_Out_3;
    Unity_Lerp_float4(_Lerp_9e66aa5a846e4eddbe0e155bea8804f3_Out_3, _Property_daf81264874e4526b919bc7f8bc13e47_Out_0, (_Split_87fbd0bb230d4a0784e738b46f97a57c_G_2.xxxx), _Lerp_2f7f64a6ea45453e970bd48cfecb8e65_Out_3);
    float4 _Property_d52e9affb5734547aec6ff3a4d66f912_Out_0 = _Color04;
    float4 _Lerp_5dbe54b940cb4c5f90ac1314933cb8f2_Out_3;
    Unity_Lerp_float4(_Lerp_2f7f64a6ea45453e970bd48cfecb8e65_Out_3, _Property_d52e9affb5734547aec6ff3a4d66f912_Out_0, (_Split_87fbd0bb230d4a0784e738b46f97a57c_B_3.xxxx), _Lerp_5dbe54b940cb4c5f90ac1314933cb8f2_Out_3);
    float4 _Property_db3ed196c533417f940ca1a604af234d_Out_0 = _Color05;
    float4 _Lerp_8f6d30cfd54f4903a5949b9b51e20250_Out_3;
    Unity_Lerp_float4(_Lerp_5dbe54b940cb4c5f90ac1314933cb8f2_Out_3, _Property_db3ed196c533417f940ca1a604af234d_Out_0, (_Split_87fbd0bb230d4a0784e738b46f97a57c_A_4.xxxx), _Lerp_8f6d30cfd54f4903a5949b9b51e20250_Out_3);
    float4 _Property_afe137a643bd4ee797e36d52b401ff1f_Out_0 = _GridColor;
    float4 _Lerp_9ba2170387c141e28ff9cb1a7263d01a_Out_3;
    Unity_Lerp_float4(_Lerp_8f6d30cfd54f4903a5949b9b51e20250_Out_3, _Property_afe137a643bd4ee797e36d52b401ff1f_Out_0, (_MainCustomFunction_fc80707c41a14d5e95900dad01640841_Outline_15.xxxx), _Lerp_9ba2170387c141e28ff9cb1a7263d01a_Out_3);
    float4 _Property_f652c59ef3b44017916bb0f0279111e1_Out_0 = IsGammaSpace() ? LinearToSRGB(_OverlayColor) : _OverlayColor;
    float _Split_bdfe6245792145ca8af4f1fcfde9056f_R_1 = _Property_f652c59ef3b44017916bb0f0279111e1_Out_0[0];
    float _Split_bdfe6245792145ca8af4f1fcfde9056f_G_2 = _Property_f652c59ef3b44017916bb0f0279111e1_Out_0[1];
    float _Split_bdfe6245792145ca8af4f1fcfde9056f_B_3 = _Property_f652c59ef3b44017916bb0f0279111e1_Out_0[2];
    float _Split_bdfe6245792145ca8af4f1fcfde9056f_A_4 = _Property_f652c59ef3b44017916bb0f0279111e1_Out_0[3];
    float _Multiply_c7ffa99a71ce416481cd9ef6023cb14f_Out_2;
    Unity_Multiply_float(_Split_bdfe6245792145ca8af4f1fcfde9056f_A_4, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14, _Multiply_c7ffa99a71ce416481cd9ef6023cb14f_Out_2);
    float4 _Lerp_34a3d0bbb09048e0ab33526147848616_Out_3;
    Unity_Lerp_float4(_Lerp_9ba2170387c141e28ff9cb1a7263d01a_Out_3, _Property_f652c59ef3b44017916bb0f0279111e1_Out_0, (_Multiply_c7ffa99a71ce416481cd9ef6023cb14f_Out_2.xxxx), _Lerp_34a3d0bbb09048e0ab33526147848616_Out_3);
    surface.BaseColor = (_Lerp_34a3d0bbb09048e0ab33526147848616_Out_3.xyz);
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
float2 _Resolution;
float _Zoom;
float _GridThickness;
float4 _GridColor;
float4 _Color01;
float4 _Color02;
float4 _Color03;
float4 _Color04;
float4 _Color05;
float4 _OverlayColor;
float2 _MousePosition;
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

// 93f1b5a12dbda84fb8a3942dd5477f86
#include "Assets/Rendering/Map/V5/MapV5.hlsl"

void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
{
    Out = lerp(A, B, T);
}

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
    float3 BaseColor;
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    float4 _Property_9f678e9bf00f4ed6b34d99575970e57e_Out_0 = _Color01;
    float4 _Property_07f58d26e0f34f869946fc9c7068d7ed_Out_0 = _Color02;
    UnityTexture2D _Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0 = UnityBuildTexture2DStructNoScale(_Map);
    UnityTexture2D _Property_e943bfd340cf4709a76ba852685dbf55_Out_0 = UnityBuildTexture2DStructNoScale(_EstateIDMap);
    float2 _Property_98fd361fb8814411aa26fc5b1ea8ec6f_Out_0 = _Resolution;
    float _Property_3f1b5cf8b252493c8e63de84a7cb057d_Out_0 = _Zoom;
    float2 _Property_edde0349814e405a8f77a67715a72a11_Out_0 = _SizeOfTexture;
    float _Property_c4b62049d152467eb90794c337831029_Out_0 = _GridThickness;
    float2 _Property_6e173f8bb5fc495d954d69b84c789f41_Out_0 = _MousePosition;
    float2 _Add_ed3a8efc70be4ed492ffb4db5598643e_Out_2;
    Unity_Add_float2(_Property_6e173f8bb5fc495d954d69b84c789f41_Out_0, float2(0, -1), _Add_ed3a8efc70be4ed492ffb4db5598643e_Out_2);
    float4 _UV_863ce765fc6140529ac9ef62a7a9fb96_Out_0 = IN.uv0;
    float4 _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Outline_15;
    float _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14;
    Main_float(_Property_7ee93798ebb2444388c5de6c7291b1ee_Out_0, _Property_e943bfd340cf4709a76ba852685dbf55_Out_0, _Property_98fd361fb8814411aa26fc5b1ea8ec6f_Out_0, _Property_3f1b5cf8b252493c8e63de84a7cb057d_Out_0, _Property_edde0349814e405a8f77a67715a72a11_Out_0, _Property_c4b62049d152467eb90794c337831029_Out_0, _Add_ed3a8efc70be4ed492ffb4db5598643e_Out_2, (_UV_863ce765fc6140529ac9ef62a7a9fb96_Out_0.xy), _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Outline_15, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14);
    float _Split_87fbd0bb230d4a0784e738b46f97a57c_R_1 = _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8[0];
    float _Split_87fbd0bb230d4a0784e738b46f97a57c_G_2 = _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8[1];
    float _Split_87fbd0bb230d4a0784e738b46f97a57c_B_3 = _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8[2];
    float _Split_87fbd0bb230d4a0784e738b46f97a57c_A_4 = _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Color_8[3];
    float4 _Lerp_9e66aa5a846e4eddbe0e155bea8804f3_Out_3;
    Unity_Lerp_float4(_Property_9f678e9bf00f4ed6b34d99575970e57e_Out_0, _Property_07f58d26e0f34f869946fc9c7068d7ed_Out_0, (_Split_87fbd0bb230d4a0784e738b46f97a57c_R_1.xxxx), _Lerp_9e66aa5a846e4eddbe0e155bea8804f3_Out_3);
    float4 _Property_daf81264874e4526b919bc7f8bc13e47_Out_0 = _Color03;
    float4 _Lerp_2f7f64a6ea45453e970bd48cfecb8e65_Out_3;
    Unity_Lerp_float4(_Lerp_9e66aa5a846e4eddbe0e155bea8804f3_Out_3, _Property_daf81264874e4526b919bc7f8bc13e47_Out_0, (_Split_87fbd0bb230d4a0784e738b46f97a57c_G_2.xxxx), _Lerp_2f7f64a6ea45453e970bd48cfecb8e65_Out_3);
    float4 _Property_d52e9affb5734547aec6ff3a4d66f912_Out_0 = _Color04;
    float4 _Lerp_5dbe54b940cb4c5f90ac1314933cb8f2_Out_3;
    Unity_Lerp_float4(_Lerp_2f7f64a6ea45453e970bd48cfecb8e65_Out_3, _Property_d52e9affb5734547aec6ff3a4d66f912_Out_0, (_Split_87fbd0bb230d4a0784e738b46f97a57c_B_3.xxxx), _Lerp_5dbe54b940cb4c5f90ac1314933cb8f2_Out_3);
    float4 _Property_db3ed196c533417f940ca1a604af234d_Out_0 = _Color05;
    float4 _Lerp_8f6d30cfd54f4903a5949b9b51e20250_Out_3;
    Unity_Lerp_float4(_Lerp_5dbe54b940cb4c5f90ac1314933cb8f2_Out_3, _Property_db3ed196c533417f940ca1a604af234d_Out_0, (_Split_87fbd0bb230d4a0784e738b46f97a57c_A_4.xxxx), _Lerp_8f6d30cfd54f4903a5949b9b51e20250_Out_3);
    float4 _Property_afe137a643bd4ee797e36d52b401ff1f_Out_0 = _GridColor;
    float4 _Lerp_9ba2170387c141e28ff9cb1a7263d01a_Out_3;
    Unity_Lerp_float4(_Lerp_8f6d30cfd54f4903a5949b9b51e20250_Out_3, _Property_afe137a643bd4ee797e36d52b401ff1f_Out_0, (_MainCustomFunction_fc80707c41a14d5e95900dad01640841_Outline_15.xxxx), _Lerp_9ba2170387c141e28ff9cb1a7263d01a_Out_3);
    float4 _Property_f652c59ef3b44017916bb0f0279111e1_Out_0 = IsGammaSpace() ? LinearToSRGB(_OverlayColor) : _OverlayColor;
    float _Split_bdfe6245792145ca8af4f1fcfde9056f_R_1 = _Property_f652c59ef3b44017916bb0f0279111e1_Out_0[0];
    float _Split_bdfe6245792145ca8af4f1fcfde9056f_G_2 = _Property_f652c59ef3b44017916bb0f0279111e1_Out_0[1];
    float _Split_bdfe6245792145ca8af4f1fcfde9056f_B_3 = _Property_f652c59ef3b44017916bb0f0279111e1_Out_0[2];
    float _Split_bdfe6245792145ca8af4f1fcfde9056f_A_4 = _Property_f652c59ef3b44017916bb0f0279111e1_Out_0[3];
    float _Multiply_c7ffa99a71ce416481cd9ef6023cb14f_Out_2;
    Unity_Multiply_float(_Split_bdfe6245792145ca8af4f1fcfde9056f_A_4, _MainCustomFunction_fc80707c41a14d5e95900dad01640841_Highlight_14, _Multiply_c7ffa99a71ce416481cd9ef6023cb14f_Out_2);
    float4 _Lerp_34a3d0bbb09048e0ab33526147848616_Out_3;
    Unity_Lerp_float4(_Lerp_9ba2170387c141e28ff9cb1a7263d01a_Out_3, _Property_f652c59ef3b44017916bb0f0279111e1_Out_0, (_Multiply_c7ffa99a71ce416481cd9ef6023cb14f_Out_2.xxxx), _Lerp_34a3d0bbb09048e0ab33526147848616_Out_3);
    surface.BaseColor = (_Lerp_34a3d0bbb09048e0ab33526147848616_Out_3.xyz);
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