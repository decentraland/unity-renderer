
// Digitizer top to Bottom 
// x red , y green , z blue , Alpha White
float DigitBinary(const int x)
{
    // return Magic Number for Digits
    return x==0?480599.0:x==1?139810.0:x==2?476951.0:x==3?476999.0:x==4?350020.0:x==5?464711.0:x==6?464727.0:x==7?476228.0:x==8?481111.0:x==9?481095.0:0.0;
}

// print value
float PrintValue2(float2 fragCoord, float2 pixelCoord, float2 fontSize, float fValue, float digits, float decimalPoints)
{
    float2 charCoord = (fragCoord - pixelCoord) / fontSize;

    if(charCoord.y < 0.0 || charCoord.y >= 1.0) 
    {
        return 0.0;
    }

    float pixels = 0.0;
    float digitIndA = digits - floor(charCoord.x)+ 1.0;

    if(- digitIndA <= decimalPoints) 
    {
        float pow1 = pow(10.0, digitIndA);
        float absValue = abs(fValue);
        float pivot = max(absValue, 1.5) * 10.0;
        
        if(pivot < pow1) 
        {
            if(fValue < 0.0 && pivot >= pow1 * 0.1) pixels = 1792.0;
        } 
        else if(digitIndA == 0.0) 
        {
            if(decimalPoints > 0.0) pixels = 2.0;
        } 
        else 
        {
            fValue = digitIndA < 0.0 ? frac(absValue) : absValue * 10.0;
            pixels = DigitBinary(int (fmod(fValue / pow1, 10.0)));
        }
    }

    return floor(fmod(pixels / pow(2.0, floor(frac(charCoord.x) * 4.0) + floor(charCoord.y * 5.0) * 4.0), 2.0));
}

// float debugger
void Debug_float(float4 val, float2 uv, float2 textPos, float textScale, out float4 res)
{
    // value printer
    // uses PrintValue2
    float a = PrintValue2(uv*textScale, float2(0, 60) + textPos, float2(8,15), val.x, 10, 3);
    float b = PrintValue2(uv*textScale, float2(0, 40) + textPos, float2(8,15), val.y, 10, 3);
    float c = PrintValue2(uv*textScale, float2(0, 20) + textPos, float2(8,15), val.z, 10, 3);    
    float4 d = PrintValue2(uv*textScale, float2(0, 0) + textPos, float2(8,15), val.w, 10, 3);

    // combine x,y,z,a
    float4 vectorizer = float4(a, b, c, 1);

    res = vectorizer + d;
}