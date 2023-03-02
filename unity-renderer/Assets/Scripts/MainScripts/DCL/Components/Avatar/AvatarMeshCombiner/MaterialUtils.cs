using DCL.Helpers;
using DCL.Shaders;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL.Helpers
{
    public static class MaterialUtils
    {
        public static void SetOpaque(Material material)
        {
            material.SetInt(ShaderUtils.SrcBlend, (int)BlendMode.One);
            material.SetInt(ShaderUtils.DstBlend, (int)BlendMode.Zero);
            material.SetInt(ShaderUtils.Surface, 0);
            material.SetFloat(ShaderUtils.ZWrite, 1);
            material.EnableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetOverrideTag("RenderType", "TransparentCutout");
        }

        public static void SetTransparent(Material material)
        {
            material.SetInt(ShaderUtils.SrcBlend, (int)BlendMode.SrcAlpha);
            material.SetInt(ShaderUtils.DstBlend, (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt(ShaderUtils.Surface, 1);
            material.SetFloat(ShaderUtils.ZWrite, 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetOverrideTag("RenderType", "Transparent");
        }
    }
}