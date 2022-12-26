Shader "BlurRT/Objects/Unlit/UI/BlurUIImageElement"
{
    Properties
    {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

        // hide to ensure that the parameters are not accidentally used
		[HideInInspector] _StencilComp("Stencil Layer Comp", Float) = 8
		[HideInInspector] _Stencil("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "False"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil // stencil internal reference
        
		{
			Ref[_Stencil] // 8
			Comp[_StencilComp] // Always 8
			Pass[_StencilOp]  // Replace 0
			ReadMask[_StencilReadMask] // 255 // 255
			WriteMask[_StencilWriteMask] // 255
		}

		
		ZWrite Off
		ZTest[unity_GUIZTestMode] // Always 4 Test mode
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Name "Default"
			
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0 // OpenGL ES 2.0 for runtime specific to DCL when trying to compile in CGPROGRAM

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT // UI clip for images
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP // UI alpha clip for images now we can remove the mask with ragged edges

            // component data
			struct appdata_t
			{
				float4 vertex    : POSITION; // vertex position
				float4 color     : COLOR;
				float2 texcoord  : TEXCOORD0; // texture coordinates for vertex through interpolator
				float4 screenPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID // instance id
			};
		    
		    // vertex to fragment shader // interpolators
			struct v2f
			{
				float4 vertex : SV_POSITION; // vertex position
				fixed4 color : COLOR; // color
			    
				float2 texcoord : TEXCOORD0; // texture coordinates for vertex through interpolator
				float4 worldPosition : TEXCOORD1; // world position
				float4 screenPos : TEXCOORD2;
			    
				UNITY_VERTEX_OUTPUT_STEREO // stereo
			};

			sampler2D _MainTex;
			sampler2D _blurTexture;
			
			fixed4 _Color; 
			fixed4 _TextureSampleAdd; // add color to texture sample
			float4 _ClipRect; // clip rect for edges of image and overall alpha clip
			float4 _MainTex_ST;

            // vertex shader
			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
			    
				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition); // vertex position
				OUT.screenPos = ComputeScreenPos(OUT.vertex); // screen position

				OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex); // texture coordinates for vertex through interpolator to avoid the flipped image

				OUT.color = v.color * _Color;
				
				return OUT;
			}

		    // fragment shader
			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

				float2 screenUV = IN.screenPos.xy / IN.screenPos.w; // screen position
				color.rgb *= tex2D(_blurTexture, screenUV).rgb; // blur texture sample on top of screen uvs 
				
				// UI clips
                // alpha clip
			    // must be included #if UNITY_UI_CLIP_RECT
				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
				clip(color.a - 0.001);
				#endif

				return color;
			}
		
		ENDCG
		    
        }
    }
    
    // try with fallback
    FallBack "Diffuse"
		    
}
    