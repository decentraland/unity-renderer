using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Shaders;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AvatarSystem
{
    public static class AvatarSystemUtils
    {
        public const float AVATAR_Y_OFFSET = 0.75f;
        private const string AB_FEATURE_FLAG_NAME = "wearable_asset_bundles";

        public static bool IsCategoryRequired(string category) { return WearableLiterals.Categories.REQUIRED_CATEGORIES.Contains(category); }

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

        public static void CopyBones(Transform rootBone, Transform[] bones, IEnumerable<SkinnedMeshRenderer> targets)
        {
            if (rootBone == null || bones == null)
                return;

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in targets)
            {
                CopyBones(rootBone, bones, skinnedMeshRenderer);
            }
        }

        public static void CopyBones(Transform rootBone, Transform[] bones, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if (rootBone == null || bones == null || skinnedMeshRenderer == null)
                return;

            skinnedMeshRenderer.rootBone = rootBone;
            skinnedMeshRenderer.bones = bones;
        }

        public static void PrepareMaterialColors(Rendereable rendereable, Color skinColor, Color hairColor)
        {
            foreach (Renderer renderer in rendereable.renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.ToLower().Contains("skin"))
                        material.SetColor(ShaderUtils.BaseColor, skinColor);
                    else if (material.name.ToLower().Contains("hair"))
                        material.SetColor(ShaderUtils.BaseColor, hairColor);
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

            foreach (Renderer r in rendereable.renderers)
            {
                if (!(r is SkinnedMeshRenderer renderer))
                    continue;

                string name = "";

                name = renderer.name.ToLower();

                // we still support the old gltf hierarchy for ABs
                if (name.Contains("primitive"))
                    name = renderer.transform.parent.name.ToLower();

                if (name.Contains("head"))
                    head = renderer;
                else if (name.Contains("ubody"))
                    upperBody = renderer;
                else if (name.Contains("lbody"))
                    lowerBody = renderer;
                else if (name.Contains("feet"))
                    feet = renderer;
                else if (name.Contains("eyes"))
                    eyes = renderer;
                else if (name.Contains("eyebrows"))
                    eyebrows = renderer;
                else if (name.Contains("mouth"))
                    mouth = renderer;
                else
                    Debug.LogWarning($"{name} is not a body part?", r);
            }

            return (head, upperBody, lowerBody, feet, eyes, eyebrows, mouth);
        }

        /// <summary>
        /// Filters hidden wearables.
        /// Conflicts will be resolved by order in the array
        /// </summary>
        /// <param name="bodyshapeId"></param>
        /// <param name="wearables"></param>
        /// <returns></returns>
        public static List<WearableItem> FilterHiddenWearables(string bodyshapeId, IEnumerable<WearableItem> wearables)
        {
            HashSet<string> hiddenCategories = new HashSet<string>();
            List<WearableItem> filteredWearables = new List<WearableItem>();
            foreach (WearableItem wearable in wearables)
            {
                if (hiddenCategories.Contains(wearable.data.category))
                {
                    continue;
                }

                filteredWearables.Add(wearable);
                hiddenCategories.UnionWith(wearable.GetHidesList(bodyshapeId));
            }

            return filteredWearables;
        }

        public static (bool headVisible, bool upperBodyVisible, bool lowerBodyVisible, bool feetVisible) GetActiveBodyParts(string bodyshapeId, IEnumerable<WearableItem> wearables)
        {
            bool headVisible = true;
            bool upperBodyVisible = true;
            bool lowerBodyVisible = true;
            bool feetVisible = true;

            HashSet<string> hiddenList = new HashSet<string>();
            HashSet<string> usedCategories = new HashSet<string>();

            foreach (WearableItem wearable in wearables)
            {
                usedCategories.Add(wearable.data.category);
                string[] hiddenByThisWearable = wearable.GetHidesList(bodyshapeId);
                if (hiddenByThisWearable != null)
                    hiddenList.UnionWith(hiddenByThisWearable);
            }

            headVisible = !hiddenList.Contains(WearableLiterals.Misc.HEAD) && !usedCategories.Contains(WearableLiterals.Misc.HEAD);
            upperBodyVisible = !hiddenList.Contains(WearableLiterals.Categories.UPPER_BODY) && !usedCategories.Contains(WearableLiterals.Categories.UPPER_BODY);
            lowerBodyVisible = !hiddenList.Contains(WearableLiterals.Categories.LOWER_BODY) && !usedCategories.Contains(WearableLiterals.Categories.LOWER_BODY);
            feetVisible = !hiddenList.Contains(WearableLiterals.Categories.FEET) && !usedCategories.Contains(WearableLiterals.Categories.FEET);
            return (headVisible, upperBodyVisible, lowerBodyVisible, feetVisible);
        }

        public static List<SkinnedMeshRenderer> GetActiveBodyPartsRenderers(IBodyshapeLoader bodyshapeLoader, bool headVisible, bool upperBodyVisible, bool lowerBodyVisible, bool feetVisible)
        {
            List<SkinnedMeshRenderer> result = new List<SkinnedMeshRenderer>();
            if (headVisible)
                result.Add(bodyshapeLoader.headRenderer);
            if (upperBodyVisible)
                result.Add(bodyshapeLoader.upperBodyRenderer);
            if (lowerBodyVisible)
                result.Add(bodyshapeLoader.lowerBodyRenderer);
            if (feetVisible)
                result.Add(bodyshapeLoader.feetRenderer);
            return result;
        }

        public static void SpawnAvatarLoadedParticles(Transform avatarContainer, GameObject particlePrefab)
        {
            if (!particlePrefab.TryGetComponent(out DestroyParticlesOnFinish selfDestroyScript))
                throw new Exception("A self destructive particles prefab is expected");

            GameObject particles = Object.Instantiate(particlePrefab);
            particles.transform.position = avatarContainer.position + particlePrefab.transform.position;
        }
    }
}
