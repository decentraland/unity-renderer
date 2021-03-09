Shader "DCL/FX/Hologram"
{
    Properties
    {
        _Color( "Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
        _RimColor( "Rim Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
        _RimPower( "Rim Power", Range( 0.01, 10.0 ) ) = 3.0

        _ThrobbScale("Throbbing Scale", float ) = 0.1

        [PerRendererData] _CullYPlane ("Cull Y Plane", Float) = 0.5
        _FadeThickness ("Fade Thickness", Float) = 5
        _FadeDirection ("Fade Direction", Float) = 0
        _MaxRenderingDistance ("Max Rendering Distance", Float) = 45
    }

    SubShader
    {
        Pass
        {
            Tags { "RenderType"="Transparent" "Queue"="Transparent+500" "RenderPipeline"="UniversalPipeline" }

            Blend SrcAlpha One
            ZWrite Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;

                float4 _RimColor;
                float _RimPower;

                float _ThrobbScale;
                float _FadeDirection;
                float _FadeThickness;
                float _CullYPlane;
                float _MaxRenderingDistance;
            CBUFFER_END

            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
            };

            vertexOutput vert(vertexInput v)
            {
                vertexOutput o;

                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.normalDir = normalize( mul( float4( v.normal, 0.0 ), unity_WorldToObject ).xyz );;

                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float GetLambertAttenuation(vertexOutput i)
            {
                float3 lightDirection = normalize( _WorldSpaceLightPos0.xyz );
                return saturate( dot( i.normalDir, lightDirection ) );
            }

            float GetRimLighting(vertexOutput i)
            {
                float3 viewDirection = normalize( _WorldSpaceCameraPos.xyz - i.posWorld.xyz );
                return pow( 1.0 - saturate( dot( viewDirection, i.normalDir ) ), _RimPower );
            }

            float4 frag(vertexOutput i) : COLOR
            {
                float distanceAlphaMultiplier = 1;
                float renderingDistance = length(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

                if (renderingDistance > _MaxRenderingDistance) return (0,0,0,0);

                float halfMaxRenderingDistance = _MaxRenderingDistance / 2;
                distanceAlphaMultiplier = 1 - ((renderingDistance - halfMaxRenderingDistance) / halfMaxRenderingDistance);

                float lambertAtten = GetLambertAttenuation(i);
                float rim = GetRimLighting(i);

                // NOTE(Brian): custom function to make the pulsating glow
                float animatedThrobbing = saturate(sin( -i.posWorld.y * _ThrobbScale + _Time[3] * 2 - i.normalDir.y * 4.0 ) );

                float finalRimFactor = rim * animatedThrobbing;
                float3 finalRimColor = _RimColor.rgb * (lambertAtten * finalRimFactor);

                float4 finalColor = float4( _Color.rgb + finalRimColor, _Color.a + finalRimFactor );
                finalColor.a = saturate(finalColor.a * distanceAlphaMultiplier);

                // NOTE(Brian): fading code
                bool insideFadeThreshold;

                if ( _FadeDirection == 0 )
                    insideFadeThreshold = i.posWorld.y < _CullYPlane;
                else
                    insideFadeThreshold = i.posWorld.y > (_CullYPlane - _FadeThickness);

                if (insideFadeThreshold)
                {
                    float dif = 0;

                    if ( _FadeDirection == 0 )
                        dif = (_FadeThickness - (_CullYPlane - i.posWorld.y)) / _FadeThickness;
                    else
                        dif = (_FadeThickness - (i.posWorld.y - (_CullYPlane - _FadeThickness))) / _FadeThickness;

                    finalColor.a = clamp(dif, 0, finalColor.a);
                }

                return finalColor;
            }
        ENDCG
        }
    }
}
