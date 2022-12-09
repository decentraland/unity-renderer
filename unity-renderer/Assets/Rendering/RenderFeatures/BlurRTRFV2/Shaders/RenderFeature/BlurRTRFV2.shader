Shader "Custom/RF/BlurRTRFV2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
      //   _offset ("Offset", float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            // Data from mesh or object
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Data to be passed to the pixel shader
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            
            float _offset;

            // Vertex shader
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Fragment shader
            fixed4 frag (v2f input) : SV_Target
            {
                // Sample the texture at the pixel's UV coordinates
                float2 res = _MainTex_TexelSize.xy;
                
                // offsetting 
                float i = _offset;
                
                // coloration
                fixed4 col;    
                            
                col.rgb = tex2D( _MainTex, input.uv ).rgb;
                
                col.rgb += tex2D( _MainTex, input.uv + float2( i, i ) * res ).rgb;
                col.rgb += tex2D( _MainTex, input.uv + float2( i, -i ) * res ).rgb;
                col.rgb += tex2D( _MainTex, input.uv + float2( -i, i ) * res ).rgb;
                col.rgb += tex2D( _MainTex, input.uv + float2( -i, -i ) * res ).rgb;
                
                col.rgb /= 5.0f;
                
                return col;
            }
            ENDCG
        }
    }
}
