void RotateAroundYInDegrees_float(float3 vertex, float degrees, out float3 rotatedPos)
{
    float alpha = degrees * 3.14159265359 / 180.0;
    float sina, cosa;
    sincos(alpha, sina, cosa);
    float2x2 m = float2x2(cosa, -sina, sina, cosa);
    rotatedPos = float3(mul(m, vertex.xz), vertex.y).xzy;
}

void AlphaPremultiply_float(float4 input, out float4 output)
{
    input.rgb *= input.a;
    output = input;
}

void AlphaBlend_float(float4 base, float4 blend, float opacity, out float4 output)
{
    float a = blend.a * opacity;
    output.rgb = a * blend.rgb + (1 - a) * base.rgb;
    output.a = 1;
}