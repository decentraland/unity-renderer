using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Components;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public static class AvatarSystemUtils
{
    private const string AB_FEATURE_FLAG_NAME = "wearable_asset_bundles";

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

        string maskhash = representation?.contents?.FirstOrDefault(x => x.key.ToLower().Contains("_mask.png"))?.hash;

        string mainTextureUrl = facialFeature.baseUrl + mainTextureHash;
        string maskTextureUrl = maskhash == null ? null : facialFeature.baseUrl + maskhash;

        return (mainTextureUrl, maskTextureUrl);
    }

    public static void CopyBones(SkinnedMeshRenderer source, IEnumerable<SkinnedMeshRenderer> targets)
    {
        if (source == null)
            return;

        //Debug.Log($"Source: {source.transform.GetHierarchyPath()}");

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in targets)
        {
            //Debug.Log($"Target: {skinnedMeshRenderer.transform.GetHierarchyPath()}");
            skinnedMeshRenderer.rootBone = source.rootBone;
            skinnedMeshRenderer.bones = source.bones;
        }
    }
}