//This shader generates the outline - color is set here
Shader "Custom/OutlineEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _SceneTex ("Scene Texture", 2D) = "black" {}
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uvs : TEXCOORD0;
            };
 
            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uvs = o.pos.xy / 2 + 0.5;
                return o;
            }
 
            sampler2D _MainTex;
            sampler2D _SceneTex;
            float2 _MainTex_TexelSize;
 
            half4 frag (v2f i) : COLOR
            {
                int numIterations = 2;
 
                float tx_x = _MainTex_TexelSize.x;
                float tx_y = _MainTex_TexelSize.y;
 
                float colIntensity = 0;
 
                if (tex2D(_MainTex, i.uvs.xy).r > 0)
                {
                    return tex2D(_SceneTex, float2(i.uvs.x, i.uvs.y));
                }
 
                for (int k = 0; k < numIterations; k+=1)
                {
                    for (int j = 0; j < numIterations; j+=1)
                    {
                        float2 thing = float2((k - numIterations / 2)*tx_x, (j - numIterations / 2) * tx_y);
                        colIntensity += tex2D(_MainTex, i.uvs.xy + thing).r;
                    }
                }
 
                half4 outcolor = colIntensity * half4(1,0.2,0,0.3) * 2 + (1 - colIntensity) * tex2D(_SceneTex, float2(i.uvs.x, i.uvs.y));
 
                return outcolor;
            }
            ENDCG
        }
    }
}
