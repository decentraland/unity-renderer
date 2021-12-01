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

        public static (WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables) SplitWearables(IEnumerable<WearableItem> wearables)
        {
            WearableItem bodyshape = null;
            WearableItem eyes = null;
            WearableItem eyebrows = null;
            WearableItem mouth = null;
            List<WearableItem> resultWearables = new List<WearableItem>();
            foreach (WearableItem wearable in wearables)
            {
                switch (wearable.data.category)
                {
                    case WearableLiterals.Categories.BODY_SHAPE:
                        bodyshape = wearable;
                        break;
                    case WearableLiterals.Categories.EYES:
                        eyes = wearable;
                        break;
                    case WearableLiterals.Categories.EYEBROWS:
                        eyebrows = wearable;
                        break;
                    case WearableLiterals.Categories.MOUTH:
                        mouth = wearable;
                        break;
                    default:
                        resultWearables.Add(wearable);
                        break;
                }
            }
            return (bodyshape, eyes, eyebrows, mouth, resultWearables);
        }

        /// <summary>
        /// Extract bodyparts of a Rendereable.
        ///
        /// Using this on a Rendereable that doesn't comes from a bodyshape might result in unexpected result
        /// </summary>
        /// <param name="rendereable"></param>
        /// <returns></returns>
        public static (
            SkinnedMeshRenderer head,
            SkinnedMeshRenderer upperBody,
            SkinnedMeshRenderer lowerBody,
            SkinnedMeshRenderer feet,
            SkinnedMeshRenderer eyes,
            SkinnedMeshRenderer eyebrows,
            SkinnedMeshRenderer mouth
            ) ExtractBodyshapeParts(Rendereable rendereable)
        {
            SkinnedMeshRenderer head = null;
            SkinnedMeshRenderer upperBody = null;
            SkinnedMeshRenderer lowerBody = null;
            SkinnedMeshRenderer feet = null;
            SkinnedMeshRenderer eyes = null;
            SkinnedMeshRenderer eyebrows = null;
            SkinnedMeshRenderer mouth = null;
            for (int i = 0; i < rendereable.renderers.Count; i++)
            {
                if (!(rendereable.renderers[i] is SkinnedMeshRenderer renderer))
                    continue;

                string parentName = renderer.transform.parent.name.ToLower();

                if (parentName.Contains("head"))
                    head = renderer;
                else if (parentName.Contains("ubody"))
                    upperBody = renderer;
                else if (parentName.Contains("lbody"))
                    lowerBody = renderer;
                else if (parentName.Contains("feet"))
                    feet = renderer;
                else if (parentName.Contains("eyes"))
                    eyes = renderer;
                else if (parentName.Contains("eyebrows"))
                    eyebrows = renderer;
                else if (parentName.Contains("mouth"))
                    mouth = renderer;
            }
            return (head, upperBody, lowerBody, feet, eyes, eyebrows, mouth);
        }
    }
}