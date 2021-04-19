Shader "Builder/Outlined Uniform"
{
	Properties
	{
		_OutlineColor ("Outline color", Color) = (0,0,0,1)
		_OutlineWidth ("Outlines width", Range (0.0, 10.0)) = 1.1
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
	};

	struct v2f
	{
		float4 pos : POSITION;
	};

	uniform float _OutlineWidth;
	uniform float4 _OutlineColor;

	ENDCG

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" }

		Pass //Outline
		{
			ZWrite On
			Cull front
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata v)
			{
				appdata original = v;
				v.vertex.xyz += _OutlineWidth * normalize(v.vertex.xyz);

				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;

			}

			half4 frag(v2f i) : COLOR
			{
				return _OutlineColor;
			}

			ENDCG
		}
	}
}