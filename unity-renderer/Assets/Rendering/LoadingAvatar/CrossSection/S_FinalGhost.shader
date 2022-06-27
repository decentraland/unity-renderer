Shader "Unlit/S_FinalGhost"
{
    Properties
    {
        [HDR] _Color("Color", Color) = (0, 3.294118, 4, 0.972549)
        _EdgeThickness("EdgeThickness", Float) = 1
        _PatternSpeed("PatternSpeed", Vector) = (0.01, 0.02, 0, 0)
        _RevealPosition("RevealPosition", Vector) = (0, 1, 0, 0)
        _RevealNormal("RevealNormal", Vector) = (0, 1, 0, 0)
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
float _EdgeThickness;
float2 _PatternSpeed;
float3 _RevealPosition;
float3 _RevealNormal;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
{
    Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
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


inline float2 Unity_Voronoi_RandomVector_float(float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)));
    return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
}

void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
{
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float t = 8.0;
    float3 res = float3(8.0, 0.0, 0.0);

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);

            if (d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                Out = res.x;
                Cells = res.y;
            }
        }
    }
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float4 _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
    float _Split_c4fcfdf55667442c9a2583548295eecd_R_1 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[0];
    float _Split_c4fcfdf55667442c9a2583548295eecd_G_2 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[1];
    float _Split_c4fcfdf55667442c9a2583548295eecd_B_3 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[2];
    float _Split_c4fcfdf55667442c9a2583548295eecd_A_4 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[3];
    float _Property_32a188cf77894717bb5480a217a0da2d_Out_0 = _EdgeThickness;
    float _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3;
    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_32a188cf77894717bb5480a217a0da2d_Out_0, _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3);
    float4 _ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
    float2 _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0 = _PatternSpeed;
    float2 _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2;
    Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0, _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2);
    float2 _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3;
    Unity_TilingAndOffset_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), float2 (1, 1), _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2, _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3);
    float _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2;
    Unity_SimpleNoise_float(_TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3, 15, _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2);
    float _Remap_6580e272816a438bb9846264ccb35ed6_Out_3;
    Unity_Remap_float(_SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2, float2 (0, 1), float2 (-10, 50), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3);
    float _Voronoi_1b29ba50777341649006361a20f59bec_Out_3;
    float _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4;
    Unity_Voronoi_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3, 12.1, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4);
    float _Multiply_d8545755016841e68a8d8d1bad287796_Out_2;
    Unity_Multiply_float(_FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2);
    float _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
    Unity_Multiply_float(_Split_c4fcfdf55667442c9a2583548295eecd_A_4, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2, _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2);
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
    surface.BaseColor = (_Property_527c4de35cfa4edfaf31179d87ae0770_Out_0.xyz);
    surface.Alpha = _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
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
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_VIEWDIRECTION_WS
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
float _EdgeThickness;
float2 _PatternSpeed;
float3 _RevealPosition;
float3 _RevealNormal;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
{
    Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
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


inline float2 Unity_Voronoi_RandomVector_float(float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)));
    return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
}

void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
{
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float t = 8.0;
    float3 res = float3(8.0, 0.0, 0.0);

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);

            if (d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                Out = res.x;
                Cells = res.y;
            }
        }
    }
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    float4 _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
    float _Split_c4fcfdf55667442c9a2583548295eecd_R_1 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[0];
    float _Split_c4fcfdf55667442c9a2583548295eecd_G_2 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[1];
    float _Split_c4fcfdf55667442c9a2583548295eecd_B_3 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[2];
    float _Split_c4fcfdf55667442c9a2583548295eecd_A_4 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[3];
    float _Property_32a188cf77894717bb5480a217a0da2d_Out_0 = _EdgeThickness;
    float _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3;
    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_32a188cf77894717bb5480a217a0da2d_Out_0, _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3);
    float4 _ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
    float2 _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0 = _PatternSpeed;
    float2 _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2;
    Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0, _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2);
    float2 _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3;
    Unity_TilingAndOffset_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), float2 (1, 1), _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2, _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3);
    float _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2;
    Unity_SimpleNoise_float(_TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3, 15, _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2);
    float _Remap_6580e272816a438bb9846264ccb35ed6_Out_3;
    Unity_Remap_float(_SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2, float2 (0, 1), float2 (-10, 50), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3);
    float _Voronoi_1b29ba50777341649006361a20f59bec_Out_3;
    float _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4;
    Unity_Voronoi_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3, 12.1, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4);
    float _Multiply_d8545755016841e68a8d8d1bad287796_Out_2;
    Unity_Multiply_float(_FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2);
    float _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
    Unity_Multiply_float(_Split_c4fcfdf55667442c9a2583548295eecd_A_4, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2, _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2);
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
    surface.Alpha = _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
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
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_VIEWDIRECTION_WS
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
float _EdgeThickness;
float2 _PatternSpeed;
float3 _RevealPosition;
float3 _RevealNormal;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
{
    Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
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


inline float2 Unity_Voronoi_RandomVector_float(float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)));
    return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
}

void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
{
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float t = 8.0;
    float3 res = float3(8.0, 0.0, 0.0);

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);

            if (d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                Out = res.x;
                Cells = res.y;
            }
        }
    }
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    float4 _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
    float _Split_c4fcfdf55667442c9a2583548295eecd_R_1 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[0];
    float _Split_c4fcfdf55667442c9a2583548295eecd_G_2 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[1];
    float _Split_c4fcfdf55667442c9a2583548295eecd_B_3 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[2];
    float _Split_c4fcfdf55667442c9a2583548295eecd_A_4 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[3];
    float _Property_32a188cf77894717bb5480a217a0da2d_Out_0 = _EdgeThickness;
    float _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3;
    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_32a188cf77894717bb5480a217a0da2d_Out_0, _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3);
    float4 _ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
    float2 _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0 = _PatternSpeed;
    float2 _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2;
    Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0, _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2);
    float2 _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3;
    Unity_TilingAndOffset_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), float2 (1, 1), _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2, _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3);
    float _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2;
    Unity_SimpleNoise_float(_TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3, 15, _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2);
    float _Remap_6580e272816a438bb9846264ccb35ed6_Out_3;
    Unity_Remap_float(_SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2, float2 (0, 1), float2 (-10, 50), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3);
    float _Voronoi_1b29ba50777341649006361a20f59bec_Out_3;
    float _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4;
    Unity_Voronoi_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3, 12.1, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4);
    float _Multiply_d8545755016841e68a8d8d1bad287796_Out_2;
    Unity_Multiply_float(_FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2);
    float _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
    Unity_Multiply_float(_Split_c4fcfdf55667442c9a2583548295eecd_A_4, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2, _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2);
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
    surface.Alpha = _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
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
float _EdgeThickness;
float2 _PatternSpeed;
float3 _RevealPosition;
float3 _RevealNormal;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
{
    Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
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


inline float2 Unity_Voronoi_RandomVector_float(float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)));
    return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
}

void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
{
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float t = 8.0;
    float3 res = float3(8.0, 0.0, 0.0);

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);

            if (d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                Out = res.x;
                Cells = res.y;
            }
        }
    }
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float4 _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
    float _Split_c4fcfdf55667442c9a2583548295eecd_R_1 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[0];
    float _Split_c4fcfdf55667442c9a2583548295eecd_G_2 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[1];
    float _Split_c4fcfdf55667442c9a2583548295eecd_B_3 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[2];
    float _Split_c4fcfdf55667442c9a2583548295eecd_A_4 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[3];
    float _Property_32a188cf77894717bb5480a217a0da2d_Out_0 = _EdgeThickness;
    float _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3;
    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_32a188cf77894717bb5480a217a0da2d_Out_0, _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3);
    float4 _ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
    float2 _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0 = _PatternSpeed;
    float2 _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2;
    Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0, _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2);
    float2 _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3;
    Unity_TilingAndOffset_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), float2 (1, 1), _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2, _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3);
    float _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2;
    Unity_SimpleNoise_float(_TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3, 15, _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2);
    float _Remap_6580e272816a438bb9846264ccb35ed6_Out_3;
    Unity_Remap_float(_SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2, float2 (0, 1), float2 (-10, 50), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3);
    float _Voronoi_1b29ba50777341649006361a20f59bec_Out_3;
    float _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4;
    Unity_Voronoi_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3, 12.1, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4);
    float _Multiply_d8545755016841e68a8d8d1bad287796_Out_2;
    Unity_Multiply_float(_FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2);
    float _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
    Unity_Multiply_float(_Split_c4fcfdf55667442c9a2583548295eecd_A_4, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2, _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2);
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
    surface.BaseColor = (_Property_527c4de35cfa4edfaf31179d87ae0770_Out_0.xyz);
    surface.Alpha = _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
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
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_VIEWDIRECTION_WS
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
float _EdgeThickness;
float2 _PatternSpeed;
float3 _RevealPosition;
float3 _RevealNormal;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
{
    Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
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


inline float2 Unity_Voronoi_RandomVector_float(float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)));
    return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
}

void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
{
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float t = 8.0;
    float3 res = float3(8.0, 0.0, 0.0);

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);

            if (d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                Out = res.x;
                Cells = res.y;
            }
        }
    }
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    float4 _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
    float _Split_c4fcfdf55667442c9a2583548295eecd_R_1 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[0];
    float _Split_c4fcfdf55667442c9a2583548295eecd_G_2 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[1];
    float _Split_c4fcfdf55667442c9a2583548295eecd_B_3 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[2];
    float _Split_c4fcfdf55667442c9a2583548295eecd_A_4 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[3];
    float _Property_32a188cf77894717bb5480a217a0da2d_Out_0 = _EdgeThickness;
    float _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3;
    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_32a188cf77894717bb5480a217a0da2d_Out_0, _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3);
    float4 _ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
    float2 _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0 = _PatternSpeed;
    float2 _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2;
    Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0, _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2);
    float2 _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3;
    Unity_TilingAndOffset_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), float2 (1, 1), _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2, _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3);
    float _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2;
    Unity_SimpleNoise_float(_TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3, 15, _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2);
    float _Remap_6580e272816a438bb9846264ccb35ed6_Out_3;
    Unity_Remap_float(_SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2, float2 (0, 1), float2 (-10, 50), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3);
    float _Voronoi_1b29ba50777341649006361a20f59bec_Out_3;
    float _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4;
    Unity_Voronoi_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3, 12.1, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4);
    float _Multiply_d8545755016841e68a8d8d1bad287796_Out_2;
    Unity_Multiply_float(_FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2);
    float _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
    Unity_Multiply_float(_Split_c4fcfdf55667442c9a2583548295eecd_A_4, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2, _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2);
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
    surface.Alpha = _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
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
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_VIEWDIRECTION_WS
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
float _EdgeThickness;
float2 _PatternSpeed;
float3 _RevealPosition;
float3 _RevealNormal;
CBUFFER_END

// Object and Global properties

    // Graph Functions

void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
{
    Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
}

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
{
    Out = UV * Tiling + Offset;
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


inline float2 Unity_Voronoi_RandomVector_float(float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)));
    return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
}

void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
{
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float t = 8.0;
    float3 res = float3(8.0, 0.0, 0.0);

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);

            if (d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                Out = res.x;
                Cells = res.y;
            }
        }
    }
}

void Unity_Multiply_float(float A, float B, out float Out)
{
    Out = A * B;
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
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    float4 _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0 = IsGammaSpace() ? LinearToSRGB(_Color) : _Color;
    float _Split_c4fcfdf55667442c9a2583548295eecd_R_1 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[0];
    float _Split_c4fcfdf55667442c9a2583548295eecd_G_2 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[1];
    float _Split_c4fcfdf55667442c9a2583548295eecd_B_3 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[2];
    float _Split_c4fcfdf55667442c9a2583548295eecd_A_4 = _Property_527c4de35cfa4edfaf31179d87ae0770_Out_0[3];
    float _Property_32a188cf77894717bb5480a217a0da2d_Out_0 = _EdgeThickness;
    float _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3;
    Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_32a188cf77894717bb5480a217a0da2d_Out_0, _FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3);
    float4 _ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
    float2 _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0 = _PatternSpeed;
    float2 _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2;
    Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_ccf019d13ef1487a8e30f407a51a6251_Out_0, _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2);
    float2 _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3;
    Unity_TilingAndOffset_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), float2 (1, 1), _Multiply_f7ad563f87df4569bfb5743d8741c745_Out_2, _TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3);
    float _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2;
    Unity_SimpleNoise_float(_TilingAndOffset_ede40353f59d4e729d3f5a4c8be6ab1a_Out_3, 15, _SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2);
    float _Remap_6580e272816a438bb9846264ccb35ed6_Out_3;
    Unity_Remap_float(_SimpleNoise_9993768210c8484d947619ace0bf5f9c_Out_2, float2 (0, 1), float2 (-10, 50), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3);
    float _Voronoi_1b29ba50777341649006361a20f59bec_Out_3;
    float _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4;
    Unity_Voronoi_float((_ScreenPosition_53ec65caf8f34e899bcbcc7dbd522442_Out_0.xy), _Remap_6580e272816a438bb9846264ccb35ed6_Out_3, 12.1, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Cells_4);
    float _Multiply_d8545755016841e68a8d8d1bad287796_Out_2;
    Unity_Multiply_float(_FresnelEffect_7290b970d0d64478ac423e59562b3643_Out_3, _Voronoi_1b29ba50777341649006361a20f59bec_Out_3, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2);
    float _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
    Unity_Multiply_float(_Split_c4fcfdf55667442c9a2583548295eecd_A_4, _Multiply_d8545755016841e68a8d8d1bad287796_Out_2, _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2);
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
    surface.Alpha = _Multiply_a37f214c4deb42bf8ad00406f942a112_Out_2;
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
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

    ENDHLSL
}
    }
        FallBack "Hidden/Shader Graph/FallbackError"
}