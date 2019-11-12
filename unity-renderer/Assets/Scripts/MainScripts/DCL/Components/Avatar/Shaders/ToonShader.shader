Shader "DCL/Toon Shader"
{
	Properties
	{
		_BaseMap ("Base (RGB)", 2D) = "white" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
		_BaseColor ("Color", Color) = (0,0,0,0)
		_EmissionColor ("Emission Color", Color) = (0,0,0,0)
	}
	
	Subshader
	{
		Tags { "RenderType"="Opaque" "RenderPipeline" = "LightweightPipeline"}
		
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fog
				#include "UnityCG.cginc"
				
				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 uv 	: TEXCOORD0;
					float2 cap	: TEXCOORD1;
					UNITY_FOG_COORDS(2)
				};
				
				uniform float4 _BaseMap_ST;
				
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
				
				uniform sampler2D _BaseMap;
				uniform sampler2D _MatCap;

				fixed4 _BaseColor;
				float4 _EmissionColor; // NOTE(Brian): just added this to prevent gltf warning
				
				fixed4 frag (v2f i) : COLOR
				{
					fixed4 tex = tex2D(_BaseMap, i.uv) * _BaseColor;
					fixed4 mc = tex2D(_MatCap, i.cap);

			#ifndef UNITY_COLORSPACE_GAMMA
					// perform the blending operation in gamma space to get the same result in linear space
					tex.rgb = LinearToGammaSpace(tex.rgb);
					mc.rgb = LinearToGammaSpace(mc.rgb);
					mc *= 2.0;
					mc = saturate(tex + mc - 1.0);
					mc.rgb = GammaToLinearSpace(mc.rgb);
					UNITY_APPLY_FOG(i.fogCoord, mc);
					return mc ;
			#else
					mc.rgb = tex.rgb + (mc.rgb * 2.0) - 1.0;
					mc += _EmissionColor;

					UNITY_APPLY_FOG(i.fogCoord, mc);
					return mc;
			#endif
				}
			ENDCG
		}
	}
	
//Fallback "Lightweight Render Pipeline/Lit"

}
