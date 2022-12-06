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
        
        LOD 100
        
        Pass
        {
        /*
            Tags
             {
             "LightMode" = "UniversalForward"
             }
             */

            // HLSL code
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            //#pragma multi_compile_fog
            
            #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // SRP Batcher Compatibility
            
            CBUFFER_START(UnityPerMaterial)
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            float _offset;
            
            CBUFFER_END
            
                        
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float4 frag(v2f input) : SV_Target
            {
                float2 inputData = _MainTex_TexelSize.xy;
                float i = _offset;
                
                float4 col;
                
                col.rgb = tex2D(_MainTex, input.uv).rgb;
                col.rgb += tex2D(_MainTex, input.uv + float2(i, i) * inputData).rgb;
                col.rgb += tex2D(_MainTex, input.uv + float2(i, -i) * inputData).rgb;
                col.rgb += tex2D(_MainTex, input.uv + float2(-i, i) * inputData).rgb;
                col.rgb += tex2D(_MainTex, input.uv + float2(-i, -i) * inputData).rgb;
                col.rgb /= 5.0f;
                
                return col;
            }
            
            ENDHLSL
        }
    }
}