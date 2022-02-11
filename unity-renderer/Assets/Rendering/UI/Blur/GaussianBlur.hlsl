void GaussianBlur_float(float Samples, float BlurAmount, float StandarDeviation, float2 UV, UnityTexture2D MainTex, out float4 Col)
{
#define E 2.71828182846

float sum = 0;
float4 col1 = float4(0,0,0,0);
float4 col2 = float4(0,0,0,0);
float halfSamples = floor(Samples/2);
#if StandarDeviation <= 0
	for(float index = 0; index < halfSamples; index++)
	{
			float offset = (index/(halfSamples-1) - 0.5) * BlurAmount;

			float2 uv = UV + float2(0, offset);

			float stDevSquared = StandarDeviation*StandarDeviation;
			float gauss = (1 / sqrt(2*PI*stDevSquared)) * pow(E, -((offset*offset)/(2*stDevSquared)));

			sum += gauss;
			
			col1 += tex2D(MainTex, uv) * gauss;
	}
	
	for(float index = 0; index < halfSamples; index++)
	{
			float offset = (index/(halfSamples-1) - 0.5) * BlurAmount;

			float2 uv = UV + float2(offset, 0);

			float stDevSquared = StandarDeviation*StandarDeviation;
			float gauss = (1 / sqrt(2*PI*stDevSquared)) * pow(E, -((offset*offset)/(2*stDevSquared)));

			sum += gauss;
			
			col2 += tex2D(MainTex, uv) * gauss;
	}
	
	Col = (col1 + col2)/ sum;
#else
	Col = tex2D(MainTex, UV);
#endif
}