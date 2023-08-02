using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MainScripts.DCL.Components.Avatar.VRMExporter
{
    public static class VRMExporterUtils
    {
        private static readonly int _Color = Shader.PropertyToID("_Color");
        private static readonly int _BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int _ShadeTexture = Shader.PropertyToID("_ShadeTexture");
        private static readonly int _ShadeColor = Shader.PropertyToID("_ShadeColor");
        private static readonly int _CullMode = Shader.PropertyToID("_CullMode");
        private static readonly int _Cull = Shader.PropertyToID("_Cull");
        private static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int _EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int _BlendMode = Shader.PropertyToID("_BlendMode");
        private static readonly int _SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int _DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int _ZWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int _Cutoff = Shader.PropertyToID("_Cutoff");
        private static readonly int _AlphaToMask = Shader.PropertyToID("_AlphaToMask");
        private static readonly int _EyesTexture = Shader.PropertyToID("_EyesTexture");
        private static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
        private const string _ALPHATEST_ON = "_ALPHATEST_ON";
        private const string _ALPHABLEND_ON = "_ALPHABLEND_ON";
        private const string _ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";
        private const string RenderType = "RenderType";
        private const string Opaque = "Opaque";
        private const string TransparentCutout = "TransparentCutout";
        private const string Transparent = "Transparent";

        public static Dictionary<string, Transform> CacheFBXBones(Transform root)
        {
            void GatherBonesRecursively(Transform current, Dictionary<string, Transform> bones)
            {
                bones.Add(current.name, current);

                foreach (Transform child in current)
                    GatherBonesRecursively(child, bones);
            }

            var bones = new Dictionary<string, Transform>();
            GatherBonesRecursively(root, bones);
            return bones;
        }

        public static SkinnedMeshRenderer CloneSmr(GameObject container, SkinnedMeshRenderer source)
        {
            var smr = container.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = source.sharedMesh;
            smr.quality = source.quality;
            smr.updateWhenOffscreen = source.updateWhenOffscreen;
            return smr;
        }

        public static void ConvertEyesMaterial(Material fromMaterial, Texture mainTex, Material outMaterial)
        {
            if (fromMaterial == null)
                return;

            outMaterial.name = fromMaterial.name;

            // Set main textures and colors
            outMaterial.mainTexture = mainTex;
            outMaterial.SetFloat(_Cutoff, outMaterial.GetFloat(_Cutoff));

            // Set render queue and blend mode
            outMaterial.renderQueue = fromMaterial.renderQueue;
            SetBlendMode(outMaterial, 1);
        }

        public static void ConvertLitMaterial(Material fromMaterial, Material outMaterial)
        {
            const int EMISSIVE_FACTOR = 20;

            if (fromMaterial == null)
                return;

            var mainTex = fromMaterial.mainTexture;

            outMaterial.name = fromMaterial.name;

            // Set main textures and colors
            outMaterial.mainTexture = mainTex;
            outMaterial.SetTexture(_ShadeTexture, mainTex);

            var baseColor = fromMaterial.GetColor(_BaseColor);
            outMaterial.SetColor(_Color, baseColor);
            outMaterial.SetColor(_ShadeColor, baseColor);

            outMaterial.SetColor(_EmissionColor, fromMaterial.GetColor(_EmissionColor) * EMISSIVE_FACTOR);
            outMaterial.SetFloat(_CullMode, fromMaterial.GetFloat(_Cull));
            if(fromMaterial.HasProperty(_EmissionMap))
                outMaterial.SetTexture(_EmissionMap, fromMaterial.GetTexture(_EmissionMap));

            // Set render queue and blend mode
            outMaterial.renderQueue = fromMaterial.renderQueue;
            if (fromMaterial.renderQueue <= 2450)
                SetBlendMode(outMaterial, 0);
            else if (fromMaterial.renderQueue < 3000)
                SetBlendMode(outMaterial, 1);
            else
                SetBlendMode(outMaterial, 2);
        }

        private static void SetBlendMode(Material material, int value)
        {
            if (value == 0)
            {
                material.SetFloat(_BlendMode, 0);

                material.SetOverrideTag(RenderType, Opaque);
                material.SetInt(_SrcBlend, (int)BlendMode.One);
                material.SetInt(_DstBlend, (int)BlendMode.Zero);
                material.SetInt(_ZWrite, 1);
                material.SetInt(_AlphaToMask, 0);
                material.DisableKeyword(_ALPHATEST_ON);
                material.DisableKeyword(_ALPHABLEND_ON);
                material.DisableKeyword(_ALPHAPREMULTIPLY_ON);
            }
            else if (value == 1)
            {
                material.SetFloat(_BlendMode, 1);

                material.SetOverrideTag(RenderType, TransparentCutout);
                material.SetInt(_SrcBlend, (int)BlendMode.One);
                material.SetInt(_DstBlend, (int)BlendMode.Zero);
                material.SetInt(_ZWrite, 1);
                material.SetInt(_AlphaToMask, 1);
                material.EnableKeyword(_ALPHATEST_ON);
                material.DisableKeyword(_ALPHABLEND_ON);
                material.DisableKeyword(_ALPHAPREMULTIPLY_ON);
            }
            else
            {
                material.SetFloat(_BlendMode, 2);

                material.SetOverrideTag(RenderType, Transparent);
                material.SetInt(_SrcBlend, (int)BlendMode.SrcAlpha);
                material.SetInt(_DstBlend, (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt(_ZWrite, 0);
                material.SetInt(_AlphaToMask, 0);
                material.DisableKeyword(_ALPHATEST_ON);
                material.EnableKeyword(_ALPHABLEND_ON);
                material.DisableKeyword(_ALPHAPREMULTIPLY_ON);
            }
        }

        public static Texture2D ExtractComposedEyesTexture(Material source) => ExtractComposedTexture(source, source.GetTexture(_EyesTexture));
        public static Texture2D ExtractComposedBaseMapTexture(Material source) => ExtractComposedTexture(source, source.GetTexture(_BaseMap));

        private static Texture2D ExtractComposedTexture(Material source, Texture sourceMainTex)
        {
            float cutoff = source.GetFloat(_Cutoff);
            source.SetFloat(_Cutoff, 0.0f);
            var rt = RenderTexture.GetTemporary(sourceMainTex.width, sourceMainTex.height);
            Graphics.Blit(sourceMainTex, rt, source);
            source.SetFloat(_Cutoff, cutoff);

            Texture2D tex = new Texture2D(sourceMainTex.width, sourceMainTex.height);
            tex.ReadPixels(new Rect(0,0, tex.width, tex.height), 0, 0);
            tex.Apply();
            return tex;
        }
    }
}
