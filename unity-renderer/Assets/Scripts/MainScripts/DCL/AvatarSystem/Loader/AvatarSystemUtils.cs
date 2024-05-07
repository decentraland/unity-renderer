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

        public static bool IsCategoryRequired(string category)
        {
            return WearableLiterals.Categories.REQUIRED_CATEGORIES.Contains(category);
        }

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

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in targets) { CopyBones(rootBone, bones, skinnedMeshRenderer); }
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
                    // If this is modified, check DecentralandMaterialGenerator.SetMaterialName,
                    // its important for the asset bundles materials to have normalized names but this functionality should work too
                    string name = material.name.ToLower();

                    if (name.Contains("skin"))
                        material.SetColor(ShaderUtils.BaseColor, skinColor);
                    else if (name.Contains("hair"))
                        material.SetColor(ShaderUtils.BaseColor, hairColor);
                }
            }
        }

        /// <summary>
        /// Extract bodyparts of a Rendereable.
        ///
        /// Using this on a Rendereable that doesn't comes from a bodyshape might result in unexpected result
        /// </summary>
        /// <param name="rendereable"></param>
        /// <returns></returns>

        // TODO: The list of body shapes that need to be extracted should come from an external source,
        // in order to make this scalable we should return a Dictionary<string, SkinnedMeshRenderer> with the results
        public static (
            SkinnedMeshRenderer head,
            SkinnedMeshRenderer upperBody,
            SkinnedMeshRenderer lowerBody,
            SkinnedMeshRenderer feet,
            SkinnedMeshRenderer eyes,
            SkinnedMeshRenderer eyebrows,
            SkinnedMeshRenderer mouth,
            SkinnedMeshRenderer hands,
            List<SkinnedMeshRenderer> extraParts ) ExtractBodyShapeParts(Rendereable rendereable)
        {
            SkinnedMeshRenderer head = null;
            SkinnedMeshRenderer upperBody = null;
            SkinnedMeshRenderer lowerBody = null;
            SkinnedMeshRenderer feet = null;
            SkinnedMeshRenderer eyes = null;
            SkinnedMeshRenderer eyebrows = null;
            SkinnedMeshRenderer mouth = null;
            SkinnedMeshRenderer hands = null;
            var extraParts = new List<SkinnedMeshRenderer>();

            foreach (Renderer r in rendereable.renderers)
            {
                if (r is not SkinnedMeshRenderer renderer)
                    continue;

                string name = renderer.name.ToLower();

                // we still support the old gltf hierarchy for ABs
                if (name.Contains("primitive"))
                    name = renderer.transform.parent.name.ToLower();

                if (name.Contains("head"))
                    head = renderer;
                else if (name.Contains("ubody"))
                    upperBody = renderer;
                else if (name.Contains("lbody"))
                    lowerBody = renderer;
                else if (name.Contains("hands"))
                    hands = renderer;
                else if (name.Contains("feet"))
                    feet = renderer;
                else if (name.Contains("eyes"))
                    eyes = renderer;
                else if (name.Contains("eyebrows"))
                    eyebrows = renderer;
                else if (name.Contains("mouth"))
                    mouth = renderer;
                else
                {
                    Debug.LogWarning($"{name} has not been set-up as a valid body part", r);
                    extraParts.Add(renderer);
                }

            }

            return (head, upperBody, lowerBody, feet, eyes, eyebrows, mouth, hands, extraParts);
        }

        public static List<SkinnedMeshRenderer> GetActiveBodyPartsRenderers(IBodyshapeLoader bodyshapeLoader, string bodyShapeId, IEnumerable<WearableItem> wearables)
        {
            HashSet<string> hiddenList = new HashSet<string>();
            HashSet<string> usedCategories = new HashSet<string>();

            foreach (WearableItem wearable in wearables)
            {
                usedCategories.Add(wearable.data.category);
                string[] hiddenByThisWearable = wearable.GetHidesList(bodyShapeId);

                if (hiddenByThisWearable != null)
                    hiddenList.UnionWith(hiddenByThisWearable);
            }

            List<SkinnedMeshRenderer> result = new List<SkinnedMeshRenderer>();

            if (!hiddenList.Contains(WearableLiterals.Categories.HEAD) && !usedCategories.Contains(WearableLiterals.Categories.HEAD))
                result.Add(bodyshapeLoader.headRenderer);

            if (!hiddenList.Contains(WearableLiterals.Categories.UPPER_BODY) && !usedCategories.Contains(WearableLiterals.Categories.UPPER_BODY))
                result.Add(bodyshapeLoader.upperBodyRenderer);

            if (!hiddenList.Contains(WearableLiterals.Categories.LOWER_BODY) && !usedCategories.Contains(WearableLiterals.Categories.LOWER_BODY))
                result.Add(bodyshapeLoader.lowerBodyRenderer);

            if (!hiddenList.Contains(WearableLiterals.Categories.FEET) && !usedCategories.Contains(WearableLiterals.Categories.FEET))
                result.Add(bodyshapeLoader.feetRenderer);

            if (!hiddenList.Contains(WearableLiterals.Categories.HANDS) && !usedCategories.Contains(WearableLiterals.Categories.HANDS))
                result.Add(bodyshapeLoader.handsRenderer);

            // We dont want to hide new body parts that are not configured yet
            result.AddRange(bodyshapeLoader.extraRenderers);

            return result;
        }

        public static void SpawnAvatarLoadedParticles(Transform avatarContainer, GameObject particlePrefab)
        {
            if (!particlePrefab.TryGetComponent(out DestroyParticlesOnFinish _))
                throw new Exception("A self destructive particles prefab is expected");

            GameObject particles = Object.Instantiate(particlePrefab);
            particles.transform.position = avatarContainer.position + particlePrefab.transform.position;
        }
    }
}
