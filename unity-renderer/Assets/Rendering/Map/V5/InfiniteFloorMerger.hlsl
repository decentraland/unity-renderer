void Merger_float(
    float4 MapColor, float MapOutline, float RandomTilingMixed, float RandomTilingOwned, float GrassTexture, float RoadTexture, float GrassGrid, 
    float4 ColorGrid, float4 ColorPlaza, float4 ColorDistricts, float4 ColorStreets, float4 ColorParcels, float4 ColorOwned, float4 ColorEmpty, float4 ColorGrassGrid, out float4 Out)
{
    Out = (0, 0, 0, 0);
    Out = lerp(ColorDistricts, ColorOwned, RandomTilingOwned);
    Out = Out * RandomTilingMixed;

    Out = lerp(ColorPlaza, Out, MapColor.r);

    Out = Out * GrassTexture;
    Out = lerp(Out, ColorGrassGrid, GrassGrid);

    Out = lerp(Out, ColorStreets * RoadTexture, MapColor.g);

    float4 temp = ((lerp(ColorParcels, ColorOwned, RandomTilingOwned) * RandomTilingMixed) * GrassTexture);
    temp = lerp(temp, ColorGrassGrid, GrassGrid);

    Out = lerp(Out, temp, MapColor.b);

    Out = lerp(Out, ColorEmpty, MapColor.a);

    Out = lerp(Out, ColorGrid, ColorGrid.a * MapOutline);
}