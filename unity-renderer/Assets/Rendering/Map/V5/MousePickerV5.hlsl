static float2 sobelSamplePoints[9] =
{
    float2(-1, 1), float2(0, 1), float2(1, 1),
    float2(-1, 0), float2(0, 0), float2(1, 0),
    float2(-1, -1), float2(0, -1), float2(1, -1),
};


static float sobelXMatrix[9] =
{
    1, 0, -1,
    2, 0, -2,
    1, 0, -1
};


static float sobelYMatrix[9] =
{
    1, 2, 1,
    0, 0, 0,
    -1, -2, -1
};


float MapSobel(UnityTexture2D Input, float2 UV, float Thickness, float Zoom)
{
    float2 sobelR = 0;
    float2 sobelG = 0;
    float2 sobelB = 0;


    [unroll] for (int i = 0; i < 9; i++)
    {
        float4 map = tex2D(Input, UV + sobelSamplePoints[i] * (Thickness / 3500));
        float2 kernel = float2(sobelXMatrix[i], sobelYMatrix[i]);
        
        sobelR += map.r * kernel;
        sobelG += map.g * kernel;
        sobelB += map.b * kernel;
    }

    return max(length(sobelR), max(length(sobelG), length(sobelB)));
}

float2 PickerScaleFromCenter(float2 Scale, float2 UV)
{
    float2 offset = (((Scale.x * -1) / 2) + 0.5, ((Scale.y * -1) / 2) + 0.5);
    float2 finalUV = UV * Scale + offset;

    return finalUV;
}

SAMPLER(custom_point_clamp_sampler);
void MousePicker_float(UnityTexture2D Input, float2 TextureSize, float2 Resolution, float Zoom, float2 UV, float2 MousePos, float OutlineThickness, out float Out, out float Outline, out float Mask)
{
    SamplerState ss = custom_point_clamp_sampler;

    float2 uv = PickerScaleFromCenter(1/Zoom, UV);

    float2 mouseOffset = (0.5, 0.5);
    float2 mouseTile = (MousePos + mouseOffset + TextureSize / 2) / TextureSize;


    float4 mouseData = SAMPLE_TEXTURE2D(Input, ss, mouseTile);
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

    Outline = step(0.001, MapSobel(Input, uv, OutlineThickness, Zoom));
    Mask = step(0.1, (rawData.r + rawData.g + rawData.b));
}