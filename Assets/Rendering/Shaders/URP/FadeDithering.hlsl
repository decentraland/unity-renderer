#ifndef DCL_FADE_DITHERING_INCLUDED
#define DCL_FADE_DITHERING_INCLUDED


float4 fadeDithering(float4 color, float3 positionWS, float4 positionSS)
{
	const float SURFACE_OPAQUE = 0;
	bool insideFadeThreshold;
	float3 worldPos = positionWS;

	if ( _FadeDirection == 0 )
		insideFadeThreshold = worldPos.y < _CullYPlane;
	else 
		insideFadeThreshold = worldPos.y > (_CullYPlane - _FadeThickness);

	if (insideFadeThreshold)
	{
		float dif = 0;

		if ( _FadeDirection == 0 )
			dif = (_FadeThickness - (_CullYPlane - worldPos.y)) / _FadeThickness;
		else
			dif = (_FadeThickness - (worldPos.y - (_CullYPlane - _FadeThickness))) / _FadeThickness;

		float hideAmount = dif;

		// Screen-door transparency: Discard pixel if below threshold.
		const float4x4 thresholdMatrix =
		{
			1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
			13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
			4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
			16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
		};

		const float4x4 _RowAccess = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
		float2 pos = positionSS.xy / positionSS.w;
		pos *= _ScreenParams.xy; // pixel position
		clip(hideAmount - thresholdMatrix[fmod(pos.x, 4)] * _RowAccess[fmod(pos.y, 4)]);
	}

	return color;
}
#endif
