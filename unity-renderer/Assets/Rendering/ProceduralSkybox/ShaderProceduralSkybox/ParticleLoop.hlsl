float GetRandomNumber(float2 seed, float min, float max)
{
    float temp = frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);

    return lerp(min, max, temp);
}

float2 Flipbook(float2 UV, float Width, float Height, float Tile, float2 Invert)
{
    Tile = fmod(Tile, Width * Height);
    float2 tileCount = float2(1.0, 1.0) / float2(Width, Height);
    float tileY = abs(Invert.y * Height - (floor(Tile * tileCount.x) + Invert.y * 1));
    float tileX = abs(Invert.x * Width - ((Tile - Width * floor(Tile * tileCount.x)) + Invert.x * 1));
    return (UV + float2(tileX, tileY)) * tileCount;
}

void ParticleLoop_half(Texture2D Texture, float repetition, float2 UV, float offset, SamplerState Sample, float3 flipbookParams, float2 flipbookInvert, out half4 Out)
{
	half4 result = float4(0,0,0,0);

	for(int i = 0; i < repetition; i++)
	{
        float randomNoX = GetRandomNumber(float2(1, 1), -1, 1);
        float randomNoY = GetRandomNumber(float2(-1, 0.5), -1, 1);
		float2 finalUV = UV;
		finalUV.x = UV.x + offset + randomNoX;
		finalUV.y = UV.y + offset + randomNoY;

        finalUV = Flipbook(finalUV, flipbookParams.x, flipbookParams.y, flipbookParams.z, flipbookInvert);

		result += SAMPLE_TEXTURE2D(Texture, Sample, (finalUV));
		result *= clamp(result + (i/ repetition),0,1);
	}

	Out = result;
}