void SampleGradient_float(float4 Color01, float4 Color02, float4 Color03, float4 Color04, float4 Color05, float4 Color06, float4 Color07, float4 Color08,
                          float4 Positions01, float4 Positions02, float ColorAmount, float2 UV, out float4 Out)
{
    float4 colors[8] = { Color01, Color02, Color03, Color04, Color05, Color06, Color07, Color08 };
    float positions[8] = { Positions01.x, Positions01.y, Positions01.z, Positions01.w, Positions02.x, Positions02.y, Positions02.z, Positions02.w };

    float3 color = colors[0].rgb;


    [unroll]
    for (int c = 1; c < ColorAmount; c++)
    {
        float colorPos = saturate((UV.r - positions[c - 1]) / (positions[c] - positions[c - 1])) * step(c, 8 - 1);
        color = lerp(color, colors[c].rgb, colorPos);
    }

    Out = float4(color, 0);
}