void SampleGradient(float4 Color01, float4 Color02, float4 Color03, float4 Color04, float Time, out float3 Out)
{
    float colorAmount = 4;
    float4 allColors[4] = {Color01, Color02, Color03, Color04};
    Out = allColors[0].rgb;

    [unroll]
    for (int c = 1; c < colorAmount; c++)
    {
        float colorPos = saturate((Time - allColors[c - 1].w) / (allColors[c].w - allColors[c - 1].w) * step(c, colorAmount - 1));

        Out = lerp(Out, allColors[c].rgb, colorPos);
    }
}

void PolarCoordinates(float2 UV, out float Out)
{
    float legthScale = 1;
    float2 center = (0.5, 0.5);
    float2 delta = UV - center;

    Out = atan2(delta.x, delta.y) * 1.0 / 6.28 * legthScale;
}

void RotateDegrees(float2 UV, float Rotation, out float2 Out)
{
    float2 center = (0.5, 0.5);

    Rotation = Rotation * (3.1415926f / 180.0f);
    UV -= center;

    float s = sin(Rotation);
    float c = cos(Rotation);

    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    UV.xy = mul(UV.xy, rMatrix);
    UV += center;

    Out = UV;
}

void RadialGradient_float(float4 Color01, float4 Color02, float4 Color03, float4 Color04, float4 ColorPositions01, float Rotation, float2 UV, out float3 Out) //MAIN FUNCTION
{
    float2 tempUV;
    RotateDegrees(UV, Rotation, tempUV);

    float gradientMap;
    PolarCoordinates(tempUV, gradientMap);

    gradientMap = gradientMap + 0.5;

    SampleGradient(Color01, Color02, Color03, Color04, gradientMap, Out);
}