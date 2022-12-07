Shader "Custom/Blur URP CG"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        // offest of the blur
        //[HideInIspector]_offset("Offset", Float) = -1 // for SRP we need to set the default value
                
    }
        SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            
        }
        //LOD 100
        Pass
        {
            
            Tags
             {
             "LightMode" = "UniversalForward"
             }
            
            // HLSL program            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            // obj data
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // v2f data interpolator
            struct v2f
            {
                float2 uv : TEXCOORD0;
                
                float4 vertex : SV_POSITION;
            };

            // SRP batcher compatible
            CBUFFER_START(UnityPerMaterial)
            
            sampler2D _MainTex; // 0
            float4 _MainTex_TexelSize; // Texture size in pixels
            float4 _MainTex_ST; // Texture tiling and offset (interpolated)
            
            float _offset;            
            
            CBUFFER_END
            
            // vertex shader
            v2f vert(appdata v)
            {
                v2f v2_f;
                v2_f.vertex = UnityObjectToClipPos(v.vertex);
                v2_f.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return v2_f;
            }

            // fragment shader 
            fixed4 frag(v2f input) : SV_Target
            {
                float2 resolution = _MainTex_TexelSize.xy; // 1.0 / textureSize(_MainTex, 0);
                float matrixI = _offset; // 0.0;
                
                
                fixed4 coloration;

                // coloration offset
                coloration.rgb = tex2D(_MainTex, input.uv).rgb;
                coloration.a = 1.0;
                coloration.rgb += tex2D(_MainTex, input.uv + float2(matrixI, matrixI) * resolution).rgb;
                coloration.rgb += tex2D(_MainTex, input.uv + float2(matrixI, -matrixI) * resolution).rgb;
                coloration.rgb += tex2D(_MainTex, input.uv + float2(-matrixI, matrixI) * resolution).rgb;
                coloration.rgb += tex2D(_MainTex, input.uv + float2(-matrixI, -matrixI) * resolution).rgb;
                coloration.rgb /= 5.0f;
                
                return coloration;
            }
                        
            ENDHLSL
        }
    }
}