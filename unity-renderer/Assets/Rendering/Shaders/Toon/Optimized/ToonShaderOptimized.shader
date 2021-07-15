Shader "DCL/Toon Shader Legacy (Texture Arrays)"
{
    Properties
    {
        _MatCap ("MatCap (RGB)", 2D) = "white" {}
        //_BaseColor ("Color", Color) = (0, 0, 0, 0)
        //_EmissionMap ("Emission Map (RGB)", 2DArray) = "black" {}
        //[HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)
        //_Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5
        _GlobalAvatarTextureArray_ST ("Global Texture Transform", Vector) = (1,1,0,0)

        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
    }

    Subshader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }
        Blend [_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]
        Cull [_Cull]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma require 2darray
            //#pragma multi_compile _ _EMISSION
            //#pragma multi_compile _ _ALPHATEST_ON

            #include "UnityCG.cginc"

            struct mesh_data
            {
                float4 color : COLOR;
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 emission_color : TEXCOORD6;
                float2 texture_indexes : TEXCOORD7;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 cap : TEXCOORD1;
                float2 texture_indexes : TEXCOORD7;
                float4 emission_color : TEXCOORD6;
                float4 color : COLOR;
                UNITY_FOG_COORDS(2)
            };


            UNITY_DECLARE_TEX2DARRAY(_GlobalAvatarTextureArray);

            uniform sampler2D _MatCap;

            CBUFFER_START(UnityPerMaterial)
            float4 _GlobalAvatarTextureArray_ST;
            fixed4 _BaseColor;
            float4 _EmissionColor;
            float _Cutoff;
            CBUFFER_END

            v2f vert(mesh_data v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _GlobalAvatarTextureArray);
                o.texture_indexes = float2(abs(v.texture_indexes[0]), abs(v.texture_indexes[1]));
                o.color = v.color;
                o.emission_color = v.emission_color;

                float3 worldNorm = normalize(
                    unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y +
                    unity_WorldToObject[2].xyz * v.normal.z);
                worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
                o.cap.xy = worldNorm.xy * 0.5 + 0.5;

                UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                float4 albedo = 1, emission = 0;

                if (i.texture_indexes[0] >= 0)
                {
                    albedo = UNITY_SAMPLE_TEX2DARRAY(_GlobalAvatarTextureArray,
                                                     float3(i.uv.x, i.uv.y, i.texture_indexes[0]));
                }

                if (i.texture_indexes[1] >= 0)
                {
                    emission = UNITY_SAMPLE_TEX2DARRAY(_GlobalAvatarTextureArray,
                                                       float3(i.uv.x, i.uv.y, i.texture_indexes[1]));
                }

                fixed4 tex = albedo * i.color;
                fixed4 matcap = tex2D(_MatCap, i.cap);

                #ifdef _ALPHATEST_ON
					clip(tex.a - _Cutoff);
                #endif

                // perform the blending operation in gamma space to get the same result in linear space
                //tex.rgb = LinearToGammaSpace(tex.rgb);
                //matcap.rgb = LinearToGammaSpace(matcap.rgb);
                matcap *= 3.0;
                matcap = saturate(tex + matcap - 1.0);
                //matcap.rgb = GammaToLinearSpace(matcap.rgb);

                //#ifdef _EMISSION
                float4 emissionColor = i.emission_color;
                matcap.rgb += (emission * emissionColor).rgb;
                //#endif
                matcap.a = tex.a;
                UNITY_APPLY_FOG(i.fogCoord, matcap);
                return matcap;
            }
            ENDCG
        }
    }

    Fallback "DCL/LWRP/Lit"
}