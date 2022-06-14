// pixel-perfect UV based on an arbitrary presicion UV + texture size
float2 ClampToTexture(float2 uv, float2 size)
{
    float2 clampUv = ceil(uv * size) / size;
    float2 pixel = float2(1.0, 1.0) / size;

    return clampUv - pixel * float2(0.5, 0.5);
}

// this function returns a color for the tiles based on a texture color
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

// this function detects a change in the pixel-perfect UV mapping of the texture
// using a screen pixel as delta
float2 DetectEdges(float2 positionInScreen, float2 size, float zoom, float2 Resolution)
{
    float2 delta = float2(1.0, -1.0) / zoom / Resolution; // top-left delta

    // fix aspect ratio
    if (Resolution.x > Resolution.y) 
    {
        delta.x *= Resolution.x / Resolution.y;
    }
    else
    {
        delta.y *= Resolution.y / Resolution.x;
    }

    float2 uv = ClampToTexture(positionInScreen - delta, size);
    float2 newUv = ClampToTexture(positionInScreen, size);

    return (newUv - uv);
}


// given a pixel in screen, texture size and information of the tile, render the tile or the lines
float DetectEdgesFloat(float2 positionInScreen, float2 size, float zoom, float info, float2 Resolution)
{
    float2 t = DetectEdges(positionInScreen, size, zoom, Resolution);

    bool hasTopBorder = false;
    bool hasLeftBorder = false;
    bool hasTopLeftPoint = false;

    bool inTopBorder = t.y != 0.0;
    bool inLeftBorder = t.x != 0.0;

    // read bit flags
    if (info >= 32.0) { hasTopLeftPoint = true; info -= 32.0; }
    if (info >= 16.0) { hasLeftBorder = true; info -= 16.0; }
    if (info >= 8.0) { hasTopBorder = true; info -= 8.0; }

    if (!hasTopBorder && !hasLeftBorder)
    {
        // disconnected everywhere: it's a square
        if (inLeftBorder || inTopBorder)
        {
            return 0.0;
        }
        return 1.0;
    }
    else
    {
        if (hasTopBorder && hasLeftBorder && hasTopLeftPoint)
        {
            // connected everywhere: it's a square with lines
            return 1.0;
        }
        else
        {
            // connected left: it's a rectangle
            if (hasTopBorder && !inLeftBorder) return 1.0;
            // connected top: it's a rectangle
            if (hasLeftBorder && !inTopBorder) return 1.0;
        }
    }

    return 0.0;
}

SAMPLER(point_clamp_sampler);

void Main_float(UnityTexture2D Input, float2 Resolution, float TileSizeInPixels, float2 SizeOfTheTexture, float GridThickness, float2 UV, out float4 outColor, out float Grid)
{
    Grid = 0;

    float2 u_mouse = float2(0.5 * Resolution.x, 0.5 * Resolution.y);

    if (GridThickness == 0)
    {
        GridThickness = 0.001;
    }

    // offset = center of the map in uniform coords
    float2 offset = u_mouse / Resolution;
    float zoom;

    float4 tempCol = float4(0, 0, 0, 0);

    if (Resolution.x > Resolution.y)
    {
        UV -= 0.5;
        zoom = SizeOfTheTexture.x / Resolution.y * TileSizeInPixels;
    }
    else
    {
        UV -= 0.5;
        zoom = SizeOfTheTexture.y / Resolution.x * TileSizeInPixels;
    }

    // tileOfInterest, represented as UV coords of the MAP texture
    float2 tileOfInterest = UV / zoom + offset;

    // render black tiles outside of the image map data-range
    if (tileOfInterest.x > 1.0 || tileOfInterest.y > 1.0 || tileOfInterest.y < 0.0 || tileOfInterest.x < 0.0)
    {
        tempCol = float4(0.0, 0.0, 0.0, 1.0);
        outColor = tempCol;
    }
    else
    {
        // uvs of the texture for the center of coordinates
        // and the pixel of the screen (tileOfInterest) 
        float2 uv = ClampToTexture(tileOfInterest, SizeOfTheTexture);
        SamplerState ss = point_clamp_sampler;


        float4 data = SAMPLE_TEXTURE2D(Input, ss, uv);

        if (data.a > 0.0)
        {
            if (DetectEdgesFloat(tileOfInterest, SizeOfTheTexture, zoom / GridThickness, data.r * 256.0, Resolution) == 0.0)
            {
                Grid = 1;
            }
            else
            {
                Grid = 0;
            }

            tempCol = ColorFromType(data.g * 256.0);
        }
        else
        {
            if (DetectEdgesFloat(tileOfInterest, SizeOfTheTexture, zoom, data.r * 256.0, Resolution) == 0.0)
            {
                Grid = 1;
            }
            else
            {
                Grid = 0;
            }

            tempCol = float4(0, 0, 0, 1);
        }

        outColor = tempCol;
    }
}