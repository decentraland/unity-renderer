Shader "DCL/UI-SimpleGaussianBlur" {
    Properties {
        [HideInInspector][PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255

        [HideInInspector]_ColorMask ("Color Mask", Float) = 15

        [HideInInspector][Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _BlurSize ("Blur Size", Range(0.0, 5.0)) = 1.0
    }
    
    SubShader {
        
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float _BlurSize;
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };
            
            v2f vert (appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                fixed4 color = tex2D(_MainTex, i.uv);
                
                // Gaussian Blur
                fixed2 texelSize = 1.0 / _ScreenParams.xy;
                fixed3 blur = fixed3(0, 0, 0);
                fixed3 weights = fixed3(0.227027, 0.316216, 0.070270);
                
                for (int x = -2; x <= 2; x++) {
                    for (int y = -2; y <= 2; y++) {
                        fixed2 offset = fixed2(x, y) * _BlurSize * texelSize;
                        blur += tex2D(_MainTex, i.uv + offset).rgb * weights[abs(x) + abs(y)];
                    }
                }
                
                return fixed4(blur / 2.0, color.a);
            }
            
            ENDCG
        }
    }
}