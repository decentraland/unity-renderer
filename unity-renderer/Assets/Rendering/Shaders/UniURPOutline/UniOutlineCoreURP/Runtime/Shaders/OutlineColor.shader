


// Renders everything with while color.
// Modified version of 'Custom/DrawSimple' shader reference   willweissman


Shader "CustomShaders/HLSL/URPUniOutline/OutlineColor"
{
    properties
    {
        // is enabled as a toggle in the inspector
        
        [Toggle] _IsEnabled("Is Enabled", Float) = 1
    }
	HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

	    float _IsEnabled;

		half _Cutoff;

		half4 FragmentSimple(Varyings input) : SV_Target
		{
            if (_IsEnabled == true)
            {
                return 1;
            }
		    else
		    {
		        return 1;
		    }
			
		}

		half4 FragmentAlphaTest(Varyings input) : SV_Target
		{
            if (_IsEnabled)
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
			    clip(c.a - _Cutoff);
			    return 1;
            }
		    else
		    {
		        return 0;
		    }
			
		}

	ENDHLSL

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }

		Cull Off
		ZWrite Off
		ZTest LEqual
		Lighting Off

		Pass
		{
			Name "Opaque"

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma vertex Vert
			#pragma fragment FragmentSimple

			ENDHLSL
		}

		Pass
		{
			Name "Transparent"

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#pragma vertex Vert
			#pragma fragment FragmentAlphaTest

			ENDHLSL
		}
	}
}
