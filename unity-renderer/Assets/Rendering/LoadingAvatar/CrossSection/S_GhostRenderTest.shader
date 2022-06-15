Shader "Unlit/S_GhostRenderTest"
{
    Properties
    {
        [HDR] _Color("Color", Color) = (0, 3.294118, 4, 0.9137255)
        EdgeThickness("FresnelThickness", Float) = 1
        Vector1_ede85aae148744889648b5927a63123a("InnerThickness", Float) = 3.36
        _RevealPosition("RevealPosition", Vector) = (0, 1, 0, 0)
        _RevealNormal("RevealNormal", Vector) = (0, -1, 0, 0)
        Vector1_61a7a0ad63c841748fe6f6cfb1a67e1f("Speed", Float) = 0.5
        Vector1_e2ecd75ce0f24a9084fc1095eb79e423("Intensity", Float) = 0
        [NoScaleOffset]Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc("Refraction", 2D) = "white" {}
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
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
        #define _AlphaClip 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
    #define REQUIRE_OPAQUE_TEXTURE
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
        float3 normalWS;
        float3 viewDirectionWS;
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
        float3 WorldSpaceNormal;
        float3 WorldSpaceViewDirection;
        float3 ObjectSpacePosition;
        float3 WorldSpacePosition;
        float4 ScreenPosition;
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
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float3 interp2 : TEXCOORD2;
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
        output.interp2.xyz = input.viewDirectionWS;
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
        output.viewDirectionWS = input.interp2.xyz;
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
float4 _Color;
float EdgeThickness;
float Vector1_ede85aae148744889648b5927a63123a;
float3 _RevealPosition;
float3 _RevealNormal;
float Vector1_61a7a0ad63c841748fe6f6cfb1a67e1f;
float Vector1_e2ecd75ce0f24a9084fc1095eb79e423;
float4 Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc);
SAMPLER(samplerTexture2D_a9a548e6a66f49bd87f1a8233bfa84fc);

// Graph Functions

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
{
    Out = A * B;
}

void Unity_Add_float4(float4 A, float4 B, out float4 Out)
{
    Out = A + B;
}

void Unity_SceneColor_float(float4 UV, out float3 Out)
{
    Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
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

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Ellipse_float(float2 UV, float Width, float Height, out float Out)
{
    float d = length((UV * 2 - 1) / float2(Width, Height));
    Out = saturate((1 - d) / fwidth(d));
}

void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
{
    Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
{
    Out = lerp(A, B, T);
}

void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
{
    Out = A - B;
}

void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
{
    Out = A * B;
}

void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
{
    Out = dot(A, B);
}

void Unity_Step_float(float Edge, float In, out float Out)
{
    Out = step(Edge, In);
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
    float4 _ScreenPosition_51c979e473f6424eb112c366aa00f013_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
    UnityTexture2D _Property_13cd49361de7469c83bb2c7ee6cc4e47_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc);
    float4 _ScreenPosition_c8bcdc29352b4b02a861e810277401da_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
    float _Multiply_8dbf9770b0054ddab481df2bda8e15da_Out_2;
    Unity_Multiply_float(IN.TimeParameters.x, 0.1, _Multiply_8dbf9770b0054ddab481df2bda8e15da_Out_2);
    float2 _TilingAndOffset_e97d57b59ed1465abebe5d99dde7501a_Out_3;
    Unity_TilingAndOffset_float((_ScreenPosition_c8bcdc29352b4b02a861e810277401da_Out_0.xy), float2 (1, 1), (_Multiply_8dbf9770b0054ddab481df2bda8e15da_Out_2.xx), _TilingAndOffset_e97d57b59ed1465abebe5d99dde7501a_Out_3);
    float4 _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0 = SAMPLE_TEXTURE2D(_Property_13cd49361de7469c83bb2c7ee6cc4e47_Out_0.tex, _Property_13cd49361de7469c83bb2c7ee6cc4e47_Out_0.samplerstate, _TilingAndOffset_e97d57b59ed1465abebe5d99dde7501a_Out_3);
    float _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_R_4 = _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0.r;
    float _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_G_5 = _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0.g;
    float _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_B_6 = _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0.b;
    float _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_A_7 = _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0.a;
    float4 _Multiply_26d81ecace474152861749fa07e40e90_Out_2;
    Unity_Multiply_float(_ScreenPosition_51c979e473f6424eb112c366aa00f013_Out_0, _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0, _Multiply_26d81ecace474152861749fa07e40e90_Out_2);
    float _Property_2eeb2dc375e34722bccbd72d2f4950c6_Out_0 = Vector1_e2ecd75ce0f24a9084fc1095eb79e423;
    float4 _Multiply_5ebb0546ed444813a1931cb93cbbf177_Out_2;
    Unity_Multiply_float(_Multiply_26d81ecace474152861749fa07e40e90_Out_2, (_Property_2eeb2dc375e34722bccbd72d2f4950c6_Out_0.xxxx), _Multiply_5ebb0546ed444813a1931cb93cbbf177_Out_2);
    float4 _Add_44c6b0acb6d34e5b8722ce5aa7335dd8_Out_2;
    Unity_Add_float4(_ScreenPosition_51c979e473f6424eb112c366aa00f013_Out_0, _Multiply_5ebb0546ed444813a1931cb93cbbf177_Out_2, _Add_44c6b0acb6d34e5b8722ce5aa7335dd8_Out_2);
    float3 _SceneColor_56439d8697bb4347bda2d031913214cc_Out_1;
    Unity_SceneColor_float(_Add_44c6b0acb6d34e5b8722ce5aa7335dd8_Out_2, _SceneColor_56439d8697bb4347bda2d031913214cc_Out_1);
    float4 _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
    float _Property_0a7605a0a5994b849b49d8da5a3e6f0e_Out_0 = Vector1_61a7a0ad63c841748fe6f6cfb1a67e1f;
    float _Multiply_a14781bfbf4b4ccfabd90b7ce473f366_Out_2;
    Unity_Multiply_float(IN.TimeParameters.x, _Property_0a7605a0a5994b849b49d8da5a3e6f0e_Out_0, _Multiply_a14781bfbf4b4ccfabd90b7ce473f366_Out_2);
    float2 _TilingAndOffset_dd4ec7b192384d87be15e5c17c910f34_Out_3;
    Unity_TilingAndOffset_float((IN.ObjectSpacePosition.xy), float2 (1, 1), (_Multiply_a14781bfbf4b4ccfabd90b7ce473f366_Out_2.xx), _TilingAndOffset_dd4ec7b192384d87be15e5c17c910f34_Out_3);
    float _SimpleNoise_3f69444b82a44595b296e76dc7aec36f_Out_2;
    Unity_SimpleNoise_float(_TilingAndOffset_dd4ec7b192384d87be15e5c17c910f34_Out_3, 30, _SimpleNoise_3f69444b82a44595b296e76dc7aec36f_Out_2);
    float _Remap_73d2dfe8310d4964ab152e3dd50cda84_Out_3;
    Unity_Remap_float(_SimpleNoise_3f69444b82a44595b296e76dc7aec36f_Out_2, float2 (0, 1), float2 (-10, 10), _Remap_73d2dfe8310d4964ab152e3dd50cda84_Out_3);
    float _OneMinus_5afec1fab9164194b1a58f5c58d0f889_Out_1;
    Unity_OneMinus_float(_Multiply_a14781bfbf4b4ccfabd90b7ce473f366_Out_2, _OneMinus_5afec1fab9164194b1a58f5c58d0f889_Out_1);
    float2 _TilingAndOffset_29d4ff5d52b6478f88a3b8f74dc3cbc0_Out_3;
    Unity_TilingAndOffset_float((IN.ObjectSpacePosition.xy), float2 (1, 1), (_OneMinus_5afec1fab9164194b1a58f5c58d0f889_Out_1.xx), _TilingAndOffset_29d4ff5d52b6478f88a3b8f74dc3cbc0_Out_3);
    float _SimpleNoise_7bfa3373055440d2b7428a11e04268ea_Out_2;
    Unity_SimpleNoise_float(_TilingAndOffset_29d4ff5d52b6478f88a3b8f74dc3cbc0_Out_3, 10, _SimpleNoise_7bfa3373055440d2b7428a11e04268ea_Out_2);
    float _Remap_ed78f82791934a67923196a0132472b5_Out_3;
    Unity_Remap_float(_SimpleNoise_7bfa3373055440d2b7428a11e04268ea_Out_2, float2 (0, 1), float2 (-10, 10), _Remap_ed78f82791934a67923196a0132472b5_Out_3);
    float _Multiply_b6a07b564ee74040b6f1e83029462dc1_Out_2;
    Unity_Multiply_float(_Remap_73d2dfe8310d4964ab152e3dd50cda84_Out_3, _Remap_ed78f82791934a67923196a0132472b5_Out_3, _Multiply_b6a07b564ee74040b6f1e83029462dc1_Out_2);
    float _Ellipse_68ab55f23ae645b48a2275b57c3b4a09_Out_4;
    Unity_Ellipse_float((_Multiply_b6a07b564ee74040b6f1e83029462dc1_Out_2.xx), 0.5, 0.5, _Ellipse_68ab55f23ae645b48a2275b57c3b4a09_Out_4);
    float _Property_aece623fc2974b1395b7a7348a9aeab5_Out_0 = Vector1_ede85aae148744889648b5927a63123a;
    float _FresnelEffect_a50e052a91e54b719b8a2fedb55d6dd8_Out_3;
    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_aece623fc2974b1395b7a7348a9aeab5_Out_0, _FresnelEffect_a50e052a91e54b719b8a2fedb55d6dd8_Out_3);
    float _Multiply_a146bdd5a6be4578b867a07f122fa42b_Out_2;
    Unity_Multiply_float(_Ellipse_68ab55f23ae645b48a2275b57c3b4a09_Out_4, _FresnelEffect_a50e052a91e54b719b8a2fedb55d6dd8_Out_3, _Multiply_a146bdd5a6be4578b867a07f122fa42b_Out_2);
    float _Property_32a188cf77894717bb5480a217a0da2d_Out_0 = EdgeThickness;
    float _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3;
    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_32a188cf77894717bb5480a217a0da2d_Out_0, _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3);
    float _Add_1d2c0eac10ff4677be22e4308b7dcfd7_Out_2;
    Unity_Add_float(_Multiply_a146bdd5a6be4578b867a07f122fa42b_Out_2, _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3, _Add_1d2c0eac10ff4677be22e4308b7dcfd7_Out_2);
    float3 _Lerp_387e4d23e31049d3938b37c42d7c430d_Out_3;
    Unity_Lerp_float3(_SceneColor_56439d8697bb4347bda2d031913214cc_Out_1, (_Property_527c4de35cfa4edfaf31179d87ae0770_Out_0.xyz), (_Add_1d2c0eac10ff4677be22e4308b7dcfd7_Out_2.xxx), _Lerp_387e4d23e31049d3938b37c42d7c430d_Out_3);
    float3 _Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0 = _RevealPosition;
    float _Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0 = 1;
    float3 _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2;
    Unity_Subtract_float3(_Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0, (_Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0.xxx), _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2);
    float _Float_00426fa8395a47689012697b07bd0ec6_Out_0 = -1;
    float3 _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2;
    Unity_Multiply_float(_Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2, (_Float_00426fa8395a47689012697b07bd0ec6_Out_0.xxx), _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2);
    float3 _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2;
    Unity_Subtract_float3(IN.ObjectSpacePosition, _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2, _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2);
    float3 _Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0 = _RevealNormal;
    float _Float_046751997f5c432180dff64c70184248_Out_0 = -1;
    float3 _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2;
    Unity_Multiply_float(_Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0, (_Float_046751997f5c432180dff64c70184248_Out_0.xxx), _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2);
    float _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2;
    Unity_DotProduct_float3(_Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2, _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2);
    float _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
    Unity_Step_float(0, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2, _Step_8d80aca51b4a48799d67582f0796e12c_Out_2);
    surface.BaseColor = _Lerp_387e4d23e31049d3938b37c42d7c430d_Out_3;
    surface.Alpha = 1;
    surface.AlphaClipThreshold = _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
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

    // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
    float3 unnormalizedNormalWS = input.normalWS;
    const float renormFactor = 1.0 / length(unnormalizedNormalWS);


    output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph


    output.WorldSpaceViewDirection = input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
    output.WorldSpacePosition = input.positionWS;
    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #define _AlphaClip 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
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
        float3 ObjectSpacePosition;
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

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
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
float4 _Color;
float EdgeThickness;
float Vector1_ede85aae148744889648b5927a63123a;
float3 _RevealPosition;
float3 _RevealNormal;
float Vector1_61a7a0ad63c841748fe6f6cfb1a67e1f;
float Vector1_e2ecd75ce0f24a9084fc1095eb79e423;
float4 Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc);
SAMPLER(samplerTexture2D_a9a548e6a66f49bd87f1a8233bfa84fc);

// Graph Functions

void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
{
    Out = A - B;
}

void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
{
    Out = A * B;
}

void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
{
    Out = dot(A, B);
}

void Unity_Step_float(float Edge, float In, out float Out)
{
    Out = step(Edge, In);
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
    float3 _Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0 = _RevealPosition;
    float _Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0 = 1;
    float3 _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2;
    Unity_Subtract_float3(_Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0, (_Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0.xxx), _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2);
    float _Float_00426fa8395a47689012697b07bd0ec6_Out_0 = -1;
    float3 _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2;
    Unity_Multiply_float(_Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2, (_Float_00426fa8395a47689012697b07bd0ec6_Out_0.xxx), _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2);
    float3 _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2;
    Unity_Subtract_float3(IN.ObjectSpacePosition, _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2, _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2);
    float3 _Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0 = _RevealNormal;
    float _Float_046751997f5c432180dff64c70184248_Out_0 = -1;
    float3 _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2;
    Unity_Multiply_float(_Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0, (_Float_046751997f5c432180dff64c70184248_Out_0.xxx), _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2);
    float _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2;
    Unity_DotProduct_float3(_Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2, _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2);
    float _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
    Unity_Step_float(0, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2, _Step_8d80aca51b4a48799d67582f0796e12c_Out_2);
    surface.Alpha = 1;
    surface.AlphaClipThreshold = _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
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





    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
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
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
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
        float3 ObjectSpacePosition;
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

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
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
float4 _Color;
float EdgeThickness;
float Vector1_ede85aae148744889648b5927a63123a;
float3 _RevealPosition;
float3 _RevealNormal;
float Vector1_61a7a0ad63c841748fe6f6cfb1a67e1f;
float Vector1_e2ecd75ce0f24a9084fc1095eb79e423;
float4 Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc);
SAMPLER(samplerTexture2D_a9a548e6a66f49bd87f1a8233bfa84fc);

// Graph Functions

void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
{
    Out = A - B;
}

void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
{
    Out = A * B;
}

void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
{
    Out = dot(A, B);
}

void Unity_Step_float(float Edge, float In, out float Out)
{
    Out = step(Edge, In);
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
    float3 _Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0 = _RevealPosition;
    float _Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0 = 1;
    float3 _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2;
    Unity_Subtract_float3(_Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0, (_Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0.xxx), _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2);
    float _Float_00426fa8395a47689012697b07bd0ec6_Out_0 = -1;
    float3 _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2;
    Unity_Multiply_float(_Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2, (_Float_00426fa8395a47689012697b07bd0ec6_Out_0.xxx), _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2);
    float3 _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2;
    Unity_Subtract_float3(IN.ObjectSpacePosition, _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2, _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2);
    float3 _Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0 = _RevealNormal;
    float _Float_046751997f5c432180dff64c70184248_Out_0 = -1;
    float3 _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2;
    Unity_Multiply_float(_Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0, (_Float_046751997f5c432180dff64c70184248_Out_0.xxx), _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2);
    float _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2;
    Unity_DotProduct_float3(_Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2, _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2);
    float _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
    Unity_Step_float(0, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2, _Step_8d80aca51b4a48799d67582f0796e12c_Out_2);
    surface.Alpha = 1;
    surface.AlphaClipThreshold = _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
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





    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
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
        #define _AlphaClip 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
    #define REQUIRE_OPAQUE_TEXTURE
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
        float3 normalWS;
        float3 viewDirectionWS;
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
        float3 WorldSpaceNormal;
        float3 WorldSpaceViewDirection;
        float3 ObjectSpacePosition;
        float3 WorldSpacePosition;
        float4 ScreenPosition;
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
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float3 interp2 : TEXCOORD2;
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
        output.interp2.xyz = input.viewDirectionWS;
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
        output.viewDirectionWS = input.interp2.xyz;
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
float4 _Color;
float EdgeThickness;
float Vector1_ede85aae148744889648b5927a63123a;
float3 _RevealPosition;
float3 _RevealNormal;
float Vector1_61a7a0ad63c841748fe6f6cfb1a67e1f;
float Vector1_e2ecd75ce0f24a9084fc1095eb79e423;
float4 Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc);
SAMPLER(samplerTexture2D_a9a548e6a66f49bd87f1a8233bfa84fc);

// Graph Functions

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
}

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
{
    Out = A * B;
}

void Unity_Add_float4(float4 A, float4 B, out float4 Out)
{
    Out = A + B;
}

void Unity_SceneColor_float(float4 UV, out float3 Out)
{
    Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
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

void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
{
    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Ellipse_float(float2 UV, float Width, float Height, out float Out)
{
    float d = length((UV * 2 - 1) / float2(Width, Height));
    Out = saturate((1 - d) / fwidth(d));
}

void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
{
    Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
}

void Unity_Add_float(float A, float B, out float Out)
{
    Out = A + B;
}

void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
{
    Out = lerp(A, B, T);
}

void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
{
    Out = A - B;
}

void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
{
    Out = A * B;
}

void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
{
    Out = dot(A, B);
}

void Unity_Step_float(float Edge, float In, out float Out)
{
    Out = step(Edge, In);
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
    float4 _ScreenPosition_51c979e473f6424eb112c366aa00f013_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
    UnityTexture2D _Property_13cd49361de7469c83bb2c7ee6cc4e47_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc);
    float4 _ScreenPosition_c8bcdc29352b4b02a861e810277401da_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
    float _Multiply_8dbf9770b0054ddab481df2bda8e15da_Out_2;
    Unity_Multiply_float(IN.TimeParameters.x, 0.1, _Multiply_8dbf9770b0054ddab481df2bda8e15da_Out_2);
    float2 _TilingAndOffset_e97d57b59ed1465abebe5d99dde7501a_Out_3;
    Unity_TilingAndOffset_float((_ScreenPosition_c8bcdc29352b4b02a861e810277401da_Out_0.xy), float2 (1, 1), (_Multiply_8dbf9770b0054ddab481df2bda8e15da_Out_2.xx), _TilingAndOffset_e97d57b59ed1465abebe5d99dde7501a_Out_3);
    float4 _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0 = SAMPLE_TEXTURE2D(_Property_13cd49361de7469c83bb2c7ee6cc4e47_Out_0.tex, _Property_13cd49361de7469c83bb2c7ee6cc4e47_Out_0.samplerstate, _TilingAndOffset_e97d57b59ed1465abebe5d99dde7501a_Out_3);
    float _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_R_4 = _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0.r;
    float _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_G_5 = _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0.g;
    float _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_B_6 = _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0.b;
    float _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_A_7 = _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0.a;
    float4 _Multiply_26d81ecace474152861749fa07e40e90_Out_2;
    Unity_Multiply_float(_ScreenPosition_51c979e473f6424eb112c366aa00f013_Out_0, _SampleTexture2D_7116e48db6cf47458d29d116a4738fae_RGBA_0, _Multiply_26d81ecace474152861749fa07e40e90_Out_2);
    float _Property_2eeb2dc375e34722bccbd72d2f4950c6_Out_0 = Vector1_e2ecd75ce0f24a9084fc1095eb79e423;
    float4 _Multiply_5ebb0546ed444813a1931cb93cbbf177_Out_2;
    Unity_Multiply_float(_Multiply_26d81ecace474152861749fa07e40e90_Out_2, (_Property_2eeb2dc375e34722bccbd72d2f4950c6_Out_0.xxxx), _Multiply_5ebb0546ed444813a1931cb93cbbf177_Out_2);
    float4 _Add_44c6b0acb6d34e5b8722ce5aa7335dd8_Out_2;
    Unity_Add_float4(_ScreenPosition_51c979e473f6424eb112c366aa00f013_Out_0, _Multiply_5ebb0546ed444813a1931cb93cbbf177_Out_2, _Add_44c6b0acb6d34e5b8722ce5aa7335dd8_Out_2);
    float3 _SceneColor_56439d8697bb4347bda2d031913214cc_Out_1;
    Unity_SceneColor_float(_Add_44c6b0acb6d34e5b8722ce5aa7335dd8_Out_2, _SceneColor_56439d8697bb4347bda2d031913214cc_Out_1);
    float4 _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
    float _Property_0a7605a0a5994b849b49d8da5a3e6f0e_Out_0 = Vector1_61a7a0ad63c841748fe6f6cfb1a67e1f;
    float _Multiply_a14781bfbf4b4ccfabd90b7ce473f366_Out_2;
    Unity_Multiply_float(IN.TimeParameters.x, _Property_0a7605a0a5994b849b49d8da5a3e6f0e_Out_0, _Multiply_a14781bfbf4b4ccfabd90b7ce473f366_Out_2);
    float2 _TilingAndOffset_dd4ec7b192384d87be15e5c17c910f34_Out_3;
    Unity_TilingAndOffset_float((IN.ObjectSpacePosition.xy), float2 (1, 1), (_Multiply_a14781bfbf4b4ccfabd90b7ce473f366_Out_2.xx), _TilingAndOffset_dd4ec7b192384d87be15e5c17c910f34_Out_3);
    float _SimpleNoise_3f69444b82a44595b296e76dc7aec36f_Out_2;
    Unity_SimpleNoise_float(_TilingAndOffset_dd4ec7b192384d87be15e5c17c910f34_Out_3, 30, _SimpleNoise_3f69444b82a44595b296e76dc7aec36f_Out_2);
    float _Remap_73d2dfe8310d4964ab152e3dd50cda84_Out_3;
    Unity_Remap_float(_SimpleNoise_3f69444b82a44595b296e76dc7aec36f_Out_2, float2 (0, 1), float2 (-10, 10), _Remap_73d2dfe8310d4964ab152e3dd50cda84_Out_3);
    float _OneMinus_5afec1fab9164194b1a58f5c58d0f889_Out_1;
    Unity_OneMinus_float(_Multiply_a14781bfbf4b4ccfabd90b7ce473f366_Out_2, _OneMinus_5afec1fab9164194b1a58f5c58d0f889_Out_1);
    float2 _TilingAndOffset_29d4ff5d52b6478f88a3b8f74dc3cbc0_Out_3;
    Unity_TilingAndOffset_float((IN.ObjectSpacePosition.xy), float2 (1, 1), (_OneMinus_5afec1fab9164194b1a58f5c58d0f889_Out_1.xx), _TilingAndOffset_29d4ff5d52b6478f88a3b8f74dc3cbc0_Out_3);
    float _SimpleNoise_7bfa3373055440d2b7428a11e04268ea_Out_2;
    Unity_SimpleNoise_float(_TilingAndOffset_29d4ff5d52b6478f88a3b8f74dc3cbc0_Out_3, 10, _SimpleNoise_7bfa3373055440d2b7428a11e04268ea_Out_2);
    float _Remap_ed78f82791934a67923196a0132472b5_Out_3;
    Unity_Remap_float(_SimpleNoise_7bfa3373055440d2b7428a11e04268ea_Out_2, float2 (0, 1), float2 (-10, 10), _Remap_ed78f82791934a67923196a0132472b5_Out_3);
    float _Multiply_b6a07b564ee74040b6f1e83029462dc1_Out_2;
    Unity_Multiply_float(_Remap_73d2dfe8310d4964ab152e3dd50cda84_Out_3, _Remap_ed78f82791934a67923196a0132472b5_Out_3, _Multiply_b6a07b564ee74040b6f1e83029462dc1_Out_2);
    float _Ellipse_68ab55f23ae645b48a2275b57c3b4a09_Out_4;
    Unity_Ellipse_float((_Multiply_b6a07b564ee74040b6f1e83029462dc1_Out_2.xx), 0.5, 0.5, _Ellipse_68ab55f23ae645b48a2275b57c3b4a09_Out_4);
    float _Property_aece623fc2974b1395b7a7348a9aeab5_Out_0 = Vector1_ede85aae148744889648b5927a63123a;
    float _FresnelEffect_a50e052a91e54b719b8a2fedb55d6dd8_Out_3;
    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_aece623fc2974b1395b7a7348a9aeab5_Out_0, _FresnelEffect_a50e052a91e54b719b8a2fedb55d6dd8_Out_3);
    float _Multiply_a146bdd5a6be4578b867a07f122fa42b_Out_2;
    Unity_Multiply_float(_Ellipse_68ab55f23ae645b48a2275b57c3b4a09_Out_4, _FresnelEffect_a50e052a91e54b719b8a2fedb55d6dd8_Out_3, _Multiply_a146bdd5a6be4578b867a07f122fa42b_Out_2);
    float _Property_32a188cf77894717bb5480a217a0da2d_Out_0 = EdgeThickness;
    float _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3;
    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_32a188cf77894717bb5480a217a0da2d_Out_0, _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3);
    float _Add_1d2c0eac10ff4677be22e4308b7dcfd7_Out_2;
    Unity_Add_float(_Multiply_a146bdd5a6be4578b867a07f122fa42b_Out_2, _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3, _Add_1d2c0eac10ff4677be22e4308b7dcfd7_Out_2);
    float3 _Lerp_387e4d23e31049d3938b37c42d7c430d_Out_3;
    Unity_Lerp_float3(_SceneColor_56439d8697bb4347bda2d031913214cc_Out_1, (_Property_527c4de35cfa4edfaf31179d87ae0770_Out_0.xyz), (_Add_1d2c0eac10ff4677be22e4308b7dcfd7_Out_2.xxx), _Lerp_387e4d23e31049d3938b37c42d7c430d_Out_3);
    float3 _Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0 = _RevealPosition;
    float _Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0 = 1;
    float3 _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2;
    Unity_Subtract_float3(_Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0, (_Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0.xxx), _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2);
    float _Float_00426fa8395a47689012697b07bd0ec6_Out_0 = -1;
    float3 _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2;
    Unity_Multiply_float(_Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2, (_Float_00426fa8395a47689012697b07bd0ec6_Out_0.xxx), _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2);
    float3 _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2;
    Unity_Subtract_float3(IN.ObjectSpacePosition, _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2, _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2);
    float3 _Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0 = _RevealNormal;
    float _Float_046751997f5c432180dff64c70184248_Out_0 = -1;
    float3 _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2;
    Unity_Multiply_float(_Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0, (_Float_046751997f5c432180dff64c70184248_Out_0.xxx), _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2);
    float _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2;
    Unity_DotProduct_float3(_Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2, _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2);
    float _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
    Unity_Step_float(0, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2, _Step_8d80aca51b4a48799d67582f0796e12c_Out_2);
    surface.BaseColor = _Lerp_387e4d23e31049d3938b37c42d7c430d_Out_3;
    surface.Alpha = 1;
    surface.AlphaClipThreshold = _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
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

    // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
    float3 unnormalizedNormalWS = input.normalWS;
    const float renormFactor = 1.0 / length(unnormalizedNormalWS);


    output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph


    output.WorldSpaceViewDirection = input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
    output.WorldSpacePosition = input.positionWS;
    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        #define _AlphaClip 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
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
        float3 ObjectSpacePosition;
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

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
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
float4 _Color;
float EdgeThickness;
float Vector1_ede85aae148744889648b5927a63123a;
float3 _RevealPosition;
float3 _RevealNormal;
float Vector1_61a7a0ad63c841748fe6f6cfb1a67e1f;
float Vector1_e2ecd75ce0f24a9084fc1095eb79e423;
float4 Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc);
SAMPLER(samplerTexture2D_a9a548e6a66f49bd87f1a8233bfa84fc);

// Graph Functions

void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
{
    Out = A - B;
}

void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
{
    Out = A * B;
}

void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
{
    Out = dot(A, B);
}

void Unity_Step_float(float Edge, float In, out float Out)
{
    Out = step(Edge, In);
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
    float3 _Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0 = _RevealPosition;
    float _Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0 = 1;
    float3 _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2;
    Unity_Subtract_float3(_Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0, (_Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0.xxx), _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2);
    float _Float_00426fa8395a47689012697b07bd0ec6_Out_0 = -1;
    float3 _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2;
    Unity_Multiply_float(_Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2, (_Float_00426fa8395a47689012697b07bd0ec6_Out_0.xxx), _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2);
    float3 _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2;
    Unity_Subtract_float3(IN.ObjectSpacePosition, _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2, _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2);
    float3 _Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0 = _RevealNormal;
    float _Float_046751997f5c432180dff64c70184248_Out_0 = -1;
    float3 _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2;
    Unity_Multiply_float(_Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0, (_Float_046751997f5c432180dff64c70184248_Out_0.xxx), _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2);
    float _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2;
    Unity_DotProduct_float3(_Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2, _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2);
    float _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
    Unity_Step_float(0, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2, _Step_8d80aca51b4a48799d67582f0796e12c_Out_2);
    surface.Alpha = 1;
    surface.AlphaClipThreshold = _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
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





    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
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
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
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
        float3 ObjectSpacePosition;
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

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
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
float4 _Color;
float EdgeThickness;
float Vector1_ede85aae148744889648b5927a63123a;
float3 _RevealPosition;
float3 _RevealNormal;
float Vector1_61a7a0ad63c841748fe6f6cfb1a67e1f;
float Vector1_e2ecd75ce0f24a9084fc1095eb79e423;
float4 Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_a9a548e6a66f49bd87f1a8233bfa84fc);
SAMPLER(samplerTexture2D_a9a548e6a66f49bd87f1a8233bfa84fc);

// Graph Functions

void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
{
    Out = A - B;
}

void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
{
    Out = A * B;
}

void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
{
    Out = dot(A, B);
}

void Unity_Step_float(float Edge, float In, out float Out)
{
    Out = step(Edge, In);
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
    float3 _Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0 = _RevealPosition;
    float _Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0 = 1;
    float3 _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2;
    Unity_Subtract_float3(_Property_ed030226fd3a4d06bcf8672cfaebbbaf_Out_0, (_Float_d12b6b5f63aa45f482f2107a7c15c444_Out_0.xxx), _Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2);
    float _Float_00426fa8395a47689012697b07bd0ec6_Out_0 = -1;
    float3 _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2;
    Unity_Multiply_float(_Subtract_205c09e04c674a5e9eaeecb8795c4aad_Out_2, (_Float_00426fa8395a47689012697b07bd0ec6_Out_0.xxx), _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2);
    float3 _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2;
    Unity_Subtract_float3(IN.ObjectSpacePosition, _Multiply_fe3748341c4b4fa79a07a72a7c2246a5_Out_2, _Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2);
    float3 _Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0 = _RevealNormal;
    float _Float_046751997f5c432180dff64c70184248_Out_0 = -1;
    float3 _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2;
    Unity_Multiply_float(_Property_c16a0e03ba324bf0a763fc1d615183fa_Out_0, (_Float_046751997f5c432180dff64c70184248_Out_0.xxx), _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2);
    float _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2;
    Unity_DotProduct_float3(_Subtract_a74db8075edd4f5fa81366b2814a5e97_Out_2, _Multiply_0d3bda4433b444feabef0b0a335b0dc8_Out_2, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2);
    float _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
    Unity_Step_float(0, _DotProduct_f3e1a8420c3d497ab88d8b286a70d934_Out_2, _Step_8d80aca51b4a48799d67582f0796e12c_Out_2);
    surface.Alpha = 1;
    surface.AlphaClipThreshold = _Step_8d80aca51b4a48799d67582f0796e12c_Out_2;
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





    output.ObjectSpacePosition = TransformWorldToObject(input.positionWS);
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