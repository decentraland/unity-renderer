Shader "DCL/UI-SimpleGaussianBlur-Alpha" {
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
        _Alpha ("Alpha", Range(0.0, 1.0)) = 0.3
    }
    
    HLSLINCLUDE
        #include "UnityCG.cginc"
        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 pos : SV_POSITION;
        };
            
        v2f FullscreenVert (appdata_base v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            return o;
        }

        sampler2D _MainTex;

        fixed4 FragBlurH (v2f i) : SV_Target
        {
            fixed3 offset = fixed3(0.0, 1.3846153846 / _ScreenParams.x, 3.2307692308 / _ScreenParams.x);
            fixed3 weight = fixed3(0.2270270270, 0.3162162162, 0.0702702703);
            fixed4 FragmentColor = tex2D(_MainTex, i.uv) * weight[0];

            for (int x=1; x<3; x++)
            {
                FragmentColor += tex2D(_MainTex, (i.uv + float2(offset[x], 0.0))) * weight[x];
                FragmentColor += tex2D(_MainTex, (i.uv - float2(offset[x], 0.0))) * weight[x];
            }

            return fixed4(FragmentColor.rgb, FragmentColor.a * 0.5f);
        }

        fixed4 FragBlurV (v2f i) : SV_Target
        {
            fixed3 offset = fixed3(0.0, 1.3846153846 / _ScreenParams.y, 3.2307692308 / _ScreenParams.y);
            fixed3 weight = fixed3(0.2270270270, 0.3162162162, 0.0702702703);
            fixed4 FragmentColor = tex2D(_MainTex, i.uv) * weight[0];

            for (int y=1; y<3; y++)
            {
                FragmentColor += tex2D(_MainTex, (i.uv + float2(0.0, offset[y]))) * weight[y];
                FragmentColor += tex2D(_MainTex, (i.uv - float2(0.0, offset[y]))) * weight[y];
            }

            return fixed4(FragmentColor.rgb, FragmentColor.a * 0.5f);
        }
    ENDHLSL

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

        Pass
        {
            Name "Gaussian Blur Horizontal"
            HLSLPROGRAM
                #pragma vertex FullscreenVert
                #pragma fragment FragBlurH  
            ENDHLSL
        }

        Pass
        {
            Name "Gaussian Blur Vertical"
            HLSLPROGRAM
                #pragma vertex FullscreenVert
                #pragma fragment FragBlurV
            ENDHLSL
        }
    }
}