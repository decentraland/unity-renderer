void SampleTexture_float(float3 defaultColor, float textureIndex, float2 uv, out float3 Color)
{
    float3 result;

    textureIndex = abs(textureIndex);

    if (textureIndex < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap1), uv);
    else if (abs(1 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap2), uv);
    else if (abs(2 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap3), uv);
    else if (abs(3 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap4), uv);
    else if (abs(4 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap5), uv);
    else if (abs(5 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap6), uv);
    else if (abs(6 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap7), uv);
    else if (abs(7 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap8), uv);
    else if (abs(8 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap9), uv);
    else if (abs(9 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap10), uv);
    else if (abs(10 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap11), uv);
    else if (abs(11 - textureIndex) < 0.01)
        result = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap12), uv);
    else
        result = defaultColor;

    Color = result.rgb;
}

void GetLightingInformation_float(out float3 Direction, out float3 Color, out float Attenuation)
{
    #ifdef SHADERGRAPH_PREVIEW
        Direction = float3(-0.5,0.5,-0.5);
        Color = float3(1,1,1);
        Attenuation = 0.4;
    #else
    Light light = GetMainLight();
    Direction = light.direction;
    Attenuation = light.distanceAttenuation;
    Color = light.color;
    #endif
}

void GetShadowInformation_float(float3 WorldPos, out float3 ShadowAtten)
{
    #ifdef SHADERGRAPH_PREVIEW
        ShadowAtten = 1;
    #else
    #if SHADOWS_SCREEN
            float4 clipPos = TransformWorldToHClip(WorldPos);
            float4 shadowCoord = ComputeScreenPos(clipPos);
    #else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    #endif

    #if SHADOWS_SCREEN
            ShadowAtten = SampleScreenSpaceShadowmap(shadowCoord);
    #else
    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
    float shadowStrength = GetMainLightShadowStrength();
    ShadowAtten = SampleShadowmap(shadowCoord, TEXTURE2D_ARGS(_MainLightShadowmapTexture,
                                                              sampler_MainLightShadowmapTexture),
                                  shadowSamplingData, shadowStrength, false);
    #endif

    #if defined(_RECEIVE_SHADOWS_OFF)
            ShadowAtten = 1;
    #endif

    #endif
}
