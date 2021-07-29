using DCL;
using DCL.Components;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityGLTF.Cache;
using Object = UnityEngine.Object;

public class WearableController
{
    private const string MATERIAL_FILTER_HAIR = "hair";
    private const string MATERIAL_FILTER_SKIN = "skin";

    public readonly WearableItem wearable;
    protected RendereableAssetLoadHelper loader;

    public string id => wearable.id;
    public string category => wearable.data.category;

    public GameObject assetContainer => loader?.loadedAsset;
    public bool isReady => loader != null && loader.isFinished && assetContainer != null;

    protected SkinnedMeshRenderer[] assetRenderers;

    public bool boneRetargetingDirty = false;

    internal string lastMainFileLoaded = null;

    public IReadOnlyList<SkinnedMeshRenderer> GetRenderers()
    {
        return new ReadOnlyCollection<SkinnedMeshRenderer>(assetRenderers);
    }

    public WearableController(WearableItem wearableItem) { this.wearable = wearableItem; }

    protected WearableController(WearableController original)
    {
        wearable = original.wearable;
        loader = original.loader;
        assetRenderers = original.assetRenderers;
    }

    public virtual void Load(string bodyShapeId, Transform parent, Action<WearableController> onSuccess, Action<WearableController> onFail)
    {
        if (isReady)
            return;

        boneRetargetingDirty = true;

        var representation = wearable.GetRepresentation(bodyShapeId);

        if (representation == null)
        {
            onFail?.Invoke(this);
            return;
        }

        var provider = wearable.GetContentProvider(bodyShapeId);

        loader = new RendereableAssetLoadHelper(provider, wearable.baseUrlBundles);

        loader.settings.forceNewInstance = false;
        loader.settings.initialLocalPosition = Vector3.up * 0.75f;
        loader.settings.cachingFlags = MaterialCachingHelper.Mode.CACHE_EVERYTHING;
        loader.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE;
        loader.settings.parent = parent;
        loader.settings.layer = parent.gameObject.layer;

        assetRenderers = null;

        void OnSuccessWrapper(GameObject gameObject)
        {
            if (loader != null)
            {
                loader.OnSuccessEvent -= OnSuccessWrapper;
            }

            assetRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            PrepareWearable(gameObject);
            onSuccess?.Invoke(this);
        }

        loader.OnSuccessEvent += OnSuccessWrapper;

        void OnFailEventWrapper()
        {
            if (loader != null)
            {
                loader.OnFailEvent -= OnFailEventWrapper;
                loader.ClearEvents();
                lastMainFileLoaded = null;
                loader = null;
            }

            onFail?.Invoke(this);
        }

        loader.OnFailEvent += OnFailEventWrapper;

        lastMainFileLoaded = representation.mainFile;
        loader.Load(representation.mainFile);
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

    public void CleanUp()
    {
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

    protected virtual void PrepareWearable(GameObject assetContainer)
    {
    }

    public virtual void UpdateVisibility(HashSet<string> hiddenList)
    {
        SetAssetRenderersEnabled(!hiddenList.Contains(wearable.data.category));
    }

    public bool IsLoadedForBodyShape(string bodyShapeId)
    {
        if (loader == null || !isReady || lastMainFileLoaded == null)
            return false;

        return wearable.data.representations.FirstOrDefault(x => x.bodyShapes.Contains(bodyShapeId))?.mainFile == lastMainFileLoaded;
    }
}