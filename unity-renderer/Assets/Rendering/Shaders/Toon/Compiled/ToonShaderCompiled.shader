Shader "DCL/Toon Shader"
{
    Properties
    {
        [HDR]_BaseColor("Base Color", Color) = (0.6132076, 0.6132076, 0.6132076, 1)
        [NoScaleOffset]_BaseMap("Base Map", 2D) = "white" {}
        [NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}
        [HDR]_EmissionColor("Emission Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_MatCap("Diffuse MatCap", 2D) = "white" {}
        [NoScaleOffset]_GlossMatCap("Gloss MatCap", 2D) = "white" {}
        [NoScaleOffset]_FresnelMatCap("Fresnel MatCap", 2D) = "white" {}
        _SSSIntensity("SSS Intensity", Float) = 0
        _SSSParams("SSSParams", Vector) = (0, 0, 0, 0)
        _Cutoff("AlphaClipThreshold", Float) = 0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
    }
    SubShader
    {
        Tags
        {
        }
        
        Pass
        {
            Name "Pass"
            Tags 
            { 
                // LightMode: <None>
            }
           
            // Render State
            Blend [_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]
            // ColorMask: <None>
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
        
            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma shader_feature _ _SAMPLE_GI
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
        #define ATTRIBUTES_NEED_NORMAL
        #endif
        
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
        #define ATTRIBUTES_NEED_TANGENT
        #endif
        
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
        #define ATTRIBUTES_NEED_TEXCOORD0
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
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #endif
        
        
        
        
        
            #define SHADERPASS_UNLIT
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _EmissionColor;
            float _SSSIntensity;
            float4 _SSSParams;
            float _Cutoff;

            float4 _BaseMap_TexelSize;
            float4 _EmissionMap_TexelSize;
            float4 _MatCap_TexelSize;
            float4 _GlossMatCap_TexelSize;
            float4 _FresnelMatCap_TexelSize;
            CBUFFER_END

            float4 _TintColor;
            float3 _LightDir;
            float4 _LightColor;

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap); 
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap); 
            TEXTURE2D(_MatCap); SAMPLER(sampler_MatCap); 
            TEXTURE2D(_GlossMatCap); SAMPLER(sampler_GlossMatCap); 
            TEXTURE2D(_FresnelMatCap); SAMPLER(sampler_FresnelMatCap); 

            SAMPLER(_SampleTexture2D_F1EE66C6_Sampler_3_Linear_Repeat);
            SAMPLER(_SampleTexture2D_592A5A2C_Sampler_3_Linear_Repeat);
            SAMPLER(_SampleTexture2D_ED76F6F0_Sampler_3_Linear_Repeat);
            SAMPLER(_SampleTexture2D_373639E5_Sampler_3_Linear_Repeat);
            SAMPLER(_SampleTexture2D_D0991351_Sampler_3_Linear_Repeat);
        
            // Graph Functions
            
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
                half4 uv0;
            };
            
            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2, TEXTURE2D_PARAM(Texture2D_4C630F34, samplerTexture2D_4C630F34), float4 Texture2D_4C630F34_TexelSize, Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_26469F85_Out_0 = Vector4_DA7BBBB2;
                float4 _SampleTexture2D_592A5A2C_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_4C630F34, samplerTexture2D_4C630F34, IN.uv0.xy);
                float _SampleTexture2D_592A5A2C_R_4 = _SampleTexture2D_592A5A2C_RGBA_0.r;
                float _SampleTexture2D_592A5A2C_G_5 = _SampleTexture2D_592A5A2C_RGBA_0.g;
                float _SampleTexture2D_592A5A2C_B_6 = _SampleTexture2D_592A5A2C_RGBA_0.b;
                float _SampleTexture2D_592A5A2C_A_7 = _SampleTexture2D_592A5A2C_RGBA_0.a;
                float4 _Multiply_B44FA885_Out_2;
                Unity_Multiply_float(_Property_26469F85_Out_0, _SampleTexture2D_592A5A2C_RGBA_0, _Multiply_B44FA885_Out_2);
                float4 _Property_B97430A3_Out_0 = Vector4_DA7BBBB2;
                float _Split_20609F7_R_1 = _Property_B97430A3_Out_0[0];
                float _Split_20609F7_G_2 = _Property_B97430A3_Out_0[1];
                float _Split_20609F7_B_3 = _Property_B97430A3_Out_0[2];
                float _Split_20609F7_A_4 = _Property_B97430A3_Out_0[3];
                float _Multiply_39CACBA9_Out_2;
                Unity_Multiply_float(_SampleTexture2D_592A5A2C_A_7, _Split_20609F7_A_4, _Multiply_39CACBA9_Out_2);
                Color_1 = _Multiply_B44FA885_Out_2;
                Alpha_2 = _Multiply_39CACBA9_Out_2;
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
            
            void SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(TEXTURE2D_PARAM(Texture2D_8319D4A3, samplerTexture2D_8319D4A3), float4 Texture2D_8319D4A3_TexelSize, TEXTURE2D_PARAM(Texture2D_E7BDB00A, samplerTexture2D_E7BDB00A), float4 Texture2D_E7BDB00A_TexelSize, TEXTURE2D_PARAM(Texture2D_73021B24, samplerTexture2D_73021B24), float4 Texture2D_73021B24_TexelSize, float Vector1_1431D892, float Vector1_C2B7F43A, float4 Vector4_453A6B1F, float Vector1_55629F98, float4 Vector4_752D3319, float3 Vector3_FEFB7F68, float4 Vector4_D503119C, float Vector1_11D631DE, float4 Vector4_92D383D7, Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 IN, out float4 OutVector4_1)
            {
                float4 _Property_901B8B6D_Out_0 = Vector4_D503119C;
                float3 _Normalize_8273837E_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_8273837E_Out_1);
                float3 _Property_372BB985_Out_0 = Vector3_FEFB7F68;
                float3 _Normalize_ADC4EE0_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_ADC4EE0_Out_1);
                float3 _Add_E06668E8_Out_2;
                Unity_Add_float3(_Property_372BB985_Out_0, _Normalize_ADC4EE0_Out_1, _Add_E06668E8_Out_2);
                float3 _Normalize_2D0DBBC_Out_1;
                Unity_Normalize_float3(_Add_E06668E8_Out_2, _Normalize_2D0DBBC_Out_1);
                float _DotProduct_76A480D2_Out_2;
                Unity_DotProduct_float3(_Normalize_8273837E_Out_1, _Normalize_2D0DBBC_Out_1, _DotProduct_76A480D2_Out_2);
                float _Property_C53F68C2_Out_0 = Vector1_55629F98;
                float _Power_5DFD9006_Out_2;
                Unity_Power_float(_DotProduct_76A480D2_Out_2, _Property_C53F68C2_Out_0, _Power_5DFD9006_Out_2);
                float _Clamp_D9CC571E_Out_3;
                Unity_Clamp_float(_Power_5DFD9006_Out_2, 0.38, 0.99, _Clamp_D9CC571E_Out_3);
                float2 _Vector2_44A7BA58_Out_0 = float2(_Clamp_D9CC571E_Out_3, 1.2);
                float4 _SampleTexture2D_ED76F6F0_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_E7BDB00A, samplerTexture2D_E7BDB00A, _Vector2_44A7BA58_Out_0);
                float _SampleTexture2D_ED76F6F0_R_4 = _SampleTexture2D_ED76F6F0_RGBA_0.r;
                float _SampleTexture2D_ED76F6F0_G_5 = _SampleTexture2D_ED76F6F0_RGBA_0.g;
                float _SampleTexture2D_ED76F6F0_B_6 = _SampleTexture2D_ED76F6F0_RGBA_0.b;
                float _SampleTexture2D_ED76F6F0_A_7 = _SampleTexture2D_ED76F6F0_RGBA_0.a;
                float4 _Property_C0DBEB4D_Out_0 = Vector4_752D3319;
                float4 _Multiply_D3D0BB31_Out_2;
                Unity_Multiply_float(_SampleTexture2D_ED76F6F0_RGBA_0, _Property_C0DBEB4D_Out_0, _Multiply_D3D0BB31_Out_2);
                float _Property_B87B6692_Out_0 = Vector1_1431D892;
                float _FresnelEffect_AAA0B3B5_Out_3;
                Unity_FresnelEffect_float(IN.WorldSpaceNormal, IN.WorldSpaceViewDirection, _Property_B87B6692_Out_0, _FresnelEffect_AAA0B3B5_Out_3);
                float3 _Property_B13EEF70_Out_0 = Vector3_FEFB7F68;
                float _Vector1_719CBC0E_Out_0 = -1;
                float3 _Normalize_E4610EF9_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceNormal, _Normalize_E4610EF9_Out_1);
                float3 _Multiply_D1A2DE9D_Out_2;
                Unity_Multiply_float((_Vector1_719CBC0E_Out_0.xxx), _Normalize_E4610EF9_Out_1, _Multiply_D1A2DE9D_Out_2);
                float3 _Normalize_F92BE419_Out_1;
                Unity_Normalize_float3(IN.WorldSpaceViewDirection, _Normalize_F92BE419_Out_1);
                float3 _Add_3414F046_Out_2;
                Unity_Add_float3(_Multiply_D1A2DE9D_Out_2, _Normalize_F92BE419_Out_1, _Add_3414F046_Out_2);
                float3 _Normalize_789E00EE_Out_1;
                Unity_Normalize_float3(_Add_3414F046_Out_2, _Normalize_789E00EE_Out_1);
                float _DotProduct_A6D85DF4_Out_2;
                Unity_DotProduct_float3(_Property_B13EEF70_Out_0, _Normalize_789E00EE_Out_1, _DotProduct_A6D85DF4_Out_2);
                float _Property_46322E1F_Out_0 = Vector1_C2B7F43A;
                float2 _Vector2_12C0A627_Out_0 = float2(1, _Property_46322E1F_Out_0);
                float _Remap_1D35D756_Out_3;
                Unity_Remap_float(_DotProduct_A6D85DF4_Out_2, float2 (-1, 1), _Vector2_12C0A627_Out_0, _Remap_1D35D756_Out_3);
                float _Multiply_F7826E20_Out_2;
                Unity_Multiply_float(_FresnelEffect_AAA0B3B5_Out_3, _Remap_1D35D756_Out_3, _Multiply_F7826E20_Out_2);
                float _Remap_619D6601_Out_3;
                Unity_Remap_float(_Multiply_F7826E20_Out_2, float2 (-1, 1), float2 (0.01, 0.99), _Remap_619D6601_Out_3);
                float2 _Vector2_7E1F1EEA_Out_0 = float2(_Remap_619D6601_Out_3, 0.6);
                float4 _SampleTexture2D_373639E5_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_8319D4A3, samplerTexture2D_8319D4A3, _Vector2_7E1F1EEA_Out_0);
                float _SampleTexture2D_373639E5_R_4 = _SampleTexture2D_373639E5_RGBA_0.r;
                float _SampleTexture2D_373639E5_G_5 = _SampleTexture2D_373639E5_RGBA_0.g;
                float _SampleTexture2D_373639E5_B_6 = _SampleTexture2D_373639E5_RGBA_0.b;
                float _SampleTexture2D_373639E5_A_7 = _SampleTexture2D_373639E5_RGBA_0.a;
                float4 _Property_3F2E1C08_Out_0 = Vector4_453A6B1F;
                float4 _Multiply_CE434B4A_Out_2;
                Unity_Multiply_float(_Property_3F2E1C08_Out_0, float4(1, 1, 1, 1), _Multiply_CE434B4A_Out_2);
                float4 _Multiply_56052616_Out_2;
                Unity_Multiply_float(_SampleTexture2D_373639E5_RGBA_0, _Multiply_CE434B4A_Out_2, _Multiply_56052616_Out_2);
                float3 _Property_8698A325_Out_0 = Vector3_FEFB7F68;
                float _DotProduct_1FDA7E42_Out_2;
                Unity_DotProduct_float3(IN.WorldSpaceNormal, _Property_8698A325_Out_0, _DotProduct_1FDA7E42_Out_2);
                float _Remap_8C993BE6_Out_3;
                Unity_Remap_float(_DotProduct_1FDA7E42_Out_2, float2 (-1, 1), float2 (0.01, 0.99), _Remap_8C993BE6_Out_3);
                float _Vector1_CA43906C_Out_0 = 1;
                float2 _Vector2_5DAC94A7_Out_0 = float2(_Remap_8C993BE6_Out_3, _Vector1_CA43906C_Out_0);
                float4 _SampleTexture2D_D0991351_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_73021B24, samplerTexture2D_73021B24, _Vector2_5DAC94A7_Out_0);
                float _SampleTexture2D_D0991351_R_4 = _SampleTexture2D_D0991351_RGBA_0.r;
                float _SampleTexture2D_D0991351_G_5 = _SampleTexture2D_D0991351_RGBA_0.g;
                float _SampleTexture2D_D0991351_B_6 = _SampleTexture2D_D0991351_RGBA_0.b;
                float _SampleTexture2D_D0991351_A_7 = _SampleTexture2D_D0991351_RGBA_0.a;
                float4 Color_348AB651 = IsGammaSpace() ? float4(0.7075472, 0.7075472, 0.7075472, 0) : float4(SRGBToLinear(float3(0.7075472, 0.7075472, 0.7075472)), 0);
                float4 _Multiply_A21999C2_Out_2;
                Unity_Multiply_float(_SampleTexture2D_D0991351_RGBA_0, Color_348AB651, _Multiply_A21999C2_Out_2);
                float4 _Add_2FDF1664_Out_2;
                Unity_Add_float4(_Multiply_56052616_Out_2, _Multiply_A21999C2_Out_2, _Add_2FDF1664_Out_2);
                float4 _Add_4BDC99FF_Out_2;
                Unity_Add_float4(_Multiply_D3D0BB31_Out_2, _Add_2FDF1664_Out_2, _Add_4BDC99FF_Out_2);
                float4 _Multiply_B754EF2_Out_2;
                Unity_Multiply_float(_Property_901B8B6D_Out_0, _Add_4BDC99FF_Out_2, _Multiply_B754EF2_Out_2);
                OutVector4_1 = _Multiply_B754EF2_Out_2;
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
            
            void SG_Fog_db57d56e4661e4144b06df0b3edef8a9(float4 Color_42779DA4, Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 IN, out float4 Color_1)
            {
                float4 _Property_618E73CB_Out_0 = Color_42779DA4;
                float4 _Fog_CC47E476_Color_0;
                float _Fog_CC47E476_Density_1;
                Unity_Fog_float(_Fog_CC47E476_Color_0, _Fog_CC47E476_Density_1, IN.ViewSpacePosition);
                float _OneMinus_ABEEFC9F_Out_1;
                Unity_OneMinus_float(_Fog_CC47E476_Density_1, _OneMinus_ABEEFC9F_Out_1);
                float4 _Lerp_B5B3D015_Out_3;
                Unity_Lerp_float4(_Property_618E73CB_Out_0, _Fog_CC47E476_Color_0, (_OneMinus_ABEEFC9F_Out_1.xxxx), _Lerp_B5B3D015_Out_3);
                Color_1 = _Lerp_B5B3D015_Out_3;
            }
            
            struct Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb
            {
                float3 ViewSpacePosition;
            };
            
            void SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(float4 Color_E739F888, float4 Color_D4F585C6, float4 Color_D7818A04, float4 Color_546468F9, Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb IN, out float4 FinalColor_1)
            {
                float4 _Property_4D5FC0A4_Out_0 = Color_E739F888;
                float4 _Property_463AA739_Out_0 = Color_D4F585C6;
                float4 _Property_F9FDD8A9_Out_0 = Color_546468F9;
                float4 _Property_3D5D922_Out_0 = Color_D7818A04;
                float4 _Add_B566B9BB_Out_2;
                Unity_Add_float4(_Property_F9FDD8A9_Out_0, _Property_3D5D922_Out_0, _Add_B566B9BB_Out_2);
                float4 _Blend_2E558B58_Out_2;
                Unity_Blend_Multiply_float4(_Property_463AA739_Out_0, _Add_B566B9BB_Out_2, _Blend_2E558B58_Out_2, 1);
                float4 _Add_380AFE1D_Out_2;
                Unity_Add_float4(_Property_4D5FC0A4_Out_0, _Blend_2E558B58_Out_2, _Add_380AFE1D_Out_2);
                Bindings_Fog_db57d56e4661e4144b06df0b3edef8a9 _Fog_2A6D8F80;
                _Fog_2A6D8F80.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _Fog_2A6D8F80_Color_1;
                SG_Fog_db57d56e4661e4144b06df0b3edef8a9(_Add_380AFE1D_Out_2, _Fog_2A6D8F80, _Fog_2A6D8F80_Color_1);
                FinalColor_1 = _Fog_2A6D8F80_Color_1;
            }
        
            // Graph Vertex
            // GraphVertex: <None>
            
            // Graph Pixel
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
            };
            
            struct SurfaceDescription
            {
                float3 Color;
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_432780F8_Out_0 = _EmissionColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _SampleTexture2D_F1EE66C6_RGBA_0 = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv0.xy);
                float _SampleTexture2D_F1EE66C6_R_4 = _SampleTexture2D_F1EE66C6_RGBA_0.r;
                float _SampleTexture2D_F1EE66C6_G_5 = _SampleTexture2D_F1EE66C6_RGBA_0.g;
                float _SampleTexture2D_F1EE66C6_B_6 = _SampleTexture2D_F1EE66C6_RGBA_0.b;
                float _SampleTexture2D_F1EE66C6_A_7 = _SampleTexture2D_F1EE66C6_RGBA_0.a;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_66E414AD_Out_2;
                Unity_Multiply_float(_Property_432780F8_Out_0, _SampleTexture2D_F1EE66C6_RGBA_0, _Multiply_66E414AD_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_DC445BEF_Out_0 = _BaseColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_7368FD5F;
                _TextureSample_7368FD5F.uv0 = IN.uv0;
                float4 _TextureSample_7368FD5F_Color_1;
                float _TextureSample_7368FD5F_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(_Property_DC445BEF_Out_0, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap), _BaseMap_TexelSize, _TextureSample_7368FD5F, _TextureSample_7368FD5F_Color_1, _TextureSample_7368FD5F_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 Color_A56BBF16 = IsGammaSpace() ? float4(0.6698113, 0.6698113, 0.6698113, 0) : float4(SRGBToLinear(float3(0.6698113, 0.6698113, 0.6698113)), 0);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 Color_2C5C3282 = IsGammaSpace() ? float4(0.6509434, 0.6509434, 0.6509434, 0) : float4(SRGBToLinear(float3(0.6509434, 0.6509434, 0.6509434)), 0);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 Color_6481DB60 = IsGammaSpace() ? float4(1.317959, 1.317959, 1.317959, 1) : float4(SRGBToLinear(float3(1.317959, 1.317959, 1.317959)), 1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 Color_FC39AC41 = IsGammaSpace() ? float4(0.6603774, 0.6603774, 0.6603774, 1) : float4(SRGBToLinear(float3(0.6603774, 0.6603774, 0.6603774)), 1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float3 _Property_EE0DEFE3_Out_0 = _LightDir;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_DC394E50_Out_0 = _LightColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Property_F0A41D64_Out_0 = _SSSIntensity;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_34EA98AC_Out_0 = _SSSParams;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_ToonShader_4cadc1cb7cae444909bd6637f15fdf84 _ToonShader_FA30CA77;
                _ToonShader_FA30CA77.WorldSpaceNormal = IN.WorldSpaceNormal;
                _ToonShader_FA30CA77.WorldSpaceViewDirection = IN.WorldSpaceViewDirection;
                float4 _ToonShader_FA30CA77_OutVector4_1;
                SG_ToonShader_4cadc1cb7cae444909bd6637f15fdf84(TEXTURE2D_ARGS(_FresnelMatCap, sampler_FresnelMatCap), _FresnelMatCap_TexelSize, TEXTURE2D_ARGS(_GlossMatCap, sampler_GlossMatCap), _GlossMatCap_TexelSize, TEXTURE2D_ARGS(_MatCap, sampler_MatCap), _MatCap_TexelSize, 1.77, -0.27, Color_6481DB60, 0.17, Color_FC39AC41, _Property_EE0DEFE3_Out_0, _Property_DC394E50_Out_0, _Property_F0A41D64_Out_0, _Property_34EA98AC_Out_0, _ToonShader_FA30CA77, _ToonShader_FA30CA77_OutVector4_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Multiply_BD25ADC4_Out_2;
                Unity_Multiply_float(Color_2C5C3282, _ToonShader_FA30CA77_OutVector4_1, _Multiply_BD25ADC4_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Add_EB51409C_Out_2;
                Unity_Add_float4(Color_A56BBF16, _Multiply_BD25ADC4_Out_2, _Add_EB51409C_Out_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_8AD7C09E_Out_0 = _TintColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb _FinalCombine_D0158F10;
                _FinalCombine_D0158F10.ViewSpacePosition = IN.ViewSpacePosition;
                float4 _FinalCombine_D0158F10_FinalColor_1;
                SG_FinalCombine_71273f7177aa0cc4f9ddbeefd14a5fbb(_Multiply_66E414AD_Out_2, _TextureSample_7368FD5F_Color_1, _Add_EB51409C_Out_2, _Property_8AD7C09E_Out_0, _FinalCombine_D0158F10, _FinalCombine_D0158F10_FinalColor_1);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Property_5330B560_Out_0 = _Cutoff;
                #endif
                surface.Color = (_FinalCombine_D0158F10_FinalColor_1.xyz);
                surface.Alpha = _TextureSample_7368FD5F_Alpha_2;
                surface.AlphaClipThreshold = _Property_5330B560_Out_0;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
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
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };
        
            // Generated Type: Varyings
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
                float3 viewDirectionWS;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };
            
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float3 interp00 : TEXCOORD0;
                float3 interp01 : TEXCOORD1;
                float4 interp02 : TEXCOORD2;
                float3 interp03 : TEXCOORD3;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyz = input.positionWS;
                output.interp01.xyz = input.normalWS;
                output.interp02.xyzw = input.texCoord0;
                output.interp03.xyz = input.viewDirectionWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.positionWS = input.interp00.xyz;
                output.normalWS = input.interp01.xyz;
                output.texCoord0 = input.interp02.xyzw;
                output.viewDirectionWS = input.interp03.xyz;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            #endif
        
            // --------------------------------------------------
            // Build Graph Inputs
        
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
            output.WorldSpaceNormal =            renormFactor*input.normalWS.xyz;		// we want a unit length Normal Vector node in shader graph
            #endif
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            output.WorldSpaceViewDirection =     input.viewDirectionWS; //TODO: by default normalized in HD, but not in universal
            #endif
            
            
            
            
            
            
            
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            output.ViewSpacePosition =           TransformWorldToView(input.positionWS);
            #endif
            
            
            
            
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            output.uv0 =                         input.texCoord0;
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
            Blend [_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            Cull Off
            // ColorMask: <None>
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
        
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
        #define ATTRIBUTES_NEED_NORMAL
        #endif
        
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
        #define ATTRIBUTES_NEED_TANGENT
        #endif
        
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
        #define ATTRIBUTES_NEED_TEXCOORD0
        #endif
        
        
        
        
        
        
        
        
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
        #define VARYINGS_NEED_TEXCOORD0
        #endif
        
        
        
        
        
        
        
        
        
        
            #define SHADERPASS_SHADOWCASTER
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _EmissionColor;
            float _SSSIntensity;
            float4 _SSSParams;
            float _Cutoff;

            float4 _BaseMap_TexelSize;
            float4 _EmissionMap_TexelSize;            
            float4 _MatCap_TexelSize;
            float4 _GlossMatCap_TexelSize;
            float4 _FresnelMatCap_TexelSize;
            CBUFFER_END

            float4 _TintColor;
            float3 _LightDir;
            float4 _LightColor;
            
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap); 
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap); 
            TEXTURE2D(_MatCap); SAMPLER(sampler_MatCap); 
            TEXTURE2D(_GlossMatCap); SAMPLER(sampler_GlossMatCap); 
            TEXTURE2D(_FresnelMatCap); SAMPLER(sampler_FresnelMatCap); 
            SAMPLER(_SampleTexture2D_592A5A2C_Sampler_3_Linear_Repeat);
        
            // Graph Functions
            
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
                half4 uv0;
            };
            
            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2, TEXTURE2D_PARAM(Texture2D_4C630F34, samplerTexture2D_4C630F34), float4 Texture2D_4C630F34_TexelSize, Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_26469F85_Out_0 = Vector4_DA7BBBB2;
                float4 _SampleTexture2D_592A5A2C_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_4C630F34, samplerTexture2D_4C630F34, IN.uv0.xy);
                float _SampleTexture2D_592A5A2C_R_4 = _SampleTexture2D_592A5A2C_RGBA_0.r;
                float _SampleTexture2D_592A5A2C_G_5 = _SampleTexture2D_592A5A2C_RGBA_0.g;
                float _SampleTexture2D_592A5A2C_B_6 = _SampleTexture2D_592A5A2C_RGBA_0.b;
                float _SampleTexture2D_592A5A2C_A_7 = _SampleTexture2D_592A5A2C_RGBA_0.a;
                float4 _Multiply_B44FA885_Out_2;
                Unity_Multiply_float(_Property_26469F85_Out_0, _SampleTexture2D_592A5A2C_RGBA_0, _Multiply_B44FA885_Out_2);
                float4 _Property_B97430A3_Out_0 = Vector4_DA7BBBB2;
                float _Split_20609F7_R_1 = _Property_B97430A3_Out_0[0];
                float _Split_20609F7_G_2 = _Property_B97430A3_Out_0[1];
                float _Split_20609F7_B_3 = _Property_B97430A3_Out_0[2];
                float _Split_20609F7_A_4 = _Property_B97430A3_Out_0[3];
                float _Multiply_39CACBA9_Out_2;
                Unity_Multiply_float(_SampleTexture2D_592A5A2C_A_7, _Split_20609F7_A_4, _Multiply_39CACBA9_Out_2);
                Color_1 = _Multiply_B44FA885_Out_2;
                Alpha_2 = _Multiply_39CACBA9_Out_2;
            }
        
            // Graph Vertex
            // GraphVertex: <None>
            
            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
            };
            
            struct SurfaceDescription
            {
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_DC445BEF_Out_0 = _BaseColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_7368FD5F;
                _TextureSample_7368FD5F.uv0 = IN.uv0;
                float4 _TextureSample_7368FD5F_Color_1;
                float _TextureSample_7368FD5F_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(_Property_DC445BEF_Out_0, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap), _BaseMap_TexelSize, _TextureSample_7368FD5F, _TextureSample_7368FD5F_Color_1, _TextureSample_7368FD5F_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Property_5330B560_Out_0 = _Cutoff;
                #endif
                surface.Alpha = _TextureSample_7368FD5F_Alpha_2;
                surface.AlphaClipThreshold = _Property_5330B560_Out_0;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
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
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };
        
            // Generated Type: Varyings
            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };
            
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float4 interp00 : TEXCOORD0;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyzw = input.texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.interp00.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            #endif
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            output.uv0 =                         input.texCoord0;
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
            Blend [_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]
            ColorMask 0
            
        
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            // Pragmas
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
        
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
        #define ATTRIBUTES_NEED_NORMAL
        #endif
        
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
        #define ATTRIBUTES_NEED_TANGENT
        #endif
        
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
        #define ATTRIBUTES_NEED_TEXCOORD0
        #endif
        
        
        
        
        
        
        
        
        #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
        #define VARYINGS_NEED_TEXCOORD0
        #endif
        
        
        
        
        
        
        
        
        
        
            #define SHADERPASS_DEPTHONLY
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _EmissionColor;
            float _SSSIntensity;
            float4 _SSSParams;
            float _Cutoff;
            float4 _BaseMap_TexelSize;            
            float4 _EmissionMap_TexelSize;
            float4 _MatCap_TexelSize;
            float4 _GlossMatCap_TexelSize;            
            float4 _FresnelMatCap_TexelSize;            
            CBUFFER_END

            float4 _TintColor;
            float3 _LightDir;
            float4 _LightColor;
            
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap); 
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap); 
            TEXTURE2D(_MatCap); SAMPLER(sampler_MatCap); 
            TEXTURE2D(_GlossMatCap); SAMPLER(sampler_GlossMatCap); 
            TEXTURE2D(_FresnelMatCap); SAMPLER(sampler_FresnelMatCap); 
            SAMPLER(_SampleTexture2D_592A5A2C_Sampler_3_Linear_Repeat);
        
            // Graph Functions
            
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
                half4 uv0;
            };
            
            void SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(float4 Vector4_DA7BBBB2, TEXTURE2D_PARAM(Texture2D_4C630F34, samplerTexture2D_4C630F34), float4 Texture2D_4C630F34_TexelSize, Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 IN, out float4 Color_1, out float Alpha_2)
            {
                float4 _Property_26469F85_Out_0 = Vector4_DA7BBBB2;
                float4 _SampleTexture2D_592A5A2C_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_4C630F34, samplerTexture2D_4C630F34, IN.uv0.xy);
                float _SampleTexture2D_592A5A2C_R_4 = _SampleTexture2D_592A5A2C_RGBA_0.r;
                float _SampleTexture2D_592A5A2C_G_5 = _SampleTexture2D_592A5A2C_RGBA_0.g;
                float _SampleTexture2D_592A5A2C_B_6 = _SampleTexture2D_592A5A2C_RGBA_0.b;
                float _SampleTexture2D_592A5A2C_A_7 = _SampleTexture2D_592A5A2C_RGBA_0.a;
                float4 _Multiply_B44FA885_Out_2;
                Unity_Multiply_float(_Property_26469F85_Out_0, _SampleTexture2D_592A5A2C_RGBA_0, _Multiply_B44FA885_Out_2);
                float4 _Property_B97430A3_Out_0 = Vector4_DA7BBBB2;
                float _Split_20609F7_R_1 = _Property_B97430A3_Out_0[0];
                float _Split_20609F7_G_2 = _Property_B97430A3_Out_0[1];
                float _Split_20609F7_B_3 = _Property_B97430A3_Out_0[2];
                float _Split_20609F7_A_4 = _Property_B97430A3_Out_0[3];
                float _Multiply_39CACBA9_Out_2;
                Unity_Multiply_float(_SampleTexture2D_592A5A2C_A_7, _Split_20609F7_A_4, _Multiply_39CACBA9_Out_2);
                Color_1 = _Multiply_B44FA885_Out_2;
                Alpha_2 = _Multiply_39CACBA9_Out_2;
            }
        
            // Graph Vertex
            // GraphVertex: <None>
            
            // Graph Pixel
            struct SurfaceDescriptionInputs
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 uv0;
                #endif
            };
            
            struct SurfaceDescription
            {
                float Alpha;
                float AlphaClipThreshold;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 _Property_DC445BEF_Out_0 = _BaseColor;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                Bindings_TextureSample_556dc6c40f462774daf8a2ff53e16c59 _TextureSample_7368FD5F;
                _TextureSample_7368FD5F.uv0 = IN.uv0;
                float4 _TextureSample_7368FD5F_Color_1;
                float _TextureSample_7368FD5F_Alpha_2;
                SG_TextureSample_556dc6c40f462774daf8a2ff53e16c59(_Property_DC445BEF_Out_0, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap), _BaseMap_TexelSize, _TextureSample_7368FD5F, _TextureSample_7368FD5F_Color_1, _TextureSample_7368FD5F_Alpha_2);
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float _Property_5330B560_Out_0 = _Cutoff;
                #endif
                surface.Alpha = _TextureSample_7368FD5F_Alpha_2;
                surface.AlphaClipThreshold = _Property_5330B560_Out_0;
                return surface;
            }
        
            // --------------------------------------------------
            // Structs and Packing
        
            // Generated Type: Attributes
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
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
                #endif
            };
        
            // Generated Type: Varyings
            struct Varyings
            {
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 positionCS : SV_POSITION;
                #endif
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                float4 texCoord0;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
                #endif
            };
            
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            // Generated Type: PackedVaryings
            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                float4 interp00 : TEXCOORD0;
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            
            // Packed Type: Varyings
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output = (PackedVaryings)0;
                output.positionCS = input.positionCS;
                output.interp00.xyzw = input.texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            
            // Unpacked Type: Varyings
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.interp00.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }
            #endif
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            #if defined(KEYWORD_PERMUTATION_0) || defined(KEYWORD_PERMUTATION_1) || defined(KEYWORD_PERMUTATION_2) || defined(KEYWORD_PERMUTATION_3) || defined(KEYWORD_PERMUTATION_4) || defined(KEYWORD_PERMUTATION_5) || defined(KEYWORD_PERMUTATION_6) || defined(KEYWORD_PERMUTATION_7)
            output.uv0 =                         input.texCoord0;
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
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
            ENDHLSL
        }
        
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}
