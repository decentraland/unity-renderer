Shader "DCL/Toon Shader Legacy (Branching)"
{
    Properties
    {
        _MatCap ("MatCap (RGB)", 2D) = "white" {}

        _AvatarMap1("Avatar Map #1", 2D) = "white" {}
        _AvatarMap2("Avatar Map #2", 2D) = "white" {}
        _AvatarMap3("Avatar Map #3", 2D) = "white" {}
        _AvatarMap4("Avatar Map #4", 2D) = "white" {}
        _AvatarMap5("Avatar Map #5", 2D) = "white" {}
        _AvatarMap6("Avatar Map #6", 2D) = "white" {}
        _AvatarMap7("Avatar Map #7", 2D) = "white" {}
        _AvatarMap8("Avatar Map #8", 2D) = "white" {}
        _AvatarMap9("Avatar Map #9", 2D) = "white" {}
        _AvatarMap10("Avatar Map #10", 2D) = "white" {}
        _AvatarMap11("Avatar Map #11", 2D) = "white" {}
        _AvatarMap12("Avatar Map #12", 2D) = "white" {}

        //_BaseColor ("Color", Color) = (0, 0, 0, 0)
        //[HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)
        //_Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5
        //_GlobalAvatarTextureArray_ST ("Global Texture Transform", Vector) = (1,1,0,0)

        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
    }

    Subshader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }
        Blend [_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]
        Cull [_Cull]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            //#pragma multi_compile _ _EMISSION
            //#pragma multi_compile _ _ALPHATEST_ON

            #include "UnityCG.cginc"

            struct MeshData
            {
                float4 color : COLOR;
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 emission_color : TEXCOORD3;
                float2 texture_indexes : TEXCOORD4;
            };

            struct v2f
            {
                float4 color : COLOR;
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 cap : TEXCOORD1;
                UNITY_FOG_COORDS(2)
                float4 emission_color : TEXCOORD3;
                float2 texture_indexes : TEXCOORD4;
            };


            uniform sampler2D _MatCap;

            uniform sampler2D _AvatarMap1;
            uniform sampler2D _AvatarMap2;
            uniform sampler2D _AvatarMap3;
            uniform sampler2D _AvatarMap4;
            uniform sampler2D _AvatarMap5;
            uniform sampler2D _AvatarMap6;
            uniform sampler2D _AvatarMap7;
            uniform sampler2D _AvatarMap8;
            uniform sampler2D _AvatarMap9;
            uniform sampler2D _AvatarMap10;
            uniform sampler2D _AvatarMap11;
            uniform sampler2D _AvatarMap12;

            CBUFFER_START(UnityPerMaterial)
            float _Cutoff;
            CBUFFER_END

            v2f vert(MeshData v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.texture_indexes = float2(abs(v.texture_indexes[0]), abs(v.texture_indexes[1]));
                o.color = v.color;
                o.emission_color = v.emission_color;

                float3 worldNorm = normalize(
                    unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y +
                    unity_WorldToObject[2].xyz * v.normal.z);
                worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
                o.cap.xy = worldNorm.xy * 0.5 + 0.5;

                UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }

            float4 SampleTexture(fixed4 defaultColor, float2 uv, float textureIndex)
            {
                float4 result;

                if (textureIndex < 0.01)
                    result = tex2D(_AvatarMap1, uv);
                else if (abs(1 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap2, uv);
                else if (abs(2 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap3, uv);
                else if (abs(3 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap4, uv);
                else if (abs(4 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap5, uv);
                else if (abs(5 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap6, uv);
                else if (abs(6 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap7, uv);
                else if (abs(7 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap8, uv);
                else if (abs(8 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap9, uv);
                else if (abs(9 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap10, uv);
                else if (abs(10 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap11, uv);
                else if (abs(11 - textureIndex) < 0.01)
                    result = tex2D(_AvatarMap12, uv);
                else
                    result = defaultColor;


                // if (index >= 0 && index <= 4)
                // {
                //     result = tex2D(_AvatarMap1, uv);
                //     //result += tex2D(_AvatarMap2, uv) * int(mask & (1 << 1));
                //     //result += tex2D(_AvatarMap3, uv) * int(mask & (1 << 2));
                //     //result += tex2D(_AvatarMap4, uv) * int(mask & (1 << 3));
                // }
                // else if (index > 4 && index <= 8)
                // {
                //     result += tex2D(_AvatarMap5, uv) * (mask & (1 << 4));
                //     result += tex2D(_AvatarMap6, uv) * (mask & (1 << 5));
                //     result += tex2D(_AvatarMap7, uv) * (mask & (1 << 6));
                //     result += tex2D(_AvatarMap8, uv) * (mask & (1 << 7));
                // }
                // else if (index > 8 && index <= 12)
                // {
                //     result += tex2D(_AvatarMap9, uv) * (mask & (1 << 8));
                //     result += tex2D(_AvatarMap10, uv) * (mask & (1 << 9));
                //     result += tex2D(_AvatarMap11, uv) * (mask & (1 << 10));
                //     result += tex2D(_AvatarMap12, uv) * (mask & (1 << 11));
                // }
                // else
                // {
                //     return defaultColor;
                // }

                return result;
            }

            fixed4 frag(v2f i) : COLOR
            {
                const float4 albedo = SampleTexture(1, i.uv, i.texture_indexes[0]);
                const float4 emission = SampleTexture(0, i.uv, i.texture_indexes[1]);

                const fixed4 tex = albedo * i.color;

                fixed4 matcap = tex2D(_MatCap, i.cap);

                #ifdef _ALPHATEST_ON
					clip(tex.a - _Cutoff);
                #endif

                matcap *= 2.0;
                matcap = saturate(tex + matcap - 1.0);
                float4 emissionColor = i.emission_color;
                matcap.rgb += (emission * emissionColor).rgb;
                matcap.a = tex.a;

                UNITY_APPLY_FOG(i.fogCoord, matcap);

                return matcap;
            }
            ENDCG
        }
    }
}