﻿Shader "AVProVideo/Internal/Preview"
{
	Properties
	{
		_MainTex("Texture", any) = "" {}
	}

	SubShader
	{
		Tags { "ForceSupported" = "True" }

		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			Name "ALPHA BLEND"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "../AVProVideo.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 clipUV : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _MainTex;
			uniform sampler2D _GUIClipTexture;
			uniform bool _ManualTex2SRGB;
			uniform float4 _MainTex_ST;
			uniform fixed4 _Color;
			uniform float4x4 unity_GUIClipTextureMatrix;

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 eyePos = UnityObjectToViewPos(v.vertex);
				o.clipUV = mul(unity_GUIClipTextureMatrix, float4(eyePos.xy, 0, 1.0));
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 colTex = tex2D(_MainTex, i.texcoord);
				if (_ManualTex2SRGB)
					colTex.rgb = LinearToGamma(colTex.rgb);
				fixed4 col = colTex * i.color;
				col.a *= tex2D(_GUIClipTexture, i.clipUV).a;
				return col;
			}
			ENDCG
		}
	}

	Fallback off
}