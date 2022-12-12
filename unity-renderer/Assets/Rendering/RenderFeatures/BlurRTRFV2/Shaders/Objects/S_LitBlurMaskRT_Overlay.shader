Shader "BlurRT/Objects/Unlit/S_LitBlurMaskRT_Overlay"
{
    Properties
    {
        [ToggleUI]_isActivated("isActivated", Float) = 0
        _Color("Color", Color) = (0, 0, 0, 0)
        [ToggleUI]_isFlipped("isFlipped", Float) = 0
        _AlphaClip("AlphaClip", Float) = 1
        
        _XVariant("XVariant", Float) = 0
        _YVariant("YVariant", Float) = 0
        _ZVariant("ZVariant", Float) = 0
        _Center("Center", Vector) = (0, 0, 0, 0)
        _Rotation("Rotation", Float) = 0
        _Center_1("Center (1)", Float) = 0
        _WidthHeight("WidthHeight", Vector) = (0, 0, 0, 0)
        _Roundness("Roundness", Float) = 0.2
        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalLitSubTarget"
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
        //#pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        //#pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _LIGHT_LAYERS
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _LIGHT_COOKIES
        #pragma multi_compile _ _CLUSTERED_RENDERING
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
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _ALPHATEST_ON 1
        #define _RECEIVE_SHADOWS_OFF 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
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
             float3 WorldSpacePosition;
             float4 ScreenPosition;
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
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float3 interp4 : INTERP4;
             float2 interp5 : INTERP5;
             float2 interp6 : INTERP6;
             float3 interp7 : INTERP7;
             float4 interp8 : INTERP8;
             float4 interp9 : INTERP9;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp5.xy =  input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.interp6.xy =  input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp7.xyz =  input.sh;
            #endif
            output.interp8.xyzw =  input.fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.interp9.xyzw =  input.shadowCoord;
            #endif
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.viewDirectionWS = input.interp4.xyz;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.interp5.xy;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.interp6.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp7.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp8.xyzw;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.interp9.xyzw;
            #endif
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Flip_float4(float4 In, float4 Flip, out float4 Out)
        {
            Out = (Flip * -2 + 1) * In;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
        {
            //rotation matrix
            Rotation = Rotation * (3.1415926f/180.0f);
            UV -= Center;
            float s = sin(Rotation);
            float c = cos(Rotation);
        
            //center rotation matrix
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix*2 - 1;
        
            //multiply the UVs by the rotation matrix
            UV.xy = mul(UV.xy, rMatrix);
            UV += Center;
        
            Out = UV;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
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
            float _Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0 = _isActivated;
            float _Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0 = _isFlipped;
            UnityTexture2D _Property_8c3635df75b141839e7230099601413e_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float _Property_8168c9a702464c8cafc9881be5d4c79c_Out_0 = _XVariant;
            float4 _ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Out_1;
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Flip = float4 (0, 0, 0, 0);
            Unity_Flip_float4(_ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0, _Flip_01440d2e290c4d579356922ce30efa7f_Flip, _Flip_01440d2e290c4d579356922ce30efa7f_Out_1);
            float _Split_26dbab37d11c4565b9fee88bbf41041e_R_1 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[0];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_G_2 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[1];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_B_3 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[2];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_A_4 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[3];
            float _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2;
            Unity_Subtract_float(_Property_8168c9a702464c8cafc9881be5d4c79c_Out_0, _Split_26dbab37d11c4565b9fee88bbf41041e_R_1, _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2);
            float _Property_d6e8855c1a4641129f89b5ff01861123_Out_0 = _YVariant;
            float _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_G_2, _Property_d6e8855c1a4641129f89b5ff01861123_Out_0, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2);
            float _Property_1b78ccc14672426d8f8942ac2238423e_Out_0 = _ZVariant;
            float _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_B_3, _Property_1b78ccc14672426d8f8942ac2238423e_Out_0, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2);
            float4 _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4;
            float3 _Combine_437532df47c4437bbff35bdfa5022549_RGB_5;
            float2 _Combine_437532df47c4437bbff35bdfa5022549_RG_6;
            Unity_Combine_float(_Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2, _Split_26dbab37d11c4565b9fee88bbf41041e_A_4, _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4, _Combine_437532df47c4437bbff35bdfa5022549_RGB_5, _Combine_437532df47c4437bbff35bdfa5022549_RG_6);
            float2 _Property_e55a4973b15044f6b75584756c96bcd5_Out_0 = _Center;
            float _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0 = _Rotation;
            float2 _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3;
            Unity_Rotate_Degrees_float((_Combine_437532df47c4437bbff35bdfa5022549_RGBA_4.xy), _Property_e55a4973b15044f6b75584756c96bcd5_Out_0, _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0, _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3);
            float4 _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8c3635df75b141839e7230099601413e_Out_0.tex, _Property_8c3635df75b141839e7230099601413e_Out_0.samplerstate, _Property_8c3635df75b141839e7230099601413e_Out_0.GetTransformedUV(_Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3));
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_R_4 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.r;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_G_5 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.g;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_B_6 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.b;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_A_7 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.a;
            float4 _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0 = _Color;
            float4 _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0, _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2);
            UnityTexture2D _Property_308c011ee92ae28ab272bd73414e8816_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float4 _ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0 = SAMPLE_TEXTURE2D(_Property_308c011ee92ae28ab272bd73414e8816_Out_0.tex, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.samplerstate, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.GetTransformedUV((_ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0.xy)));
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_R_4 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.r;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_G_5 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.g;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_B_6 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.b;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_A_7 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.a;
            float4 _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0 = _Color;
            float4 _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0, _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2);
            float4 _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3;
            Unity_Branch_float4(_Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3);
            float4 _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3;
            Unity_Branch_float4(_Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3, float4(0, 0, 0, 0), _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3);
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.BaseColor = (_Branch_ef9121835cfd4f13af132c2c0905930c_Out_3.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = 0;
            surface.Smoothness = 0;
            surface.Occlusion = 1;
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
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
        //#pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        //#pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        #pragma multi_compile_fragment _ _LIGHT_LAYERS
        #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
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
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_GBUFFER
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _ALPHATEST_ON 1
        #define _RECEIVE_SHADOWS_OFF 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
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
             float3 WorldSpacePosition;
             float4 ScreenPosition;
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
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float3 interp4 : INTERP4;
             float2 interp5 : INTERP5;
             float2 interp6 : INTERP6;
             float3 interp7 : INTERP7;
             float4 interp8 : INTERP8;
             float4 interp9 : INTERP9;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp5.xy =  input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.interp6.xy =  input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp7.xyz =  input.sh;
            #endif
            output.interp8.xyzw =  input.fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.interp9.xyzw =  input.shadowCoord;
            #endif
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.viewDirectionWS = input.interp4.xyz;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.interp5.xy;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.interp6.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp7.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp8.xyzw;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.interp9.xyzw;
            #endif
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Flip_float4(float4 In, float4 Flip, out float4 Out)
        {
            Out = (Flip * -2 + 1) * In;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
        {
            //rotation matrix
            Rotation = Rotation * (3.1415926f/180.0f);
            UV -= Center;
            float s = sin(Rotation);
            float c = cos(Rotation);
        
            //center rotation matrix
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix*2 - 1;
        
            //multiply the UVs by the rotation matrix
            UV.xy = mul(UV.xy, rMatrix);
            UV += Center;
        
            Out = UV;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
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
            float _Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0 = _isActivated;
            float _Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0 = _isFlipped;
            UnityTexture2D _Property_8c3635df75b141839e7230099601413e_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float _Property_8168c9a702464c8cafc9881be5d4c79c_Out_0 = _XVariant;
            float4 _ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Out_1;
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Flip = float4 (0, 0, 0, 0);
            Unity_Flip_float4(_ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0, _Flip_01440d2e290c4d579356922ce30efa7f_Flip, _Flip_01440d2e290c4d579356922ce30efa7f_Out_1);
            float _Split_26dbab37d11c4565b9fee88bbf41041e_R_1 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[0];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_G_2 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[1];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_B_3 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[2];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_A_4 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[3];
            float _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2;
            Unity_Subtract_float(_Property_8168c9a702464c8cafc9881be5d4c79c_Out_0, _Split_26dbab37d11c4565b9fee88bbf41041e_R_1, _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2);
            float _Property_d6e8855c1a4641129f89b5ff01861123_Out_0 = _YVariant;
            float _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_G_2, _Property_d6e8855c1a4641129f89b5ff01861123_Out_0, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2);
            float _Property_1b78ccc14672426d8f8942ac2238423e_Out_0 = _ZVariant;
            float _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_B_3, _Property_1b78ccc14672426d8f8942ac2238423e_Out_0, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2);
            float4 _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4;
            float3 _Combine_437532df47c4437bbff35bdfa5022549_RGB_5;
            float2 _Combine_437532df47c4437bbff35bdfa5022549_RG_6;
            Unity_Combine_float(_Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2, _Split_26dbab37d11c4565b9fee88bbf41041e_A_4, _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4, _Combine_437532df47c4437bbff35bdfa5022549_RGB_5, _Combine_437532df47c4437bbff35bdfa5022549_RG_6);
            float2 _Property_e55a4973b15044f6b75584756c96bcd5_Out_0 = _Center;
            float _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0 = _Rotation;
            float2 _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3;
            Unity_Rotate_Degrees_float((_Combine_437532df47c4437bbff35bdfa5022549_RGBA_4.xy), _Property_e55a4973b15044f6b75584756c96bcd5_Out_0, _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0, _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3);
            float4 _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8c3635df75b141839e7230099601413e_Out_0.tex, _Property_8c3635df75b141839e7230099601413e_Out_0.samplerstate, _Property_8c3635df75b141839e7230099601413e_Out_0.GetTransformedUV(_Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3));
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_R_4 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.r;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_G_5 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.g;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_B_6 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.b;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_A_7 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.a;
            float4 _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0 = _Color;
            float4 _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0, _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2);
            UnityTexture2D _Property_308c011ee92ae28ab272bd73414e8816_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float4 _ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0 = SAMPLE_TEXTURE2D(_Property_308c011ee92ae28ab272bd73414e8816_Out_0.tex, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.samplerstate, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.GetTransformedUV((_ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0.xy)));
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_R_4 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.r;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_G_5 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.g;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_B_6 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.b;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_A_7 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.a;
            float4 _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0 = _Color;
            float4 _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0, _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2);
            float4 _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3;
            Unity_Branch_float4(_Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3);
            float4 _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3;
            Unity_Branch_float4(_Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3, float4(0, 0, 0, 0), _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3);
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.BaseColor = (_Branch_ef9121835cfd4f13af132c2c0905930c_Out_3.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = 0;
            surface.Smoothness = 0;
            surface.Occlusion = 1;
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
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
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        //#pragma exclude_renderers gles gles3 glcore
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
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALS
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float3 normalWS;
             float4 tangentWS;
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
             float3 TangentSpaceNormal;
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
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.tangentWS;
            output.interp2.xyzw =  input.texCoord0;
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.tangentWS = input.interp1.xyzw;
            output.texCoord0 = input.interp2.xyzw;
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
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
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
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
        //#pragma exclude_renderers gles gles3 glcore
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma shader_feature _ EDITOR_VISUALIZATION
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
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        #define _FOG_FRAGMENT 1
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float4 texCoord1;
             float4 texCoord2;
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
             float3 WorldSpacePosition;
             float4 ScreenPosition;
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
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
            output.interp2.xyzw =  input.texCoord1;
            output.interp3.xyzw =  input.texCoord2;
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            output.texCoord1 = input.interp2.xyzw;
            output.texCoord2 = input.interp3.xyzw;
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Flip_float4(float4 In, float4 Flip, out float4 Out)
        {
            Out = (Flip * -2 + 1) * In;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
        {
            //rotation matrix
            Rotation = Rotation * (3.1415926f/180.0f);
            UV -= Center;
            float s = sin(Rotation);
            float c = cos(Rotation);
        
            //center rotation matrix
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix*2 - 1;
        
            //multiply the UVs by the rotation matrix
            UV.xy = mul(UV.xy, rMatrix);
            UV += Center;
        
            Out = UV;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
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
            float _Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0 = _isActivated;
            float _Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0 = _isFlipped;
            UnityTexture2D _Property_8c3635df75b141839e7230099601413e_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float _Property_8168c9a702464c8cafc9881be5d4c79c_Out_0 = _XVariant;
            float4 _ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Out_1;
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Flip = float4 (0, 0, 0, 0);
            Unity_Flip_float4(_ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0, _Flip_01440d2e290c4d579356922ce30efa7f_Flip, _Flip_01440d2e290c4d579356922ce30efa7f_Out_1);
            float _Split_26dbab37d11c4565b9fee88bbf41041e_R_1 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[0];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_G_2 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[1];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_B_3 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[2];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_A_4 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[3];
            float _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2;
            Unity_Subtract_float(_Property_8168c9a702464c8cafc9881be5d4c79c_Out_0, _Split_26dbab37d11c4565b9fee88bbf41041e_R_1, _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2);
            float _Property_d6e8855c1a4641129f89b5ff01861123_Out_0 = _YVariant;
            float _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_G_2, _Property_d6e8855c1a4641129f89b5ff01861123_Out_0, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2);
            float _Property_1b78ccc14672426d8f8942ac2238423e_Out_0 = _ZVariant;
            float _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_B_3, _Property_1b78ccc14672426d8f8942ac2238423e_Out_0, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2);
            float4 _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4;
            float3 _Combine_437532df47c4437bbff35bdfa5022549_RGB_5;
            float2 _Combine_437532df47c4437bbff35bdfa5022549_RG_6;
            Unity_Combine_float(_Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2, _Split_26dbab37d11c4565b9fee88bbf41041e_A_4, _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4, _Combine_437532df47c4437bbff35bdfa5022549_RGB_5, _Combine_437532df47c4437bbff35bdfa5022549_RG_6);
            float2 _Property_e55a4973b15044f6b75584756c96bcd5_Out_0 = _Center;
            float _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0 = _Rotation;
            float2 _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3;
            Unity_Rotate_Degrees_float((_Combine_437532df47c4437bbff35bdfa5022549_RGBA_4.xy), _Property_e55a4973b15044f6b75584756c96bcd5_Out_0, _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0, _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3);
            float4 _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8c3635df75b141839e7230099601413e_Out_0.tex, _Property_8c3635df75b141839e7230099601413e_Out_0.samplerstate, _Property_8c3635df75b141839e7230099601413e_Out_0.GetTransformedUV(_Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3));
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_R_4 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.r;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_G_5 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.g;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_B_6 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.b;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_A_7 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.a;
            float4 _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0 = _Color;
            float4 _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0, _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2);
            UnityTexture2D _Property_308c011ee92ae28ab272bd73414e8816_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float4 _ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0 = SAMPLE_TEXTURE2D(_Property_308c011ee92ae28ab272bd73414e8816_Out_0.tex, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.samplerstate, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.GetTransformedUV((_ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0.xy)));
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_R_4 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.r;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_G_5 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.g;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_B_6 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.b;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_A_7 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.a;
            float4 _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0 = _Color;
            float4 _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0, _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2);
            float4 _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3;
            Unity_Branch_float4(_Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3);
            float4 _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3;
            Unity_Branch_float4(_Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3, float4(0, 0, 0, 0), _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3);
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.BaseColor = (_Branch_ef9121835cfd4f13af132c2c0905930c_Out_3.xyz);
            surface.Emission = float3(0, 0, 0);
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
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
        //#pragma exclude_renderers gles gles3 glcore
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
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float4 interp0 : INTERP0;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        //#pragma exclude_renderers gles gles3 glcore
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
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float4 interp0 : INTERP0;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
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
        //#pragma exclude_renderers gles gles3 glcore
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
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float3 WorldSpacePosition;
             float4 ScreenPosition;
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
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
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
        
        Varyings UnpackVaryings (PackedVaryings input)
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
        float4 _Color;
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Flip_float4(float4 In, float4 Flip, out float4 Out)
        {
            Out = (Flip * -2 + 1) * In;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
        {
            //rotation matrix
            Rotation = Rotation * (3.1415926f/180.0f);
            UV -= Center;
            float s = sin(Rotation);
            float c = cos(Rotation);
        
            //center rotation matrix
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix*2 - 1;
        
            //multiply the UVs by the rotation matrix
            UV.xy = mul(UV.xy, rMatrix);
            UV += Center;
        
            Out = UV;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
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
            float _Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0 = _isActivated;
            float _Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0 = _isFlipped;
            UnityTexture2D _Property_8c3635df75b141839e7230099601413e_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float _Property_8168c9a702464c8cafc9881be5d4c79c_Out_0 = _XVariant;
            float4 _ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Out_1;
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Flip = float4 (0, 0, 0, 0);
            Unity_Flip_float4(_ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0, _Flip_01440d2e290c4d579356922ce30efa7f_Flip, _Flip_01440d2e290c4d579356922ce30efa7f_Out_1);
            float _Split_26dbab37d11c4565b9fee88bbf41041e_R_1 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[0];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_G_2 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[1];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_B_3 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[2];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_A_4 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[3];
            float _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2;
            Unity_Subtract_float(_Property_8168c9a702464c8cafc9881be5d4c79c_Out_0, _Split_26dbab37d11c4565b9fee88bbf41041e_R_1, _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2);
            float _Property_d6e8855c1a4641129f89b5ff01861123_Out_0 = _YVariant;
            float _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_G_2, _Property_d6e8855c1a4641129f89b5ff01861123_Out_0, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2);
            float _Property_1b78ccc14672426d8f8942ac2238423e_Out_0 = _ZVariant;
            float _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_B_3, _Property_1b78ccc14672426d8f8942ac2238423e_Out_0, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2);
            float4 _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4;
            float3 _Combine_437532df47c4437bbff35bdfa5022549_RGB_5;
            float2 _Combine_437532df47c4437bbff35bdfa5022549_RG_6;
            Unity_Combine_float(_Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2, _Split_26dbab37d11c4565b9fee88bbf41041e_A_4, _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4, _Combine_437532df47c4437bbff35bdfa5022549_RGB_5, _Combine_437532df47c4437bbff35bdfa5022549_RG_6);
            float2 _Property_e55a4973b15044f6b75584756c96bcd5_Out_0 = _Center;
            float _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0 = _Rotation;
            float2 _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3;
            Unity_Rotate_Degrees_float((_Combine_437532df47c4437bbff35bdfa5022549_RGBA_4.xy), _Property_e55a4973b15044f6b75584756c96bcd5_Out_0, _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0, _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3);
            float4 _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8c3635df75b141839e7230099601413e_Out_0.tex, _Property_8c3635df75b141839e7230099601413e_Out_0.samplerstate, _Property_8c3635df75b141839e7230099601413e_Out_0.GetTransformedUV(_Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3));
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_R_4 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.r;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_G_5 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.g;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_B_6 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.b;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_A_7 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.a;
            float4 _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0 = _Color;
            float4 _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0, _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2);
            UnityTexture2D _Property_308c011ee92ae28ab272bd73414e8816_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float4 _ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0 = SAMPLE_TEXTURE2D(_Property_308c011ee92ae28ab272bd73414e8816_Out_0.tex, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.samplerstate, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.GetTransformedUV((_ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0.xy)));
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_R_4 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.r;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_G_5 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.g;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_B_6 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.b;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_A_7 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.a;
            float4 _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0 = _Color;
            float4 _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0, _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2);
            float4 _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3;
            Unity_Branch_float4(_Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3);
            float4 _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3;
            Unity_Branch_float4(_Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3, float4(0, 0, 0, 0), _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3);
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.BaseColor = (_Branch_ef9121835cfd4f13af132c2c0905930c_Out_3.xyz);
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalLitSubTarget"
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
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        //#pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>
        
        // Keywords
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _LIGHT_LAYERS
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _LIGHT_COOKIES
        #pragma multi_compile _ _CLUSTERED_RENDERING
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
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _ALPHATEST_ON 1
        #define _RECEIVE_SHADOWS_OFF 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
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
             float3 WorldSpacePosition;
             float4 ScreenPosition;
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
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float3 interp4 : INTERP4;
             float2 interp5 : INTERP5;
             float2 interp6 : INTERP6;
             float3 interp7 : INTERP7;
             float4 interp8 : INTERP8;
             float4 interp9 : INTERP9;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp5.xy =  input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.interp6.xy =  input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp7.xyz =  input.sh;
            #endif
            output.interp8.xyzw =  input.fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.interp9.xyzw =  input.shadowCoord;
            #endif
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.viewDirectionWS = input.interp4.xyz;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.interp5.xy;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.interp6.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp7.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp8.xyzw;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.interp9.xyzw;
            #endif
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Flip_float4(float4 In, float4 Flip, out float4 Out)
        {
            Out = (Flip * -2 + 1) * In;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
        {
            //rotation matrix
            Rotation = Rotation * (3.1415926f/180.0f);
            UV -= Center;
            float s = sin(Rotation);
            float c = cos(Rotation);
        
            //center rotation matrix
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix*2 - 1;
        
            //multiply the UVs by the rotation matrix
            UV.xy = mul(UV.xy, rMatrix);
            UV += Center;
        
            Out = UV;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
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
            float _Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0 = _isActivated;
            float _Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0 = _isFlipped;
            UnityTexture2D _Property_8c3635df75b141839e7230099601413e_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float _Property_8168c9a702464c8cafc9881be5d4c79c_Out_0 = _XVariant;
            float4 _ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Out_1;
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Flip = float4 (0, 0, 0, 0);
            Unity_Flip_float4(_ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0, _Flip_01440d2e290c4d579356922ce30efa7f_Flip, _Flip_01440d2e290c4d579356922ce30efa7f_Out_1);
            float _Split_26dbab37d11c4565b9fee88bbf41041e_R_1 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[0];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_G_2 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[1];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_B_3 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[2];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_A_4 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[3];
            float _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2;
            Unity_Subtract_float(_Property_8168c9a702464c8cafc9881be5d4c79c_Out_0, _Split_26dbab37d11c4565b9fee88bbf41041e_R_1, _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2);
            float _Property_d6e8855c1a4641129f89b5ff01861123_Out_0 = _YVariant;
            float _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_G_2, _Property_d6e8855c1a4641129f89b5ff01861123_Out_0, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2);
            float _Property_1b78ccc14672426d8f8942ac2238423e_Out_0 = _ZVariant;
            float _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_B_3, _Property_1b78ccc14672426d8f8942ac2238423e_Out_0, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2);
            float4 _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4;
            float3 _Combine_437532df47c4437bbff35bdfa5022549_RGB_5;
            float2 _Combine_437532df47c4437bbff35bdfa5022549_RG_6;
            Unity_Combine_float(_Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2, _Split_26dbab37d11c4565b9fee88bbf41041e_A_4, _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4, _Combine_437532df47c4437bbff35bdfa5022549_RGB_5, _Combine_437532df47c4437bbff35bdfa5022549_RG_6);
            float2 _Property_e55a4973b15044f6b75584756c96bcd5_Out_0 = _Center;
            float _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0 = _Rotation;
            float2 _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3;
            Unity_Rotate_Degrees_float((_Combine_437532df47c4437bbff35bdfa5022549_RGBA_4.xy), _Property_e55a4973b15044f6b75584756c96bcd5_Out_0, _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0, _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3);
            float4 _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8c3635df75b141839e7230099601413e_Out_0.tex, _Property_8c3635df75b141839e7230099601413e_Out_0.samplerstate, _Property_8c3635df75b141839e7230099601413e_Out_0.GetTransformedUV(_Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3));
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_R_4 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.r;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_G_5 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.g;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_B_6 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.b;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_A_7 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.a;
            float4 _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0 = _Color;
            float4 _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0, _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2);
            UnityTexture2D _Property_308c011ee92ae28ab272bd73414e8816_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float4 _ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0 = SAMPLE_TEXTURE2D(_Property_308c011ee92ae28ab272bd73414e8816_Out_0.tex, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.samplerstate, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.GetTransformedUV((_ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0.xy)));
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_R_4 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.r;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_G_5 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.g;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_B_6 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.b;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_A_7 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.a;
            float4 _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0 = _Color;
            float4 _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0, _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2);
            float4 _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3;
            Unity_Branch_float4(_Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3);
            float4 _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3;
            Unity_Branch_float4(_Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3, float4(0, 0, 0, 0), _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3);
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.BaseColor = (_Branch_ef9121835cfd4f13af132c2c0905930c_Out_3.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = 0;
            surface.Smoothness = 0;
            surface.Occlusion = 1;
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
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
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALS
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float3 normalWS;
             float4 tangentWS;
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
             float3 TangentSpaceNormal;
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
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.tangentWS;
            output.interp2.xyzw =  input.texCoord0;
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.tangentWS = input.interp1.xyzw;
            output.texCoord0 = input.interp2.xyzw;
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
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
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
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
        #pragma shader_feature _ EDITOR_VISUALIZATION
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
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        #define _FOG_FRAGMENT 1
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float4 texCoord1;
             float4 texCoord2;
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
             float3 WorldSpacePosition;
             float4 ScreenPosition;
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
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
            output.interp2.xyzw =  input.texCoord1;
            output.interp3.xyzw =  input.texCoord2;
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            output.texCoord1 = input.interp2.xyzw;
            output.texCoord2 = input.interp3.xyzw;
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Flip_float4(float4 In, float4 Flip, out float4 Out)
        {
            Out = (Flip * -2 + 1) * In;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
        {
            //rotation matrix
            Rotation = Rotation * (3.1415926f/180.0f);
            UV -= Center;
            float s = sin(Rotation);
            float c = cos(Rotation);
        
            //center rotation matrix
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix*2 - 1;
        
            //multiply the UVs by the rotation matrix
            UV.xy = mul(UV.xy, rMatrix);
            UV += Center;
        
            Out = UV;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
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
            float _Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0 = _isActivated;
            float _Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0 = _isFlipped;
            UnityTexture2D _Property_8c3635df75b141839e7230099601413e_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float _Property_8168c9a702464c8cafc9881be5d4c79c_Out_0 = _XVariant;
            float4 _ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Out_1;
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Flip = float4 (0, 0, 0, 0);
            Unity_Flip_float4(_ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0, _Flip_01440d2e290c4d579356922ce30efa7f_Flip, _Flip_01440d2e290c4d579356922ce30efa7f_Out_1);
            float _Split_26dbab37d11c4565b9fee88bbf41041e_R_1 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[0];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_G_2 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[1];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_B_3 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[2];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_A_4 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[3];
            float _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2;
            Unity_Subtract_float(_Property_8168c9a702464c8cafc9881be5d4c79c_Out_0, _Split_26dbab37d11c4565b9fee88bbf41041e_R_1, _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2);
            float _Property_d6e8855c1a4641129f89b5ff01861123_Out_0 = _YVariant;
            float _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_G_2, _Property_d6e8855c1a4641129f89b5ff01861123_Out_0, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2);
            float _Property_1b78ccc14672426d8f8942ac2238423e_Out_0 = _ZVariant;
            float _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_B_3, _Property_1b78ccc14672426d8f8942ac2238423e_Out_0, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2);
            float4 _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4;
            float3 _Combine_437532df47c4437bbff35bdfa5022549_RGB_5;
            float2 _Combine_437532df47c4437bbff35bdfa5022549_RG_6;
            Unity_Combine_float(_Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2, _Split_26dbab37d11c4565b9fee88bbf41041e_A_4, _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4, _Combine_437532df47c4437bbff35bdfa5022549_RGB_5, _Combine_437532df47c4437bbff35bdfa5022549_RG_6);
            float2 _Property_e55a4973b15044f6b75584756c96bcd5_Out_0 = _Center;
            float _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0 = _Rotation;
            float2 _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3;
            Unity_Rotate_Degrees_float((_Combine_437532df47c4437bbff35bdfa5022549_RGBA_4.xy), _Property_e55a4973b15044f6b75584756c96bcd5_Out_0, _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0, _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3);
            float4 _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8c3635df75b141839e7230099601413e_Out_0.tex, _Property_8c3635df75b141839e7230099601413e_Out_0.samplerstate, _Property_8c3635df75b141839e7230099601413e_Out_0.GetTransformedUV(_Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3));
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_R_4 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.r;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_G_5 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.g;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_B_6 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.b;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_A_7 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.a;
            float4 _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0 = _Color;
            float4 _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0, _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2);
            UnityTexture2D _Property_308c011ee92ae28ab272bd73414e8816_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float4 _ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0 = SAMPLE_TEXTURE2D(_Property_308c011ee92ae28ab272bd73414e8816_Out_0.tex, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.samplerstate, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.GetTransformedUV((_ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0.xy)));
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_R_4 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.r;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_G_5 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.g;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_B_6 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.b;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_A_7 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.a;
            float4 _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0 = _Color;
            float4 _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0, _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2);
            float4 _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3;
            Unity_Branch_float4(_Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3);
            float4 _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3;
            Unity_Branch_float4(_Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3, float4(0, 0, 0, 0), _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3);
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.BaseColor = (_Branch_ef9121835cfd4f13af132c2c0905930c_Out_3.xyz);
            surface.Emission = float3(0, 0, 0);
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
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
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float4 interp0 : INTERP0;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
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
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float4 interp0 : INTERP0;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
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
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
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
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
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
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
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
        #define _ALPHATEST_ON 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
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
             float3 WorldSpacePosition;
             float4 ScreenPosition;
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
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
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
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
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
        
        Varyings UnpackVaryings (PackedVaryings input)
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
        float4 _Color;
        float _isFlipped;
        float _XVariant;
        float _YVariant;
        float _ZVariant;
        float _Rotation;
        float2 _Center;
        float _AlphaClip;
        float2 _WidthHeight;
        float _Roundness;
        float _Center_1;
        float _isActivated;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_blurTexture);
        SAMPLER(sampler_blurTexture);
        float4 _blurTexture_TexelSize;
        TEXTURE2D(Texture2D_3425DFF1);
        SAMPLER(samplerTexture2D_3425DFF1);
        float4 Texture2D_3425DFF1_TexelSize;
        float4 Texture2D_3425DFF1_ST;
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Flip_float4(float4 In, float4 Flip, out float4 Out)
        {
            Out = (Flip * -2 + 1) * In;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
        {
            //rotation matrix
            Rotation = Rotation * (3.1415926f/180.0f);
            UV -= Center;
            float s = sin(Rotation);
            float c = cos(Rotation);
        
            //center rotation matrix
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix*2 - 1;
        
            //multiply the UVs by the rotation matrix
            UV.xy = mul(UV.xy, rMatrix);
            UV += Center;
        
            Out = UV;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void RoundedPolygon_Func_float(float2 UV, float Width, float Height, float Sides, float Roundness, out float Out)
        {
            UV = UV * 2. + float2(-1.,-1.);
            float epsilon = 1e-6;
            UV.x = UV.x / ( Width + (Width==0)*epsilon);
            UV.y = UV.y / ( Height + (Height==0)*epsilon);
            Roundness = clamp(Roundness, 1e-6, 1.);
            float i_sides = floor( abs( Sides ) );
            float fullAngle = 2. * PI / i_sides;
            float halfAngle = fullAngle / 2.;
            float opositeAngle = HALF_PI - halfAngle;
            float diagonal = 1. / cos( halfAngle );
            // Chamfer values
            float chamferAngle = Roundness * halfAngle; // Angle taken by the chamfer
            float remainingAngle = halfAngle - chamferAngle; // Angle that remains
            float ratio = tan(remainingAngle) / tan(halfAngle); // This is the ratio between the length of the polygon's triangle and the distance of the chamfer center to the polygon center
            // Center of the chamfer arc
            float2 chamferCenter = float2(
                cos(halfAngle) ,
                sin(halfAngle)
            )* ratio * diagonal;
            // starting of the chamfer arc
            float2 chamferOrigin = float2(
                1.,
                tan(remainingAngle)
            );
            // Using Al Kashi algebra, we determine:
            // The distance distance of the center of the chamfer to the center of the polygon (side A)
            float distA = length(chamferCenter);
            // The radius of the chamfer (side B)
            float distB = 1. - chamferCenter.x;
            // The refence length of side C, which is the distance to the chamfer start
            float distCref = length(chamferOrigin);
            // This will rescale the chamfered polygon to fit the uv space
            // diagonal = length(chamferCenter) + distB;
            float uvScale = diagonal;
            UV *= uvScale;
            float2 polaruv = float2 (
                atan2( UV.y, UV.x ),
                length(UV)
            );
            polaruv.x += HALF_PI + 2*PI;
            polaruv.x = fmod( polaruv.x + halfAngle, fullAngle );
            polaruv.x = abs(polaruv.x - halfAngle);
            UV = float2( cos(polaruv.x), sin(polaruv.x) ) * polaruv.y;
            // Calculate the angle needed for the Al Kashi algebra
            float angleRatio = 1. - (polaruv.x-remainingAngle) / chamferAngle;
            // Calculate the distance of the polygon center to the chamfer extremity
            float distC = sqrt( distA*distA + distB*distB - 2.*distA*distB*cos( PI - halfAngle * angleRatio ) );
            Out = UV.x;
            float chamferZone = ( halfAngle - polaruv.x ) < chamferAngle;
            Out = lerp( UV.x, polaruv.y / distC, chamferZone );
            // Output this to have the shape mask instead of the distance field
        #if defined(SHADER_STAGE_RAY_TRACING)
            Out = saturate((1 - Out) * 1e7);
        #else
            Out = saturate((1 - Out) / fwidth(Out));
        #endif
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
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
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
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
            float _Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0 = _isActivated;
            float _Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0 = _isFlipped;
            UnityTexture2D _Property_8c3635df75b141839e7230099601413e_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float _Property_8168c9a702464c8cafc9881be5d4c79c_Out_0 = _XVariant;
            float4 _ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Out_1;
            float4 _Flip_01440d2e290c4d579356922ce30efa7f_Flip = float4 (0, 0, 0, 0);
            Unity_Flip_float4(_ScreenPosition_aa1787b6aa6f4fd1a9b98dcb5cbc8ec6_Out_0, _Flip_01440d2e290c4d579356922ce30efa7f_Flip, _Flip_01440d2e290c4d579356922ce30efa7f_Out_1);
            float _Split_26dbab37d11c4565b9fee88bbf41041e_R_1 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[0];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_G_2 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[1];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_B_3 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[2];
            float _Split_26dbab37d11c4565b9fee88bbf41041e_A_4 = _Flip_01440d2e290c4d579356922ce30efa7f_Out_1[3];
            float _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2;
            Unity_Subtract_float(_Property_8168c9a702464c8cafc9881be5d4c79c_Out_0, _Split_26dbab37d11c4565b9fee88bbf41041e_R_1, _Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2);
            float _Property_d6e8855c1a4641129f89b5ff01861123_Out_0 = _YVariant;
            float _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_G_2, _Property_d6e8855c1a4641129f89b5ff01861123_Out_0, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2);
            float _Property_1b78ccc14672426d8f8942ac2238423e_Out_0 = _ZVariant;
            float _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2;
            Unity_Multiply_float_float(_Split_26dbab37d11c4565b9fee88bbf41041e_B_3, _Property_1b78ccc14672426d8f8942ac2238423e_Out_0, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2);
            float4 _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4;
            float3 _Combine_437532df47c4437bbff35bdfa5022549_RGB_5;
            float2 _Combine_437532df47c4437bbff35bdfa5022549_RG_6;
            Unity_Combine_float(_Subtract_81934203c66848e89b9ee7e8b1ae3cae_Out_2, _Multiply_6f1a04c090da4ed4b8e595fb0012436f_Out_2, _Multiply_eb2ff6e50b1e4a2cbf8a25327e917ed4_Out_2, _Split_26dbab37d11c4565b9fee88bbf41041e_A_4, _Combine_437532df47c4437bbff35bdfa5022549_RGBA_4, _Combine_437532df47c4437bbff35bdfa5022549_RGB_5, _Combine_437532df47c4437bbff35bdfa5022549_RG_6);
            float2 _Property_e55a4973b15044f6b75584756c96bcd5_Out_0 = _Center;
            float _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0 = _Rotation;
            float2 _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3;
            Unity_Rotate_Degrees_float((_Combine_437532df47c4437bbff35bdfa5022549_RGBA_4.xy), _Property_e55a4973b15044f6b75584756c96bcd5_Out_0, _Property_620c3ed7471f4ed5aa4075a78fb52898_Out_0, _Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3);
            float4 _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8c3635df75b141839e7230099601413e_Out_0.tex, _Property_8c3635df75b141839e7230099601413e_Out_0.samplerstate, _Property_8c3635df75b141839e7230099601413e_Out_0.GetTransformedUV(_Rotate_c21a12a52fad466db0fde48d05e1dc71_Out_3));
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_R_4 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.r;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_G_5 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.g;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_B_6 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.b;
            float _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_A_7 = _SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0.a;
            float4 _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0 = _Color;
            float4 _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_12f0ec9ab95e458fb0352c9216eebad3_RGBA_0, _Property_4029f66ef8a64990b6bf62f6ee411045_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2);
            UnityTexture2D _Property_308c011ee92ae28ab272bd73414e8816_Out_0 = UnityBuildTexture2DStructNoScale(_blurTexture);
            float4 _ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float4 _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0 = SAMPLE_TEXTURE2D(_Property_308c011ee92ae28ab272bd73414e8816_Out_0.tex, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.samplerstate, _Property_308c011ee92ae28ab272bd73414e8816_Out_0.GetTransformedUV((_ScreenPosition_db044b7dee0f8b8b89c36f7826d2cda9_Out_0.xy)));
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_R_4 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.r;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_G_5 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.g;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_B_6 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.b;
            float _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_A_7 = _SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0.a;
            float4 _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0 = _Color;
            float4 _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_2e503c71a5b26d8fb0712b7182f1d636_RGBA_0, _Property_e82af5809f2040d4ab8175ad70bccf69_Out_0, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2);
            float4 _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3;
            Unity_Branch_float4(_Property_c4aa2a2b8b724b638bfabcdaef503879_Out_0, _Multiply_84df3f6d74a046fca8b6cf61edcb4c09_Out_2, _Multiply_92f0b83d43b546c99e7ad7f5c8f4d575_Out_2, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3);
            float4 _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3;
            Unity_Branch_float4(_Property_17d700c5cc5741e49f75fd8a66e79b14_Out_0, _Branch_83a5ba350f414ff6a00401d50428b1ba_Out_3, float4(0, 0, 0, 0), _Branch_ef9121835cfd4f13af132c2c0905930c_Out_3);
            float _Property_6fb988081629465087579cc3f6c6cf70_Out_0 = _isActivated;
            float4 _UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0 = IN.uv0;
            float _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3;
            Unity_Rectangle_Fastest_float((_UV_39fb9d1e980f44a48b4b1e87d93870fa_Out_0.xy), 1, 1, _Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3);
            float4 _UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0 = IN.uv0;
            float _Float_0d9eec0edaca4967b36d7993695c1bff_Out_0 = 0.3;
            float _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0 = _Center_1;
            float4 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4;
            float3 _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5;
            float2 _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6;
            Unity_Combine_float(_Float_0d9eec0edaca4967b36d7993695c1bff_Out_0, _Property_23f9fe05761b4a20a81acc9f2bebdaba_Out_0, 0, 0, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGBA_4, _Combine_b725d53e914245de8ceae3a4d343a7ce_RGB_5, _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6);
            float2 _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3;
            Unity_TilingAndOffset_float((_UV_a96366586c5146dabaaee0a5b79e6d2e_Out_0.xy), float2 (1, 1), _Combine_b725d53e914245de8ceae3a4d343a7ce_RG_6, _TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3);
            float2 _Property_adf16741b0414d8db19782b2593b9dc1_Out_0 = _WidthHeight;
            float _Split_68623d6579274f3db940a2abd93a4a18_R_1 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[0];
            float _Split_68623d6579274f3db940a2abd93a4a18_G_2 = _Property_adf16741b0414d8db19782b2593b9dc1_Out_0[1];
            float _Split_68623d6579274f3db940a2abd93a4a18_B_3 = 0;
            float _Split_68623d6579274f3db940a2abd93a4a18_A_4 = 0;
            float _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2);
            float _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0 = _Roundness;
            float _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_8d3c4cc827894ccf9d8184233ef8bfc1_Out_3, _Subtract_d94903bbdfbc43dd89991f063e32a835_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5);
            float4 _UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0 = IN.uv0;
            float _Float_1a5e88c6f1064a10b0757459f51cd441_Out_0 = -0.3;
            float _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0 = _Center_1;
            float4 _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4;
            float3 _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5;
            float2 _Combine_2a22bea91aa24074a7b21b57139930da_RG_6;
            Unity_Combine_float(_Float_1a5e88c6f1064a10b0757459f51cd441_Out_0, _Property_1a02c10041b94b05b56e04f3d0004d37_Out_0, 0, 0, _Combine_2a22bea91aa24074a7b21b57139930da_RGBA_4, _Combine_2a22bea91aa24074a7b21b57139930da_RGB_5, _Combine_2a22bea91aa24074a7b21b57139930da_RG_6);
            float2 _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3;
            Unity_TilingAndOffset_float((_UV_d17b1c16662a4daa82c3dea6434b2ec4_Out_0.xy), float2 (1, 1), _Combine_2a22bea91aa24074a7b21b57139930da_RG_6, _TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3);
            float _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2;
            Unity_Subtract_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.5, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2);
            float _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_61242a1b613043d287137e9368f8f5f6_Out_3, _Subtract_8ad95d02eaea496e88059c742be4b5d3_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5);
            float _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2;
            Unity_Add_float(_RoundedPolygon_f895585b54fd46eeb999d44b4d10ae42_Out_5, _RoundedPolygon_a3ca7ca2be95462982d923cafc7ddeee_Out_5, _Add_bc9e96d8ed534f999dd543c74766a62f_Out_2);
            float4 _UV_62c3ea27df5b44038907a6508a6579e9_Out_0 = IN.uv0;
            float _Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0 = 0;
            float _Property_0bf7b709954d42559e9479178017f719_Out_0 = _Center_1;
            float4 _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4;
            float3 _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5;
            float2 _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6;
            Unity_Combine_float(_Float_520069a03c4d4e9cb6e60fccde4fe2c7_Out_0, _Property_0bf7b709954d42559e9479178017f719_Out_0, 0, 0, _Combine_ae928e0b132946d29f842c8e422c98d3_RGBA_4, _Combine_ae928e0b132946d29f842c8e422c98d3_RGB_5, _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6);
            float2 _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3;
            Unity_TilingAndOffset_float((_UV_62c3ea27df5b44038907a6508a6579e9_Out_0.xy), float2 (1, 1), _Combine_ae928e0b132946d29f842c8e422c98d3_RG_6, _TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3);
            float _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2;
            Unity_Add_float(_Split_68623d6579274f3db940a2abd93a4a18_R_1, 0.2, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2);
            float _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5;
            RoundedPolygon_Func_float(_TilingAndOffset_f9d23f89941c42639a096625087d456b_Out_3, _Add_2d17eb9432874cefb525d6a8bb6b1ef7_Out_2, _Split_68623d6579274f3db940a2abd93a4a18_G_2, 4, _Property_5cbffdebfc7a4114be748cd87224f6fa_Out_0, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5);
            float _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2;
            Unity_Add_float(_Add_bc9e96d8ed534f999dd543c74766a62f_Out_2, _RoundedPolygon_bb4f62997ebf49d2ba5e759adaccb355_Out_5, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2);
            float _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2;
            Unity_Subtract_float(_Rectangle_f64d948dfb6043659b2abcfa0c0eb506_Out_3, _Add_65a23b7e78204b2e8bcd44144bb23040_Out_2, _Subtract_1745d73b75bd4a4396b204599e96528e_Out_2);
            float _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1;
            Unity_OneMinus_float(_Subtract_1745d73b75bd4a4396b204599e96528e_Out_2, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1);
            float _Branch_028656a19a704245ad871b652b140c44_Out_3;
            Unity_Branch_float(_Property_6fb988081629465087579cc3f6c6cf70_Out_0, _OneMinus_1429c8a3569e409b843da0ca595636af_Out_1, 0, _Branch_028656a19a704245ad871b652b140c44_Out_3);
            float _Property_60b342454604428e8c87ef4e7117f46b_Out_0 = _AlphaClip;
            surface.BaseColor = (_Branch_ef9121835cfd4f13af132c2c0905930c_Out_3.xyz);
            surface.Alpha = _Branch_028656a19a704245ad871b652b140c44_Out_3;
            surface.AlphaClipThreshold = _Property_60b342454604428e8c87ef4e7117f46b_Out_0;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpacePosition = input.positionWS;
            output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
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
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphLitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}