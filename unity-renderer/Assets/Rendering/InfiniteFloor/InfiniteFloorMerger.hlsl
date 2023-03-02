void Merger_float(
    float4 MapColor, float MapOutline, float MapOutlineInner, float RandomTilingMixed, float RandomTilingOwned, float GrassTexture, float RoadTexture, float GrassGrid, float FogMask,
    float4 ColorGrid, float4 ColorPlaza, float4 ColorDistricts, float4 ColorStreets, float4 ColorParcels, float4 ColorOwned, float4 ColorEmpty, float4 ColorGrassGrid, float4 FogColor,
    out float4 Out)
{
    Out = float4(0, 0, 0, 0);
    Out = lerp(ColorDistricts, ColorOwned, RandomTilingOwned);
    Out = Out * RandomTilingMixed;

    Out = lerp(ColorPlaza, Out, MapColor.r);
    Out = lerp(Out, ColorEmpty, MapColor.a);

    Out = Out * GrassTexture;
    Out = Out + Out * ColorGrassGrid * GrassGrid * (1 - MapOutline);

    Out = lerp(Out, ColorStreets * RoadTexture, MapColor.g);

    float4 temp = ((lerp(ColorParcels, ColorOwned, RandomTilingOwned) * RandomTilingMixed) * GrassTexture);
    temp = temp + temp * ColorGrassGrid * GrassGrid * (1 - MapOutline);

    Out = lerp(Out, temp, MapColor.b);

    Out = Out + Out * ColorGrid.a * MapOutline; 

    Out = lerp(Out, ColorGrid, MapOutlineInner);

    Out = lerp(Out, FogColor, FogMask);
}