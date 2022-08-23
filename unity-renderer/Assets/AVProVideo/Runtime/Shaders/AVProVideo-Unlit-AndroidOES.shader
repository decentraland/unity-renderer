Shader "AVProVideo/Unlit/Opaque (texture+color+stereo support) - Android OES ONLY"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_ChromaTex("Chroma", 2D) = "gray" {}			// For fallback shader
		_Color("Main Color", Color) = (1,1,1,1)			// For fallback shader

		[KeywordEnum(None, Top_Bottom, Left_Right)] Stereo("Stereo Mode", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug("Stereo Debug Tinting", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "IgnoreProjector"="False" "Queue"="Geometry" }
		LOD 100
		Lighting Off
		Cull Off

		Pass
		{
			GLSLPROGRAM

			#pragma only_renderers gles gles3
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT STEREO_CUSTOM_UV
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USING_DEFAULT_TEXTURE
			#pragma multi_compile __ STEREO_DEBUG

			#extension GL_OES_EGL_image_external : require
			#extension GL_OES_EGL_image_external_essl3 : enable
			precision mediump float;

			#ifdef VERTEX

			#include "UnityCG.glslinc"
			#if defined(STEREO_MULTIVIEW_ON)
				UNITY_SETUP_STEREO_RENDERING
			#endif
			#define SHADERLAB_GLSL
			#include "AVProVideo.cginc"
		
			varying vec2 texVal;
			uniform vec4 _MainTex_ST;
			uniform mat4 _TextureMatrix;

#if defined(STEREO_DEBUG)
			varying vec4 tint;
#endif

			/// @fix: explicit TRANSFORM_TEX(); Unity's preprocessor chokes when attempting to use the TRANSFORM_TEX() macro in UnityCG.glslinc
			/// 	(as of Unity 4.5.0f6; issue dates back to 2011 or earlier: http://forum.unity3d.com/threads/glsl-transform_tex-and-tiling.93756/)
			vec2 transformTex(vec4 texCoord, vec4 texST) 
			{
				return (texCoord.xy * texST.xy + texST.zw);
			}

			INLINE bool Android_IsStereoEyeLeft()
			{
				#if defined(STEREO_MULTIVIEW_ON)
					int eyeIndex = SetupStereoEyeIndex();
					return (eyeIndex == 0);
				#else
					return IsStereoEyeLeft();
				#endif
			}

			void main()
			{
				gl_Position = XFormObjectToClip(gl_Vertex);
				texVal = transformTex(gl_MultiTexCoord0, _MainTex_ST);
				//texVal.x = 1.0 - texVal.x;

				// Apply texture transformation matrix - adjusts for offset/cropping (when the decoder decodes in blocks that overrun the video frame size, it pads)
				texVal.xy = (_TextureMatrix * vec4(texVal.x, texVal.y, 0.0, 1.0) ).xy;

#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
				vec4 scaleOffset = GetStereoScaleOffset(Android_IsStereoEyeLeft(), false);

				texVal.xy *= scaleOffset.xy;
				texVal.xy += scaleOffset.zw;
#endif

#if defined(STEREO_DEBUG)
				tint = GetStereoDebugTint(Android_IsStereoEyeLeft());
#endif
			}
			#endif

			#ifdef FRAGMENT

			varying vec2 texVal;

#if defined(APPLY_GAMMA)
			vec3 GammaToLinear(vec3 col)
			{
				return col * (col * (col * 0.305306011 + 0.682171111) + 0.012522878);
			}
#endif

#if defined(USING_DEFAULT_TEXTURE)
			uniform sampler2D _MainTex;
#else
			uniform samplerExternalOES _MainTex;
#endif

#if defined(STEREO_DEBUG)
			varying vec4 tint;
#endif

			void main()
			{

#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
				vec4 col = texture2D(_MainTex, texVal.xy);
#else
				vec4 col = vec4(1.0, 1.0, 0.0, 1.0);
#endif

#if defined(APPLY_GAMMA)
				col.rgb = GammaToLinear(col.rgb);
#endif

#if defined(STEREO_DEBUG)
				col *= tint;
#endif

				gl_FragColor = col;
			}
			#endif

			ENDGLSL
		}
	}
	
	Fallback "AVProVideo/Unlit/Opaque (texture+color+fog+stereo support)"
}