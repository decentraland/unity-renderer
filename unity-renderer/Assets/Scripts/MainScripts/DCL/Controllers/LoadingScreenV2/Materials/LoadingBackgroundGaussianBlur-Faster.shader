Shader "Custom/LoadingBackgroundGaussianBlur-Faster"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Sigma("Sigma", Range(0.0, 10.0)) = 1.0
    }
    SubShader
    {
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
            float _Sigma;
            float4 _MainTex_TexelSize;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float normpdf(in float x, in float sigma)
            {
                return 0.39894*exp(-0.5*x*x/(sigma*sigma))/sigma;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 originalColor = tex2D(_MainTex, i.uv);

                // If pixel is transparent, interpret as black.
                if (originalColor.a == 0.0)
                {
                    originalColor = float4(0.0, 0.0, 0.0, 1.0);
                }

                //declare stuff
                const int mSize = 11;
                const int kSize = (mSize-1)/2;
                float kernel[mSize];
                float3 final_colour = float3(0.0, 0.0, 0.0);
                
                //create the 1-D kernel
                float sigma = _Sigma;
                float Z = 0.0;
                for (int j1 = 0; j1 <= kSize; ++j1)
                {
                    kernel[kSize+j1] = kernel[kSize-j1] = normpdf(float(j1), sigma);
                }
                
                //get the normalization factor (as the gaussian has been clamped)
                for (int j2 = 0; j2 < mSize; ++j2)
                {
                    Z += kernel[j2];
                }
                
                //read out the texels
                for (int x=-kSize; x <= kSize; ++x)
                {
                    for (int y=-kSize; y <= kSize; ++y)
                    {
                        float2 sampleUV = i.uv + float2(x,y)*_MainTex_TexelSize.xy;
                        
                        //check if the sampled pixel is within UV range
                        if (sampleUV.x >= 0.0 && sampleUV.x <= 1.0 && sampleUV.y >= 0.0 && sampleUV.y <= 1.0)
                        {
                            final_colour += kernel[kSize+y]*kernel[kSize+x]*tex2D(_MainTex, sampleUV).rgb;
                        }
                    }
                }
                
                fixed3 finalColorRGB = final_colour/(Z*Z);
                float alpha = 0.1;
                fixed3 blendedColor = lerp(float3(0, 0, 0), finalColorRGB, alpha);
                
                return fixed4(blendedColor, originalColor.a);
            }
            
            ENDCG
        }
    }
}
