// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DCL/Eyes Shader"
{
    Properties
    {
		_EyesTexture("Eyes Texture", 2D) = "white" {}
		_IrisMask("Iris Mask", 2D) = "white" {}
		_EyeTint("EyeTint", Color) = (1,0.03688662,0,0)
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Cull Back

		HLSLINCLUDE

		#pragma target 3.0
		ENDHLSL
		
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Name "Base"

            Blend One Zero
			ZWrite On
			ZTest LEqual
			ColorMask RGBA

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			sampler2D _EyesTexture;
			sampler2D _IrisMask;
 
            CBUFFER_START(UnityPerMaterial)
			float4 _EyesTexture_ST;
			float4 _EyeTint;
			float4 _IrisMask_ST;
			float _Cutoff;
            CBUFFER_END
            
            struct GraphVertexInput
            {
                float4 vertex : POSITION;
				float4 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
            };

            struct GraphVertexOutput
            {
                float4 position : POSITION;
				float4 ase_texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

			
            GraphVertexOutput vert (GraphVertexInput v)
            {
                GraphVertexOutput o = (GraphVertexOutput)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				float3 vertexValue =  float3( 0, 0, 0 ) ;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue; 
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal;
                o.position = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            half4 frag (GraphVertexOutput IN ) : SV_Target
            {
				float2 uv_EyesTexture = IN.ase_texcoord.xy * _EyesTexture_ST.xy + _EyesTexture_ST.zw;
				float4 tex2DNode3 = tex2D( _EyesTexture, uv_EyesTexture );
				float2 uv_IrisMask = IN.ase_texcoord.xy * _IrisMask_ST.xy + _IrisMask_ST.zw;
				float4 lerpResult8 = lerp( tex2DNode3, ( tex2DNode3 * _EyeTint ) , ( 1.0 - tex2D( _IrisMask, uv_IrisMask ).r ));
				
		        float3 Color = lerpResult8.rgb;
		        float Alpha = tex2DNode3.a;		  
                clip(Alpha - _Cutoff);
                return half4(Color, Alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}

