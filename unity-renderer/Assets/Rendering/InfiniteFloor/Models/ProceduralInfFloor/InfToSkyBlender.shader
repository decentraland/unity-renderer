Shader "CustomShader/CG/Unlit/InfToSkyBlender"
{
    Properties
    {
        
        // a color
        _ColorSky ("Color Sky Area", Color) = (1,1,1,1)
        _ColorInfFloor ("Color Infinite Floor Area", Color) = (1,1,1,1)
        
        // slider to control the lerp of the two colors 
        _Blender ("Color Blender", Range(-1,1)) = 0
        _MiddlePoint ("Middle Point", Range(-1,1)) = 0
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        // 2 sided 
        Cull Off
        
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            // get data from unity        
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // convertors
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // colors
            fixed4 _ColorInfFloor;
            fixed4 _ColorSky;

            // blender
            float _Blender;

            // middle point
            float _MiddlePoint;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f_img i) : SV_Target
            {
                // adding vertex uv x to _UVX property
				_Blender = (i.uv.y + _MiddlePoint) + _Blender;
                
				
			    // lerping the colors based on _UVX
                fixed3 color = lerp(_ColorSky , _ColorInfFloor , _Blender);
				
                return fixed4(color, 1.0);
                
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                
            }
            ENDCG
        }
    }
}
