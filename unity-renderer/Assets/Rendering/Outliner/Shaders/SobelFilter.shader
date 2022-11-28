Shader "Hidden/DCL/SobelFilter"
{

    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {} 
        _OutlineColor("Outline color", Color) = (.25, .5, .5, 1)
        _DeltaX ("Delta X", Float) = 0.01
        _DeltaY ("Delta Y", Float) = 0.01
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
        }
        LOD 200

        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _Source;
        float4 _OutlineColor;
        float _DeltaX;
        float _DeltaY;

        float sobel(sampler2D tex, float2 uv)
        {
            float2 delta = float2(_DeltaX, _DeltaY);

            float4 hr = float4(0, 0, 0, 0);
            float4 vt = float4(0, 0, 0, 0);

            hr += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) * 1.0;
            hr += tex2D(tex, (uv + float2(0.0, -1.0) * delta)) * 0.0;
            hr += tex2D(tex, (uv + float2(1.0, -1.0) * delta)) * -1.0;
            hr += tex2D(tex, (uv + float2(-1.0, 0.0) * delta)) * 2.0;
            hr += tex2D(tex, (uv + float2(0.0, 0.0) * delta)) * 0.0;
            hr += tex2D(tex, (uv + float2(1.0, 0.0) * delta)) * -2.0;
            hr += tex2D(tex, (uv + float2(-1.0, 1.0) * delta)) * 1.0;
            hr += tex2D(tex, (uv + float2(0.0, 1.0) * delta)) * 0.0;
            hr += tex2D(tex, (uv + float2(1.0, 1.0) * delta)) * -1.0;

            vt += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) * 1.0;
            vt += tex2D(tex, (uv + float2(0.0, -1.0) * delta)) * 2.0;
            vt += tex2D(tex, (uv + float2(1.0, -1.0) * delta)) * 1.0;
            vt += tex2D(tex, (uv + float2(-1.0, 0.0) * delta)) * 0.0;
            vt += tex2D(tex, (uv + float2(0.0, 0.0) * delta)) * 0.0;
            vt += tex2D(tex, (uv + float2(1.0, 0.0) * delta)) * 0.0;
            vt += tex2D(tex, (uv + float2(-1.0, 1.0) * delta)) * -1.0;
            vt += tex2D(tex, (uv + float2(0.0, 1.0) * delta)) * -2.0;
            vt += tex2D(tex, (uv + float2(1.0, 1.0) * delta)) * -1.0;

            return sqrt(hr * hr + vt * vt);
        }

        float4 frag(v2f_img IN) : COLOR
        {
            float4 color = tex2D(_Source, IN.uv);
            float s = saturate(sobel(_MainTex, IN.uv));
            return lerp(color, _OutlineColor, s);
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }

    }
    FallBack "Diffuse"
}