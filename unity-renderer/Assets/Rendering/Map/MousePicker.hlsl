float2 OverlayClampToTexture(float2 uv, float2 size)
{
    float2 clampUv = ceil(uv * size) / size;
    float2 pixel = float2(1.0, 1.0) / size;

    return clampUv - pixel * float2(0.5, 0.5);
}

SAMPLER(custom_point_clamp_sampler);
void MousePicker_float(UnityTexture2D Input, float2 TextureSize, float2 Resolution, float Zoom, float2 UV, float2 MousePos, out float Out)
{
    SamplerState ss = custom_point_clamp_sampler;
    float2 zoom;
    float2 offset = float2(0.5, 0.5);

    if (Resolution.x > Resolution.y)
    {
        UV -= 0.5;
        zoom = TextureSize.x / Resolution.y * Zoom;
    }
    else
    {
        UV -= 0.5;
        zoom = TextureSize.y / Resolution.x * Zoom;
    }

    float2 mouseTile = ((MousePos / zoom + offset) + TextureSize/2)/TextureSize;
    float2 mouseUV = OverlayClampToTexture(mouseTile, TextureSize);


    float2 tileOfInterest = UV / zoom + offset;
    float2 uv = OverlayClampToTexture(tileOfInterest, TextureSize);

    float4 mouseData = SAMPLE_TEXTURE2D(Input, ss, mouseUV);
    float4 rawData = SAMPLE_TEXTURE2D(Input, ss, uv);

    if (mouseData.b + mouseData.g + mouseData.r != 0)
    {
        if (mouseData.b == rawData.b && mouseData.g == rawData.g && mouseData.r == rawData.r)
        {
            Out = 1;
        }
        else
        {
            Out = 0;
        }
    }
    else
    {
        Out = 0;
    }

}