Shader "DCL/FX/Blocker Hologram"
{
    Properties
    {
        [HDR]_BaseColor( "Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
        [HDR]_GlowColor ( "Glow Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
        _BaseMap( "Base Map", 2D ) = "white"

        _RimPower ("Rim Power", Float) = 0.5
        _CullYPlane ("Cull Y Plane", Float) = 0.5
        _FadeThickness ("Fade Thickness", Float) = 5
        _RenderingDistanceStart ("Rendering Distance Start", Float) = 20.0
        _RenderingDistanceEnd ("Rendering Distance End", Float) = 40.0
        
        _GlowMap ("Glow Map", 2D) = "white"
        _GlowVelocity ("Glow Velocity", Vector) = (0,0,0,0)
        _BaseMapVelocity ("Base Map Velocity", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Pass
        {
            Tags { "RenderType"="Transparent" "Queue"="Transparent+500" "RenderPipeline"="UniversalPipeline" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            
            #include "UnityCG.cginc" 

            #pragma vertex vert
            #pragma fragment frag

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _GlowColor;

                float _FadeThickness;
                float _CullYPlane;
                float _RenderingDistanceStart;
                float _RenderingDistanceEnd;
                float4 _GlowVelocity;
                float4 _BaseMapVelocity;

                float4 _BaseMap_ST;
                float4 _GlowMap_ST;
                float _RimPower;
            CBUFFER_END
            
            sampler2D _GlowMap;
            sampler2D _BaseMap;

            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 uv : TEXCOORD0;
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
            };

            vertexOutput vert(vertexInput v)
            {
                vertexOutput o;

                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.normalDir = normalize( mul( float4( v.normal, 0.0 ), unity_WorldToObject ).xyz );;

                o.uv = TRANSFORM_TEX( v.uv, _BaseMap );
                o.uv2 = TRANSFORM_TEX( v.uv, _GlowMap );

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
                float renderingDistance = length(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

                if (renderingDistance > _RenderingDistanceEnd)
                {
                    return 0;
                }
                
                float distanceAlphaMultiplier = 1;
                float fadeStart = _RenderingDistanceStart;
                float fadeEnd = _RenderingDistanceEnd;
                float fadeWidth = fadeEnd - fadeStart;
                
                float lambertAtten = GetLambertAttenuation(i);
                float rim = GetRimLighting(i);


                distanceAlphaMultiplier = clamp(fadeEnd - renderingDistance, 0, fadeWidth) / fadeWidth;

                float4 mapColor2 = tex2D( _BaseMap, i.uv + (_Time[0] * _BaseMapVelocity.zw) );
                float4 mapColor = tex2D( _BaseMap, i.uv + (_Time[0] * _BaseMapVelocity.xy) ) * mapColor2;
                float4 glowColor = tex2D( _GlowMap, i.uv2 + (_Time[0] * _GlowVelocity.xy) ) * _GlowColor;
                float4 finalColor = float4((mapColor.rgb * _BaseColor.rgb) + glowColor.rgb, (mapColor.a * _BaseColor.a * glowColor.a));
                finalColor *= rim;
                finalColor.a = saturate(finalColor.a * distanceAlphaMultiplier);

                // NOTE(Brian): fading code
                bool insideFadeThreshold;

                insideFadeThreshold = i.posWorld.y > (_CullYPlane - _FadeThickness);

                float dif = 0;
                dif = (_FadeThickness - (i.posWorld.y - (_CullYPlane - _FadeThickness))) / _FadeThickness;
                finalColor.a = clamp(dif, 0, finalColor.a);

                return finalColor;
            }
        ENDCG
        }
    }
}
