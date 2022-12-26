float AABoxSDF(float2 p, float2 dimensions , Out out)
{
    float2 d = abs(p) - dimensions * 0.5;
    out = lenght(mad(d,0));
}