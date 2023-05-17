#ifndef DCL_LW_TOON_INCLUDED
#define DCL_LW_TOON_INCLUDED

//from unity compiled shadergrah
float FresnelEffect(float3 worldNormal, float3 viewDir, float Power)
{
    return pow(1.0 - saturate(dot(normalize(worldNormal), normalize(viewDir))), Power);
}

//from unity compiled shadergrah
float Remap(float In, float2 InMinMax, float2 OutMinMax)
{
    return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

float GetTopDownLight(float3 worldNormal, float3 viewDir, float3 lightDir)
{
    return dot(lightDir, normalize(normalize(worldNormal) * -1 + normalize(viewDir)));
}

//from unity compiled shadergrah
float4 Blend(float4 Base, float4 Blend, float Opacity)
{
    return lerp(Base, Base * Blend, Opacity);
}

float4 GetToonBlend(float3 worldNormal, float3 viewDir, sampler2D textureSampler, float fresnelIntensity,
                    float glossIntensity, float3 lightDir, float4 lightColor)
{
    const float fresnelAngle = -0.5;
    const float fresnelSize = 1.77;
    const float glossSize = 5;

    // Top light
    const float fresnel = FresnelEffect(worldNormal, viewDir, fresnelSize);
    const float remap = Remap(GetTopDownLight(worldNormal, viewDir, lightDir), float2(-1, 1), float2(1, fresnelAngle));
    const float secondRemap = Remap(fresnel * remap, float2(-1, 1), float2(0.01, 0.99));
    const fixed4 sampleTop = tex2D(textureSampler, float2(secondRemap, 0.99)) * fresnelIntensity;

    // Gloss
    const float glossValue = pow(dot(normalize(worldNormal), normalize(lightDir + normalize(viewDir))), glossSize);
    const float glossUV = float2(clamp(glossValue, 0.38, 0.99), 0.5);
    const fixed4 sampleGloss = tex2D(textureSampler, glossUV) * glossIntensity;

    // Shade
    const float2 shadeUV = float2(Remap(dot(worldNormal, lightDir), float2(-1, 1), float2(0.01, 0.99)), 0.1);
    const fixed4 shadeSample = tex2D(textureSampler, shadeUV);
    const fixed4 shadeColor = shadeSample * float4(0.7, 0.7, 0.7, 1);

    return (sampleGloss + sampleTop + shadeColor) * lightColor;
}

//from unity compiled shadergrah
float3 ColorspaceConversion_RGB_Linear(float3 In)
{
    float3 linearRGBLo = In / 12.92;
    float3 linearRGBHi = pow(max(abs((In + 0.055) / 1.055), 1.192092896e-07), float3(2.4, 2.4, 2.4));
    return float3(In <= 0.04045) ? linearRGBLo : linearRGBHi;
}

//from unity compiled shadergrah
float UnityDither(float In, float4 ScreenPosition)
{
    float2 uv = ScreenPosition.xy * _ScreenParams.xy;
    float DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    return In - DITHER_THRESHOLDS[index];
}

// Made from Toon Shader Dither shadergraph
float3 DitherAlpha(float ditherFade, float alphaClip, float3 revealPosition, float3 revealNormal, float4 screenPosition,
                   float3 spacePosition)
{
    return max(
        UnityDither(
            Remap(
                lerp(ditherFade, 0,
                     step(0, dot(spacePosition - (1 - revealPosition), revealNormal * -1))),
                float2(1, 0),
                float2(0, 2)),
            screenPosition),
        alphaClip);
}

#endif