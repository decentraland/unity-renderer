SAMPLER(custom_point_clamp_sampler);
void MousePicker_float(UnityTexture2D Input, float2 Resolution, float2 UV, float2 MousePos, out float Out)
{
    SamplerState ss = custom_point_clamp_sampler;

    float2 mouseUV = MousePos / Resolution;

    float4 mouseData = SAMPLE_TEXTURE2D(Input, ss, mouseUV);
    float4 rawData = SAMPLE_TEXTURE2D(Input, ss, UV);

    if (mouseData.b == rawData.b)
    {
        Out = 1;
    }
    else
    {
        Out = 0;
    }
}