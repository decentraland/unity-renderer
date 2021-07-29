Shader "DCL/Toon Shader"
{
    Properties
    {
        [NoScaleOffset]_MatCap("MatCap", 2D) = "white" {}
        _GlossIntensity("GlossIntensity", Color) = (0.4811321, 0.4811321, 0.4811321, 1)
        _MultiplyColor("MultiplyColor", Color) = (0.3679245, 0.3679245, 0.3679245, 1)
        _AddColor("AddColor", Color) = (0.2358491, 0.2358491, 0.2358491, 1)
        _LightDir("LightDir", Vector) = (-0.1, 0.8, -0.4, 0)
        _LightColor("LightColor", Color) = (1, 1, 1, 1)
        [HDR]_FresnelIntensity("FresnelIntensity", Color) = (0.8705506, 0.8705506, 0.8705506, 1)
        [NoScaleOffset]_AvatarMap1("AvatarMap1", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap2("AvatarMap2", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap3("AvatarMap3", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap4("AvatarMap4", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap5("AvatarMap5", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap6("AvatarMap6", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap7("AvatarMap7", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap8("AvatarMap8", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap9("AvatarMap9", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap10("AvatarMap10", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap11("AvatarMap11", 2D) = "white" {}
        [NoScaleOffset]_AvatarMap12("AvatarMap12", 2D) = "white" {}
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // Render State
            Cull [_Cull]
            Blend [_SrcBlend] [_DstBlend]
            ZTest LEqual
            ZWrite [_ZWrite]

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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD3
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_POSITION_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_NORMAL_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TANGENT_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD3
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 viewDirectionWS;
                #endif
                #if defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            float2 lightmapUV;
                #endif
                #endif
                #if !defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 sh;
                #endif
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 fogFactorAndVertexLight;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 shadowCoord;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 TangentSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceViewDirection;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ViewSpacePosition;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp4 : TEXCOORD4;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp5 : TEXCOORD5;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp6 : TEXCOORD6;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp7 : TEXCOORD7;
                #endif
                #if defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            float2 interp8 : TEXCOORD8;
                #endif
                #endif
                #if !defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp9 : TEXCOORD9;
                #endif
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp10 : TEXCOORD10;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp11 : TEXCOORD11;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.positionWS;
                output.interp1.xyz = input.normalWS;
                output.interp2.xyzw = input.tangentWS;
                output.interp3.xyzw = input.texCoord0;
                output.interp4.xyzw = input.texCoord2;
                output.interp5.xyzw = input.texCoord3;
                output.interp6.xyzw = input.color;
                output.interp7.xyz = input.viewDirectionWS;
                #if defined(LIGHTMAP_ON)
            output.interp8.xy =  input.lightmapUV;
                #endif
                #if !defined(LIGHTMAP_ON)
                output.interp9.xyz = input.sh;
                #endif
                output.interp10.xyzw = input.fogFactorAndVertexLight;
                output.interp11.xyzw = input.shadowCoord;
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
                output.texCoord2 = input.interp4.xyzw;
                output.texCoord3 = input.interp5.xyzw;
                output.color = input.interp6.xyzw;
                output.viewDirectionWS = input.interp7.xyz;
                #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.interp8.xy;
                #endif
                #if !defined(LIGHTMAP_ON)
                output.sh = input.interp9.xyz;
                #endif
                output.fogFactorAndVertexLight = input.interp10.xyzw;
                output.shadowCoord = input.interp11.xyzw;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
            }

            void Unity_Normalize_float3(float3 In, out float3 Out)
            {
                Out = normalize(In);
            }

            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }

            void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
            {
                Out = dot(A, B);
            }

            void Unity_Power_float(float A, float B, out float Out)
            {
                Out = pow(A, B);
            }

            void Unity_Clamp_float(float In, float Min, float Max, out float Out)
            {
                Out = clamp(In, Min, Max);
            }

            void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
            {
                Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            void Unity_Add_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A + B;
            }

            struct Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceViewDirection;
            };

            void SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(UnityTexture2D Texture2D_73021B24,
                                                                float Vector1_1431D892, float Vector1_C2B7F43A,
                                                                float4 Vector4_453A6B1F, float Vector1_55629F98,
                                                                float4 Vector4_752D3319, float3 Vector3_FEFB7F68,
                                                                float4 Color_778878808e8a4165941cd59bf37b29e0,
                                                                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 IN,
                                                                out float4 OutVector4_1)
            {
                float4 _Property_13c5118e874f43ca87025f5b8cbc0834_Out_0 = Color_778878808e8a4165941cd59bf37b29e0;
                UnityTexture2D _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0 = Texture2D_73021B24;
                float3 _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1);
                float3 _Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0 = Vector3_FEFB7F68;
                float3 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_b2e19ca52f945686b2519392081846d0_Out_1);
                float3 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2;
                Unity_Add_float3(_Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0,
                                 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1,
                                 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2);
                float3 _Normalize_c35275e5de48c48a91895e915706d93a_Out_1;
                Unity_Normalize_float3(_Add_59467d0d541a7a8491396b40d6b459c0_Out_2,
                                       _Normalize_c35275e5de48c48a91895e915706d93a_Out_1);
                float _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2;
                Unity_DotProduct_float3(_Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1,
                                        _Normalize_c35275e5de48c48a91895e915706d93a_Out_1,
                                        _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2);
                float _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0 = Vector1_55629F98;
                float _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2;
                Unity_Power_float(_DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2,
                                  _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0,
                                  _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2);
                float _Clamp_f5039b8aba04a988afa60b6204315584_Out_3;
                Unity_Clamp_float(_Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2, 0.38, 0.99,
                                  _Clamp_f5039b8aba04a988afa60b6204315584_Out_3);
                float2 _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0 = float2(
                    _Clamp_f5039b8aba04a988afa60b6204315584_Out_3, 0.5);
                float4 _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.tex,
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.samplerstate,
                    _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0);
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_R_4 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.r;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_G_5 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.g;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_B_6 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.b;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_A_7 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.a;
                float4 _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0 = Vector4_752D3319;
                float4 _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2;
                Unity_Multiply_float(_SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0,
                                     _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0,
                                     _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2);
                UnityTexture2D _Property_1fe3423555394477a6d62590b0d06e66_Out_0 = Texture2D_73021B24;
                float _Property_264a79bba6deba80a88c754a36867c9d_Out_0 = Vector1_1431D892;
                float _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3;
                Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection,
                                          _Property_264a79bba6deba80a88c754a36867c9d_Out_0,
                                          _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3);
                float3 _Property_220a7f36ced91186a16e0b942c69428e_Out_0 = Vector3_FEFB7F68;
                float _Float_b30529d24ed97481877a14515ddbb316_Out_0 = -1;
                float3 _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1);
                float3 _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2;
                Unity_Multiply_float((_Float_b30529d24ed97481877a14515ddbb316_Out_0.xxx),
                                     _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1,
                                     _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2);
                float3 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1);
                float3 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2;
                Unity_Add_float3(_Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2,
                                 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1,
                                 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2);
                float3 _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1;
                Unity_Normalize_float3(_Add_024c1620de06b1808a8886cfa5f69c1d_Out_2,
                                       _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1);
                float _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2;
                Unity_DotProduct_float3(_Property_220a7f36ced91186a16e0b942c69428e_Out_0,
                                        _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1,
                                        _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2);
                float _Property_31359727423ffa80bc89eacf7e7596a9_Out_0 = Vector1_C2B7F43A;
                float2 _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0 = float2(
                    1, _Property_31359727423ffa80bc89eacf7e7596a9_Out_0);
                float _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3;
                Unity_Remap_float(_DotProduct_759d147c71911b81979499b9567e8b0b_Out_2, float2(-1, 1),
                                  _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0,
                                  _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3);
                float _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2;
                Unity_Multiply_float(_FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3,
                                     _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3,
                                     _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2);
                float _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3;
                Unity_Remap_float(_Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3);
                float2 _Vector2_902927a430b8848cac4001e2b7162a76_Out_0 = float2(
                    _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3, 0.99);
                float4 _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.tex,
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.samplerstate,
                    _Vector2_902927a430b8848cac4001e2b7162a76_Out_0);
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_R_4 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.r;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_G_5 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.g;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_B_6 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.b;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_A_7 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.a;
                float4 _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0 = Vector4_453A6B1F;
                float4 _Multiply_313986370303ba8c966db164632517d4_Out_2;
                Unity_Multiply_float(_SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0,
                                     _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0,
                                     _Multiply_313986370303ba8c966db164632517d4_Out_2);
                UnityTexture2D _Property_48552ab976c9128589df548370016ff3_Out_0 = Texture2D_73021B24;
                float3 _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0 = Vector3_FEFB7F68;
                float _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2;
                Unity_DotProduct_float3(IN.WorldSpaceNormal, _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0,
                                        _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2);
                float _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3;
                Unity_Remap_float(_DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3);
                float _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0 = 0;
                float2 _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0 = float2(
                    _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3, _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0);
                float4 _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_48552ab976c9128589df548370016ff3_Out_0.tex,
                    _Property_48552ab976c9128589df548370016ff3_Out_0.samplerstate,
                    _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0);
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_R_4 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.r;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_G_5 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.g;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_B_6 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.b;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_A_7 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.a;
                float4 Color_1caa66260b89b684a08e41b09e33f75d = IsGammaSpace()
                                                                    ? float4(0.7075472, 0.7075472, 0.7075472, 0)
                                                                    : float4(SRGBToLinear(
                                                                                 float3(0.7075472, 0.7075472,
                                                                                     0.7075472)), 0);
                float4 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2;
                Unity_Multiply_float(_SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0,
                                     Color_1caa66260b89b684a08e41b09e33f75d,
                                     _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2);
                float4 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2;
                Unity_Add_float4(_Multiply_313986370303ba8c966db164632517d4_Out_2,
                                 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2);
                float4 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2;
                Unity_Add_float4(_Multiply_9a724e75d670828c932f94803fda8a1e_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2,
                                 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2);
                float4 _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
                Unity_Multiply_float(_Property_13c5118e874f43ca87025f5b8cbc0834_Out_0,
                                     _Add_5adf131eb3b73a82810f17cdc8575166_Out_2,
                                     _Multiply_359a706b4442918d924cabd0309779e3_Out_2);
                OutVector4_1 = _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
            }

            void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base * Blend;
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
            {
                SHADERGRAPH_FOG(Position, Color, Density);
            }

            void Unity_OneMinus_float(float In, out float Out)
            {
                Out = 1 - In;
            }

            void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
            {
                Out = lerp(A, B, T);
            }

            struct Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9
            {
                float3 ViewSpacePosition;
            };

            void SG_Fog_db57d56e4661e4144b06df0b3edef8a9(float4 Color_42779DA4,
                                                         Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 IN,
                                                         out float4 Color_1)
            {
                float4 _Property_92d920f66f871787b8efd9892e387049_Out_0 = Color_42779DA4;
                float4 _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0;
                float _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1;
                Unity_Fog_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1, IN.ViewSpacePosition);
                float _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1;
                Unity_OneMinus_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Density_1,
                                     _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1);
                float4 _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
                Unity_Lerp_float4(_Property_92d920f66f871787b8efd9892e387049_Out_0,
                                  _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                  (_OneMinus_5194723b380f67819cad19ed50e5789e_Out_1.xxxx),
                                  _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3);
                Color_1 = _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
            }

            struct Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
            {
                float3 ViewSpacePosition;
            };

            void SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(float4 Color_D4F585C6, float4 Color_D7818A04,
                                                                  float4 Color_546468F9,
                                                                  Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
                                                                  IN, out float4 FinalColor_1)
            {
                float4 _Property_519fcfdd306ddb839c113ca07f0f606c_Out_0 = Color_D4F585C6;
                float4 _Property_3e015b35bff76284853220d6ed019e2e_Out_0 = Color_546468F9;
                float4 _Property_ae789b17ad00778693ee4740849f6c53_Out_0 = Color_D7818A04;
                float4 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2;
                Unity_Add_float4(_Property_3e015b35bff76284853220d6ed019e2e_Out_0,
                                 _Property_ae789b17ad00778693ee4740849f6c53_Out_0,
                                 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2);
                float4 _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2;
                Unity_Blend_Multiply_float4(_Property_519fcfdd306ddb839c113ca07f0f606c_Out_0,
                                            _Add_9c1c74d0fa17908ca9503658a239355d_Out_2,
                                            _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2, 1);
                Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 _Fog_18fab81a74b9d7858633e8fc73111a15;
                _Fog_18fab81a74b9d7858633e8fc73111a15.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
                SG_Fog_db57d56e4661e4144b06df0b3edef8a9(_Blend_4690b56c61915281a6230c45fd4ae04b_Out_2,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1);
                FinalColor_1 = _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0 = _AddColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                UnityTexture2D _Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0 = UnityBuildTexture2DStructNoScale(
                    _MatCap);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0 = IsGammaSpace()
                                                                              ? LinearToSRGB(_FresnelIntensity)
                                                                              : _FresnelIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_8243a735744a4993b94d1750ac0b3167_Out_0 = _GlossIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0 = _LightDir;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0 = _LightColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 _ToonShader_7540b263a099a28799339fadc2a1d8ac;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceNormal = IN.WorldSpaceNormal;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceViewDirection = IN.WorldSpaceViewDirection;
                float4 _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1;
                SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(_Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0, 1.77,
                                                               -0.27, _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0,
                                                               5, _Property_8243a735744a4993b94d1750ac0b3167_Out_0,
                                                               _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0,
                                                               _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0 = _MultiplyColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2;
                Unity_Multiply_float(_ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1,
                                     _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0,
                                     _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2;
                Unity_Add_float4(_Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0,
                                 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2,
                                 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_22b7e6c299217e8c995d810ee580d78b_Out_0 = _TintColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb _FinalCombine_18a3c84d4498a188a6a77b7159cd568c;
                _FinalCombine_18a3c84d4498a188a6a77b7159cd568c.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1;
                SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(
                    _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                    _Add_9aa5ce01d828b48c973710b335523c8a_Out_2, _Property_22b7e6c299217e8c995d810ee580d78b_Out_0,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_ba7bf77a544742d588bb64f92f7554da_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3;
                SampleTexture_float(float4(0, 0, 0, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2,
                                    (_UV_ba7bf77a544742d588bb64f92f7554da_Out_0.xy),
                                    _SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_1a397fc2a77b4d1bab2b4b1399a194be_Out_0 = IN.uv3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2;
                Unity_Multiply_float(_SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3,
                                     _UV_1a397fc2a77b4d1bab2b4b1399a194be_Out_0,
                                     _Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.BaseColor = (_FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1.xyz);
                surface.NormalTS = IN.TangentSpaceNormal;
                surface.Emission = (_Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2.xyz);
                surface.Metallic = 0;
                surface.Smoothness = 0;
                surface.Occlusion = 1;
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 unnormalizedNormalWS = input.normalWS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                const float renormFactor = 1.0 / length(unnormalizedNormalWS);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;
                // we want a unit length Normal Vector node in shader graph
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceViewDirection = input.viewDirectionWS;
                //TODO: by default normalized in HD, but not in universal
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ViewSpacePosition = TransformWorldToView(input.positionWS);
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv3 = input.texCoord3;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #include "PBRForwardPass.hlsl"
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
            #pragma multi_compile_fog
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD3
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_POSITION_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_NORMAL_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TANGENT_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD3
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 viewDirectionWS;
                #endif
                #if defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            float2 lightmapUV;
                #endif
                #endif
                #if !defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 sh;
                #endif
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 fogFactorAndVertexLight;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 shadowCoord;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 TangentSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceViewDirection;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ViewSpacePosition;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp4 : TEXCOORD4;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp5 : TEXCOORD5;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp6 : TEXCOORD6;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp7 : TEXCOORD7;
                #endif
                #if defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            float2 interp8 : TEXCOORD8;
                #endif
                #endif
                #if !defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp9 : TEXCOORD9;
                #endif
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp10 : TEXCOORD10;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp11 : TEXCOORD11;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.positionWS;
                output.interp1.xyz = input.normalWS;
                output.interp2.xyzw = input.tangentWS;
                output.interp3.xyzw = input.texCoord0;
                output.interp4.xyzw = input.texCoord2;
                output.interp5.xyzw = input.texCoord3;
                output.interp6.xyzw = input.color;
                output.interp7.xyz = input.viewDirectionWS;
                #if defined(LIGHTMAP_ON)
            output.interp8.xy =  input.lightmapUV;
                #endif
                #if !defined(LIGHTMAP_ON)
                output.interp9.xyz = input.sh;
                #endif
                output.interp10.xyzw = input.fogFactorAndVertexLight;
                output.interp11.xyzw = input.shadowCoord;
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
                output.texCoord2 = input.interp4.xyzw;
                output.texCoord3 = input.interp5.xyzw;
                output.color = input.interp6.xyzw;
                output.viewDirectionWS = input.interp7.xyz;
                #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.interp8.xy;
                #endif
                #if !defined(LIGHTMAP_ON)
                output.sh = input.interp9.xyz;
                #endif
                output.fogFactorAndVertexLight = input.interp10.xyzw;
                output.shadowCoord = input.interp11.xyzw;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
            }

            void Unity_Normalize_float3(float3 In, out float3 Out)
            {
                Out = normalize(In);
            }

            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }

            void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
            {
                Out = dot(A, B);
            }

            void Unity_Power_float(float A, float B, out float Out)
            {
                Out = pow(A, B);
            }

            void Unity_Clamp_float(float In, float Min, float Max, out float Out)
            {
                Out = clamp(In, Min, Max);
            }

            void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
            {
                Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            void Unity_Add_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A + B;
            }

            struct Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceViewDirection;
            };

            void SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(UnityTexture2D Texture2D_73021B24,
                                                                float Vector1_1431D892, float Vector1_C2B7F43A,
                                                                float4 Vector4_453A6B1F, float Vector1_55629F98,
                                                                float4 Vector4_752D3319, float3 Vector3_FEFB7F68,
                                                                float4 Color_778878808e8a4165941cd59bf37b29e0,
                                                                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 IN,
                                                                out float4 OutVector4_1)
            {
                float4 _Property_13c5118e874f43ca87025f5b8cbc0834_Out_0 = Color_778878808e8a4165941cd59bf37b29e0;
                UnityTexture2D _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0 = Texture2D_73021B24;
                float3 _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1);
                float3 _Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0 = Vector3_FEFB7F68;
                float3 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_b2e19ca52f945686b2519392081846d0_Out_1);
                float3 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2;
                Unity_Add_float3(_Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0,
                                 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1,
                                 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2);
                float3 _Normalize_c35275e5de48c48a91895e915706d93a_Out_1;
                Unity_Normalize_float3(_Add_59467d0d541a7a8491396b40d6b459c0_Out_2,
                                       _Normalize_c35275e5de48c48a91895e915706d93a_Out_1);
                float _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2;
                Unity_DotProduct_float3(_Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1,
                                        _Normalize_c35275e5de48c48a91895e915706d93a_Out_1,
                                        _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2);
                float _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0 = Vector1_55629F98;
                float _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2;
                Unity_Power_float(_DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2,
                                  _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0,
                                  _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2);
                float _Clamp_f5039b8aba04a988afa60b6204315584_Out_3;
                Unity_Clamp_float(_Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2, 0.38, 0.99,
                                  _Clamp_f5039b8aba04a988afa60b6204315584_Out_3);
                float2 _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0 = float2(
                    _Clamp_f5039b8aba04a988afa60b6204315584_Out_3, 0.5);
                float4 _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.tex,
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.samplerstate,
                    _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0);
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_R_4 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.r;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_G_5 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.g;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_B_6 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.b;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_A_7 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.a;
                float4 _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0 = Vector4_752D3319;
                float4 _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2;
                Unity_Multiply_float(_SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0,
                                     _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0,
                                     _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2);
                UnityTexture2D _Property_1fe3423555394477a6d62590b0d06e66_Out_0 = Texture2D_73021B24;
                float _Property_264a79bba6deba80a88c754a36867c9d_Out_0 = Vector1_1431D892;
                float _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3;
                Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection,
                                          _Property_264a79bba6deba80a88c754a36867c9d_Out_0,
                                          _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3);
                float3 _Property_220a7f36ced91186a16e0b942c69428e_Out_0 = Vector3_FEFB7F68;
                float _Float_b30529d24ed97481877a14515ddbb316_Out_0 = -1;
                float3 _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1);
                float3 _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2;
                Unity_Multiply_float((_Float_b30529d24ed97481877a14515ddbb316_Out_0.xxx),
                                     _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1,
                                     _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2);
                float3 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1);
                float3 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2;
                Unity_Add_float3(_Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2,
                                 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1,
                                 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2);
                float3 _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1;
                Unity_Normalize_float3(_Add_024c1620de06b1808a8886cfa5f69c1d_Out_2,
                                       _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1);
                float _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2;
                Unity_DotProduct_float3(_Property_220a7f36ced91186a16e0b942c69428e_Out_0,
                                        _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1,
                                        _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2);
                float _Property_31359727423ffa80bc89eacf7e7596a9_Out_0 = Vector1_C2B7F43A;
                float2 _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0 = float2(
                    1, _Property_31359727423ffa80bc89eacf7e7596a9_Out_0);
                float _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3;
                Unity_Remap_float(_DotProduct_759d147c71911b81979499b9567e8b0b_Out_2, float2(-1, 1),
                                  _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0,
                                  _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3);
                float _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2;
                Unity_Multiply_float(_FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3,
                                     _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3,
                                     _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2);
                float _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3;
                Unity_Remap_float(_Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3);
                float2 _Vector2_902927a430b8848cac4001e2b7162a76_Out_0 = float2(
                    _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3, 0.99);
                float4 _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.tex,
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.samplerstate,
                    _Vector2_902927a430b8848cac4001e2b7162a76_Out_0);
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_R_4 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.r;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_G_5 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.g;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_B_6 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.b;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_A_7 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.a;
                float4 _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0 = Vector4_453A6B1F;
                float4 _Multiply_313986370303ba8c966db164632517d4_Out_2;
                Unity_Multiply_float(_SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0,
                                     _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0,
                                     _Multiply_313986370303ba8c966db164632517d4_Out_2);
                UnityTexture2D _Property_48552ab976c9128589df548370016ff3_Out_0 = Texture2D_73021B24;
                float3 _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0 = Vector3_FEFB7F68;
                float _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2;
                Unity_DotProduct_float3(IN.WorldSpaceNormal, _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0,
                                        _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2);
                float _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3;
                Unity_Remap_float(_DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3);
                float _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0 = 0;
                float2 _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0 = float2(
                    _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3, _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0);
                float4 _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_48552ab976c9128589df548370016ff3_Out_0.tex,
                    _Property_48552ab976c9128589df548370016ff3_Out_0.samplerstate,
                    _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0);
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_R_4 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.r;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_G_5 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.g;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_B_6 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.b;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_A_7 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.a;
                float4 Color_1caa66260b89b684a08e41b09e33f75d = IsGammaSpace()
                                                                    ? float4(0.7075472, 0.7075472, 0.7075472, 0)
                                                                    : float4(SRGBToLinear(
                                                                                 float3(0.7075472, 0.7075472,
                                                                                     0.7075472)), 0);
                float4 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2;
                Unity_Multiply_float(_SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0,
                                     Color_1caa66260b89b684a08e41b09e33f75d,
                                     _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2);
                float4 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2;
                Unity_Add_float4(_Multiply_313986370303ba8c966db164632517d4_Out_2,
                                 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2);
                float4 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2;
                Unity_Add_float4(_Multiply_9a724e75d670828c932f94803fda8a1e_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2,
                                 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2);
                float4 _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
                Unity_Multiply_float(_Property_13c5118e874f43ca87025f5b8cbc0834_Out_0,
                                     _Add_5adf131eb3b73a82810f17cdc8575166_Out_2,
                                     _Multiply_359a706b4442918d924cabd0309779e3_Out_2);
                OutVector4_1 = _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
            }

            void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base * Blend;
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
            {
                SHADERGRAPH_FOG(Position, Color, Density);
            }

            void Unity_OneMinus_float(float In, out float Out)
            {
                Out = 1 - In;
            }

            void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
            {
                Out = lerp(A, B, T);
            }

            struct Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9
            {
                float3 ViewSpacePosition;
            };

            void SG_Fog_db57d56e4661e4144b06df0b3edef8a9(float4 Color_42779DA4,
                                                         Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 IN,
                                                         out float4 Color_1)
            {
                float4 _Property_92d920f66f871787b8efd9892e387049_Out_0 = Color_42779DA4;
                float4 _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0;
                float _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1;
                Unity_Fog_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1, IN.ViewSpacePosition);
                float _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1;
                Unity_OneMinus_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Density_1,
                                     _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1);
                float4 _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
                Unity_Lerp_float4(_Property_92d920f66f871787b8efd9892e387049_Out_0,
                                  _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                  (_OneMinus_5194723b380f67819cad19ed50e5789e_Out_1.xxxx),
                                  _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3);
                Color_1 = _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
            }

            struct Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
            {
                float3 ViewSpacePosition;
            };

            void SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(float4 Color_D4F585C6, float4 Color_D7818A04,
                                                                  float4 Color_546468F9,
                                                                  Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
                                                                  IN, out float4 FinalColor_1)
            {
                float4 _Property_519fcfdd306ddb839c113ca07f0f606c_Out_0 = Color_D4F585C6;
                float4 _Property_3e015b35bff76284853220d6ed019e2e_Out_0 = Color_546468F9;
                float4 _Property_ae789b17ad00778693ee4740849f6c53_Out_0 = Color_D7818A04;
                float4 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2;
                Unity_Add_float4(_Property_3e015b35bff76284853220d6ed019e2e_Out_0,
                                 _Property_ae789b17ad00778693ee4740849f6c53_Out_0,
                                 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2);
                float4 _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2;
                Unity_Blend_Multiply_float4(_Property_519fcfdd306ddb839c113ca07f0f606c_Out_0,
                                            _Add_9c1c74d0fa17908ca9503658a239355d_Out_2,
                                            _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2, 1);
                Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 _Fog_18fab81a74b9d7858633e8fc73111a15;
                _Fog_18fab81a74b9d7858633e8fc73111a15.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
                SG_Fog_db57d56e4661e4144b06df0b3edef8a9(_Blend_4690b56c61915281a6230c45fd4ae04b_Out_2,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1);
                FinalColor_1 = _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0 = _AddColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                UnityTexture2D _Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0 = UnityBuildTexture2DStructNoScale(
                    _MatCap);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0 = IsGammaSpace()
                                                                              ? LinearToSRGB(_FresnelIntensity)
                                                                              : _FresnelIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_8243a735744a4993b94d1750ac0b3167_Out_0 = _GlossIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0 = _LightDir;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0 = _LightColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 _ToonShader_7540b263a099a28799339fadc2a1d8ac;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceNormal = IN.WorldSpaceNormal;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceViewDirection = IN.WorldSpaceViewDirection;
                float4 _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1;
                SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(_Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0, 1.77,
                                                               -0.27, _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0,
                                                               5, _Property_8243a735744a4993b94d1750ac0b3167_Out_0,
                                                               _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0,
                                                               _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0 = _MultiplyColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2;
                Unity_Multiply_float(_ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1,
                                     _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0,
                                     _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2;
                Unity_Add_float4(_Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0,
                                 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2,
                                 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_22b7e6c299217e8c995d810ee580d78b_Out_0 = _TintColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb _FinalCombine_18a3c84d4498a188a6a77b7159cd568c;
                _FinalCombine_18a3c84d4498a188a6a77b7159cd568c.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1;
                SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(
                    _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                    _Add_9aa5ce01d828b48c973710b335523c8a_Out_2, _Property_22b7e6c299217e8c995d810ee580d78b_Out_0,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_ba7bf77a544742d588bb64f92f7554da_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3;
                SampleTexture_float(float4(0, 0, 0, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2,
                                    (_UV_ba7bf77a544742d588bb64f92f7554da_Out_0.xy),
                                    _SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_1a397fc2a77b4d1bab2b4b1399a194be_Out_0 = IN.uv3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2;
                Unity_Multiply_float(_SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3,
                                     _UV_1a397fc2a77b4d1bab2b4b1399a194be_Out_0,
                                     _Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.BaseColor = (_FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1.xyz);
                surface.NormalTS = IN.TangentSpaceNormal;
                surface.Emission = (_Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2.xyz);
                surface.Metallic = 0;
                surface.Smoothness = 0;
                surface.Occlusion = 1;
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 unnormalizedNormalWS = input.normalWS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                const float renormFactor = 1.0 / length(unnormalizedNormalWS);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;
                // we want a unit length Normal Vector node in shader graph
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceViewDirection = input.viewDirectionWS;
                //TODO: by default normalized in HD, but not in universal
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ViewSpacePosition = TransformWorldToView(input.positionWS);
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv3 = input.texCoord3;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyzw = input.texCoord0;
                output.interp1.xyzw = input.texCoord2;
                output.interp2.xyzw = input.color;
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
                output.texCoord2 = input.interp1.xyzw;
                output.color = input.interp2.xyzw;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyzw = input.texCoord0;
                output.interp1.xyzw = input.texCoord2;
                output.interp2.xyzw = input.color;
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
                output.texCoord2 = input.interp1.xyzw;
                output.color = input.interp2.xyzw;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_NORMAL_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TANGENT_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 TangentSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp4 : TEXCOORD4;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.normalWS;
                output.interp1.xyzw = input.tangentWS;
                output.interp2.xyzw = input.texCoord0;
                output.interp3.xyzw = input.texCoord2;
                output.interp4.xyzw = input.color;
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
                output.texCoord0 = input.interp2.xyzw;
                output.texCoord2 = input.interp3.xyzw;
                output.color = input.interp4.xyzw;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.NormalTS = IN.TangentSpaceNormal;
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD3
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_POSITION_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_NORMAL_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD3
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 viewDirectionWS;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceViewDirection;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ViewSpacePosition;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp4 : TEXCOORD4;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp5 : TEXCOORD5;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp6 : TEXCOORD6;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.positionWS;
                output.interp1.xyz = input.normalWS;
                output.interp2.xyzw = input.texCoord0;
                output.interp3.xyzw = input.texCoord2;
                output.interp4.xyzw = input.texCoord3;
                output.interp5.xyzw = input.color;
                output.interp6.xyz = input.viewDirectionWS;
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
                output.texCoord0 = input.interp2.xyzw;
                output.texCoord2 = input.interp3.xyzw;
                output.texCoord3 = input.interp4.xyzw;
                output.color = input.interp5.xyzw;
                output.viewDirectionWS = input.interp6.xyz;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
            }

            void Unity_Normalize_float3(float3 In, out float3 Out)
            {
                Out = normalize(In);
            }

            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }

            void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
            {
                Out = dot(A, B);
            }

            void Unity_Power_float(float A, float B, out float Out)
            {
                Out = pow(A, B);
            }

            void Unity_Clamp_float(float In, float Min, float Max, out float Out)
            {
                Out = clamp(In, Min, Max);
            }

            void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
            {
                Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            void Unity_Add_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A + B;
            }

            struct Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceViewDirection;
            };

            void SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(UnityTexture2D Texture2D_73021B24,
                                                                float Vector1_1431D892, float Vector1_C2B7F43A,
                                                                float4 Vector4_453A6B1F, float Vector1_55629F98,
                                                                float4 Vector4_752D3319, float3 Vector3_FEFB7F68,
                                                                float4 Color_778878808e8a4165941cd59bf37b29e0,
                                                                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 IN,
                                                                out float4 OutVector4_1)
            {
                float4 _Property_13c5118e874f43ca87025f5b8cbc0834_Out_0 = Color_778878808e8a4165941cd59bf37b29e0;
                UnityTexture2D _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0 = Texture2D_73021B24;
                float3 _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1);
                float3 _Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0 = Vector3_FEFB7F68;
                float3 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_b2e19ca52f945686b2519392081846d0_Out_1);
                float3 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2;
                Unity_Add_float3(_Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0,
                                 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1,
                                 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2);
                float3 _Normalize_c35275e5de48c48a91895e915706d93a_Out_1;
                Unity_Normalize_float3(_Add_59467d0d541a7a8491396b40d6b459c0_Out_2,
                                       _Normalize_c35275e5de48c48a91895e915706d93a_Out_1);
                float _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2;
                Unity_DotProduct_float3(_Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1,
                                        _Normalize_c35275e5de48c48a91895e915706d93a_Out_1,
                                        _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2);
                float _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0 = Vector1_55629F98;
                float _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2;
                Unity_Power_float(_DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2,
                                  _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0,
                                  _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2);
                float _Clamp_f5039b8aba04a988afa60b6204315584_Out_3;
                Unity_Clamp_float(_Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2, 0.38, 0.99,
                                  _Clamp_f5039b8aba04a988afa60b6204315584_Out_3);
                float2 _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0 = float2(
                    _Clamp_f5039b8aba04a988afa60b6204315584_Out_3, 0.5);
                float4 _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.tex,
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.samplerstate,
                    _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0);
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_R_4 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.r;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_G_5 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.g;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_B_6 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.b;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_A_7 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.a;
                float4 _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0 = Vector4_752D3319;
                float4 _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2;
                Unity_Multiply_float(_SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0,
                                     _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0,
                                     _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2);
                UnityTexture2D _Property_1fe3423555394477a6d62590b0d06e66_Out_0 = Texture2D_73021B24;
                float _Property_264a79bba6deba80a88c754a36867c9d_Out_0 = Vector1_1431D892;
                float _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3;
                Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection,
                                          _Property_264a79bba6deba80a88c754a36867c9d_Out_0,
                                          _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3);
                float3 _Property_220a7f36ced91186a16e0b942c69428e_Out_0 = Vector3_FEFB7F68;
                float _Float_b30529d24ed97481877a14515ddbb316_Out_0 = -1;
                float3 _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1);
                float3 _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2;
                Unity_Multiply_float((_Float_b30529d24ed97481877a14515ddbb316_Out_0.xxx),
                                     _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1,
                                     _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2);
                float3 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1);
                float3 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2;
                Unity_Add_float3(_Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2,
                                 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1,
                                 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2);
                float3 _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1;
                Unity_Normalize_float3(_Add_024c1620de06b1808a8886cfa5f69c1d_Out_2,
                                       _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1);
                float _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2;
                Unity_DotProduct_float3(_Property_220a7f36ced91186a16e0b942c69428e_Out_0,
                                        _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1,
                                        _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2);
                float _Property_31359727423ffa80bc89eacf7e7596a9_Out_0 = Vector1_C2B7F43A;
                float2 _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0 = float2(
                    1, _Property_31359727423ffa80bc89eacf7e7596a9_Out_0);
                float _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3;
                Unity_Remap_float(_DotProduct_759d147c71911b81979499b9567e8b0b_Out_2, float2(-1, 1),
                                  _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0,
                                  _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3);
                float _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2;
                Unity_Multiply_float(_FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3,
                                     _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3,
                                     _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2);
                float _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3;
                Unity_Remap_float(_Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3);
                float2 _Vector2_902927a430b8848cac4001e2b7162a76_Out_0 = float2(
                    _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3, 0.99);
                float4 _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.tex,
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.samplerstate,
                    _Vector2_902927a430b8848cac4001e2b7162a76_Out_0);
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_R_4 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.r;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_G_5 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.g;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_B_6 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.b;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_A_7 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.a;
                float4 _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0 = Vector4_453A6B1F;
                float4 _Multiply_313986370303ba8c966db164632517d4_Out_2;
                Unity_Multiply_float(_SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0,
                                     _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0,
                                     _Multiply_313986370303ba8c966db164632517d4_Out_2);
                UnityTexture2D _Property_48552ab976c9128589df548370016ff3_Out_0 = Texture2D_73021B24;
                float3 _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0 = Vector3_FEFB7F68;
                float _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2;
                Unity_DotProduct_float3(IN.WorldSpaceNormal, _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0,
                                        _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2);
                float _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3;
                Unity_Remap_float(_DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3);
                float _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0 = 0;
                float2 _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0 = float2(
                    _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3, _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0);
                float4 _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_48552ab976c9128589df548370016ff3_Out_0.tex,
                    _Property_48552ab976c9128589df548370016ff3_Out_0.samplerstate,
                    _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0);
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_R_4 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.r;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_G_5 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.g;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_B_6 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.b;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_A_7 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.a;
                float4 Color_1caa66260b89b684a08e41b09e33f75d = IsGammaSpace()
                                                                    ? float4(0.7075472, 0.7075472, 0.7075472, 0)
                                                                    : float4(SRGBToLinear(
                                                                                 float3(0.7075472, 0.7075472,
                                                                                     0.7075472)), 0);
                float4 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2;
                Unity_Multiply_float(_SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0,
                                     Color_1caa66260b89b684a08e41b09e33f75d,
                                     _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2);
                float4 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2;
                Unity_Add_float4(_Multiply_313986370303ba8c966db164632517d4_Out_2,
                                 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2);
                float4 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2;
                Unity_Add_float4(_Multiply_9a724e75d670828c932f94803fda8a1e_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2,
                                 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2);
                float4 _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
                Unity_Multiply_float(_Property_13c5118e874f43ca87025f5b8cbc0834_Out_0,
                                     _Add_5adf131eb3b73a82810f17cdc8575166_Out_2,
                                     _Multiply_359a706b4442918d924cabd0309779e3_Out_2);
                OutVector4_1 = _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
            }

            void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base * Blend;
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
            {
                SHADERGRAPH_FOG(Position, Color, Density);
            }

            void Unity_OneMinus_float(float In, out float Out)
            {
                Out = 1 - In;
            }

            void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
            {
                Out = lerp(A, B, T);
            }

            struct Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9
            {
                float3 ViewSpacePosition;
            };

            void SG_Fog_db57d56e4661e4144b06df0b3edef8a9(float4 Color_42779DA4,
                                                         Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 IN,
                                                         out float4 Color_1)
            {
                float4 _Property_92d920f66f871787b8efd9892e387049_Out_0 = Color_42779DA4;
                float4 _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0;
                float _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1;
                Unity_Fog_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1, IN.ViewSpacePosition);
                float _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1;
                Unity_OneMinus_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Density_1,
                                     _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1);
                float4 _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
                Unity_Lerp_float4(_Property_92d920f66f871787b8efd9892e387049_Out_0,
                                  _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                  (_OneMinus_5194723b380f67819cad19ed50e5789e_Out_1.xxxx),
                                  _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3);
                Color_1 = _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
            }

            struct Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
            {
                float3 ViewSpacePosition;
            };

            void SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(float4 Color_D4F585C6, float4 Color_D7818A04,
                                                                  float4 Color_546468F9,
                                                                  Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
                                                                  IN, out float4 FinalColor_1)
            {
                float4 _Property_519fcfdd306ddb839c113ca07f0f606c_Out_0 = Color_D4F585C6;
                float4 _Property_3e015b35bff76284853220d6ed019e2e_Out_0 = Color_546468F9;
                float4 _Property_ae789b17ad00778693ee4740849f6c53_Out_0 = Color_D7818A04;
                float4 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2;
                Unity_Add_float4(_Property_3e015b35bff76284853220d6ed019e2e_Out_0,
                                 _Property_ae789b17ad00778693ee4740849f6c53_Out_0,
                                 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2);
                float4 _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2;
                Unity_Blend_Multiply_float4(_Property_519fcfdd306ddb839c113ca07f0f606c_Out_0,
                                            _Add_9c1c74d0fa17908ca9503658a239355d_Out_2,
                                            _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2, 1);
                Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 _Fog_18fab81a74b9d7858633e8fc73111a15;
                _Fog_18fab81a74b9d7858633e8fc73111a15.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
                SG_Fog_db57d56e4661e4144b06df0b3edef8a9(_Blend_4690b56c61915281a6230c45fd4ae04b_Out_2,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1);
                FinalColor_1 = _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0 = _AddColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                UnityTexture2D _Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0 = UnityBuildTexture2DStructNoScale(
                    _MatCap);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0 = IsGammaSpace()
                                                                              ? LinearToSRGB(_FresnelIntensity)
                                                                              : _FresnelIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_8243a735744a4993b94d1750ac0b3167_Out_0 = _GlossIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0 = _LightDir;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0 = _LightColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 _ToonShader_7540b263a099a28799339fadc2a1d8ac;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceNormal = IN.WorldSpaceNormal;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceViewDirection = IN.WorldSpaceViewDirection;
                float4 _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1;
                SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(_Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0, 1.77,
                                                               -0.27, _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0,
                                                               5, _Property_8243a735744a4993b94d1750ac0b3167_Out_0,
                                                               _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0,
                                                               _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0 = _MultiplyColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2;
                Unity_Multiply_float(_ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1,
                                     _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0,
                                     _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2;
                Unity_Add_float4(_Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0,
                                 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2,
                                 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_22b7e6c299217e8c995d810ee580d78b_Out_0 = _TintColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb _FinalCombine_18a3c84d4498a188a6a77b7159cd568c;
                _FinalCombine_18a3c84d4498a188a6a77b7159cd568c.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1;
                SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(
                    _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                    _Add_9aa5ce01d828b48c973710b335523c8a_Out_2, _Property_22b7e6c299217e8c995d810ee580d78b_Out_0,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_ba7bf77a544742d588bb64f92f7554da_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3;
                SampleTexture_float(float4(0, 0, 0, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2,
                                    (_UV_ba7bf77a544742d588bb64f92f7554da_Out_0.xy),
                                    _SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_1a397fc2a77b4d1bab2b4b1399a194be_Out_0 = IN.uv3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2;
                Unity_Multiply_float(_SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3,
                                     _UV_1a397fc2a77b4d1bab2b4b1399a194be_Out_0,
                                     _Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.BaseColor = (_FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1.xyz);
                surface.Emission = (_Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2.xyz);
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 unnormalizedNormalWS = input.normalWS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                const float renormFactor = 1.0 / length(unnormalizedNormalWS);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;
                // we want a unit length Normal Vector node in shader graph
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceViewDirection = input.viewDirectionWS;
                //TODO: by default normalized in HD, but not in universal
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ViewSpacePosition = TransformWorldToView(input.positionWS);
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv3 = input.texCoord3;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_POSITION_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_NORMAL_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 viewDirectionWS;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceViewDirection;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ViewSpacePosition;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp4 : TEXCOORD4;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp5 : TEXCOORD5;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.positionWS;
                output.interp1.xyz = input.normalWS;
                output.interp2.xyzw = input.texCoord0;
                output.interp3.xyzw = input.texCoord2;
                output.interp4.xyzw = input.color;
                output.interp5.xyz = input.viewDirectionWS;
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
                output.texCoord0 = input.interp2.xyzw;
                output.texCoord2 = input.interp3.xyzw;
                output.color = input.interp4.xyzw;
                output.viewDirectionWS = input.interp5.xyz;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
            }

            void Unity_Normalize_float3(float3 In, out float3 Out)
            {
                Out = normalize(In);
            }

            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }

            void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
            {
                Out = dot(A, B);
            }

            void Unity_Power_float(float A, float B, out float Out)
            {
                Out = pow(A, B);
            }

            void Unity_Clamp_float(float In, float Min, float Max, out float Out)
            {
                Out = clamp(In, Min, Max);
            }

            void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
            {
                Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            void Unity_Add_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A + B;
            }

            struct Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceViewDirection;
            };

            void SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(UnityTexture2D Texture2D_73021B24,
                                                                float Vector1_1431D892, float Vector1_C2B7F43A,
                                                                float4 Vector4_453A6B1F, float Vector1_55629F98,
                                                                float4 Vector4_752D3319, float3 Vector3_FEFB7F68,
                                                                float4 Color_778878808e8a4165941cd59bf37b29e0,
                                                                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 IN,
                                                                out float4 OutVector4_1)
            {
                float4 _Property_13c5118e874f43ca87025f5b8cbc0834_Out_0 = Color_778878808e8a4165941cd59bf37b29e0;
                UnityTexture2D _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0 = Texture2D_73021B24;
                float3 _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1);
                float3 _Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0 = Vector3_FEFB7F68;
                float3 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_b2e19ca52f945686b2519392081846d0_Out_1);
                float3 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2;
                Unity_Add_float3(_Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0,
                                 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1,
                                 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2);
                float3 _Normalize_c35275e5de48c48a91895e915706d93a_Out_1;
                Unity_Normalize_float3(_Add_59467d0d541a7a8491396b40d6b459c0_Out_2,
                                       _Normalize_c35275e5de48c48a91895e915706d93a_Out_1);
                float _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2;
                Unity_DotProduct_float3(_Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1,
                                        _Normalize_c35275e5de48c48a91895e915706d93a_Out_1,
                                        _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2);
                float _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0 = Vector1_55629F98;
                float _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2;
                Unity_Power_float(_DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2,
                                  _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0,
                                  _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2);
                float _Clamp_f5039b8aba04a988afa60b6204315584_Out_3;
                Unity_Clamp_float(_Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2, 0.38, 0.99,
                                  _Clamp_f5039b8aba04a988afa60b6204315584_Out_3);
                float2 _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0 = float2(
                    _Clamp_f5039b8aba04a988afa60b6204315584_Out_3, 0.5);
                float4 _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.tex,
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.samplerstate,
                    _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0);
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_R_4 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.r;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_G_5 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.g;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_B_6 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.b;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_A_7 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.a;
                float4 _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0 = Vector4_752D3319;
                float4 _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2;
                Unity_Multiply_float(_SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0,
                                     _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0,
                                     _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2);
                UnityTexture2D _Property_1fe3423555394477a6d62590b0d06e66_Out_0 = Texture2D_73021B24;
                float _Property_264a79bba6deba80a88c754a36867c9d_Out_0 = Vector1_1431D892;
                float _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3;
                Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection,
                                          _Property_264a79bba6deba80a88c754a36867c9d_Out_0,
                                          _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3);
                float3 _Property_220a7f36ced91186a16e0b942c69428e_Out_0 = Vector3_FEFB7F68;
                float _Float_b30529d24ed97481877a14515ddbb316_Out_0 = -1;
                float3 _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1);
                float3 _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2;
                Unity_Multiply_float((_Float_b30529d24ed97481877a14515ddbb316_Out_0.xxx),
                                     _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1,
                                     _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2);
                float3 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1);
                float3 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2;
                Unity_Add_float3(_Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2,
                                 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1,
                                 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2);
                float3 _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1;
                Unity_Normalize_float3(_Add_024c1620de06b1808a8886cfa5f69c1d_Out_2,
                                       _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1);
                float _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2;
                Unity_DotProduct_float3(_Property_220a7f36ced91186a16e0b942c69428e_Out_0,
                                        _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1,
                                        _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2);
                float _Property_31359727423ffa80bc89eacf7e7596a9_Out_0 = Vector1_C2B7F43A;
                float2 _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0 = float2(
                    1, _Property_31359727423ffa80bc89eacf7e7596a9_Out_0);
                float _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3;
                Unity_Remap_float(_DotProduct_759d147c71911b81979499b9567e8b0b_Out_2, float2(-1, 1),
                                  _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0,
                                  _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3);
                float _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2;
                Unity_Multiply_float(_FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3,
                                     _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3,
                                     _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2);
                float _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3;
                Unity_Remap_float(_Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3);
                float2 _Vector2_902927a430b8848cac4001e2b7162a76_Out_0 = float2(
                    _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3, 0.99);
                float4 _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.tex,
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.samplerstate,
                    _Vector2_902927a430b8848cac4001e2b7162a76_Out_0);
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_R_4 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.r;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_G_5 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.g;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_B_6 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.b;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_A_7 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.a;
                float4 _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0 = Vector4_453A6B1F;
                float4 _Multiply_313986370303ba8c966db164632517d4_Out_2;
                Unity_Multiply_float(_SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0,
                                     _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0,
                                     _Multiply_313986370303ba8c966db164632517d4_Out_2);
                UnityTexture2D _Property_48552ab976c9128589df548370016ff3_Out_0 = Texture2D_73021B24;
                float3 _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0 = Vector3_FEFB7F68;
                float _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2;
                Unity_DotProduct_float3(IN.WorldSpaceNormal, _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0,
                                        _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2);
                float _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3;
                Unity_Remap_float(_DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3);
                float _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0 = 0;
                float2 _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0 = float2(
                    _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3, _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0);
                float4 _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_48552ab976c9128589df548370016ff3_Out_0.tex,
                    _Property_48552ab976c9128589df548370016ff3_Out_0.samplerstate,
                    _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0);
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_R_4 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.r;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_G_5 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.g;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_B_6 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.b;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_A_7 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.a;
                float4 Color_1caa66260b89b684a08e41b09e33f75d = IsGammaSpace()
                                                                    ? float4(0.7075472, 0.7075472, 0.7075472, 0)
                                                                    : float4(SRGBToLinear(
                                                                                 float3(0.7075472, 0.7075472,
                                                                                     0.7075472)), 0);
                float4 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2;
                Unity_Multiply_float(_SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0,
                                     Color_1caa66260b89b684a08e41b09e33f75d,
                                     _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2);
                float4 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2;
                Unity_Add_float4(_Multiply_313986370303ba8c966db164632517d4_Out_2,
                                 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2);
                float4 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2;
                Unity_Add_float4(_Multiply_9a724e75d670828c932f94803fda8a1e_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2,
                                 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2);
                float4 _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
                Unity_Multiply_float(_Property_13c5118e874f43ca87025f5b8cbc0834_Out_0,
                                     _Add_5adf131eb3b73a82810f17cdc8575166_Out_2,
                                     _Multiply_359a706b4442918d924cabd0309779e3_Out_2);
                OutVector4_1 = _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
            }

            void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base * Blend;
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
            {
                SHADERGRAPH_FOG(Position, Color, Density);
            }

            void Unity_OneMinus_float(float In, out float Out)
            {
                Out = 1 - In;
            }

            void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
            {
                Out = lerp(A, B, T);
            }

            struct Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9
            {
                float3 ViewSpacePosition;
            };

            void SG_Fog_db57d56e4661e4144b06df0b3edef8a9(float4 Color_42779DA4,
                                                         Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 IN,
                                                         out float4 Color_1)
            {
                float4 _Property_92d920f66f871787b8efd9892e387049_Out_0 = Color_42779DA4;
                float4 _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0;
                float _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1;
                Unity_Fog_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1, IN.ViewSpacePosition);
                float _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1;
                Unity_OneMinus_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Density_1,
                                     _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1);
                float4 _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
                Unity_Lerp_float4(_Property_92d920f66f871787b8efd9892e387049_Out_0,
                                  _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                  (_OneMinus_5194723b380f67819cad19ed50e5789e_Out_1.xxxx),
                                  _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3);
                Color_1 = _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
            }

            struct Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
            {
                float3 ViewSpacePosition;
            };

            void SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(float4 Color_D4F585C6, float4 Color_D7818A04,
                                                                  float4 Color_546468F9,
                                                                  Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
                                                                  IN, out float4 FinalColor_1)
            {
                float4 _Property_519fcfdd306ddb839c113ca07f0f606c_Out_0 = Color_D4F585C6;
                float4 _Property_3e015b35bff76284853220d6ed019e2e_Out_0 = Color_546468F9;
                float4 _Property_ae789b17ad00778693ee4740849f6c53_Out_0 = Color_D7818A04;
                float4 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2;
                Unity_Add_float4(_Property_3e015b35bff76284853220d6ed019e2e_Out_0,
                                 _Property_ae789b17ad00778693ee4740849f6c53_Out_0,
                                 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2);
                float4 _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2;
                Unity_Blend_Multiply_float4(_Property_519fcfdd306ddb839c113ca07f0f606c_Out_0,
                                            _Add_9c1c74d0fa17908ca9503658a239355d_Out_2,
                                            _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2, 1);
                Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 _Fog_18fab81a74b9d7858633e8fc73111a15;
                _Fog_18fab81a74b9d7858633e8fc73111a15.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
                SG_Fog_db57d56e4661e4144b06df0b3edef8a9(_Blend_4690b56c61915281a6230c45fd4ae04b_Out_2,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1);
                FinalColor_1 = _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0 = _AddColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                UnityTexture2D _Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0 = UnityBuildTexture2DStructNoScale(
                    _MatCap);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0 = IsGammaSpace()
                                                                              ? LinearToSRGB(_FresnelIntensity)
                                                                              : _FresnelIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_8243a735744a4993b94d1750ac0b3167_Out_0 = _GlossIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0 = _LightDir;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0 = _LightColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 _ToonShader_7540b263a099a28799339fadc2a1d8ac;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceNormal = IN.WorldSpaceNormal;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceViewDirection = IN.WorldSpaceViewDirection;
                float4 _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1;
                SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(_Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0, 1.77,
                                                               -0.27, _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0,
                                                               5, _Property_8243a735744a4993b94d1750ac0b3167_Out_0,
                                                               _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0,
                                                               _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0 = _MultiplyColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2;
                Unity_Multiply_float(_ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1,
                                     _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0,
                                     _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2;
                Unity_Add_float4(_Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0,
                                 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2,
                                 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_22b7e6c299217e8c995d810ee580d78b_Out_0 = _TintColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb _FinalCombine_18a3c84d4498a188a6a77b7159cd568c;
                _FinalCombine_18a3c84d4498a188a6a77b7159cd568c.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1;
                SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(
                    _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                    _Add_9aa5ce01d828b48c973710b335523c8a_Out_2, _Property_22b7e6c299217e8c995d810ee580d78b_Out_0,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.BaseColor = (_FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1.xyz);
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 unnormalizedNormalWS = input.normalWS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                const float renormFactor = 1.0 / length(unnormalizedNormalWS);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;
                // we want a unit length Normal Vector node in shader graph
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceViewDirection = input.viewDirectionWS;
                //TODO: by default normalized in HD, but not in universal
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ViewSpacePosition = TransformWorldToView(input.positionWS);
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"
            ENDHLSL
        }
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // Render State
            Cull [_Cull]
            Blend [_SrcBlend] [_DstBlend]
            ZTest LEqual
            ZWrite [_ZWrite]

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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD3
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_POSITION_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_NORMAL_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TANGENT_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD3
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 viewDirectionWS;
                #endif
                #if defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            float2 lightmapUV;
                #endif
                #endif
                #if !defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 sh;
                #endif
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 fogFactorAndVertexLight;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 shadowCoord;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 TangentSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceViewDirection;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ViewSpacePosition;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp4 : TEXCOORD4;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp5 : TEXCOORD5;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp6 : TEXCOORD6;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp7 : TEXCOORD7;
                #endif
                #if defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            float2 interp8 : TEXCOORD8;
                #endif
                #endif
                #if !defined(LIGHTMAP_ON)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp9 : TEXCOORD9;
                #endif
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp10 : TEXCOORD10;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp11 : TEXCOORD11;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.positionWS;
                output.interp1.xyz = input.normalWS;
                output.interp2.xyzw = input.tangentWS;
                output.interp3.xyzw = input.texCoord0;
                output.interp4.xyzw = input.texCoord2;
                output.interp5.xyzw = input.texCoord3;
                output.interp6.xyzw = input.color;
                output.interp7.xyz = input.viewDirectionWS;
                #if defined(LIGHTMAP_ON)
            output.interp8.xy =  input.lightmapUV;
                #endif
                #if !defined(LIGHTMAP_ON)
                output.interp9.xyz = input.sh;
                #endif
                output.interp10.xyzw = input.fogFactorAndVertexLight;
                output.interp11.xyzw = input.shadowCoord;
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
                output.texCoord2 = input.interp4.xyzw;
                output.texCoord3 = input.interp5.xyzw;
                output.color = input.interp6.xyzw;
                output.viewDirectionWS = input.interp7.xyz;
                #if defined(LIGHTMAP_ON)
            output.lightmapUV = input.interp8.xy;
                #endif
                #if !defined(LIGHTMAP_ON)
                output.sh = input.interp9.xyz;
                #endif
                output.fogFactorAndVertexLight = input.interp10.xyzw;
                output.shadowCoord = input.interp11.xyzw;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
            }

            void Unity_Normalize_float3(float3 In, out float3 Out)
            {
                Out = normalize(In);
            }

            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }

            void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
            {
                Out = dot(A, B);
            }

            void Unity_Power_float(float A, float B, out float Out)
            {
                Out = pow(A, B);
            }

            void Unity_Clamp_float(float In, float Min, float Max, out float Out)
            {
                Out = clamp(In, Min, Max);
            }

            void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
            {
                Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            void Unity_Add_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A + B;
            }

            struct Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceViewDirection;
            };

            void SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(UnityTexture2D Texture2D_73021B24,
                                                                float Vector1_1431D892, float Vector1_C2B7F43A,
                                                                float4 Vector4_453A6B1F, float Vector1_55629F98,
                                                                float4 Vector4_752D3319, float3 Vector3_FEFB7F68,
                                                                float4 Color_778878808e8a4165941cd59bf37b29e0,
                                                                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 IN,
                                                                out float4 OutVector4_1)
            {
                float4 _Property_13c5118e874f43ca87025f5b8cbc0834_Out_0 = Color_778878808e8a4165941cd59bf37b29e0;
                UnityTexture2D _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0 = Texture2D_73021B24;
                float3 _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1);
                float3 _Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0 = Vector3_FEFB7F68;
                float3 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_b2e19ca52f945686b2519392081846d0_Out_1);
                float3 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2;
                Unity_Add_float3(_Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0,
                                 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1,
                                 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2);
                float3 _Normalize_c35275e5de48c48a91895e915706d93a_Out_1;
                Unity_Normalize_float3(_Add_59467d0d541a7a8491396b40d6b459c0_Out_2,
                                       _Normalize_c35275e5de48c48a91895e915706d93a_Out_1);
                float _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2;
                Unity_DotProduct_float3(_Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1,
                                        _Normalize_c35275e5de48c48a91895e915706d93a_Out_1,
                                        _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2);
                float _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0 = Vector1_55629F98;
                float _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2;
                Unity_Power_float(_DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2,
                                  _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0,
                                  _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2);
                float _Clamp_f5039b8aba04a988afa60b6204315584_Out_3;
                Unity_Clamp_float(_Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2, 0.38, 0.99,
                                  _Clamp_f5039b8aba04a988afa60b6204315584_Out_3);
                float2 _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0 = float2(
                    _Clamp_f5039b8aba04a988afa60b6204315584_Out_3, 0.5);
                float4 _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.tex,
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.samplerstate,
                    _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0);
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_R_4 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.r;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_G_5 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.g;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_B_6 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.b;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_A_7 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.a;
                float4 _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0 = Vector4_752D3319;
                float4 _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2;
                Unity_Multiply_float(_SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0,
                                     _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0,
                                     _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2);
                UnityTexture2D _Property_1fe3423555394477a6d62590b0d06e66_Out_0 = Texture2D_73021B24;
                float _Property_264a79bba6deba80a88c754a36867c9d_Out_0 = Vector1_1431D892;
                float _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3;
                Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection,
                                          _Property_264a79bba6deba80a88c754a36867c9d_Out_0,
                                          _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3);
                float3 _Property_220a7f36ced91186a16e0b942c69428e_Out_0 = Vector3_FEFB7F68;
                float _Float_b30529d24ed97481877a14515ddbb316_Out_0 = -1;
                float3 _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1);
                float3 _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2;
                Unity_Multiply_float((_Float_b30529d24ed97481877a14515ddbb316_Out_0.xxx),
                                     _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1,
                                     _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2);
                float3 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1);
                float3 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2;
                Unity_Add_float3(_Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2,
                                 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1,
                                 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2);
                float3 _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1;
                Unity_Normalize_float3(_Add_024c1620de06b1808a8886cfa5f69c1d_Out_2,
                                       _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1);
                float _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2;
                Unity_DotProduct_float3(_Property_220a7f36ced91186a16e0b942c69428e_Out_0,
                                        _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1,
                                        _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2);
                float _Property_31359727423ffa80bc89eacf7e7596a9_Out_0 = Vector1_C2B7F43A;
                float2 _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0 = float2(
                    1, _Property_31359727423ffa80bc89eacf7e7596a9_Out_0);
                float _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3;
                Unity_Remap_float(_DotProduct_759d147c71911b81979499b9567e8b0b_Out_2, float2(-1, 1),
                                  _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0,
                                  _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3);
                float _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2;
                Unity_Multiply_float(_FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3,
                                     _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3,
                                     _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2);
                float _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3;
                Unity_Remap_float(_Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3);
                float2 _Vector2_902927a430b8848cac4001e2b7162a76_Out_0 = float2(
                    _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3, 0.99);
                float4 _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.tex,
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.samplerstate,
                    _Vector2_902927a430b8848cac4001e2b7162a76_Out_0);
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_R_4 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.r;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_G_5 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.g;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_B_6 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.b;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_A_7 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.a;
                float4 _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0 = Vector4_453A6B1F;
                float4 _Multiply_313986370303ba8c966db164632517d4_Out_2;
                Unity_Multiply_float(_SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0,
                                     _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0,
                                     _Multiply_313986370303ba8c966db164632517d4_Out_2);
                UnityTexture2D _Property_48552ab976c9128589df548370016ff3_Out_0 = Texture2D_73021B24;
                float3 _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0 = Vector3_FEFB7F68;
                float _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2;
                Unity_DotProduct_float3(IN.WorldSpaceNormal, _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0,
                                        _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2);
                float _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3;
                Unity_Remap_float(_DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3);
                float _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0 = 0;
                float2 _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0 = float2(
                    _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3, _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0);
                float4 _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_48552ab976c9128589df548370016ff3_Out_0.tex,
                    _Property_48552ab976c9128589df548370016ff3_Out_0.samplerstate,
                    _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0);
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_R_4 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.r;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_G_5 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.g;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_B_6 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.b;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_A_7 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.a;
                float4 Color_1caa66260b89b684a08e41b09e33f75d = IsGammaSpace()
                                                                    ? float4(0.7075472, 0.7075472, 0.7075472, 0)
                                                                    : float4(SRGBToLinear(
                                                                                 float3(0.7075472, 0.7075472,
                                                                                     0.7075472)), 0);
                float4 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2;
                Unity_Multiply_float(_SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0,
                                     Color_1caa66260b89b684a08e41b09e33f75d,
                                     _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2);
                float4 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2;
                Unity_Add_float4(_Multiply_313986370303ba8c966db164632517d4_Out_2,
                                 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2);
                float4 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2;
                Unity_Add_float4(_Multiply_9a724e75d670828c932f94803fda8a1e_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2,
                                 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2);
                float4 _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
                Unity_Multiply_float(_Property_13c5118e874f43ca87025f5b8cbc0834_Out_0,
                                     _Add_5adf131eb3b73a82810f17cdc8575166_Out_2,
                                     _Multiply_359a706b4442918d924cabd0309779e3_Out_2);
                OutVector4_1 = _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
            }

            void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base * Blend;
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
            {
                SHADERGRAPH_FOG(Position, Color, Density);
            }

            void Unity_OneMinus_float(float In, out float Out)
            {
                Out = 1 - In;
            }

            void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
            {
                Out = lerp(A, B, T);
            }

            struct Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9
            {
                float3 ViewSpacePosition;
            };

            void SG_Fog_db57d56e4661e4144b06df0b3edef8a9(float4 Color_42779DA4,
                                                         Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 IN,
                                                         out float4 Color_1)
            {
                float4 _Property_92d920f66f871787b8efd9892e387049_Out_0 = Color_42779DA4;
                float4 _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0;
                float _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1;
                Unity_Fog_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1, IN.ViewSpacePosition);
                float _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1;
                Unity_OneMinus_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Density_1,
                                     _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1);
                float4 _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
                Unity_Lerp_float4(_Property_92d920f66f871787b8efd9892e387049_Out_0,
                                  _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                  (_OneMinus_5194723b380f67819cad19ed50e5789e_Out_1.xxxx),
                                  _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3);
                Color_1 = _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
            }

            struct Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
            {
                float3 ViewSpacePosition;
            };

            void SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(float4 Color_D4F585C6, float4 Color_D7818A04,
                                                                  float4 Color_546468F9,
                                                                  Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
                                                                  IN, out float4 FinalColor_1)
            {
                float4 _Property_519fcfdd306ddb839c113ca07f0f606c_Out_0 = Color_D4F585C6;
                float4 _Property_3e015b35bff76284853220d6ed019e2e_Out_0 = Color_546468F9;
                float4 _Property_ae789b17ad00778693ee4740849f6c53_Out_0 = Color_D7818A04;
                float4 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2;
                Unity_Add_float4(_Property_3e015b35bff76284853220d6ed019e2e_Out_0,
                                 _Property_ae789b17ad00778693ee4740849f6c53_Out_0,
                                 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2);
                float4 _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2;
                Unity_Blend_Multiply_float4(_Property_519fcfdd306ddb839c113ca07f0f606c_Out_0,
                                            _Add_9c1c74d0fa17908ca9503658a239355d_Out_2,
                                            _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2, 1);
                Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 _Fog_18fab81a74b9d7858633e8fc73111a15;
                _Fog_18fab81a74b9d7858633e8fc73111a15.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
                SG_Fog_db57d56e4661e4144b06df0b3edef8a9(_Blend_4690b56c61915281a6230c45fd4ae04b_Out_2,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1);
                FinalColor_1 = _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0 = _AddColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                UnityTexture2D _Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0 = UnityBuildTexture2DStructNoScale(
                    _MatCap);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0 = IsGammaSpace()
                                                                              ? LinearToSRGB(_FresnelIntensity)
                                                                              : _FresnelIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_8243a735744a4993b94d1750ac0b3167_Out_0 = _GlossIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0 = _LightDir;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0 = _LightColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 _ToonShader_7540b263a099a28799339fadc2a1d8ac;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceNormal = IN.WorldSpaceNormal;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceViewDirection = IN.WorldSpaceViewDirection;
                float4 _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1;
                SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(_Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0, 1.77,
                                                               -0.27, _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0,
                                                               5, _Property_8243a735744a4993b94d1750ac0b3167_Out_0,
                                                               _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0,
                                                               _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0 = _MultiplyColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2;
                Unity_Multiply_float(_ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1,
                                     _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0,
                                     _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2;
                Unity_Add_float4(_Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0,
                                 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2,
                                 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_22b7e6c299217e8c995d810ee580d78b_Out_0 = _TintColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb _FinalCombine_18a3c84d4498a188a6a77b7159cd568c;
                _FinalCombine_18a3c84d4498a188a6a77b7159cd568c.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1;
                SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(
                    _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                    _Add_9aa5ce01d828b48c973710b335523c8a_Out_2, _Property_22b7e6c299217e8c995d810ee580d78b_Out_0,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_ba7bf77a544742d588bb64f92f7554da_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3;
                SampleTexture_float(float4(0, 0, 0, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2,
                                    (_UV_ba7bf77a544742d588bb64f92f7554da_Out_0.xy),
                                    _SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_1a397fc2a77b4d1bab2b4b1399a194be_Out_0 = IN.uv3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2;
                Unity_Multiply_float(_SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3,
                                     _UV_1a397fc2a77b4d1bab2b4b1399a194be_Out_0,
                                     _Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.BaseColor = (_FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1.xyz);
                surface.NormalTS = IN.TangentSpaceNormal;
                surface.Emission = (_Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2.xyz);
                surface.Metallic = 0;
                surface.Smoothness = 0;
                surface.Occlusion = 1;
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 unnormalizedNormalWS = input.normalWS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                const float renormFactor = 1.0 / length(unnormalizedNormalWS);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;
                // we want a unit length Normal Vector node in shader graph
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceViewDirection = input.viewDirectionWS;
                //TODO: by default normalized in HD, but not in universal
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ViewSpacePosition = TransformWorldToView(input.positionWS);
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv3 = input.texCoord3;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #include "PBRForwardPass.hlsl"
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyzw = input.texCoord0;
                output.interp1.xyzw = input.texCoord2;
                output.interp2.xyzw = input.color;
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
                output.texCoord2 = input.interp1.xyzw;
                output.color = input.interp2.xyzw;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyzw = input.texCoord0;
                output.interp1.xyzw = input.texCoord2;
                output.interp2.xyzw = input.color;
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
                output.texCoord2 = input.interp1.xyzw;
                output.color = input.interp2.xyzw;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_NORMAL_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TANGENT_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 TangentSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp4 : TEXCOORD4;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.normalWS;
                output.interp1.xyzw = input.tangentWS;
                output.interp2.xyzw = input.texCoord0;
                output.interp3.xyzw = input.texCoord2;
                output.interp4.xyzw = input.color;
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
                output.texCoord0 = input.interp2.xyzw;
                output.texCoord2 = input.interp3.xyzw;
                output.color = input.interp4.xyzw;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.NormalTS = IN.TangentSpaceNormal;
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD3
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_POSITION_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_NORMAL_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD3
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 viewDirectionWS;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceViewDirection;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ViewSpacePosition;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp4 : TEXCOORD4;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp5 : TEXCOORD5;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp6 : TEXCOORD6;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.positionWS;
                output.interp1.xyz = input.normalWS;
                output.interp2.xyzw = input.texCoord0;
                output.interp3.xyzw = input.texCoord2;
                output.interp4.xyzw = input.texCoord3;
                output.interp5.xyzw = input.color;
                output.interp6.xyz = input.viewDirectionWS;
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
                output.texCoord0 = input.interp2.xyzw;
                output.texCoord2 = input.interp3.xyzw;
                output.texCoord3 = input.interp4.xyzw;
                output.color = input.interp5.xyzw;
                output.viewDirectionWS = input.interp6.xyz;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
            }

            void Unity_Normalize_float3(float3 In, out float3 Out)
            {
                Out = normalize(In);
            }

            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }

            void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
            {
                Out = dot(A, B);
            }

            void Unity_Power_float(float A, float B, out float Out)
            {
                Out = pow(A, B);
            }

            void Unity_Clamp_float(float In, float Min, float Max, out float Out)
            {
                Out = clamp(In, Min, Max);
            }

            void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
            {
                Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            void Unity_Add_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A + B;
            }

            struct Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceViewDirection;
            };

            void SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(UnityTexture2D Texture2D_73021B24,
                                                                float Vector1_1431D892, float Vector1_C2B7F43A,
                                                                float4 Vector4_453A6B1F, float Vector1_55629F98,
                                                                float4 Vector4_752D3319, float3 Vector3_FEFB7F68,
                                                                float4 Color_778878808e8a4165941cd59bf37b29e0,
                                                                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 IN,
                                                                out float4 OutVector4_1)
            {
                float4 _Property_13c5118e874f43ca87025f5b8cbc0834_Out_0 = Color_778878808e8a4165941cd59bf37b29e0;
                UnityTexture2D _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0 = Texture2D_73021B24;
                float3 _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1);
                float3 _Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0 = Vector3_FEFB7F68;
                float3 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_b2e19ca52f945686b2519392081846d0_Out_1);
                float3 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2;
                Unity_Add_float3(_Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0,
                                 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1,
                                 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2);
                float3 _Normalize_c35275e5de48c48a91895e915706d93a_Out_1;
                Unity_Normalize_float3(_Add_59467d0d541a7a8491396b40d6b459c0_Out_2,
                                       _Normalize_c35275e5de48c48a91895e915706d93a_Out_1);
                float _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2;
                Unity_DotProduct_float3(_Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1,
                                        _Normalize_c35275e5de48c48a91895e915706d93a_Out_1,
                                        _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2);
                float _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0 = Vector1_55629F98;
                float _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2;
                Unity_Power_float(_DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2,
                                  _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0,
                                  _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2);
                float _Clamp_f5039b8aba04a988afa60b6204315584_Out_3;
                Unity_Clamp_float(_Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2, 0.38, 0.99,
                                  _Clamp_f5039b8aba04a988afa60b6204315584_Out_3);
                float2 _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0 = float2(
                    _Clamp_f5039b8aba04a988afa60b6204315584_Out_3, 0.5);
                float4 _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.tex,
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.samplerstate,
                    _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0);
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_R_4 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.r;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_G_5 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.g;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_B_6 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.b;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_A_7 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.a;
                float4 _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0 = Vector4_752D3319;
                float4 _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2;
                Unity_Multiply_float(_SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0,
                                     _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0,
                                     _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2);
                UnityTexture2D _Property_1fe3423555394477a6d62590b0d06e66_Out_0 = Texture2D_73021B24;
                float _Property_264a79bba6deba80a88c754a36867c9d_Out_0 = Vector1_1431D892;
                float _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3;
                Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection,
                                          _Property_264a79bba6deba80a88c754a36867c9d_Out_0,
                                          _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3);
                float3 _Property_220a7f36ced91186a16e0b942c69428e_Out_0 = Vector3_FEFB7F68;
                float _Float_b30529d24ed97481877a14515ddbb316_Out_0 = -1;
                float3 _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1);
                float3 _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2;
                Unity_Multiply_float((_Float_b30529d24ed97481877a14515ddbb316_Out_0.xxx),
                                     _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1,
                                     _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2);
                float3 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1);
                float3 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2;
                Unity_Add_float3(_Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2,
                                 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1,
                                 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2);
                float3 _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1;
                Unity_Normalize_float3(_Add_024c1620de06b1808a8886cfa5f69c1d_Out_2,
                                       _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1);
                float _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2;
                Unity_DotProduct_float3(_Property_220a7f36ced91186a16e0b942c69428e_Out_0,
                                        _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1,
                                        _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2);
                float _Property_31359727423ffa80bc89eacf7e7596a9_Out_0 = Vector1_C2B7F43A;
                float2 _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0 = float2(
                    1, _Property_31359727423ffa80bc89eacf7e7596a9_Out_0);
                float _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3;
                Unity_Remap_float(_DotProduct_759d147c71911b81979499b9567e8b0b_Out_2, float2(-1, 1),
                                  _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0,
                                  _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3);
                float _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2;
                Unity_Multiply_float(_FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3,
                                     _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3,
                                     _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2);
                float _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3;
                Unity_Remap_float(_Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3);
                float2 _Vector2_902927a430b8848cac4001e2b7162a76_Out_0 = float2(
                    _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3, 0.99);
                float4 _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.tex,
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.samplerstate,
                    _Vector2_902927a430b8848cac4001e2b7162a76_Out_0);
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_R_4 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.r;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_G_5 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.g;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_B_6 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.b;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_A_7 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.a;
                float4 _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0 = Vector4_453A6B1F;
                float4 _Multiply_313986370303ba8c966db164632517d4_Out_2;
                Unity_Multiply_float(_SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0,
                                     _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0,
                                     _Multiply_313986370303ba8c966db164632517d4_Out_2);
                UnityTexture2D _Property_48552ab976c9128589df548370016ff3_Out_0 = Texture2D_73021B24;
                float3 _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0 = Vector3_FEFB7F68;
                float _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2;
                Unity_DotProduct_float3(IN.WorldSpaceNormal, _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0,
                                        _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2);
                float _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3;
                Unity_Remap_float(_DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3);
                float _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0 = 0;
                float2 _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0 = float2(
                    _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3, _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0);
                float4 _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_48552ab976c9128589df548370016ff3_Out_0.tex,
                    _Property_48552ab976c9128589df548370016ff3_Out_0.samplerstate,
                    _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0);
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_R_4 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.r;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_G_5 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.g;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_B_6 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.b;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_A_7 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.a;
                float4 Color_1caa66260b89b684a08e41b09e33f75d = IsGammaSpace()
                                                                    ? float4(0.7075472, 0.7075472, 0.7075472, 0)
                                                                    : float4(SRGBToLinear(
                                                                                 float3(0.7075472, 0.7075472,
                                                                                     0.7075472)), 0);
                float4 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2;
                Unity_Multiply_float(_SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0,
                                     Color_1caa66260b89b684a08e41b09e33f75d,
                                     _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2);
                float4 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2;
                Unity_Add_float4(_Multiply_313986370303ba8c966db164632517d4_Out_2,
                                 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2);
                float4 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2;
                Unity_Add_float4(_Multiply_9a724e75d670828c932f94803fda8a1e_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2,
                                 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2);
                float4 _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
                Unity_Multiply_float(_Property_13c5118e874f43ca87025f5b8cbc0834_Out_0,
                                     _Add_5adf131eb3b73a82810f17cdc8575166_Out_2,
                                     _Multiply_359a706b4442918d924cabd0309779e3_Out_2);
                OutVector4_1 = _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
            }

            void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base * Blend;
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
            {
                SHADERGRAPH_FOG(Position, Color, Density);
            }

            void Unity_OneMinus_float(float In, out float Out)
            {
                Out = 1 - In;
            }

            void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
            {
                Out = lerp(A, B, T);
            }

            struct Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9
            {
                float3 ViewSpacePosition;
            };

            void SG_Fog_db57d56e4661e4144b06df0b3edef8a9(float4 Color_42779DA4,
                                                         Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 IN,
                                                         out float4 Color_1)
            {
                float4 _Property_92d920f66f871787b8efd9892e387049_Out_0 = Color_42779DA4;
                float4 _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0;
                float _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1;
                Unity_Fog_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1, IN.ViewSpacePosition);
                float _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1;
                Unity_OneMinus_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Density_1,
                                     _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1);
                float4 _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
                Unity_Lerp_float4(_Property_92d920f66f871787b8efd9892e387049_Out_0,
                                  _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                  (_OneMinus_5194723b380f67819cad19ed50e5789e_Out_1.xxxx),
                                  _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3);
                Color_1 = _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
            }

            struct Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
            {
                float3 ViewSpacePosition;
            };

            void SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(float4 Color_D4F585C6, float4 Color_D7818A04,
                                                                  float4 Color_546468F9,
                                                                  Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
                                                                  IN, out float4 FinalColor_1)
            {
                float4 _Property_519fcfdd306ddb839c113ca07f0f606c_Out_0 = Color_D4F585C6;
                float4 _Property_3e015b35bff76284853220d6ed019e2e_Out_0 = Color_546468F9;
                float4 _Property_ae789b17ad00778693ee4740849f6c53_Out_0 = Color_D7818A04;
                float4 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2;
                Unity_Add_float4(_Property_3e015b35bff76284853220d6ed019e2e_Out_0,
                                 _Property_ae789b17ad00778693ee4740849f6c53_Out_0,
                                 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2);
                float4 _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2;
                Unity_Blend_Multiply_float4(_Property_519fcfdd306ddb839c113ca07f0f606c_Out_0,
                                            _Add_9c1c74d0fa17908ca9503658a239355d_Out_2,
                                            _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2, 1);
                Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 _Fog_18fab81a74b9d7858633e8fc73111a15;
                _Fog_18fab81a74b9d7858633e8fc73111a15.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
                SG_Fog_db57d56e4661e4144b06df0b3edef8a9(_Blend_4690b56c61915281a6230c45fd4ae04b_Out_2,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1);
                FinalColor_1 = _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0 = _AddColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                UnityTexture2D _Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0 = UnityBuildTexture2DStructNoScale(
                    _MatCap);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0 = IsGammaSpace()
                                                                              ? LinearToSRGB(_FresnelIntensity)
                                                                              : _FresnelIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_8243a735744a4993b94d1750ac0b3167_Out_0 = _GlossIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0 = _LightDir;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0 = _LightColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 _ToonShader_7540b263a099a28799339fadc2a1d8ac;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceNormal = IN.WorldSpaceNormal;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceViewDirection = IN.WorldSpaceViewDirection;
                float4 _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1;
                SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(_Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0, 1.77,
                                                               -0.27, _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0,
                                                               5, _Property_8243a735744a4993b94d1750ac0b3167_Out_0,
                                                               _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0,
                                                               _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0 = _MultiplyColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2;
                Unity_Multiply_float(_ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1,
                                     _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0,
                                     _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2;
                Unity_Add_float4(_Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0,
                                 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2,
                                 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_22b7e6c299217e8c995d810ee580d78b_Out_0 = _TintColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb _FinalCombine_18a3c84d4498a188a6a77b7159cd568c;
                _FinalCombine_18a3c84d4498a188a6a77b7159cd568c.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1;
                SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(
                    _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                    _Add_9aa5ce01d828b48c973710b335523c8a_Out_2, _Property_22b7e6c299217e8c995d810ee580d78b_Out_0,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_ba7bf77a544742d588bb64f92f7554da_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3;
                SampleTexture_float(float4(0, 0, 0, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2,
                                    (_UV_ba7bf77a544742d588bb64f92f7554da_Out_0.xy),
                                    _SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_1a397fc2a77b4d1bab2b4b1399a194be_Out_0 = IN.uv3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2;
                Unity_Multiply_float(_SampleTextureCustomFunction_2a29fd02ac7c4af590e370b851dc2a97_Color_3,
                                     _UV_1a397fc2a77b4d1bab2b4b1399a194be_Out_0,
                                     _Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.BaseColor = (_FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1.xyz);
                surface.Emission = (_Multiply_e524b054cb554b839508e0ecae2bcdc0_Out_2.xyz);
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 unnormalizedNormalWS = input.normalWS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                const float renormFactor = 1.0 / length(unnormalizedNormalWS);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;
                // we want a unit length Normal Vector node in shader graph
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceViewDirection = input.viewDirectionWS;
                //TODO: by default normalized in HD, but not in universal
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ViewSpacePosition = TransformWorldToView(input.positionWS);
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv3 = input.texCoord3;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #pragma shader_feature _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

            #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_0
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_1
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_2
            #elif defined(_MAIN_LIGHT_SHADOWS_CASCADE)
            #define KEYWORD_PERMUTATION_3
            #elif defined(_SHADOWS_SOFT) && defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_4
            #elif defined(_SHADOWS_SOFT)
            #define KEYWORD_PERMUTATION_5
            #elif defined(_RECEIVE_SHADOWS_OFF)
            #define KEYWORD_PERMUTATION_6
            #else
            #define KEYWORD_PERMUTATION_7
            #endif


            // Defines
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _AlphaClip 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMALMAP 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define _NORMAL_DROPOFF_TS 1
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_NORMAL
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TANGENT
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define ATTRIBUTES_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_POSITION_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_NORMAL_WS
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD0
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_TEXCOORD2
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_COLOR
            #endif

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #endif

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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionOS : POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalOS : NORMAL;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 tangentOS : TANGENT;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color : COLOR;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };

            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 positionWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 normalWS;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 color;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 viewDirectionWS;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 WorldSpaceViewDirection;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ViewSpacePosition;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 VertexColor;
                #endif
            };

            struct VertexDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceNormal;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpaceTangent;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 ObjectSpacePosition;
                #endif
            };

            struct PackedVaryings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp0 : TEXCOORD0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp1 : TEXCOORD1;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp2 : TEXCOORD2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp3 : TEXCOORD3;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 interp4 : TEXCOORD4;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 interp5 : TEXCOORD5;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };

            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.positionWS;
                output.interp1.xyz = input.normalWS;
                output.interp2.xyzw = input.texCoord0;
                output.interp3.xyzw = input.texCoord2;
                output.interp4.xyzw = input.color;
                output.interp5.xyz = input.viewDirectionWS;
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
                output.texCoord0 = input.interp2.xyzw;
                output.texCoord2 = input.interp3.xyzw;
                output.color = input.interp4.xyzw;
                output.viewDirectionWS = input.interp5.xyz;
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
            float4 _MatCap_TexelSize;
            float4 _GlossIntensity;
            float4 _MultiplyColor;
            float4 _AddColor;
            float3 _LightDir;
            float4 _LightColor;
            float4 _FresnelIntensity;
            float4 _AvatarMap1_TexelSize;
            float4 _AvatarMap2_TexelSize;
            float4 _AvatarMap3_TexelSize;
            float4 _AvatarMap4_TexelSize;
            float4 _AvatarMap5_TexelSize;
            float4 _AvatarMap6_TexelSize;
            float4 _AvatarMap7_TexelSize;
            float4 _AvatarMap8_TexelSize;
            float4 _AvatarMap9_TexelSize;
            float4 _AvatarMap10_TexelSize;
            float4 _AvatarMap11_TexelSize;
            float4 _AvatarMap12_TexelSize;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);
            float4 _TintColor;
            TEXTURE2D(_AvatarMap1);
            SAMPLER(sampler_AvatarMap1);
            TEXTURE2D(_AvatarMap2);
            SAMPLER(sampler_AvatarMap2);
            TEXTURE2D(_AvatarMap3);
            SAMPLER(sampler_AvatarMap3);
            TEXTURE2D(_AvatarMap4);
            SAMPLER(sampler_AvatarMap4);
            TEXTURE2D(_AvatarMap5);
            SAMPLER(sampler_AvatarMap5);
            TEXTURE2D(_AvatarMap6);
            SAMPLER(sampler_AvatarMap6);
            TEXTURE2D(_AvatarMap7);
            SAMPLER(sampler_AvatarMap7);
            TEXTURE2D(_AvatarMap8);
            SAMPLER(sampler_AvatarMap8);
            TEXTURE2D(_AvatarMap9);
            SAMPLER(sampler_AvatarMap9);
            TEXTURE2D(_AvatarMap10);
            SAMPLER(sampler_AvatarMap10);
            TEXTURE2D(_AvatarMap11);
            SAMPLER(sampler_AvatarMap11);
            TEXTURE2D(_AvatarMap12);
            SAMPLER(sampler_AvatarMap12);

            // Graph Functions

            // a1a3e9b98a2a76cb13dae6c56d1d1a39
            #include "Assets/Rendering/Shaders/Toon/ShaderGraph/Includes/SampleTexture.hlsl"

            void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            void Unity_Multiply_float(float A, float B, out float Out)
            {
                Out = A * B;
            }

            struct Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
            {
            };

            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2,
                                                                   float4 Color_20d6639c9fdd413a829b748f8d32667c,
                                                                   Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59
                                                                   IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0 = Vector4_DA7BBBB2;
                float4 _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0 = Color_20d6639c9fdd413a829b748f8d32667c;
                float4 _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Unity_Multiply_float(_Property_a3c975eda5cc718eaa8e9ce0242afc9e_Out_0,
                                     _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0,
                                     _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2);
                float _Split_9831600e6068464b9cb169a4f4d34f89_R_1 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[0];
                float _Split_9831600e6068464b9cb169a4f4d34f89_G_2 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[1];
                float _Split_9831600e6068464b9cb169a4f4d34f89_B_3 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[2];
                float _Split_9831600e6068464b9cb169a4f4d34f89_A_4 = _Property_38cf4154db0c4e38aef26e98a44b604a_Out_0[3];
                float4 _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0 = Vector4_DA7BBBB2;
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_R_1 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[0];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_G_2 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[1];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_B_3 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[2];
                float _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4 = _Property_d23ee4b8d36c9b81b69a5a171a6a1372_Out_0[3];
                float _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
                Unity_Multiply_float(_Split_9831600e6068464b9cb169a4f4d34f89_A_4,
                                     _Split_9699a4e47bebed82bf87ba60d2fddc89_A_4,
                                     _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2);
                Color_1 = _Multiply_e33e5a91c03c7e8b88ad2c72cbb0711f_Out_2;
                Alpha_2 = _Multiply_4d89c1791203a38d83dc31d5d7fc5ef0_Out_2;
            }

            void Unity_Normalize_float3(float3 In, out float3 Out)
            {
                Out = normalize(In);
            }

            void Unity_Add_float3(float3 A, float3 B, out float3 Out)
            {
                Out = A + B;
            }

            void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
            {
                Out = dot(A, B);
            }

            void Unity_Power_float(float A, float B, out float Out)
            {
                Out = pow(A, B);
            }

            void Unity_Clamp_float(float In, float Min, float Max, out float Out)
            {
                Out = clamp(In, Min, Max);
            }

            void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
            {
                Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            void Unity_Multiply_float(float3 A, float3 B, out float3 Out)
            {
                Out = A * B;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            void Unity_Add_float4(float4 A, float4 B, out float4 Out)
            {
                Out = A + B;
            }

            struct Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceViewDirection;
            };

            void SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(UnityTexture2D Texture2D_73021B24,
                                                                float Vector1_1431D892, float Vector1_C2B7F43A,
                                                                float4 Vector4_453A6B1F, float Vector1_55629F98,
                                                                float4 Vector4_752D3319, float3 Vector3_FEFB7F68,
                                                                float4 Color_778878808e8a4165941cd59bf37b29e0,
                                                                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 IN,
                                                                out float4 OutVector4_1)
            {
                float4 _Property_13c5118e874f43ca87025f5b8cbc0834_Out_0 = Color_778878808e8a4165941cd59bf37b29e0;
                UnityTexture2D _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0 = Texture2D_73021B24;
                float3 _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1);
                float3 _Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0 = Vector3_FEFB7F68;
                float3 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_b2e19ca52f945686b2519392081846d0_Out_1);
                float3 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2;
                Unity_Add_float3(_Property_bc3bb5229697b387bebe9f3e5beeeb15_Out_0,
                                 _Normalize_b2e19ca52f945686b2519392081846d0_Out_1,
                                 _Add_59467d0d541a7a8491396b40d6b459c0_Out_2);
                float3 _Normalize_c35275e5de48c48a91895e915706d93a_Out_1;
                Unity_Normalize_float3(_Add_59467d0d541a7a8491396b40d6b459c0_Out_2,
                                       _Normalize_c35275e5de48c48a91895e915706d93a_Out_1);
                float _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2;
                Unity_DotProduct_float3(_Normalize_4ac7105f0cce6d809db0a96d3e2fb00c_Out_1,
                                        _Normalize_c35275e5de48c48a91895e915706d93a_Out_1,
                                        _DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2);
                float _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0 = Vector1_55629F98;
                float _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2;
                Unity_Power_float(_DotProduct_30f6a0341d6cfa85a1a643569804b6ea_Out_2,
                                  _Property_fa3703b7bf193f8c83680be2c59dfc0e_Out_0,
                                  _Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2);
                float _Clamp_f5039b8aba04a988afa60b6204315584_Out_3;
                Unity_Clamp_float(_Power_cb36ca02acb2de89a61f4d67cfe90ca4_Out_2, 0.38, 0.99,
                                  _Clamp_f5039b8aba04a988afa60b6204315584_Out_3);
                float2 _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0 = float2(
                    _Clamp_f5039b8aba04a988afa60b6204315584_Out_3, 0.5);
                float4 _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.tex,
                    _Property_c92afaf7dbc7425f88dd3d97bccea048_Out_0.samplerstate,
                    _Vector2_9a8bf5774b4fbe81af3b38cdf9759855_Out_0);
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_R_4 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.r;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_G_5 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.g;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_B_6 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.b;
                float _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_A_7 =
                    _SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0.a;
                float4 _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0 = Vector4_752D3319;
                float4 _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2;
                Unity_Multiply_float(_SampleTexture2D_ad1d95648856ff818e55be9d0b398cfc_RGBA_0,
                                     _Property_59d0e595b9ed3487a9c36314e0926bb5_Out_0,
                                     _Multiply_9a724e75d670828c932f94803fda8a1e_Out_2);
                UnityTexture2D _Property_1fe3423555394477a6d62590b0d06e66_Out_0 = Texture2D_73021B24;
                float _Property_264a79bba6deba80a88c754a36867c9d_Out_0 = Vector1_1431D892;
                float _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3;
                Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection,
                                          _Property_264a79bba6deba80a88c754a36867c9d_Out_0,
                                          _FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3);
                float3 _Property_220a7f36ced91186a16e0b942c69428e_Out_0 = Vector3_FEFB7F68;
                float _Float_b30529d24ed97481877a14515ddbb316_Out_0 = -1;
                float3 _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1);
                float3 _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2;
                Unity_Multiply_float((_Float_b30529d24ed97481877a14515ddbb316_Out_0.xxx),
                                     _Normalize_9d3c8bbfbea463899b2ed82535392b45_Out_1,
                                     _Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2);
                float3 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1);
                float3 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2;
                Unity_Add_float3(_Multiply_5659f7bf0e170a8cbda9a033467088f5_Out_2,
                                 _Normalize_76d7d4bf37d5df85acf5c183ee9f6086_Out_1,
                                 _Add_024c1620de06b1808a8886cfa5f69c1d_Out_2);
                float3 _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1;
                Unity_Normalize_float3(_Add_024c1620de06b1808a8886cfa5f69c1d_Out_2,
                                       _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1);
                float _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2;
                Unity_DotProduct_float3(_Property_220a7f36ced91186a16e0b942c69428e_Out_0,
                                        _Normalize_eb73c49325d7be82971eb810a7340e37_Out_1,
                                        _DotProduct_759d147c71911b81979499b9567e8b0b_Out_2);
                float _Property_31359727423ffa80bc89eacf7e7596a9_Out_0 = Vector1_C2B7F43A;
                float2 _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0 = float2(
                    1, _Property_31359727423ffa80bc89eacf7e7596a9_Out_0);
                float _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3;
                Unity_Remap_float(_DotProduct_759d147c71911b81979499b9567e8b0b_Out_2, float2(-1, 1),
                                  _Vector2_b51c97980f8ca58aae0a113b081d7088_Out_0,
                                  _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3);
                float _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2;
                Unity_Multiply_float(_FresnelEffect_25ad6848891c868787052bce8d80d955_Out_3,
                                     _Remap_5e566dcab2e87e8ca85a2b1f7fb23f3c_Out_3,
                                     _Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2);
                float _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3;
                Unity_Remap_float(_Multiply_e98732a3f840af8cafb36a5ac3582519_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3);
                float2 _Vector2_902927a430b8848cac4001e2b7162a76_Out_0 = float2(
                    _Remap_0265db7184bf1b849448ca17bcdb406c_Out_3, 0.99);
                float4 _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.tex,
                    _Property_1fe3423555394477a6d62590b0d06e66_Out_0.samplerstate,
                    _Vector2_902927a430b8848cac4001e2b7162a76_Out_0);
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_R_4 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.r;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_G_5 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.g;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_B_6 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.b;
                float _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_A_7 =
                    _SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0.a;
                float4 _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0 = Vector4_453A6B1F;
                float4 _Multiply_313986370303ba8c966db164632517d4_Out_2;
                Unity_Multiply_float(_SampleTexture2D_44f962ee9e174d828af166a2779a14e9_RGBA_0,
                                     _Property_5af2626ce1a42b858ef394924d2ea12f_Out_0,
                                     _Multiply_313986370303ba8c966db164632517d4_Out_2);
                UnityTexture2D _Property_48552ab976c9128589df548370016ff3_Out_0 = Texture2D_73021B24;
                float3 _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0 = Vector3_FEFB7F68;
                float _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2;
                Unity_DotProduct_float3(IN.WorldSpaceNormal, _Property_de336b3152b02d8ebc7398ee8ea7cb37_Out_0,
                                        _DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2);
                float _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3;
                Unity_Remap_float(_DotProduct_dbd3fc968f1428879268db7c8d36e3c4_Out_2, float2(-1, 1), float2(0.01, 0.99),
                                  _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3);
                float _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0 = 0;
                float2 _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0 = float2(
                    _Remap_37bc9bd748de8480ab7556f2a423d375_Out_3, _Float_74e4c61ce84cc9818769d35ed149cc52_Out_0);
                float4 _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0 = SAMPLE_TEXTURE2D(
                    _Property_48552ab976c9128589df548370016ff3_Out_0.tex,
                    _Property_48552ab976c9128589df548370016ff3_Out_0.samplerstate,
                    _Vector2_f73d51750aa36989a7dac5bb620f51ba_Out_0);
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_R_4 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.r;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_G_5 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.g;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_B_6 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.b;
                float _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_A_7 =
                    _SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0.a;
                float4 Color_1caa66260b89b684a08e41b09e33f75d = IsGammaSpace()
                                                                    ? float4(0.7075472, 0.7075472, 0.7075472, 0)
                                                                    : float4(SRGBToLinear(
                                                                                 float3(0.7075472, 0.7075472,
                                                                                     0.7075472)), 0);
                float4 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2;
                Unity_Multiply_float(_SampleTexture2D_c83f1a690e3cc688a0c44677a104da34_RGBA_0,
                                     Color_1caa66260b89b684a08e41b09e33f75d,
                                     _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2);
                float4 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2;
                Unity_Add_float4(_Multiply_313986370303ba8c966db164632517d4_Out_2,
                                 _Multiply_84688c7b37dedd8cbb4336df4a03ded1_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2);
                float4 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2;
                Unity_Add_float4(_Multiply_9a724e75d670828c932f94803fda8a1e_Out_2,
                                 _Add_1d4c8baf3ace1b8e8734ada603ba832a_Out_2,
                                 _Add_5adf131eb3b73a82810f17cdc8575166_Out_2);
                float4 _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
                Unity_Multiply_float(_Property_13c5118e874f43ca87025f5b8cbc0834_Out_0,
                                     _Add_5adf131eb3b73a82810f17cdc8575166_Out_2,
                                     _Multiply_359a706b4442918d924cabd0309779e3_Out_2);
                OutVector4_1 = _Multiply_359a706b4442918d924cabd0309779e3_Out_2;
            }

            void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base * Blend;
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_Fog_float(out float4 Color, out float Density, float3 Position)
            {
                SHADERGRAPH_FOG(Position, Color, Density);
            }

            void Unity_OneMinus_float(float In, out float Out)
            {
                Out = 1 - In;
            }

            void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
            {
                Out = lerp(A, B, T);
            }

            struct Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9
            {
                float3 ViewSpacePosition;
            };

            void SG_Fog_db57d56e4661e4144b06df0b3edef8a9(float4 Color_42779DA4,
                                                         Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 IN,
                                                         out float4 Color_1)
            {
                float4 _Property_92d920f66f871787b8efd9892e387049_Out_0 = Color_42779DA4;
                float4 _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0;
                float _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1;
                Unity_Fog_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                _Fog_035ea79e58ad488fb87dad9f035bd378_Density_1, IN.ViewSpacePosition);
                float _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1;
                Unity_OneMinus_float(_Fog_035ea79e58ad488fb87dad9f035bd378_Density_1,
                                     _OneMinus_5194723b380f67819cad19ed50e5789e_Out_1);
                float4 _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
                Unity_Lerp_float4(_Property_92d920f66f871787b8efd9892e387049_Out_0,
                                  _Fog_035ea79e58ad488fb87dad9f035bd378_Color_0,
                                  (_OneMinus_5194723b380f67819cad19ed50e5789e_Out_1.xxxx),
                                  _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3);
                Color_1 = _Lerp_6976717f29b1d98daad50c3c8174d62c_Out_3;
            }

            struct Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
            {
                float3 ViewSpacePosition;
            };

            void SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(float4 Color_D4F585C6, float4 Color_D7818A04,
                                                                  float4 Color_546468F9,
                                                                  Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
                                                                  IN, out float4 FinalColor_1)
            {
                float4 _Property_519fcfdd306ddb839c113ca07f0f606c_Out_0 = Color_D4F585C6;
                float4 _Property_3e015b35bff76284853220d6ed019e2e_Out_0 = Color_546468F9;
                float4 _Property_ae789b17ad00778693ee4740849f6c53_Out_0 = Color_D7818A04;
                float4 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2;
                Unity_Add_float4(_Property_3e015b35bff76284853220d6ed019e2e_Out_0,
                                 _Property_ae789b17ad00778693ee4740849f6c53_Out_0,
                                 _Add_9c1c74d0fa17908ca9503658a239355d_Out_2);
                float4 _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2;
                Unity_Blend_Multiply_float4(_Property_519fcfdd306ddb839c113ca07f0f606c_Out_0,
                                            _Add_9c1c74d0fa17908ca9503658a239355d_Out_2,
                                            _Blend_4690b56c61915281a6230c45fd4ae04b_Out_2, 1);
                Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 _Fog_18fab81a74b9d7858633e8fc73111a15;
                _Fog_18fab81a74b9d7858633e8fc73111a15.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
                SG_Fog_db57d56e4661e4144b06df0b3edef8a9(_Blend_4690b56c61915281a6230c45fd4ae04b_Out_2,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15,
                                                        _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1);
                FinalColor_1 = _Fog_18fab81a74b9d7858633e8fc73111a15_Color_1;
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
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[0];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_G_2 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[1];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_B_3 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[2];
                float _Split_f5c16559fe844f5d973dffa1a7dc8693_A_4 = _UV_a1649fbaa2c445e3aa046d338c407fd4_Out_0[3];
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_b422957916a8465cad7c951698becee9_Out_0 = IN.uv0;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3;
                SampleTexture_float(float4(1, 1, 1, 0), _Split_f5c16559fe844f5d973dffa1a7dc8693_R_1,
                                    (_UV_b422957916a8465cad7c951698becee9_Out_0.xy),
                                    _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_5719c665e8d35a839cfc3c6704e23233;
                float4 _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1;
                float _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(IN.VertexColor,
                                                                  _SampleTextureCustomFunction_3b0967cfc20349bc84865f55a6958449_Color_3,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                                                                  _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0 = _AddColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                UnityTexture2D _Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0 = UnityBuildTexture2DStructNoScale(
                    _MatCap);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0 = IsGammaSpace()
                                                                              ? LinearToSRGB(_FresnelIntensity)
                                                                              : _FresnelIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_8243a735744a4993b94d1750ac0b3167_Out_0 = _GlossIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0 = _LightDir;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0 = _LightColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 _ToonShader_7540b263a099a28799339fadc2a1d8ac;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceNormal = IN.WorldSpaceNormal;
                _ToonShader_7540b263a099a28799339fadc2a1d8ac.WorldSpaceViewDirection = IN.WorldSpaceViewDirection;
                float4 _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1;
                SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(_Property_d8f5bbea07c8f48f850b6e2c29d52497_Out_0, 1.77,
                                                               -0.27, _Property_c1c9fca6ea1249809f9114afe82c607c_Out_0,
                                                               5, _Property_8243a735744a4993b94d1750ac0b3167_Out_0,
                                                               _Property_1ca5484e7ba72584b4df1eec599fea30_Out_0,
                                                               _Property_fcc682e9923fb1898164d8fd1cd170cf_Out_0,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac,
                                                               _ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0 = _MultiplyColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2;
                Unity_Multiply_float(_ToonShader_7540b263a099a28799339fadc2a1d8ac_OutVector4_1,
                                     _Property_e2eab2d556004d30b4cf607c80ec6bdd_Out_0,
                                     _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2;
                Unity_Add_float4(_Property_19e48f334cac4d18b3d9dfb322bb7508_Out_0,
                                 _Multiply_31d30dc7067a2382993c59f8fd50e105_Out_2,
                                 _Add_9aa5ce01d828b48c973710b335523c8a_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_22b7e6c299217e8c995d810ee580d78b_Out_0 = _TintColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb _FinalCombine_18a3c84d4498a188a6a77b7159cd568c;
                _FinalCombine_18a3c84d4498a188a6a77b7159cd568c.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1;
                SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(
                    _TextureSample_5719c665e8d35a839cfc3c6704e23233_Color_1,
                    _Add_9aa5ce01d828b48c973710b335523c8a_Out_2, _Property_22b7e6c299217e8c995d810ee580d78b_Out_0,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c,
                    _FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0 = IN.uv2;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Split_59ccf41d95534d23bcd364e587c6fc61_R_1 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[0];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_G_2 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[1];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_B_3 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[2];
                float _Split_59ccf41d95534d23bcd364e587c6fc61_A_4 = _UV_cc03be3228434292a1e3bf629dbd9a0b_Out_0[3];
                #endif
                surface.BaseColor = (_FinalCombine_18a3c84d4498a188a6a77b7159cd568c_FinalColor_1.xyz);
                surface.Alpha = _TextureSample_5719c665e8d35a839cfc3c6704e23233_Alpha_2;
                surface.AlphaClipThreshold = _Split_59ccf41d95534d23bcd364e587c6fc61_B_3;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceNormal = input.normalOS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ObjectSpacePosition = input.positionOS;
                #endif


                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 unnormalizedNormalWS = input.normalWS;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                const float renormFactor = 1.0 / length(unnormalizedNormalWS);
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;
                // we want a unit length Normal Vector node in shader graph
                #endif


                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.WorldSpaceViewDirection = input.viewDirectionWS;
                //TODO: by default normalized in HD, but not in universal
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.ViewSpacePosition = TransformWorldToView(input.positionWS);
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv0 = input.texCoord0;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.uv2 = input.texCoord2;
                #endif

                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                output.VertexColor = input.color;
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
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "ShaderGraph.PBRMasterGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}