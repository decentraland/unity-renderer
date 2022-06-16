void ScaleFromCenter_float(float2 Input, out float2 Out)
{
    float valueX = ((Input.x * -1) / 2) + 0.5;
    float valueY = ((Input.y * -1) / 2) + 0.5;
    
    Out = float2(valueX, valueY);
}


void ScaleAndOffsetFromCenter_float(float2 UV, float2 Scale, float2 Offset, out float2 Out)
{
    float2 tempTiling = float2(1, 1);
    float2 newUV = UV * tempTiling + Offset;

    float scaleX = ((Scale.x * -1) / 2) + 0.5;
    float scaleY = ((Scale.y * -1) / 2) + 0.5;

    float2 finalScale = float2(scaleX, scaleY);


    Out = newUV * Scale + finalScale;
}