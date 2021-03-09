Shader "DCL/Unlit Cutout Tinted" {
Properties {
    _BaseMap ("Base (RGB) Trans (A)", 2D) = "white" {}
    _TintMask ("Mask for tint (Monochannel) (1 == no tint, 0 == tint)", 2D) = "black" {}
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    _BaseColor ("Color", Color) = (0,0,0,0)
}
SubShader {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    LOD 100
	
    Lighting Off
  
    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog
 
            #include "UnityCG.cginc"
 
            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
 
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
            sampler2D _BaseMap;
            sampler2D _TintMask;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                fixed _Cutoff;
                fixed4 _BaseColor;
            CBUFFER_END
            
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _BaseMap);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_BaseMap, i.texcoord);

                col *= lerp(float4(1,1,1,1), _BaseColor, (1.0 - tex2D(_TintMask, i.texcoord).r));

                clip(col.a - _Cutoff);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
        ENDCG
    }
}
 
}