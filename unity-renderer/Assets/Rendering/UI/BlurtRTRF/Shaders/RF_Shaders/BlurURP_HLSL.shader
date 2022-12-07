Shader "Custom/BlurRT URP HLSL"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        // offset
        [HideInInspector] _offset ("Offset", float) = 0
    }
        SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
        }
                        
        Pass
        {
        
            Tags
             {
             "LightMode" = "UniversalForward"
             }
             

            // HLSL code
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            //#pragma multi_compile_fog
            
            #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // SRP Batcher Compatibility
            
            CBUFFER_START(UnityPerMaterial)
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // forced to use the TexelSize property
            float4 _MainTex_ST; // forced to use the _MainTex_ST property  
            float _offset;
            
            CBUFFER_END

            // vertex shader     
            v2f vert(appdata v)
            {
                v2f v2_f;
                v2_f.vertex = TransformObjectToHClip(v.vertex);
                v2_f.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return v2_f;
            }
            
            // fragment shader 
            float4 frag(v2f input) : SV_Target
            {
                float2 resolution = _MainTex_TexelSize.xy;
                float i = _offset;
                
                float4 coloration;
                
                coloration.rgb = tex2D(_MainTex, input.uv).rgb;
                
                coloration.rgb += tex2D(_MainTex, input.uv + float2(i, i) * resolution).rgb;
                coloration.rgb += tex2D(_MainTex, input.uv + float2(i, -i) * resolution).rgb;
                coloration.rgb += tex2D(_MainTex, input.uv + float2(-i, i) * resolution).rgb;
                coloration.rgb += tex2D(_MainTex, input.uv + float2(-i, -i) * resolution).rgb;
                coloration.rgb /= 5.0f;
                
                return coloration;
            }
            
            ENDHLSL
        }
    }
}