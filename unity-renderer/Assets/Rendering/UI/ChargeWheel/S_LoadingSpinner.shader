Shader "S_LoadingSpinner"
{
    Properties
    {
        _color01("Color01", Color) = (1, 0, 0.2605209, 1)
        _fillHead("FillHead", Range(0, 1)) = 0.5
        _fillTail("FillTail", Range(0, 1)) = 0
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

    void Unity_Add_float(float A, float B, out float Out)
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

    void Unity_Floor_float(float In, out float Out)
    {
        Out = floor(In);
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

    void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
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
    float _Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2;
    Unity_Subtract_float(_Subtract_0f65a36c972844a3900a158bb05718ec_Out_2, _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2, _Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_1434307ef67b463a952ae990b52d0f19_Out_2;
    Unity_Add_float(_Subtract_0f65a36c972844a3900a158bb05718ec_Out_2, _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2, _Add_1434307ef67b463a952ae990b52d0f19_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_441455d41e894365b4544ab412ff17da_Out_0 = float2(_Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2, _Add_1434307ef67b463a952ae990b52d0f19_Out_2);
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
    float _Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2;
    Unity_Subtract_float(_Float_cead0e992bc449df8d9dce54181339c6_Out_0, _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2, _Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_61ac46313d39426f9d62990bead131b2_Out_2;
    Unity_Add_float(_Float_cead0e992bc449df8d9dce54181339c6_Out_0, _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2, _Add_61ac46313d39426f9d62990bead131b2_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_6fd7dbbacf17437ba05862d85169dead_Out_0 = float2(_Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2, _Add_61ac46313d39426f9d62990bead131b2_Out_2);
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
    float _Property_af98836cbbef446e9aee7df733d10232_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_732244acab914817bb9e15c35801d8c7_Out_2;
    Unity_Add_float(_Property_af98836cbbef446e9aee7df733d10232_Out_0, 0.5, _Add_732244acab914817bb9e15c35801d8c7_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_ec60eb5c409548edaa48abe5d2c12f9c_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_ec60eb5c409548edaa48abe5d2c12f9c_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_9fb9c34aa4bf422c9fe5178185c78e26_R_1 = _Rotate_ec60eb5c409548edaa48abe5d2c12f9c_Out_3[0];
    float _Split_9fb9c34aa4bf422c9fe5178185c78e26_G_2 = _Rotate_ec60eb5c409548edaa48abe5d2c12f9c_Out_3[1];
    float _Split_9fb9c34aa4bf422c9fe5178185c78e26_B_3 = 0;
    float _Split_9fb9c34aa4bf422c9fe5178185c78e26_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_73079bdf7d90403d943b12ead44370ee_Out_1;
    Unity_OneMinus_float(_Split_9fb9c34aa4bf422c9fe5178185c78e26_R_1, _OneMinus_73079bdf7d90403d943b12ead44370ee_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_85e2b79a04974d85af6db719a61b2bb1_Out_0 = float2(_OneMinus_73079bdf7d90403d943b12ead44370ee_Out_1, _Split_9fb9c34aa4bf422c9fe5178185c78e26_G_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _PolarCoordinates_95aa4af723134db092c515d2d7ff7e15_Out_4;
    Unity_PolarCoordinates_float(_Vector2_85e2b79a04974d85af6db719a61b2bb1_Out_0, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_95aa4af723134db092c515d2d7ff7e15_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_8bafe52b7e234e6fb13814ef126f5947_R_1 = _PolarCoordinates_95aa4af723134db092c515d2d7ff7e15_Out_4[0];
    float _Split_8bafe52b7e234e6fb13814ef126f5947_G_2 = _PolarCoordinates_95aa4af723134db092c515d2d7ff7e15_Out_4[1];
    float _Split_8bafe52b7e234e6fb13814ef126f5947_B_3 = 0;
    float _Split_8bafe52b7e234e6fb13814ef126f5947_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_39339fee8018448fbc97766c1bb5bb4a_Out_2;
    Unity_Add_float(_Add_732244acab914817bb9e15c35801d8c7_Out_2, _Split_8bafe52b7e234e6fb13814ef126f5947_G_2, _Add_39339fee8018448fbc97766c1bb5bb4a_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Floor_2860b0bb5c4c48f6b4998504bc794bfa_Out_1;
    Unity_Floor_float(_Add_39339fee8018448fbc97766c1bb5bb4a_Out_2, _Floor_2860b0bb5c4c48f6b4998504bc794bfa_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_db1d5b479d25461981cd82d02522f3c5_Out_2;
    Unity_Multiply_float(_Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2, _Floor_2860b0bb5c4c48f6b4998504bc794bfa_Out_1, _Multiply_db1d5b479d25461981cd82d02522f3c5_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_e2afa82cdb8042e1bf6338de12a6c804_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1;
    Unity_OneMinus_float(_Property_e2afa82cdb8042e1bf6338de12a6c804_Out_0, _OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_b93f9ab8c651486db3a3347c4ec23882_Out_2;
    Unity_Add_float(_OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1, 0.5, _Add_b93f9ab8c651486db3a3347c4ec23882_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_5dba8a67556c48349a348a930402a05a_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_5dba8a67556c48349a348a930402a05a_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_cc8bfb9cc46145c185c3e10ab55d6c70_R_1 = _Rotate_5dba8a67556c48349a348a930402a05a_Out_3[0];
    float _Split_cc8bfb9cc46145c185c3e10ab55d6c70_G_2 = _Rotate_5dba8a67556c48349a348a930402a05a_Out_3[1];
    float _Split_cc8bfb9cc46145c185c3e10ab55d6c70_B_3 = 0;
    float _Split_cc8bfb9cc46145c185c3e10ab55d6c70_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_bb1873687fa048fea73c7e8408ca892d_Out_1;
    Unity_OneMinus_float(_Split_cc8bfb9cc46145c185c3e10ab55d6c70_R_1, _OneMinus_bb1873687fa048fea73c7e8408ca892d_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_e1d9c1975ecb4328b010268b356dfc39_Out_0 = float2(_OneMinus_bb1873687fa048fea73c7e8408ca892d_Out_1, _Split_cc8bfb9cc46145c185c3e10ab55d6c70_G_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _PolarCoordinates_72db84af8f5c4fb8bd980f4e4de7ab98_Out_4;
    Unity_PolarCoordinates_float(_Vector2_e1d9c1975ecb4328b010268b356dfc39_Out_0, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_72db84af8f5c4fb8bd980f4e4de7ab98_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_580906f4c3ae4101bb68014c5a4178f3_R_1 = _PolarCoordinates_72db84af8f5c4fb8bd980f4e4de7ab98_Out_4[0];
    float _Split_580906f4c3ae4101bb68014c5a4178f3_G_2 = _PolarCoordinates_72db84af8f5c4fb8bd980f4e4de7ab98_Out_4[1];
    float _Split_580906f4c3ae4101bb68014c5a4178f3_B_3 = 0;
    float _Split_580906f4c3ae4101bb68014c5a4178f3_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Subtract_df1cdd19beb14e0f8f21f37aae93850b_Out_2;
    Unity_Subtract_float(_Add_b93f9ab8c651486db3a3347c4ec23882_Out_2, _Split_580906f4c3ae4101bb68014c5a4178f3_G_2, _Subtract_df1cdd19beb14e0f8f21f37aae93850b_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Floor_a7451e3527e043e791a56b948091e193_Out_1;
    Unity_Floor_float(_Subtract_df1cdd19beb14e0f8f21f37aae93850b_Out_2, _Floor_a7451e3527e043e791a56b948091e193_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_a97d8f72b11f4440bfabc5b83c64eeda_Out_2;
    Unity_Multiply_float(_Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2, _Floor_a7451e3527e043e791a56b948091e193_Out_1, _Multiply_a97d8f72b11f4440bfabc5b83c64eeda_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2;
    Unity_Multiply_float(_Multiply_db1d5b479d25461981cd82d02522f3c5_Out_2, _Multiply_a97d8f72b11f4440bfabc5b83c64eeda_Out_2, _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_14e672a08ae1466cbd15c2398b14ccb0_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_14e672a08ae1466cbd15c2398b14ccb0_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_90f741b6e2fc4227917d9d7984f4be4d_R_1 = _Rotate_14e672a08ae1466cbd15c2398b14ccb0_Out_3[0];
    float _Split_90f741b6e2fc4227917d9d7984f4be4d_G_2 = _Rotate_14e672a08ae1466cbd15c2398b14ccb0_Out_3[1];
    float _Split_90f741b6e2fc4227917d9d7984f4be4d_B_3 = 0;
    float _Split_90f741b6e2fc4227917d9d7984f4be4d_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_df5c05a43911445789ad9581c7033e01_Out_1;
    Unity_OneMinus_float(_Split_90f741b6e2fc4227917d9d7984f4be4d_R_1, _OneMinus_df5c05a43911445789ad9581c7033e01_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_29404d9d688d40e38266e87911225842_Out_0 = float2(_OneMinus_df5c05a43911445789ad9581c7033e01_Out_1, _Split_90f741b6e2fc4227917d9d7984f4be4d_G_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_46c11fb595824a669fb3c91101bb6966_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_d74222cfaa1f4ed2a9949ac7d8c48e0a_Out_2;
    Unity_Multiply_float(_Property_46c11fb595824a669fb3c91101bb6966_Out_0, 360, _Multiply_d74222cfaa1f4ed2a9949ac7d8c48e0a_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_308147ec599e413b9040bf4af1b98ebd_Out_3;
    Unity_Rotate_Degrees_float(_Vector2_29404d9d688d40e38266e87911225842_Out_0, float2 (0.5, 0.5), _Multiply_d74222cfaa1f4ed2a9949ac7d8c48e0a_Out_2, _Rotate_308147ec599e413b9040bf4af1b98ebd_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_5468e17a0c1f4b3e8c6ba37d7812cc28_Out_0 = Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_07daa50536f64b15908ffc839fdc18af_Out_2;
    Unity_Multiply_float(_Property_5468e17a0c1f4b3e8c6ba37d7812cc28_Out_0, 0.5, _Multiply_07daa50536f64b15908ffc839fdc18af_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_f03d766f84764f1e895efade34bb1e77_Out_2;
    Unity_Multiply_float(_Multiply_07daa50536f64b15908ffc839fdc18af_Out_2, -0.5, _Multiply_f03d766f84764f1e895efade34bb1e77_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_615a5aaa041f4245be4e0609576bcb95_Out_2;
    Unity_Add_float(0.5, _Multiply_f03d766f84764f1e895efade34bb1e77_Out_2, _Add_615a5aaa041f4245be4e0609576bcb95_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_a5dc5a50dd444c22bcfa607a284d4138_Out_0 = float2(0, _Add_615a5aaa041f4245be4e0609576bcb95_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _TilingAndOffset_babaa12e12d2477c88d0a972fd00ad02_Out_3;
    Unity_TilingAndOffset_float(_Rotate_308147ec599e413b9040bf4af1b98ebd_Out_3, float2 (1, 1), _Vector2_a5dc5a50dd444c22bcfa607a284d4138_Out_0, _TilingAndOffset_babaa12e12d2477c88d0a972fd00ad02_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Ellipse_45ddcd19743f42eabe5bc47fbba9479d_Out_4;
    Unity_Ellipse_float(_TilingAndOffset_babaa12e12d2477c88d0a972fd00ad02_Out_3, _Multiply_07daa50536f64b15908ffc839fdc18af_Out_2, _Multiply_07daa50536f64b15908ffc839fdc18af_Out_2, _Ellipse_45ddcd19743f42eabe5bc47fbba9479d_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_9afa425d6b4a4484b424ad62e647beac_Out_2;
    Unity_Add_float(_Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2, _Ellipse_45ddcd19743f42eabe5bc47fbba9479d_Out_4, _Add_9afa425d6b4a4484b424ad62e647beac_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_85abf48e7a7642a298ae62bd5b69312c_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_85abf48e7a7642a298ae62bd5b69312c_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_eafaa39bd1cd4f58ae2887cabe9ddeae_R_1 = _Rotate_85abf48e7a7642a298ae62bd5b69312c_Out_3[0];
    float _Split_eafaa39bd1cd4f58ae2887cabe9ddeae_G_2 = _Rotate_85abf48e7a7642a298ae62bd5b69312c_Out_3[1];
    float _Split_eafaa39bd1cd4f58ae2887cabe9ddeae_B_3 = 0;
    float _Split_eafaa39bd1cd4f58ae2887cabe9ddeae_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_97cbd884ea4e439c9795d45e17a706da_Out_1;
    Unity_OneMinus_float(_Split_eafaa39bd1cd4f58ae2887cabe9ddeae_R_1, _OneMinus_97cbd884ea4e439c9795d45e17a706da_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_1a8f886502744979bbfc94d61ad2aa8a_Out_0 = float2(_OneMinus_97cbd884ea4e439c9795d45e17a706da_Out_1, _Split_eafaa39bd1cd4f58ae2887cabe9ddeae_G_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_6ab291f666944af7b11fedf8e0ffaca6_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_c336baf27c544bf28c5a96d24ff3f91d_Out_2;
    Unity_Multiply_float(_Property_6ab291f666944af7b11fedf8e0ffaca6_Out_0, 360, _Multiply_c336baf27c544bf28c5a96d24ff3f91d_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_3568025fdfca4f488b7f1a7e9155908b_Out_3;
    Unity_Rotate_Degrees_float(_Vector2_1a8f886502744979bbfc94d61ad2aa8a_Out_0, float2 (0.5, 0.5), _Multiply_c336baf27c544bf28c5a96d24ff3f91d_Out_2, _Rotate_3568025fdfca4f488b7f1a7e9155908b_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_e7355a66a9e7409b8734b75787393865_Out_0 = Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_62e029343a3745e48a1e522d8dd57952_Out_2;
    Unity_Multiply_float(_Property_e7355a66a9e7409b8734b75787393865_Out_0, 0.5, _Multiply_62e029343a3745e48a1e522d8dd57952_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_ccff550cec024527b7ebdb1852692247_Out_2;
    Unity_Multiply_float(_Multiply_62e029343a3745e48a1e522d8dd57952_Out_2, -0.5, _Multiply_ccff550cec024527b7ebdb1852692247_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_16289e60be0e439ab3c1f97df0520a91_Out_2;
    Unity_Add_float(0.5, _Multiply_ccff550cec024527b7ebdb1852692247_Out_2, _Add_16289e60be0e439ab3c1f97df0520a91_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_d121d160067943c2a454b5fe8bb3456e_Out_0 = float2(0, _Add_16289e60be0e439ab3c1f97df0520a91_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _TilingAndOffset_a032dfbb58b244c59024ec234ae15b70_Out_3;
    Unity_TilingAndOffset_float(_Rotate_3568025fdfca4f488b7f1a7e9155908b_Out_3, float2 (1, 1), _Vector2_d121d160067943c2a454b5fe8bb3456e_Out_0, _TilingAndOffset_a032dfbb58b244c59024ec234ae15b70_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Ellipse_9767eca10871423db0024289beafb443_Out_4;
    Unity_Ellipse_float(_TilingAndOffset_a032dfbb58b244c59024ec234ae15b70_Out_3, _Multiply_62e029343a3745e48a1e522d8dd57952_Out_2, _Multiply_62e029343a3745e48a1e522d8dd57952_Out_2, _Ellipse_9767eca10871423db0024289beafb443_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2;
    Unity_Add_float(_Add_9afa425d6b4a4484b424ad62e647beac_Out_2, _Ellipse_9767eca10871423db0024289beafb443_Out_4, _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #if defined(ENUM_173FCA8E617C4725B6502C6FBE756CEC_SHARP)
    float _RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0 = _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2;
    #else
    float _RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0 = _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2;
    #endif
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Saturate_737696eb34c241d6b897441762787779_Out_1;
    Unity_Saturate_float(_RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0, _Saturate_737696eb34c241d6b897441762787779_Out_1);
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
    Unity_Multiply_float((_Saturate_737696eb34c241d6b897441762787779_Out_1.xxxx), _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0, _Multiply_83aae9860a96466b9d32a13ab9801b27_Out_2);
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
#include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteUnlitPass.hlsl"

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

void Unity_Add_float(float A, float B, out float Out)
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

void Unity_Floor_float(float In, out float Out)
{
    Out = floor(In);
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

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
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
    float _Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2;
    Unity_Subtract_float(_Subtract_0f65a36c972844a3900a158bb05718ec_Out_2, _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2, _Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_1434307ef67b463a952ae990b52d0f19_Out_2;
    Unity_Add_float(_Subtract_0f65a36c972844a3900a158bb05718ec_Out_2, _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2, _Add_1434307ef67b463a952ae990b52d0f19_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_441455d41e894365b4544ab412ff17da_Out_0 = float2(_Subtract_c1726a9cdc5d419abfdfbb6aa5c23ad6_Out_2, _Add_1434307ef67b463a952ae990b52d0f19_Out_2);
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
    float _Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2;
    Unity_Subtract_float(_Float_cead0e992bc449df8d9dce54181339c6_Out_0, _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2, _Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_61ac46313d39426f9d62990bead131b2_Out_2;
    Unity_Add_float(_Float_cead0e992bc449df8d9dce54181339c6_Out_0, _Multiply_0a7dd19bd7084c4fbd18c009c9794a18_Out_2, _Add_61ac46313d39426f9d62990bead131b2_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_6fd7dbbacf17437ba05862d85169dead_Out_0 = float2(_Subtract_0e1461514aca4bbe90575f0e0c8ae176_Out_2, _Add_61ac46313d39426f9d62990bead131b2_Out_2);
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
    float _Property_af98836cbbef446e9aee7df733d10232_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_732244acab914817bb9e15c35801d8c7_Out_2;
    Unity_Add_float(_Property_af98836cbbef446e9aee7df733d10232_Out_0, 0.5, _Add_732244acab914817bb9e15c35801d8c7_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_ec60eb5c409548edaa48abe5d2c12f9c_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_ec60eb5c409548edaa48abe5d2c12f9c_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_9fb9c34aa4bf422c9fe5178185c78e26_R_1 = _Rotate_ec60eb5c409548edaa48abe5d2c12f9c_Out_3[0];
    float _Split_9fb9c34aa4bf422c9fe5178185c78e26_G_2 = _Rotate_ec60eb5c409548edaa48abe5d2c12f9c_Out_3[1];
    float _Split_9fb9c34aa4bf422c9fe5178185c78e26_B_3 = 0;
    float _Split_9fb9c34aa4bf422c9fe5178185c78e26_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_73079bdf7d90403d943b12ead44370ee_Out_1;
    Unity_OneMinus_float(_Split_9fb9c34aa4bf422c9fe5178185c78e26_R_1, _OneMinus_73079bdf7d90403d943b12ead44370ee_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_85e2b79a04974d85af6db719a61b2bb1_Out_0 = float2(_OneMinus_73079bdf7d90403d943b12ead44370ee_Out_1, _Split_9fb9c34aa4bf422c9fe5178185c78e26_G_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _PolarCoordinates_95aa4af723134db092c515d2d7ff7e15_Out_4;
    Unity_PolarCoordinates_float(_Vector2_85e2b79a04974d85af6db719a61b2bb1_Out_0, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_95aa4af723134db092c515d2d7ff7e15_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_8bafe52b7e234e6fb13814ef126f5947_R_1 = _PolarCoordinates_95aa4af723134db092c515d2d7ff7e15_Out_4[0];
    float _Split_8bafe52b7e234e6fb13814ef126f5947_G_2 = _PolarCoordinates_95aa4af723134db092c515d2d7ff7e15_Out_4[1];
    float _Split_8bafe52b7e234e6fb13814ef126f5947_B_3 = 0;
    float _Split_8bafe52b7e234e6fb13814ef126f5947_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_39339fee8018448fbc97766c1bb5bb4a_Out_2;
    Unity_Add_float(_Add_732244acab914817bb9e15c35801d8c7_Out_2, _Split_8bafe52b7e234e6fb13814ef126f5947_G_2, _Add_39339fee8018448fbc97766c1bb5bb4a_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Floor_2860b0bb5c4c48f6b4998504bc794bfa_Out_1;
    Unity_Floor_float(_Add_39339fee8018448fbc97766c1bb5bb4a_Out_2, _Floor_2860b0bb5c4c48f6b4998504bc794bfa_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_db1d5b479d25461981cd82d02522f3c5_Out_2;
    Unity_Multiply_float(_Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2, _Floor_2860b0bb5c4c48f6b4998504bc794bfa_Out_1, _Multiply_db1d5b479d25461981cd82d02522f3c5_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_e2afa82cdb8042e1bf6338de12a6c804_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1;
    Unity_OneMinus_float(_Property_e2afa82cdb8042e1bf6338de12a6c804_Out_0, _OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_b93f9ab8c651486db3a3347c4ec23882_Out_2;
    Unity_Add_float(_OneMinus_405b992ad2e244728ee493a4d84ebbdf_Out_1, 0.5, _Add_b93f9ab8c651486db3a3347c4ec23882_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_5dba8a67556c48349a348a930402a05a_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_5dba8a67556c48349a348a930402a05a_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_cc8bfb9cc46145c185c3e10ab55d6c70_R_1 = _Rotate_5dba8a67556c48349a348a930402a05a_Out_3[0];
    float _Split_cc8bfb9cc46145c185c3e10ab55d6c70_G_2 = _Rotate_5dba8a67556c48349a348a930402a05a_Out_3[1];
    float _Split_cc8bfb9cc46145c185c3e10ab55d6c70_B_3 = 0;
    float _Split_cc8bfb9cc46145c185c3e10ab55d6c70_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_bb1873687fa048fea73c7e8408ca892d_Out_1;
    Unity_OneMinus_float(_Split_cc8bfb9cc46145c185c3e10ab55d6c70_R_1, _OneMinus_bb1873687fa048fea73c7e8408ca892d_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_e1d9c1975ecb4328b010268b356dfc39_Out_0 = float2(_OneMinus_bb1873687fa048fea73c7e8408ca892d_Out_1, _Split_cc8bfb9cc46145c185c3e10ab55d6c70_G_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _PolarCoordinates_72db84af8f5c4fb8bd980f4e4de7ab98_Out_4;
    Unity_PolarCoordinates_float(_Vector2_e1d9c1975ecb4328b010268b356dfc39_Out_0, float2 (0.5, 0.5), 1, 1, _PolarCoordinates_72db84af8f5c4fb8bd980f4e4de7ab98_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_580906f4c3ae4101bb68014c5a4178f3_R_1 = _PolarCoordinates_72db84af8f5c4fb8bd980f4e4de7ab98_Out_4[0];
    float _Split_580906f4c3ae4101bb68014c5a4178f3_G_2 = _PolarCoordinates_72db84af8f5c4fb8bd980f4e4de7ab98_Out_4[1];
    float _Split_580906f4c3ae4101bb68014c5a4178f3_B_3 = 0;
    float _Split_580906f4c3ae4101bb68014c5a4178f3_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Subtract_df1cdd19beb14e0f8f21f37aae93850b_Out_2;
    Unity_Subtract_float(_Add_b93f9ab8c651486db3a3347c4ec23882_Out_2, _Split_580906f4c3ae4101bb68014c5a4178f3_G_2, _Subtract_df1cdd19beb14e0f8f21f37aae93850b_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Floor_a7451e3527e043e791a56b948091e193_Out_1;
    Unity_Floor_float(_Subtract_df1cdd19beb14e0f8f21f37aae93850b_Out_2, _Floor_a7451e3527e043e791a56b948091e193_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_a97d8f72b11f4440bfabc5b83c64eeda_Out_2;
    Unity_Multiply_float(_Multiply_d212f0d8ac86494395fc8d1f4106913d_Out_2, _Floor_a7451e3527e043e791a56b948091e193_Out_1, _Multiply_a97d8f72b11f4440bfabc5b83c64eeda_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2;
    Unity_Multiply_float(_Multiply_db1d5b479d25461981cd82d02522f3c5_Out_2, _Multiply_a97d8f72b11f4440bfabc5b83c64eeda_Out_2, _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_14e672a08ae1466cbd15c2398b14ccb0_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_14e672a08ae1466cbd15c2398b14ccb0_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_90f741b6e2fc4227917d9d7984f4be4d_R_1 = _Rotate_14e672a08ae1466cbd15c2398b14ccb0_Out_3[0];
    float _Split_90f741b6e2fc4227917d9d7984f4be4d_G_2 = _Rotate_14e672a08ae1466cbd15c2398b14ccb0_Out_3[1];
    float _Split_90f741b6e2fc4227917d9d7984f4be4d_B_3 = 0;
    float _Split_90f741b6e2fc4227917d9d7984f4be4d_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_df5c05a43911445789ad9581c7033e01_Out_1;
    Unity_OneMinus_float(_Split_90f741b6e2fc4227917d9d7984f4be4d_R_1, _OneMinus_df5c05a43911445789ad9581c7033e01_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_29404d9d688d40e38266e87911225842_Out_0 = float2(_OneMinus_df5c05a43911445789ad9581c7033e01_Out_1, _Split_90f741b6e2fc4227917d9d7984f4be4d_G_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_46c11fb595824a669fb3c91101bb6966_Out_0 = _fillHead;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_d74222cfaa1f4ed2a9949ac7d8c48e0a_Out_2;
    Unity_Multiply_float(_Property_46c11fb595824a669fb3c91101bb6966_Out_0, 360, _Multiply_d74222cfaa1f4ed2a9949ac7d8c48e0a_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_308147ec599e413b9040bf4af1b98ebd_Out_3;
    Unity_Rotate_Degrees_float(_Vector2_29404d9d688d40e38266e87911225842_Out_0, float2 (0.5, 0.5), _Multiply_d74222cfaa1f4ed2a9949ac7d8c48e0a_Out_2, _Rotate_308147ec599e413b9040bf4af1b98ebd_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_5468e17a0c1f4b3e8c6ba37d7812cc28_Out_0 = Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_07daa50536f64b15908ffc839fdc18af_Out_2;
    Unity_Multiply_float(_Property_5468e17a0c1f4b3e8c6ba37d7812cc28_Out_0, 0.5, _Multiply_07daa50536f64b15908ffc839fdc18af_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_f03d766f84764f1e895efade34bb1e77_Out_2;
    Unity_Multiply_float(_Multiply_07daa50536f64b15908ffc839fdc18af_Out_2, -0.5, _Multiply_f03d766f84764f1e895efade34bb1e77_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_615a5aaa041f4245be4e0609576bcb95_Out_2;
    Unity_Add_float(0.5, _Multiply_f03d766f84764f1e895efade34bb1e77_Out_2, _Add_615a5aaa041f4245be4e0609576bcb95_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_a5dc5a50dd444c22bcfa607a284d4138_Out_0 = float2(0, _Add_615a5aaa041f4245be4e0609576bcb95_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _TilingAndOffset_babaa12e12d2477c88d0a972fd00ad02_Out_3;
    Unity_TilingAndOffset_float(_Rotate_308147ec599e413b9040bf4af1b98ebd_Out_3, float2 (1, 1), _Vector2_a5dc5a50dd444c22bcfa607a284d4138_Out_0, _TilingAndOffset_babaa12e12d2477c88d0a972fd00ad02_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Ellipse_45ddcd19743f42eabe5bc47fbba9479d_Out_4;
    Unity_Ellipse_float(_TilingAndOffset_babaa12e12d2477c88d0a972fd00ad02_Out_3, _Multiply_07daa50536f64b15908ffc839fdc18af_Out_2, _Multiply_07daa50536f64b15908ffc839fdc18af_Out_2, _Ellipse_45ddcd19743f42eabe5bc47fbba9479d_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_9afa425d6b4a4484b424ad62e647beac_Out_2;
    Unity_Add_float(_Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2, _Ellipse_45ddcd19743f42eabe5bc47fbba9479d_Out_4, _Add_9afa425d6b4a4484b424ad62e647beac_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_85abf48e7a7642a298ae62bd5b69312c_Out_3;
    Unity_Rotate_Degrees_float(IN.uv0.xy, float2 (0.5, 0.5), 180, _Rotate_85abf48e7a7642a298ae62bd5b69312c_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Split_eafaa39bd1cd4f58ae2887cabe9ddeae_R_1 = _Rotate_85abf48e7a7642a298ae62bd5b69312c_Out_3[0];
    float _Split_eafaa39bd1cd4f58ae2887cabe9ddeae_G_2 = _Rotate_85abf48e7a7642a298ae62bd5b69312c_Out_3[1];
    float _Split_eafaa39bd1cd4f58ae2887cabe9ddeae_B_3 = 0;
    float _Split_eafaa39bd1cd4f58ae2887cabe9ddeae_A_4 = 0;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _OneMinus_97cbd884ea4e439c9795d45e17a706da_Out_1;
    Unity_OneMinus_float(_Split_eafaa39bd1cd4f58ae2887cabe9ddeae_R_1, _OneMinus_97cbd884ea4e439c9795d45e17a706da_Out_1);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_1a8f886502744979bbfc94d61ad2aa8a_Out_0 = float2(_OneMinus_97cbd884ea4e439c9795d45e17a706da_Out_1, _Split_eafaa39bd1cd4f58ae2887cabe9ddeae_G_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_6ab291f666944af7b11fedf8e0ffaca6_Out_0 = _fillTail;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_c336baf27c544bf28c5a96d24ff3f91d_Out_2;
    Unity_Multiply_float(_Property_6ab291f666944af7b11fedf8e0ffaca6_Out_0, 360, _Multiply_c336baf27c544bf28c5a96d24ff3f91d_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Rotate_3568025fdfca4f488b7f1a7e9155908b_Out_3;
    Unity_Rotate_Degrees_float(_Vector2_1a8f886502744979bbfc94d61ad2aa8a_Out_0, float2 (0.5, 0.5), _Multiply_c336baf27c544bf28c5a96d24ff3f91d_Out_2, _Rotate_3568025fdfca4f488b7f1a7e9155908b_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Property_e7355a66a9e7409b8734b75787393865_Out_0 = Vector1_3862b5470ce94ed68d6ccd8d5aa57614;
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_62e029343a3745e48a1e522d8dd57952_Out_2;
    Unity_Multiply_float(_Property_e7355a66a9e7409b8734b75787393865_Out_0, 0.5, _Multiply_62e029343a3745e48a1e522d8dd57952_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Multiply_ccff550cec024527b7ebdb1852692247_Out_2;
    Unity_Multiply_float(_Multiply_62e029343a3745e48a1e522d8dd57952_Out_2, -0.5, _Multiply_ccff550cec024527b7ebdb1852692247_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_16289e60be0e439ab3c1f97df0520a91_Out_2;
    Unity_Add_float(0.5, _Multiply_ccff550cec024527b7ebdb1852692247_Out_2, _Add_16289e60be0e439ab3c1f97df0520a91_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _Vector2_d121d160067943c2a454b5fe8bb3456e_Out_0 = float2(0, _Add_16289e60be0e439ab3c1f97df0520a91_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float2 _TilingAndOffset_a032dfbb58b244c59024ec234ae15b70_Out_3;
    Unity_TilingAndOffset_float(_Rotate_3568025fdfca4f488b7f1a7e9155908b_Out_3, float2 (1, 1), _Vector2_d121d160067943c2a454b5fe8bb3456e_Out_0, _TilingAndOffset_a032dfbb58b244c59024ec234ae15b70_Out_3);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Ellipse_9767eca10871423db0024289beafb443_Out_4;
    Unity_Ellipse_float(_TilingAndOffset_a032dfbb58b244c59024ec234ae15b70_Out_3, _Multiply_62e029343a3745e48a1e522d8dd57952_Out_2, _Multiply_62e029343a3745e48a1e522d8dd57952_Out_2, _Ellipse_9767eca10871423db0024289beafb443_Out_4);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2;
    Unity_Add_float(_Add_9afa425d6b4a4484b424ad62e647beac_Out_2, _Ellipse_9767eca10871423db0024289beafb443_Out_4, _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2);
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    #if defined(ENUM_173FCA8E617C4725B6502C6FBE756CEC_SHARP)
    float _RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0 = _Multiply_94cd381760174ca1ab8a7915c124b62a_Out_2;
    #else
    float _RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0 = _Add_e60ffcefc7d640adbc2b0cd63cb6bb87_Out_2;
    #endif
    #endif
    #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1)
    float _Saturate_737696eb34c241d6b897441762787779_Out_1;
    Unity_Saturate_float(_RoundedCorners_b54e413ee8494e9d9f5ff3608460dcf7_Out_0, _Saturate_737696eb34c241d6b897441762787779_Out_1);
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
    Unity_Multiply_float((_Saturate_737696eb34c241d6b897441762787779_Out_1.xxxx), _SampleTexture2D_db4059a956484545adfbe855ead6e884_RGBA_0, _Multiply_83aae9860a96466b9d32a13ab9801b27_Out_2);
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
#include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteUnlitPass.hlsl"

    ENDHLSL
}
        }
            FallBack "Hidden/Shader Graph/FallbackError"
}