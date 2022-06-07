// pixel-perfect UV based on an arbitrary presicion UV + texture size
float2 clampToTexture(float2 uv, float2 size)
{
    float2 clampUv = ceil(uv * size) / size;
    float2 pixel = float2(1.0, 1.0) / size;
    return clampUv - pixel * float2(0.5, 0.5);
}

// this function detects a change in the pixel-perfect UV mapping of the texture
// using a screen pixel as delta
float2 detectEdges(float2 positionInScreen, float2 size, float zoom, float2 u_resolution)
{
    float2 delta = float2(1.0, -1.0) / zoom / u_resolution; // top-left delta

    // fix aspect ratio
    if (u_resolution.x > u_resolution.y) 
    {
        delta.x *= u_resolution.x / u_resolution.y;
    }
    else
    {
        delta.y *= u_resolution.y / u_resolution.x;
    }

    float2 uv = clampToTexture(positionInScreen - delta, size);
    float2 newUv = clampToTexture(positionInScreen, size);

    return (newUv - uv);
}

// this function returns a color for the tiles based on a texture color
float4 colorFromType(float type)
{
    if (type < 33.0)
        return float4(1, 0, 0, 1);

    if (type < 65.0)
        return float4(0, 1, 0, 1);

    return float4(0, 0, 1, 1);
}

// given a pixel in screen, texture size and information of the tile, render the tile or the lines
float detectEdgesFloat(float2 positionInScreen, float2 size, float zoom, float info, float2 u_resolution)
{
    float2 t = detectEdges(positionInScreen, size, zoom, u_resolution);

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
            return 0.0;
        return 1.0;
    }
    else
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

    return 0.0;
}

void Main_float(UnityTexture2D u_input, float2 u_resolution, float2 u_mouse, float tileSizeInPizels, float2 sizeOfTheTexture, float4 position, UnitySamplerState ss, float4 GridColor, out float4 outColor)
{
    // offset = center of the map in uniform coords
    float2 offset = u_mouse / u_resolution;
    float zoom;
    float2 v_texcoord; // v_texcoord is a ratio-normalized sceen pixel in uniform coords

    float4 tempCol = (0,0,0,0);

    if (u_resolution.x > u_resolution.y)
    {
        v_texcoord.x = position.x / u_resolution.y - 0.5 - 0.5 * (u_resolution.x - u_resolution.y) / u_resolution.y;
        v_texcoord.y = position.y / u_resolution.y - 0.5;
        zoom = sizeOfTheTexture.x / u_resolution.y * tileSizeInPizels;
    }
    else
    {
        v_texcoord.x = position.x / u_resolution.x - 0.5;
        v_texcoord.y = position.y / u_resolution.x - 0.5 - 0.5 * (u_resolution.y - u_resolution.x) / u_resolution.x;
        zoom = sizeOfTheTexture.y / u_resolution.x * tileSizeInPizels;
    }


    // tileOfInterest, represented as UV coords of the MAP texture
    float2 tileOfInterest = v_texcoord / zoom + offset;

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
        float2 centerUv = clampToTexture(offset, sizeOfTheTexture);
        float2 uv = clampToTexture(tileOfInterest, sizeOfTheTexture);

        // if we are rendering the center of coordinates
        if (length(centerUv - uv) == 0.0)
        {
            tempCol = float4(1.0, 0.0, 1.0, 1.0);
            outColor = tempCol;
        }
        else
        {
            float4 data = SAMPLE_TEXTURE2D(u_input, ss, uv);

            if (data.a > 0.0)
            {
                if (detectEdgesFloat(tileOfInterest, sizeOfTheTexture, zoom, data.r * 256.0, u_resolution) == 0.0)
                    tempCol = float4(0.0, 0.0, 0.0, 1.0);
                else
                    tempCol = colorFromType(data.g * 256.0);
            }
            else
            {
                if (detectEdgesFloat(tileOfInterest, sizeOfTheTexture, zoom, 0.0, u_resolution) == 0.0)
                    tempCol = float4(0.0, 0.0, 0.0, 1.0);
                else
                    tempCol = float4(0.05, 0.05, 0.05, 1.0);
            }

            outColor = tempCol;
        }
    }


}