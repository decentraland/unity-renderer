


// Renders everything with while color.
// Modified version of 'Custom/DrawSimple' shader reference   willweissman


Shader "CustomShaders/HLSL/URPUniOutline/OutlineColor"
{
	HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

		half _Cutoff;

		half4 FragmentSimple(Varyings input) : SV_Target
		{
			return 1;
		}

		half4 FragmentAlphaTest(Varyings input) : SV_Target
		{
			half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
			clip(c.a - _Cutoff);
			return 1;
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
