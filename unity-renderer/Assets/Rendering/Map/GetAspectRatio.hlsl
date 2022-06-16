void CalculateAspectRatio_float(float2 Input, out float2 Out)
{
    float tempX = ddx(Input.x);
    float tempY = ddy(Input.y);

    Input.x = (tempY / tempX) * Input.x;

    Out = Input;
}