

Shader "HLSLCustomShaders/CG/URP/Outlines/Outline Fill" {
  
  Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0

    _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
    _OutlineFillColor("Outline Fill Color", Color) = (1, 1, 1, 1)
    _OutlineWidth("Outline Width", Range(0, 10)) = 2
  }

  SubShader {
    Tags {
      "Queue" = "Transparent+110"
      "RenderType" = "Transparent"
      "DisableBatching" = "True"
    }

    Pass {
      Name "Fill"
      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
      ColorMask RGB

      Stencil {
        Ref 1
        Comp NotEqual
      }

      CGPROGRAM
      #include "UnityCG.cginc"

      #pragma vertex vert
      #pragma fragment frag

      // interpolators
      struct appdata {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float3 smoothNormal : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f {
        float4 position : SV_POSITION; // vertex position
        fixed4 color : COLOR; // vertex color
        UNITY_VERTEX_OUTPUT_STEREO  // stereo
      };

      uniform fixed4 _OutlineColor;
      uniform fixed4 _OutlineFillColor;
      uniform float _OutlineWidth;

      v2f vert(appdata input) {
        v2f output;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        // Transform vertex position
        float3 normal = any(input.smoothNormal) ? input.smoothNormal : input.normal;
        // view direction
        float3 viewPosition = UnityObjectToViewPos(input.vertex);
        // view Normal
        float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));

        // position
        output.position = UnityViewToClipPos(viewPosition + viewNormal * -viewPosition.z * (_OutlineWidth / 1000.0));

        // color
        //output.color = _OutlineColor;
        output.color = _OutlineFillColor;
        //output.color = lerp(_OutlineColor, _OutlineFillColor, 0.5);

        return output;
      }
      
      // Fragment shader
      fixed4 frag(v2f input) : SV_Target {
        
        return input.color;
        

        //return lerp(input.color, _OutlineFillColor, 0.5);
      }
      ENDCG
    }
  }
}
