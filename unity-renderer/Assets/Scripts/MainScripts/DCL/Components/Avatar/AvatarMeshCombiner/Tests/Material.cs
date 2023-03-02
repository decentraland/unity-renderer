using DCL.Helpers;
using DCL.Shaders;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL.Helpers
{
    internal static class Material
    {
        public static UnityEngine.Material Create(CullMode cullMode = CullMode.Back, Texture2D albedo = null, Texture2D emission = null)
        {
            UnityEngine.Material mat = new UnityEngine.Material(Shader.Find("DCL/Universal Render Pipeline/Lit"));
            mat.SetTexture(ShaderUtils.BaseMap, albedo);
            mat.SetTexture(ShaderUtils.EmissionMap, emission);
            mat.SetInt(ShaderUtils.Cull, (int)cullMode);
            return mat;
        }

        public static UnityEngine.Material CreateOpaque(CullMode cullMode = CullMode.Back, Texture2D albedo = null, Texture2D emission = null)
        {
            UnityEngine.Material result = Create(cullMode, albedo, emission);
            MaterialUtils.SetOpaque(result);
            return result;
        }

        public static UnityEngine.Material CreateTransparent(CullMode cullMode = CullMode.Back, Texture2D albedo = null, Texture2D emission = null)
        {
            UnityEngine.Material result = Create(cullMode, albedo, emission);
            MaterialUtils.SetTransparent(result);
            return result;
        }
    }
}