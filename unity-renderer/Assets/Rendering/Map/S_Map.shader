Shader "Unlit/S_Map"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Input ("Input", 2D) = "white" {}
        _Resolution("Resolution", Vector) = (0,0,0,0)
        _Mouse ("Mouse", Vector) = (0,0,0,0)
        _TileSizeInPixels ("TileSizeInPixels", Float) = 10
        _SizeOfTheTexture("SizeOfTheTexture", Vector) = (512, 512,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Input;
            float4 _Input_ST;
            float2 _Resolution;
            float2 _Mouse;
            float _TileSizeInPixels;
            float2 _SizeOfTheTexture;


            // pixel-perfect UV based on an arbitrary presicion UV + texture size
            float2 clampToTexture(float2 uv, float2 size)
            {
                float2 clampUv = ceil(uv * size) / size;
                float2 pixel = float2(1.0, 1.0) / size;
                return clampUv - pixel * float2(0.5, 0.5);
            }

            // this function detects a change in the pixel-perfect UV mapping of the texture
            // using a screen pixel as delta
            float2 detectEdges(float2 positionInScreen, float2 size, float zoom, float2 u_resolution)
            {
                float2 delta = float2(1.0, -1.0) / zoom / u_resolution; // top-left delta

                // fix aspect ratio
                if (u_resolution.x > u_resolution.y)
                {
                    delta.x *= u_resolution.x / u_resolution.y;
                }
                else
                {
                    delta.y *= u_resolution.y / u_resolution.x;
                }

                float2 uv = clampToTexture(positionInScreen - delta, size);
                float2 newUv = clampToTexture(positionInScreen, size);

                return (newUv - uv);
            }

            // this function returns a color for the tiles based on a texture color
            float4 colorFromType(float type)
            {
                if (type < 33.0)
                    return float4(0.4, 0.0, 0.8, 1.0);

                if (type < 65.0)
                    return float4(0.5, 0.5, 0.5, 1.0);

                return float4(0.2, 0.2, 0.2, 1.0);
            }

            // given a pixel in screen, texture size and information of the tile, render the tile or the lines
            float detectEdgesFloat(float2 positionInScreen, float2 size, float zoom, float info, float2 u_resolution)
            {
                float2 t = detectEdges(positionInScreen, size, zoom, u_resolution);

                bool hasTopBorder = false;
                bool hasLeftBorder = false;
                bool hasTopLeftPoint = false;

                bool inTopBorder = t.y != 0.0;
                bool inLeftBorder = t.x != 0.0;

                // read bit flags
                if (info >= 32.0) { hasTopLeftPoint = true; info -= 32.0; }
                if (info >= 16.0) { hasLeftBorder = true; info -= 16.0; }
                if (info >= 8.0) { hasTopBorder = true; info -= 8.0; }

                if (!hasTopBorder && !hasLeftBorder)
                {
                    // disconnected everywhere: it's a square
                    if (inLeftBorder || inTopBorder)
                        return 0.0;
                    return 1.0;
                }
                else
                {
                    if (hasTopBorder && hasLeftBorder && hasTopLeftPoint)
                    {
                        // connected everywhere: it's a square with lines
                        return 1.0;
                    }
                    else
                    {
                        // connected left: it's a rectangle
                        if (hasTopBorder && !inLeftBorder) return 1.0;
                        // connected top: it's a rectangle
                        if (hasLeftBorder && !inTopBorder) return 1.0;
                    }
                }
                return 0.0;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = (0, 0, 0, 0);

                // offset = center of the map in uniform coords
                float2 offset = _Mouse / _Resolution;
                float zoom;
                float2 v_texcoord; // v_texcoord is a ratio-normalized sceen pixel in uniform coords
                
                if (_Resolution.x > _Resolution.y)
                {
                    v_texcoord.x = i.vertex.x / _Resolution.y - 0.5 - 0.5 * (_Resolution.x - _Resolution.y) / _Resolution.y;
                    v_texcoord.y = i.vertex.y / _Resolution.y - 0.5;
                    zoom = _SizeOfTheTexture.x / _Resolution.y * _TileSizeInPixels;
                }
                else
                {
                    v_texcoord.x = i.vertex.x / _Resolution.x - 0.5;
                    v_texcoord.y = i.vertex.y / _Resolution.x - 0.5 - 0.5 * (_Resolution.y - _Resolution.x) / _Resolution.x;
                    zoom = _SizeOfTheTexture.y / _Resolution.x * _TileSizeInPixels;
                }


                // tileOfInterest, represented as UV coords of the MAP texture
                float2 tileOfInterest = v_texcoord / zoom + offset;

                // render black tiles outside of the image map data-range
                if (tileOfInterest.x > 1.0 || tileOfInterest.y > 1.0 || tileOfInterest.y < 0.0 || tileOfInterest.x < 0.0)
                {
                    col = float4(0.0, 0.0, 0.0, 1.0);
                    return col;
                }

                // uvs of the texture for the center of coordinates
                // and the pixel of the screen (tileOfInterest) 
                float2 centerUv = clampToTexture(offset, _SizeOfTheTexture);
                float2 uv = clampToTexture(tileOfInterest, _SizeOfTheTexture);

                // if we are rendering the center of coordinates
                if (length(centerUv - uv) == 0.0)
                {
                    col = float4(1.0, 0.0, 1.0, 1.0);
                    return col;
                }

                float4 data = tex2D(_Input, uv);

                if (data.a > 0.0)
                {
                    if (detectEdgesFloat(tileOfInterest, _SizeOfTheTexture, zoom, data.r * 256.0, _Resolution) == 0.0)
                        col = float4(0.0, 0.0, 0.0, 1.0);
                    else
                        col = colorFromType(data.g * 256.0);
                }
                else
                {
                    if (detectEdgesFloat(tileOfInterest, _SizeOfTheTexture, zoom, 0.0, _Resolution) == 0.0)
                        col = float4(0.0, 0.0, 0.0, 1.0);
                    else
                        col = float4(0.05, 0.05, 0.05, 1.0);
                }

                return col;
            }
            ENDCG
        }
    }
}
