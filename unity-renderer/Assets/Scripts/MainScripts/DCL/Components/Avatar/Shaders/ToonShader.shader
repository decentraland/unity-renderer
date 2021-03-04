Shader "DCL/Toon Shader Legacy"
{
	Properties
	{
		_BaseMap ("Base (RGB)", 2D) = "white" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
		_BaseColor ("Color", Color) = (0, 0, 0, 0)
		_EmissionMap ("Emission Map (RGB)", 2D) = "black" {}
		[HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)
		_Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5

		[HideInInspector] _AlphaClip("__clip", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
		[HideInInspector] _Cull("__cull", Float) = 2.0
	}
	
	Subshader
	{
		Tags { "RenderPipeline"="UniversalPipeline" }
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

			#pragma multi_compile _ _EMISSION
			#pragma multi_compile _ _ALPHATEST_ON

			#include "UnityCG.cginc"
				
			struct v2f
			{
				float4 pos	: SV_POSITION;
				float2 uv 	: TEXCOORD0;
				float2 cap	: TEXCOORD1;
				UNITY_FOG_COORDS(2)
			};

			uniform float4 _BaseMap_ST;
				
			uniform sampler2D _BaseMap;
			uniform sampler2D _MatCap;
            uniform sampler2D _EmissionMap;

			fixed4 _BaseColor;
			float4 _EmissionColor;
			float _Cutoff;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _BaseMap);
					
				float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
				worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
				o.cap.xy = worldNorm.xy * 0.5 + 0.5;
					
				UNITY_TRANSFER_FOG(o, o.pos);

				return o;
			}
				
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 tex = tex2D(_BaseMap, i.uv) * _BaseColor;
				fixed4 matcap = tex2D(_MatCap, i.cap);

				#ifdef _ALPHATEST_ON
					clip(tex.a - _Cutoff);
				#endif

				#ifdef UNITY_COLORSPACE_GAMMA
					matcap.rgb = tex.rgb + (mc.rgb * 2.0) - 1.0;
				#else
					// perform the blending operation in gamma space to get the same result in linear space
					tex.rgb = LinearToGammaSpace(tex.rgb);
					matcap.rgb = LinearToGammaSpace(matcap.rgb);
					matcap *= 2.0;
					matcap = saturate(tex + matcap - 1.0);
					matcap.rgb = GammaToLinearSpace(matcap.rgb);
				#endif

				#ifdef _EMISSION
					matcap.rgb += (tex2D(_EmissionMap, i.uv) * _EmissionColor).rgb;
				#endif
                matcap.a = tex.a;
				UNITY_APPLY_FOG(i.fogCoord, matcap);
				return matcap;

			}
			ENDCG
		}
	}

    Fallback "DCL/LWRP/Lit"
}
