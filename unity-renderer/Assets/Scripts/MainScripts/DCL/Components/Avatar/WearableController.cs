using DCL;
using DCL.Components;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class WearableController
{
    private const string MATERIAL_FILTER_HAIR = "hair";
    private const string MATERIAL_FILTER_SKIN = "skin";

    public readonly WearableItem wearable;
    protected RendereableAssetLoadHelper loader;
    private readonly string bodyShapeId;

    public string id => wearable.id;
    public string category => wearable.category;

    public GameObject assetContainer => loader?.loadedAsset;
    public bool isReady => loader != null && loader.isFinished;

    protected Renderer[] assetRenderers;

    List<Material> materials = null;

    public bool boneRetargetingDirty = false;

    public WearableController(WearableItem wearableItem, string bodyShapeId)
    {
        this.wearable = wearableItem;
        this.bodyShapeId = bodyShapeId;
    }

    protected WearableController(WearableController original)
    {
        wearable = original.wearable;
        loader = original.loader;
        bodyShapeId = original.bodyShapeId;
        assetRenderers = original.assetRenderers;
    }

    public virtual void Load(Transform parent, Action<WearableController> onSuccess, Action<WearableController> onFail)
    {
        if (isReady)
            return;

        boneRetargetingDirty = true;

        var representation = wearable.GetRepresentation(bodyShapeId);
        var provider = wearable.GetContentProvider(bodyShapeId);

        loader = new RendereableAssetLoadHelper(provider, wearable.baseUrlBundles);

        loader.settings.forceNewInstance = false;
        loader.settings.initialLocalPosition = Vector3.up * 0.75f;
        loader.settings.cachingFlags = MaterialCachingHelper.Mode.CACHE_SHADERS;
        loader.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE;
        loader.settings.parent = parent;

        assetRenderers = null;

        void OnSuccessWrapper(GameObject gameObject)
        {
            loader.OnSuccessEvent -= OnSuccessWrapper;
            assetRenderers = gameObject.GetComponentsInChildren<Renderer>();
            PrepareWearable(gameObject);
            onSuccess?.Invoke(this);
        }
        loader.OnSuccessEvent += OnSuccessWrapper;

        void OnFailEventWrapper()
        {
            loader.OnFailEvent -= OnFailEventWrapper;
            loader = null;
            onFail?.Invoke(this);
        }
        loader.OnFailEvent += OnFailEventWrapper;

        loader.Load(representation.mainFile);
    }

    Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    public void SetupDefaultMaterial(Material defaultMaterial, Color skinColor, Color hairColor)
    {
        if (assetContainer == null)
            return;

        if (materials == null)
        {
            StoreOriginalMaterials();
            materials = AvatarUtils.ReplaceMaterialsWithCopiesOf(assetContainer.transform, defaultMaterial);
        }

        AvatarUtils.SetColorInHierarchy(assetContainer.transform, MATERIAL_FILTER_SKIN, skinColor);
        AvatarUtils.SetColorInHierarchy(assetContainer.transform, MATERIAL_FILTER_HAIR, hairColor);
    }

    private void StoreOriginalMaterials()
    {
        Renderer[] renderers = assetContainer.transform.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (originalMaterials.ContainsKey(renderers[i]))
                continue;

            originalMaterials.Add(renderers[i], renderers[i].sharedMaterials.ToArray());
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

    public void SetAnimatorBones(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        if (!boneRetargetingDirty) return;

        SkinnedMeshRenderer[] skinnedRenderers = assetContainer.GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < skinnedRenderers.Length; i++)
        {
            skinnedRenderers[i].rootBone = skinnedMeshRenderer.rootBone;
            skinnedRenderers[i].bones = skinnedMeshRenderer.bones;
        }

        boneRetargetingDirty = false;
    }

    public void CleanUp()
    {
        UnloadMaterials();
        RestoreOriginalMaterials();
        assetRenderers = null;

        if (loader != null)
        {
            loader.ClearEvents();
            loader.Unload();
            loader = null;
        }
    }

    public virtual void SetAssetRenderersEnabled(bool active)
    {
        for (var i = 0; i < assetRenderers.Length; i++)
        {
            if (assetRenderers[i] != null)
                assetRenderers[i].enabled = active;
        }
    }

    protected virtual void UnloadMaterials()
    {
        if (materials == null)
            return;

        for (var i = 0; i < materials.Count; i++)
        {
            Object.Destroy(materials[i]);
        }
    }

    protected virtual void PrepareWearable(GameObject assetContainer)
    {
    }

    public virtual void UpdateVisibility(HashSet<string> hiddenList)
    {
        SetAssetRenderersEnabled(!hiddenList.Contains(wearable.category));
    }
}