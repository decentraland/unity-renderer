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

        float2 Sobel(sampler2D t, float2 uv, float3 offset)
        {
            float4 pixelCenter = tex2D(t, uv);
            float4 pixelLeft = tex2D(t, uv - offset.xz);
            float4 pixelRight = tex2D(t, uv + offset.xz);
            float4 pixelUp = tex2D(t, uv + offset.zy);
            float4 pixelDown = tex2D(t, uv - offset.zy);

            float result = abs(pixelLeft.r - pixelCenter.r) +
                abs(pixelRight.r - pixelCenter.r) +
                abs(pixelUp.r - pixelCenter.r) +
                abs(pixelDown.r - pixelCenter.r);
            
            float averageAlpha = max(pixelCenter.g, max(pixelLeft.g, pixelRight.g)); 

            return float2(result, saturate(result)*averageAlpha);
        }

        float4 frag_outline(v2f_img IN) : COLOR
        {
            float2 outline = Sobel(_MainTex, IN.uv, float3(0.001f, 0.001f, 0)*_OutlineSize);
            float2 outlineForBlur = Sobel(_MainTex, IN.uv, float3(0.001f, 0.001f, 0)*_BlurSize);
            
            /*
            * R: Outline value (full opaque)
            * G: Outline value to be blurred with transparency
            * B: Outline value transparency
            */
            return float4(outline.x, outlineForBlur.y, outline.y, 1);
        }

        float4 frag_horizontal(v2f_img i) : COLOR
        {
            pixel_info pinfo;
            pinfo.tex = _MainTex;
            pinfo.uv = i.uv;
            pinfo.texelSize = _MainTex_TexelSize;
            float4 pixel = tex2D(pinfo.tex, i.uv);
            float blur = GaussianBlur(pinfo, _BlurSigma, float2(1, 0)).g;
            return float4(pixel.r, blur, pixel.b, 1);
        }

        float4 frag_vertical(v2f_img i) : COLOR
        {
            pixel_info pinfo;
            pinfo.tex = _MainTex;
            pinfo.uv = i.uv;
            pinfo.texelSize = _MainTex_TexelSize;
            float4 pixel = tex2D(pinfo.tex, i.uv);
            float blur = GaussianBlur(pinfo, _BlurSigma, float2(0, 1)).g;
            return float4(pixel.r, blur, pixel.b, 1);
        }

        float4 frag_compose(v2f_img i) : COLOR
        {
            float4 composeMask = tex2D(_ComposeMask, i.uv); 
            float4 camera = tex2D(_Source, i.uv);
            float4 outline = tex2D(_MainTex, i.uv);

            const float outlineValue = outline.r;
            const float outlineWithAlpha = outline.b;

            // This is hard outline
            if(outlineValue > 0.5f)
            {
                return lerp(camera, _OutlineColor, outlineWithAlpha*_Fade);
            }

            // We are tinting, not outlining
            if(composeMask.r > 0.5)
            {
                return lerp(camera, _InnerColor, _InnerColor.a*_Fade*composeMask.g);
            }

            //We are blurring
            const float blurValue = outline.g;
            return lerp(camera, _BlurColor, _Fade*blurValue);
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