Shader "DCL/OutlinerEffect"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _OutlineColor("Outline color", Color) = (.25, .5, .5, 1)
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
        #include "./GaussianBlur.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;

        sampler2D _ComposeMask;
        sampler2D _Source;

        float4 _InnerColor;
        float4 _OutlineColor;
        float _OutlineSize;
        float4 _BlurColor;
        float _BlurSize;
        float _BlurSigma;
        float _Fade;

        float Sobel(sampler2D t, float2 uv, float3 offset)
        {
            float pixelCenter = tex2D(t, uv).r;
            float pixelLeft = tex2D(t, uv - offset.xz).r;
            float pixelRight = tex2D(t, uv + offset.xz).r;
            float pixelUp = tex2D(t, uv + offset.zy).r;
            float pixelDown = tex2D(t, uv - offset.zy).r;

            return abs(pixelLeft - pixelCenter) +
                abs(pixelRight - pixelCenter) +
                abs(pixelUp - pixelCenter) +
                abs(pixelDown - pixelCenter);
        }

        float4 frag_outline(v2f_img IN) : COLOR
        {

            float outline = Sobel(_MainTex, IN.uv, float3(0.001f, 0.001f, 0)*_OutlineSize);
            float outlineForBlur = Sobel(_MainTex, IN.uv, float3(0.001f, 0.001f, 0)*_BlurSize);
            
            /*
            * R: Outline value
            * G: Outline value to be blurred
            */
            return float4(outline, outlineForBlur, outline, outline);
        }

        float4 frag_horizontal(v2f_img i) : COLOR
        {
            pixel_info pinfo;
            pinfo.tex = _MainTex;
            pinfo.uv = i.uv;
            pinfo.texelSize = _MainTex_TexelSize;
            float pixel = tex2D(pinfo.tex, i.uv).r;
            float blur = GaussianBlur(pinfo, _BlurSigma, float2(1, 0)).g;
            return float4(pixel, blur, pixel, pixel);
        }

        float4 frag_vertical(v2f_img i) : COLOR
        {
            pixel_info pinfo;
            pinfo.tex = _MainTex;
            pinfo.uv = i.uv;
            pinfo.texelSize = _MainTex_TexelSize;
            float pixel = tex2D(pinfo.tex, i.uv).r;
            float blur = GaussianBlur(pinfo, _BlurSigma, float2(0, 1)).g;
            return float4(pixel, blur, pixel, pixel);
        }

        float4 frag_compose(v2f_img i) : COLOR
        {
            float4 composeMask = tex2D(_ComposeMask, i.uv); 
            float4 camera = tex2D(_Source, i.uv);
            float4 outline = tex2D(_MainTex, i.uv);

            const float outlineValue = outline.r;

            // This is hard outline
            if(outline.r > 0.99f)
                return lerp(camera, _OutlineColor, outlineValue*_Fade);

            // We are tinting, not outlining
            if(composeMask.r > 0.5)
                return lerp(camera, _InnerColor, _InnerColor.a*_Fade);

            //We are blurring
            const float blurValue = outline.g;
            return lerp(camera, _BlurColor, blurValue*_Fade);
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_outline
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_horizontal
            ENDCG
        }


        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_vertical
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_compose
            ENDCG
        }

    }
    FallBack "Diffuse"
}