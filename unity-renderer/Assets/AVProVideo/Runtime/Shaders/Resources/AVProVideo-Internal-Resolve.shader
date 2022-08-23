Shader "AVProVideo/Internal/Resolve"
{
	Properties
	{
		_MainTex("Texture", any) = "" {}
		_ChromaTex("Chroma", any) = "" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_VertScale("Vertical Scale", Range(-1, 1)) = 1.0

		[Toggle(USE_HSBC)] _UseHSBC("Use HSBC", Float) = 0
		_Hue("Hue", Range(0, 1.0)) = 0
		_Saturation("Saturation", Range(0, 1.0)) = 0.5
		_Brightness("Brightness", Range(0, 1.0)) = 0.5
		_Contrast("Contrast", Range(0, 1.0)) = 0.5
		_InvGamma("InvGamma", Range(0.0001, 10000.0)) = 1.0

		[KeywordEnum(None, Top_Bottom, Left_Right)] Stereo("Stereo Mode", Float) = 0
		[KeywordEnum(None, Left, Right)] ForceEye ("Force Eye Mode", Float) = 0		
		[KeywordEnum(None, Top_Bottom, Left_Right)] AlphaPack("Alpha Pack", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
		[Toggle(USE_YPCBCR)] _UseYpCbCr("Use YpCbCr", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"IgnoreProjector"="True" 
			"PreviewType"="Plane"
		}

		Lighting Off
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			Name "RESOLVE-OES"

			GLSLPROGRAM
			#pragma only_renderers gles gles3

			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT
			#pragma multi_compile ALPHAPACK_NONE ALPHAPACK_TOP_BOTTOM ALPHAPACK_LEFT_RIGHT
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USE_HSBC

			#extension GL_OES_EGL_image_external : require
			#extension GL_OES_EGL_image_external_essl3 : enable

			#include "UnityCG.glslinc"
		#if defined(STEREO_MULTIVIEW_ON)
			UNITY_SETUP_STEREO_RENDERING
		#endif
			#define SHADERLAB_GLSL
			#include "../AVProVideo.cginc"

			#ifdef VERTEX

			varying vec4 varTexCoord;
			varying vec4 varColor;

			uniform vec4 _Color;
			uniform vec4 _MainTex_ST;
			uniform vec4 _MainTex_TexelSize;
			uniform mat4 _TextureMatrix;
			uniform float _VertScale;

			INLINE bool Android_IsStereoEyeLeft()
			{
				#if defined(STEREO_MULTIVIEW_ON)
					int eyeIndex = SetupStereoEyeIndex();
					return (eyeIndex == 0);
				#else
					return IsStereoEyeLeft();
				#endif
			}

			vec2 transformTex(vec4 texCoord, vec4 texST) 
			{
				return (texCoord.xy * texST.xy + texST.zw);
			}

			void main()
			{
				gl_Position = XFormObjectToClip(gl_Vertex);
				varColor = gl_Color * _Color;

				varTexCoord.xy = transformTex(gl_MultiTexCoord0, _MainTex_ST);
				varTexCoord.zw = vec2(0.0, 0.0);

				// Apply texture transformation matrix - adjusts for offset/cropping (when the decoder decodes in blocks that overrun the video frame size, it pads)
				varTexCoord.xy = (_TextureMatrix * vec4(varTexCoord.x, varTexCoord.y, 0.0, 1.0)).xy;

			#if defined(STEREO_TOP_BOTTOM) || defined(STEREO_LEFT_RIGHT)
				vec4 scaleOffset = GetStereoScaleOffset(Android_IsStereoEyeLeft(), false);
				varTexCoord.xy *= scaleOffset.xy;
				varTexCoord.xy += scaleOffset.zw;
			#endif

			#if defined (ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
				varTexCoord = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, varTexCoord.xy, false);
				#if defined(ALPHAPACK_TOP_BOTTOM)
				varTexCoord.yw = varTexCoord.wy;
				#endif
			#endif
			}

			#endif

			#ifdef FRAGMENT

			varying vec4 varTexCoord;
			varying vec4 varColor;

			uniform samplerExternalOES _MainTex;
		#if defined(USE_HSBC)
			uniform	float _Hue, _Saturation, _Brightness, _Contrast, _InvGamma;
		#endif

			void main()
			{
				vec4 col = TEX_EXTERNAL(_MainTex, varTexCoord.xy);
			#if defined(APPLY_GAMMA)
				col.rgb = GammaToLinear(col.rgb);
			#endif
				
			#if defined(ALPHAPACK_TOP_BOTTOM) || defined(ALPHAPACK_LEFT_RIGHT)
				vec4 colAlpha = TEX_EXTERNAL(_MainTex, varTexCoord.zw);
				col.a = (colAlpha.r + colAlpha.g + colAlpha.b) / 3.0;
			#endif

			#if defined(USE_HSBC)
				col.rgb = ApplyHSBEffect(col.rgb, vec4(_Hue, _Saturation, _Brightness, _Contrast));
				col.rgb = pow(col.rgb, vec3(_InvGamma));
			#endif
			
				gl_FragColor = col * varColor;
			}

			#endif

			ENDGLSL
		}
	}

	SubShader
	{
		Tags
		{ 
			"IgnoreProjector"="True" 
			"PreviewType"="Plane"
		}

		Lighting Off
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			Name "RESOLVE"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// TODO: replace use multi_compile_local instead (Unity 2019.1 feature)
			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT
			#pragma multi_compile ALPHAPACK_NONE ALPHAPACK_TOP_BOTTOM ALPHAPACK_LEFT_RIGHT
			#pragma multi_compile __ APPLY_GAMMA
			#pragma multi_compile __ USE_YPCBCR
			#pragma multi_compile __ USE_HSBC
	
			#include "UnityCG.cginc"
			#include "../AVProVideo.cginc"

			struct appdata_t 
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float4 uv : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
		#if USE_YPCBCR
			uniform sampler2D _ChromaTex;
			uniform float4x4 _YpCbCrTransform;
		#endif
		#if USE_HSBC
			uniform	fixed _Hue, _Saturation, _Brightness, _Contrast, _InvGamma;
		#endif
			uniform fixed4 _Color;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform float _VertScale;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = XFormObjectToClip(v.vertex);
				o.color = v.color * _Color;
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv.wz = 0.0;

		#if STEREO_TOP_BOTTOM || STEREO_LEFT_RIGHT
				float4 scaleOffset = GetStereoScaleOffset(IsStereoEyeLeft(), _MainTex_ST.y < 0.0);

				o.uv.xy *= scaleOffset.xy;
				o.uv.xy += scaleOffset.zw;
		#endif

				// NOTE: this always runs because it's also used to flip vertically
				o.uv = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, o.uv.xy, _VertScale < 0.0);

				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half4 col;
		#if USE_YPCBCR
				col = SampleYpCbCr(_MainTex, _ChromaTex, i.uv.xy, _YpCbCrTransform);
		#else
				col = SampleRGBA(_MainTex, i.uv.xy);
		#endif

		#if ALPHAPACK_TOP_BOTTOM || ALPHAPACK_LEFT_RIGHT
				col.a = SamplePackedAlpha(_MainTex, i.uv.zw);
		#endif

		#if USE_HSBC
				col.rgb = ApplyHSBEffect(col.rgb, fixed4(_Hue, _Saturation, _Brightness, _Contrast));
				col.rgb = pow(col.rgb, _InvGamma);
		#endif
				return col * i.color;
			}
			ENDCG
		}
	}

	Fallback off
}