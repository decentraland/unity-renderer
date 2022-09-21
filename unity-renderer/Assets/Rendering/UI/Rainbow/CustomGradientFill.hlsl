void Fill_float(float Fill, float FillDirection, float2 UV, out float Out)
{
    Out = 1;
    float temp01 = saturate((1 - UV.x) + (Fill - 0.5));
    float temp02 = saturate(UV.x + (Fill - 0.5));
    float temp03 = saturate(UV.y + (Fill - 0.5));
    float temp04 = saturate((1 - UV.y) + (Fill - 0.5));

    if (FillDirection == 0)
    {
        Out = temp01;
    }
    else if (FillDirection == 1)
    {
        Out = temp02;
    }
    else if (FillDirection == 2)
    {
        Out = temp03;
    }
    else if (FillDirection == 3)
    {
        Out = temp04;
    }

    Out = step(0.5, Out);
}