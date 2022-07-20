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


float MapSobel(UnityTexture2D Input, float2 UV, float Thickness, float Zoom) //COLOR EDGE DETECTION ALGORITHIM
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

void CreateRectangle(float2 UV, float Width, float Height, out float Out) //CREATES SINGLE SQUARE
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
    Out = saturate(min(d.x, d.y));
}

float2 MapScaleFromCenter(float2 Scale, float2 UV) //SCALE UV FROM THE CENTER
{
    float2 offset = (((Scale.x * -1) / 2) + 0.5, ((Scale.y * -1) / 2) + 0.5);
    float2 finalUV = UV * Scale + offset;

    return finalUV;
}

float SinglePointer(float2 UV, float Zoom, float2 TextureSize, float2 MousePos) //HIGHLIGHT FOR SINGLE PARCELS
{
    float2 tempUV = UV;
    float2 offsetUV = -(MousePos + (TextureSize / 2)) / TextureSize;

    tempUV = (tempUV + offsetUV) * TextureSize;

    float rect;
    CreateRectangle(tempUV, 1, 1, rect);

    return rect;
}

float RegularGrid(float2 UV, float Zoom, float2 TextureSize, float Thickness) //CREATE GRID FOR INDIVIDUAL PARCELS
{
    float2 tempUV = UV * TextureSize;
    tempUV = frac(tempUV);

    float thickness = 1 / (Thickness * 1.75);
    float rect;
    CreateRectangle(tempUV, thickness, thickness, rect);
    rect = 1 - rect;

    return rect;
}

float4 ColorFromType(float type) //SPLIT PARCEL INTO DIFFERENT CHANNELS ACCORDING TO IT'S TYPE
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

void MousePicker(UnityTexture2D Input, float2 TextureSize, float Zoom, float2 UV, float2 MousePos, float OutlineThickness, out float Highlight, out float Outline, out float Mask)
{
    SamplerState ss = point_clamp_sampler;

    float2 uv = MapScaleFromCenter(1 / Zoom, UV);

    float2 mouseOffset = (0.5, 0.5);
    float2 mouseTile = (MousePos + mouseOffset + TextureSize / 2) / TextureSize;


    float4 mouseData = SAMPLE_TEXTURE2D(Input, ss, mouseTile);
    float4 rawData = SAMPLE_TEXTURE2D(Input, ss, uv);


    if (mouseData.b + mouseData.g + mouseData.r != 0)
    {
        if (mouseData.b == rawData.b && mouseData.g == rawData.g && mouseData.r == rawData.r)
        {
            Highlight = 1;
        }
        else
        {
            Highlight = 0;
        }
    }
    else
    {
        Highlight = SinglePointer(uv, Zoom, TextureSize, MousePos);
    }


    Outline = step(0.001, MapSobel(Input, uv, OutlineThickness, Zoom));

    if ((rawData.r + rawData.g + rawData.b) != 0)
    {
        Mask = 1;
    }
    else
    {
        Mask = 0;
    }
}


void Main_float(UnityTexture2D MainMap, UnityTexture2D IDMap, float Zoom, float2 TextureSize, float GridThickness, float2 MousePos, float2 UV, out float4 outColor, out float Outline, out float Highlight) //MAIN FUNCTION
{
    float4 tempCol = float4(0, 0, 0, 0);

    SamplerState ss = point_clamp_sampler;

    float2 uv = MapScaleFromCenter(1/Zoom, UV);

    float4 data = SAMPLE_TEXTURE2D(MainMap, ss, uv);

    if (data.a > 0.0)
    {
        tempCol = ColorFromType(data.g * 256.0);
    }
    else
    {
        tempCol = float4(0, 0, 0, 1);
    }

    outColor = tempCol;

    float gridMask;
    MousePicker(IDMap, TextureSize, Zoom, UV, MousePos, GridThickness, Highlight, Outline, gridMask);

    float grid = RegularGrid(uv, Zoom, TextureSize, GridThickness);
    grid *= 1 - tempCol.g;
    
    Outline = lerp(grid, Outline, gridMask);
}