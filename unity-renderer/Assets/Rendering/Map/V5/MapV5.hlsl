float2 MapScaleFromCenter(float2 Scale, float2 UV)
{
    float2 offset = (((Scale.x * -1) / 2) + 0.5, ((Scale.y * -1) / 2) + 0.5);
    float2 finalUV = UV * Scale + offset;

    return finalUV;
}

float4 ColorFromType(float type)
{
    if (type == 0)
    {
        return float4(0, 0, 0, 0);
    }
    if (type < 33.0)
    {
        return float4(1, 0, 0, 0);
    }
    if (type < 65.0)
    {
        return float4(0, 1, 0, 0);
    }
    if (type < 129.0)
    {
        return float4(0, 0, 1, 0);
    }

    return float4(0, 0, 0, 0);
}

SAMPLER(point_clamp_sampler);
void Main_float(UnityTexture2D Input, float2 Resolution, float Scale, float2 TextureSize, float GridThickness, float2 UV, out float4 outColor)
{
    float4 tempCol = float4(0, 0, 0, 0);

    SamplerState ss = point_clamp_sampler;

    float2 uv = MapScaleFromCenter(1/Scale, UV);

    float4 data = SAMPLE_TEXTURE2D(Input, ss, uv);

    if (data.a > 0.0)
    {
        tempCol = ColorFromType(data.g * 256.0);
    }
    else
    {
        tempCol = float4(0, 0, 0, 1);
    }

    outColor = tempCol;
}