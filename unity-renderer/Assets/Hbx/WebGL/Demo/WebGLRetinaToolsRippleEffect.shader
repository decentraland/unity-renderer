Shader "Hbx/WebGL/WebGLRetinaToolsRippleEffect"
{
	// Expensive fragment shader to demonstrate fill rate performance

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Scale", Range(0.01,0.5)) = 0.05
        _Speed ("Speed", Range(0,10.0)) = 1.0
		_Depth ("Depth", Range(0.0,2.0)) = 0.2
		_UserTime("UserTime", Range(0.0,10.0)) = 0.0
		_Aspect ("Aspect", Range(0.0,2.0)) = 1.6
		_RippleStartTime("StartTime", Range(0.0,5.0)) = 0.0
		_RippleStartPoint ("StartPoint", Vector) = (0.5,0.5,0.0,0.0)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off
        CGPROGRAM
        #pragma surface surf Lambert
        #include "UnityCG.cginc"
        
        fixed4 _Color;
        fixed _Scale;
        fixed _Speed;
        sampler2D _MainTex;
		fixed _Aspect;
		fixed _Depth;
		fixed _UserTime;

		#define NumRipples 10
		uniform float _RippleStartTimes[NumRipples];
		uniform float4 _RippleStartPoints[NumRipples];

		uniform float _RippleStartTime;
		uniform float4 _RippleStartPoint;
        
        struct Input {
            float2 uv_MainTex;
        };

		fixed2 calcRippleDistort(fixed2 sourceUV, fixed startTime, fixed2 center, fixed width, fixed speed)
		{
			fixed deltatime = _Time[1] - startTime;
			fixed radius = deltatime * speed;

			fixed2 uv = (sourceUV - center);//
			uv.x *= _Aspect;
			fixed dist = sqrt(uv.x*uv.x + uv.y*uv.y);//   dot(uv, uv));
  			
			fixed m = 1.0 - (1.0 + smoothstep(radius, radius + width, dist) - smoothstep(radius - width, radius, dist));
			m = (1.0 - m) * m;

			fixed uvs = 1.0 / dist;
			fixed2 ref = ((uv * uvs) * m) * _Depth;
			return ref;
		}

        void surf (Input IN, inout SurfaceOutput o)
		{
			fixed2 aspectuv = IN.uv_MainTex;
			//aspectuv.x *= _Aspect;

			fixed2 ref = fixed2(0.0,0.0);
			for(int i=0; i<NumRipples; i++)
			{
				ref += calcRippleDistort(aspectuv, _RippleStartTimes[i], _RippleStartPoints[i].xy, _Scale, _Speed);
			}
			
			//ref *= 1.0/NumRipples;
			//ref = calcRippleDistort(aspectuv, _RippleStartTime, _RippleStartPoint.xy, _Scale, _Speed);

            o.Albedo = _Color.rgb * tex2D (_MainTex, IN.uv_MainTex.xy+ref).rgb;
            o.Alpha = _Color.a;
            //o.Normal = normalize(fixed3(ref.x, ref.y, 1.0));
        }
        ENDCG
    } 
    FallBack "Diffuse"
}