#ifndef OUTLINER_BLUR
#define OUTLINER_BLUR
#define OUTLINER_PI 3.14159265

float gauss(float x, float sigma)
{
	return  1.0f / (2.0f * OUTLINER_PI * sigma * sigma) * exp(-(x * x) / (2.0f * sigma * sigma));
}

float gauss(float x, float y, float sigma)
{
    return  1.0f / (2.0f * OUTLINER_PI * sigma * sigma) * exp(-(x * x + y * y) / (2.0f * sigma * sigma));
}

struct pixel_info
{
	sampler2D tex;
	float2 uv;
	float4 texelSize;
};

int gaussianBlurQuality;

float4 GaussianBlur(pixel_info pinfo, float sigma, float2 dir)
{
    float4 o = 0;
    float sum = 0;
    float2 uvOffset;
    float weight;
	
    for(int kernelStep = - gaussianBlurQuality / 2; kernelStep <= gaussianBlurQuality / 2; ++kernelStep)
    {
        uvOffset = pinfo.uv;
        uvOffset.x += ((kernelStep) * pinfo.texelSize.x) * dir.x;
        uvOffset.y += ((kernelStep) * pinfo.texelSize.y) * dir.y;
        weight = gauss(kernelStep, sigma) + gauss(kernelStep+1, sigma);
        o += tex2D(pinfo.tex, uvOffset) * weight;
        sum += weight;
    }
    o *= (1.0f / sum);
    return o;
}

#endif