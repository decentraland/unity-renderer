Shader "HLSLCustomShaders/URP/Outline/Occlusion/OutlineEdgeURP" {
	
	// ShaderLab Properties
	Properties {
	
		[Header(PBR BASED)]
		[Space(5)]
		
		// cover basic PBR properties
		[MainColor] _BaseColor  ("Color", Color) = (1, 1, 1, 1)
		[MainTexture] _BaseMap  ("Albedo", 2D) = "white" {}
		
		[Gamma] _Metallic       ("Metallic", Range(0.0, 1.0)) = 0.0
		_Smoothness             ("Smoothness", Range(0.0, 1.0)) = 0.5
		
		[Toggle(_NORMALMAP)] _1 ("Normal Map", Float) = 1
		
		_BumpScale              ("Scale", Float) = 1.0
		_BumpMap                ("Normal Map", 2D) = "bump" {}
		
		// cover additional properties for Outline
		[Space(3)]
		[Header(Outline Properties)]
		[Space(5)]
		
		_OutlineColor  ("Outline Color", Color) = (1, 1, 1, 1)
		_OutlineWidth  ("Outline Width", Range(0, 10)) = 2
		_OutlineAmount ("Outline Amount", Range(0, 1)) = 1
		
		// cover additional properties for Occlusion-Overlay
		[Space(3)]
		_OverlayColor  ("Overlay Color", Color) = (0, 1, 0, 1)
		_Overlay       ("Overlay Amount", Range(0, 1)) = 0
				
		[MaterialToggle] _OutlineBasedVertexColorR ("Outline Based Vertex Channel Red", Float) = 0.0
		
		[Space(3)]
		[Header(Mask)]		
		[Space(5)]
		
		// cover additional properties for Mask ZTest
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest2ndPass ("ZTest 2nd Pass", Int) = 0
		
		[Space(3)]
		[Header(Fill)]
		[Space(5)]
		
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest3rdPass ("ZTest 3rd Pass", Int) = 0
	}
	
	// ShaderLab SubShader
	SubShader {
		
		Tags 
		{ 
		//"RenderType" = "Opaque" // AVOID SETTING THIS TAG for WEBGL 2.0 with URP combo
		"RenderPipeline" = "UniversalRenderPipeline" 
		"Queue" = "Transparent" 
		}

		// PASS 1
		Pass {
		
			Tags 
			{ 
			
			"LightMode" = "SRPDefaultUnlit" 
			
			}

			// URP HLSL TEMPLATED SHADER INSTRUCTIONS START

			// HLSL Program URP BASED
			HLSLPROGRAM
			// Force GLES backend
			#pragma prefer_hlslcc gles 
			// Use shader model 3.0 target, to make DX11 happy
			#pragma exclude_renderers d3d11_9x // Exclude DX9
			// target 3.0
			#pragma target 3.0

			// needed includes for HLSL
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

			// Here we cover the different possible scenarios for GPU Compilation
			
			// -------------------------------------
			// #pragma Shader Keywords as shader feature for compliler
			// -------------------------------------
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _EMISSION
			#pragma shader_feature _METALLICSPECGLOSSMAP
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			
			#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature _GLOSSYREFLECTIONS_OFF
			#pragma shader_feature _SPECULAR_SETUP
			
			#pragma shader_feature _RECEIVE_SHADOWS_OFF

			#pragma shader_feature _OCCLUSIONMAP

			// -------------------------------------
			// #pragma URP keywords for multi compile
			// -------------------------------------
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
									
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

			// -------------------------------------
			// #pragma defined keywords for multi compile
			// -------------------------------------
			#pragma multi_compile _ LIGHTMAP_ON
			
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			
			#pragma multi_compile_fog

			//--------------------------------------
			// #pragma GPU Instancing
			// -------------------------------------
			#pragma multi_compile_instancing
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment

			// URP HLSL TEMPLATED SHADER INSTRUCTIONS END


			// for first pass we need to use the same vertex shader as the Lit shader
						
			// -------------------------------------
			// Properties to parameters for first pass 
			float4 _OverlayColor;
			float _Overlay;

			// shader model attributes
			struct ShaderModelAttr
			{
				float4 positionOS : POSITION; // vertex position in object space
				float3 normalOS   : NORMAL; // vertex normal in object space
				float4 tangentOS  : TANGENT; // vertex tangent in object space
				float2 uv         : TEXCOORD0; // vertex uv
				float2 uvLM       : TEXCOORD1;  // vertex lightmap uv
				float3 color      : COLOR; // color
				
				UNITY_VERTEX_INPUT_INSTANCE_ID // vertex instance id
			};
			
			// tex coordinates
			struct TextureVariations
			{
				float2 uv               : TEXCOORD0; // vertex color
				float2 uvLM             : TEXCOORD1; // lightmap
				float4 positionWSAndFog : TEXCOORD2; // xyz: positionWS, w: vertex fog factor
				half3  normalWS         : TEXCOORD3; // normal, eye direction

				// BEWARE OF ORDER
				// in case we use normal maps
#if _NORMALMAP
				half3 tangentWS         : TEXCOORD4; // tangent
				half3 bitangentWS       : TEXCOORD5; // bitangent
#endif

				// Main Light Shadows
#ifdef _MAIN_LIGHT_SHADOWS

				float4 shadowCoord      : TEXCOORD6; // compute shadow coord per-vertex for the main light
#endif
				float4 positionCS       : SV_POSITION; // clip space position
			};
			
			TextureVariations LitPassVertex (ShaderModelAttr input) 
			
			{
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz); // get vertex position in world space
				VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS); // get vertex normal in world space
				
				float fogFactor = ComputeFogFactor(vertexInput.positionCS.z); // compute fog factor

				TextureVariations output; // output struct
				output.uv = TRANSFORM_TEX(input.uv, _BaseMap); // output uv
				output.uvLM = input.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw; // output lightmap uv
				output.positionWSAndFog = float4(vertexInput.positionWS, fogFactor); // output position in world space and fog factor
				output.normalWS = vertexNormalInput.normalWS; // output normal in world space
				
				// Normal map
#ifdef _NORMALMAP

				output.tangentWS = vertexNormalInput.tangentWS; // output tangent in world space
				output.bitangentWS = vertexNormalInput.bitangentWS; // output bitangent in world space

#endif
				// main light shadow coord
#ifdef _MAIN_LIGHT_SHADOWS
				
				output.shadowCoord = GetShadowCoord(vertexInput); // get shadow coord per-vertex for the main light

#endif
				// clip space position
				output.positionCS = vertexInput.positionCS;
				
				return output; // here we can divide in case of artifacts
			}
				// -------------------------------------
				// Light Pass Fragment
				half4 LitPassFragment (TextureVariations input) : SV_Target
				{
				// surface data
				SurfaceData surfaceData;
				// init surface data
				InitializeStandardLitSurfaceData(input.uv, surfaceData);
				// get surface data for albedo
				surfaceData.albedo = lerp(surfaceData.albedo.rgb, _OverlayColor.rgb, _Overlay);
				
				// normal map
#if _NORMALMAP
	            // normal map
				half3 normalWS = TransformTangentToWorld(surfaceData.normalTS, half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
#else
				half3 normalWS = input.normalWS;
#endif
				normalWS = normalize(normalWS);

				// lightmap
#ifdef LIGHTMAP_ON

				// sample lightmap for baked lighting
				half3 bakedGI = SampleLightmap(input.uvLM, normalWS);
#else
				// no lightmap
				half3 bakedGI = SampleSH(normalWS);
#endif

				// position and eye direction
				float3 positionWS = input.positionWSAndFog.xyz;
				half3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - positionWS);

				// lighting properties
				BRDFData brdfData; // brdf data
				InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

				// light shadows 
#ifdef _MAIN_LIGHT_SHADOWS

				Light mainLight = GetMainLight(input.shadowCoord); // main light
#else
				Light mainLight = GetMainLight(); // main light
#endif

				half3 color = GlobalIllumination(brdfData, bakedGI, surfaceData.occlusion, normalWS, viewDirectionWS); // global illumination
				color = color +  LightingPhysicallyBased(brdfData, mainLight, normalWS, viewDirectionWS); // lighting
				
				// Additional lights
#ifdef _ADDITIONAL_LIGHTS

				int additionalLightsCount = GetAdditionalLightsCount(); // get additional lights count
				
				// loop over additional lights
				for (int i = 0; i < additionalLightsCount; ++i)
				{
					Light light = GetAdditionalLight(i, positionWS); // get additional light
					color = color +  LightingPhysicallyBased(brdfData, light, normalWS, viewDirectionWS); // lighting
				}
#endif
				color = color + surfaceData.emission; // emission
				color = MixFog(color, input.positionWSAndFog.w); // fog

				// return color
				return half4(color, surfaceData.alpha); 
			}
			
			ENDHLSL
		}

		// PASS 2 for OutlineRenderMethod.OutlineStandard , OutlineVisible , OutlineOccluded , OutlineAndFill.
		
		Pass {
		
			Tags 
			{ 
			"LightMode" = "UniversalForward" 
			}
			
			Cull Off 
			ZTest [_ZTest2ndPass] 
			ZWrite Off 
			ColorMask 0
			
			Stencil 
			{
				Ref 1
				Pass Replace
			}
		}
		
		// PASS 3 for OutlineRenderMethod.OutlineStandard , OutlineVisible , OutlineOccluded , OutlineAndFill.
		
		Pass {
		
			Tags 
			
			{ 
			"LightMode" = "LightweightForward" 
			}
			
			Cull Off 
			ZTest [_ZTest3rdPass] 
			ZWrite Off 
			Blend SrcAlpha OneMinusSrcAlpha 
			ColorMask RGBA
			
			Stencil 
			{
				Ref 1
				Comp NotEqual
			}
			
			CGPROGRAM
			
			// -------------------------------------
			// Outline Pass Vertex
			#include "UnityCG.cginc"
			
			// vertex and fragment shader pragmas
			#pragma vertex vert
			#pragma fragment frag
			
			fixed4 _OutlineColor;
			
			// outline overlay COLOR
			fixed4 _OverlayColor;
			
			// outline parameters
			float _OutlineWidth, _OutlineAmount, _OutlineBasedVertexColorR;

			// vector to fragment
			struct v2f
			{
				float4 pos : SV_POSITION; // clip space position
				UNITY_VERTEX_OUTPUT_STEREO // stereo output
			};
			
			// vertex
			v2f vert (appdata_full v)
			{
				// vertex calculations
				v2f o; // output
				UNITY_SETUP_INSTANCE_ID(v); // setup instance id
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); // stereo output

				// exr direction , normalize and lerp
				float3 dir1 = normalize(v.vertex.xyz); // normalize
				float3 dir2 = v.normal; // normalize
				float3 dir = lerp(dir1, dir2, _OutlineAmount); // lerp
				
				dir = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, dir)); // normalize

				//  Red channel of vertex color based outline
				float outlineWidth = _OutlineWidth * v.color.r * _OutlineBasedVertexColorR + (1.0 - _OutlineBasedVertexColorR); // outline width

				float3 verterPosition = UnityObjectToViewPos(v.vertex); // vertex position

				o.pos = UnityViewToClipPos(verterPosition + dir * -verterPosition.z * outlineWidth * 0.001); // clip space position
				
				return o; // return output
			}
			
			// fragment
			half4 frag (v2f input) : SV_Target
			{	
				// make a new float 4 for semi transparent COLOR
				
				float4 transparentColor = float4(_OutlineColor.rgb, _OutlineColor.a * (_OverlayColor.a / 2) ); // color
				
				// return color
				// here we could also return the _OverlayColor if needed
				return _OutlineColor * (transparentColor); // added 20% transparency
			}
			
			ENDCG
		}
		
		// Use Passes for UPR
		UsePass "Universal Render Pipeline/Lit/ShadowCaster"
		UsePass "Universal Render Pipeline/Lit/DepthOnly"
		UsePass "Universal Render Pipeline/Lit/Meta"
	}
	
	// always Off for URP HLSL
	// if we set a fallback it will fallback always
	FallBack Off
	
}
