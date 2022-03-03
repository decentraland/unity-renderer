using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Helpers;
using UnityEngine;

public class WearableController
{
    private const string MATERIAL_FILTER_HAIR = "hair";
    private const string MATERIAL_FILTER_SKIN = "skin";
    private const string AB_FEATURE_FLAG_NAME = "wearable_asset_bundles";

    public readonly WearableItem wearable;
    protected RendereableAssetLoadHelper loader;

    public string id => wearable.id;
    public string category => wearable.data.category;

    public GameObject assetContainer => loader?.loadedAsset.container;
    public bool isReady => loader != null && loader.isFinished && assetContainer != null;

    public bool boneRetargetingDirty = false;
    internal string lastMainFileLoaded = null;
    internal bool useAssetBundles = true;

    protected SkinnedMeshRenderer[] assetRenderers;
    Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    public IReadOnlyList<SkinnedMeshRenderer> GetRenderers() { return new ReadOnlyCollection<SkinnedMeshRenderer>(assetRenderers); }

    public WearableController(WearableItem wearableItem)
    {
        wearable = wearableItem;
        SetupAssetBundlesConfig();
    }

    protected WearableController(WearableController original)
    {
        wearable = original.wearable;
        loader = original.loader;
        assetRenderers = original.assetRenderers;

        SetupAssetBundlesConfig();
    }

    private void SetupAssetBundlesConfig()
    {
        // In preview mode featureFlags.flags.Get() can be null.
        var featureFlags = DataStore.i.featureFlags.flags.Get();
        useAssetBundles = featureFlags != null && featureFlags.IsFeatureEnabled(AB_FEATURE_FLAG_NAME); 
    }

    public virtual void Load(string bodyShapeId, Transform parent, Action<WearableController> onSuccess, Action<WearableController, Exception> onFail)
    {
        if (isReady)
            return;

        boneRetargetingDirty = true;

        var representation = wearable.GetRepresentation(bodyShapeId);

        if (representation == null)
        {
            onFail?.Invoke(this, new Exception($"Wearable load fail. There is no representation for bodyShape {bodyShapeId} on wearable {wearable.id}"));
            return;
        }

        var provider = wearable.GetContentProvider(bodyShapeId);

        loader = new RendereableAssetLoadHelper(provider, wearable.baseUrlBundles);

        loader.settings.forceNewInstance = false;
        loader.settings.initialLocalPosition = Vector3.up * 0.75f;
        loader.settings.cachingFlags = MaterialCachingHelper.Mode.CACHE_SHADERS;
        loader.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE;
        loader.settings.parent = parent;
        loader.settings.layer = parent.gameObject.layer;

        assetRenderers = null;

        void OnSuccessWrapper(Rendereable rendereable)
        {
            if (loader != null)
            {
                loader.OnSuccessEvent -= OnSuccessWrapper;
            }

            assetRenderers = rendereable.container.GetComponentsInChildren<SkinnedMeshRenderer>();
            StoreOriginalMaterials();
            PrepareWearable(rendereable.container);
            onSuccess?.Invoke(this);
        }

        loader.OnSuccessEvent += OnSuccessWrapper;

        void OnFailEventWrapper(Exception error)
        {
            if (loader != null)
            {
                loader.OnFailEvent -= OnFailEventWrapper;
                loader.ClearEvents();
                lastMainFileLoaded = null;
                loader = null;
            }

            onFail?.Invoke(this, error);
        }

        loader.OnFailEvent += OnFailEventWrapper;

        lastMainFileLoaded = representation.mainFile;

        loader.Load(representation.mainFile, useAssetBundles ? RendereableAssetLoadHelper.LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK : RendereableAssetLoadHelper.LoadingType.GLTF_ONLY );
    }

    public void SetupHairAndSkinColors(Color skinColor, Color hairColor)
    {
        if (assetContainer == null)
            return;

        AvatarUtils.SetColorInHierarchy(assetContainer.transform, MATERIAL_FILTER_SKIN, skinColor, ShaderUtils.BaseColor);
        AvatarUtils.SetColorInHierarchy(assetContainer.transform, MATERIAL_FILTER_HAIR, hairColor, ShaderUtils.BaseColor);
    }

    public void SetAnimatorBones(Transform[] bones, Transform rootBone)
    {
        if (!boneRetargetingDirty || assetContainer == null)
            return;

        SkinnedMeshRenderer[] skinnedRenderers = assetContainer.GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < skinnedRenderers.Length; i++)
        {
            skinnedRenderers[i].rootBone = rootBone;
            skinnedRenderers[i].bones = bones;
        }

        boneRetargetingDirty = false;
    }

    public virtual void CleanUp()
    {
        RestoreOriginalMaterials();
        assetRenderers = null;

        if (loader != null)
        {
            loader.ClearEvents();
            loader.Unload();
            loader = null;
            lastMainFileLoaded = null;
        }
    }

    public void SetAssetRenderersEnabled(bool active)
    {
        for (var i = 0; i < assetRenderers.Length; i++)
        {
            if (assetRenderers[i] != null)
                assetRenderers[i].enabled = active;
        }
    }

    protected virtual void PrepareWearable(GameObject assetContainer) { }

    public virtual void UpdateVisibility(HashSet<string> hiddenList) { SetAssetRenderersEnabled(!hiddenList.Contains(wearable.data.category)); }

    public bool IsLoadedForBodyShape(string bodyShapeId)
    {
        if (loader == null || !isReady || lastMainFileLoaded == null)
            return false;

        return wearable.data.representations.FirstOrDefault(x => x.bodyShapes.Contains(bodyShapeId))?.mainFile == lastMainFileLoaded;
    }

    private void StoreOriginalMaterials()
    {
        for (int i = 0; i < assetRenderers.Length; i++)
        {
            if (originalMaterials.ContainsKey(assetRenderers[i]))
                continue;

            originalMaterials.Add(assetRenderers[i], assetRenderers[i].sharedMaterials.ToArray());
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
                kvp.Key.materials = kvp.Value;
        }

        originalMaterials.Clear();
    }

    public void SetFadeDither(float ditherFade)
    {
        if (assetRenderers == null)
            return;

        for (int i = 0; i < assetRenderers.Length; i++)
        {
            for (int j = 0; j < assetRenderers[i].materials.Length; j++)
            {
                assetRenderers[i].materials[j].SetFloat(ShaderUtils.DitherFade, ditherFade);
            }
        }
    }
}