using System.Collections.Generic;
using System.Linq;
using DCL;
using UnityEngine;

namespace AvatarSystem
{
    public static class AvatarSystemUtils
    {
        private const string AB_FEATURE_FLAG_NAME = "wearable_asset_bundles";
        public static int _BaseColor = Shader.PropertyToID("_BaseColor");
        public static int _EmissionColor = Shader.PropertyToID("_EmissionColor");
        public static int _BaseMap = Shader.PropertyToID("_BaseMap");
        public static int _EyesTexture = Shader.PropertyToID("_EyesTexture");
        public static int _EyeTint = Shader.PropertyToID("_EyeTint");
        public static int _IrisMask = Shader.PropertyToID("_IrisMask");
        public static int _TintMask = Shader.PropertyToID("_TintMask");

        public static bool IsCategoryRequired(string category) { return true; }
        public static bool UseAssetBundles()
        {
            var featureFlags = DataStore.i.featureFlags.flags.Get();
            return featureFlags != null && featureFlags.IsFeatureEnabled(AB_FEATURE_FLAG_NAME);
        }

        public static (string mainTextureUrl, string maskTextureUrl) GetFacialFeatureTexturesUrls(string bodyshapeId, WearableItem facialFeature)
        {
            if (facialFeature.data.category != WearableLiterals.Categories.EYES && facialFeature.data.category == WearableLiterals.Categories.EYEBROWS && facialFeature.data.category == WearableLiterals.Categories.MOUTH)
                return (null, null);

            var representation = facialFeature.GetRepresentation(bodyshapeId);
            string mainTextureHash = representation?.contents?.FirstOrDefault(x => x.key == representation?.mainFile)?.hash;
            if (string.IsNullOrEmpty(mainTextureHash))
                mainTextureHash = representation?.contents?.FirstOrDefault(x => !x.key.ToLower().Contains("_mask.png"))?.hash;
            if (string.IsNullOrEmpty(mainTextureHash))
                return (null, null);

            string maskTextureHash = representation?.contents?.FirstOrDefault(x => x.key.ToLower().Contains("_mask.png"))?.hash;

            string mainTextureUrl = facialFeature.baseUrl + mainTextureHash;
            string maskTextureUrl = maskTextureHash == null ? null : facialFeature.baseUrl + maskTextureHash;

            return (mainTextureUrl, maskTextureUrl);
        }

        public static void CopyBones(SkinnedMeshRenderer source, IEnumerable<SkinnedMeshRenderer> targets)
        {
            if (source == null)
                return;

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in targets)
            {
                skinnedMeshRenderer.rootBone = source.rootBone;
                skinnedMeshRenderer.bones = source.bones;
            }
        }

        public static void PrepareMaterialColors(Rendereable rendereable, Color skinColor, Color hairColor)
        {
            foreach (Renderer renderer in rendereable.renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.ToLower().Contains("skin"))
                        material.SetColor(_BaseColor, skinColor);
                    else if (material.name.ToLower().Contains("hair"))
                        material.SetColor(_BaseColor, hairColor);
                }
            }
        }
    }
}