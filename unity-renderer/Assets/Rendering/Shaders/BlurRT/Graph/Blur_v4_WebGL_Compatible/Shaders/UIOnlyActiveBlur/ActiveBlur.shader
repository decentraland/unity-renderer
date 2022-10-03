Shader "CustomShaders/CG/ActiveBlur"
{
    Properties
    {
        [Header(Main Blur Settings)]
        // Control Blur Amount
        _BlurAmount ("Blur Amount", Range(0, 2)) = 1
        [Space(10)]
        
        [header(Blur Transparency)]      
        // Blur Passes
        _BlurPasses ("Blur Transparency Passes", Range(1, 1.15)) = 1 
        
        [Header(Secondary Blur Settings)]
        // Blur Deviation
        _BlurDeviation ("Blur Deviation", Range(0.01, 0.1)) = 0.0397887        
        // Blur Power
        _BlurPower ("Blur Passes", Range(1, 24)) = 24 
        // Affected Pixels
        _AffectedPixels ("Affected Pixels", Range(0, 4)) = 3  
        
        [Space(5)]   
        // Image from Rasterized UI
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }
        
        // Masked Image
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        // Stencils
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        
    }
    
    SubShader
    {
        // Tags
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        
        // Stencil References
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        
        // Ztest usage
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            // for rasterization
            #include "UnityUI.cginc"

            // get data from unity
            struct appdata_t
            {
                float4 vertex: POSITION;
                float4 color: COLOR;
                float2 uv: TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // interpolators
            struct v2f
            {
                float4 vertex: SV_POSITION;
                fixed4 color: COLOR;
                float2 uv: TEXCOORD0;
                float4 worldPosition: TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // properties to parameters
            float _BlurAmount;
            float _BlurDeviation;
            float _BlurPower;
            float _AffectedPixels;
            float _BlurPasses;

            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            // vertex shader data
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.uv = v.uv;
                OUT.color = v.color;
                return OUT;
            }

            // gaussian blur quick Algorithm
            float gaussianBlurAlgo(float x, float y)
            {
                // returned gaussian
                float _gaussianBlurRes = _BlurDeviation * exp((x * x ) + (y * y) / _BlurPower);
                return _gaussianBlurRes;
            }

            // fragment shader data
            fixed4 frag(v2f IN): SV_Target
            {
                // starting color and gaussian blur BEFORE the per pixel loop
                half4 color = 0;
                float gaussianBlur = 0;

                // run per close - around pixel
                for (int x = -_AffectedPixels; x <= _AffectedPixels; x++)
                    for (int y = -_AffectedPixels; y <= _AffectedPixels; y++)
                    {
                        // Get gaussian value for current pixel
                        float g = gaussianBlurAlgo(x, y) * 2;
                        gaussianBlur = (gaussianBlur + g) * _BlurPasses;
                        
                        // Add color at this pixel with respect to blur strength and gaussian amount
                        color = color + ( tex2D(_MainTex, IN.uv + float2(x * _MainTex_TexelSize.x *_BlurAmount, y * _MainTex_TexelSize.y * _BlurAmount)) * g );
                    }

                // Average color based on blur strength
                color = color / gaussianBlur;
                color = color * IN.color;
                
                // alpha
                color.a = color.a * (UnityGet2DClipping(IN.worldPosition.xy, _ClipRect));

                // return color
                return color;
            }
            ENDCG
        }
    }
}
