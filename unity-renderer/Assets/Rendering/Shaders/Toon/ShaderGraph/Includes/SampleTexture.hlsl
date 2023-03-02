#ifndef SAMPLE_TEXTURE_AVATAR
#define SAMPLE_TEXTURE_AVATAR
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

void SampleTexture_float(float4 defaultColor, float textureIndex, float2 uv, out float4 Color)
{
    if (textureIndex < 0)
    {
        Color = defaultColor;
        return;
    }

    textureIndex = abs(textureIndex);

    if (textureIndex < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap1), uv);
    else if (abs(1 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap2), uv);
    else if (abs(2 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap3), uv);
    else if (abs(3 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap4), uv);
    else if (abs(4 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap5), uv);
    else if (abs(5 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap6), uv);
    else if (abs(6 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap7), uv);
    else if (abs(7 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap8), uv);
    else if (abs(8 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap9), uv);
    else if (abs(9 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap10), uv);
    else if (abs(10 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap11), uv);
    else if (abs(11 - textureIndex) < 0.01)
        Color = tex2D(UnityBuildTexture2DStructNoScale(_AvatarMap12), uv);
    else
        Color = defaultColor;
}
#endif