using DCL;
using DCL.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class WearableController
{
    private const string MATERIAL_FILTER_HAIR = "hair";
    private const string MATERIAL_FILTER_SKIN = "skin";

    public readonly WearableItem wearable;
    protected RendereableAssetLoadHelper loader;
    private readonly string bodyShapeType;

    public string id => wearable.id;
    public string category => wearable.category;

    protected GameObject assetContainer => loader?.loadedAsset;
    public bool isReady => loader != null && loader.isFinished;

    protected Renderer[] assetRenderers;

    List<Material> materials = null;

    private bool bonesRetargeted = false;

    public WearableController(WearableItem wearableItem, string bodyShapeType)
    {
        this.wearable = wearableItem;
        this.bodyShapeType = bodyShapeType;
    }

    protected WearableController(WearableController original)
    {
        wearable = original.wearable;
        loader = original.loader;
        bodyShapeType = original.bodyShapeType;
        assetRenderers = original.assetRenderers;
    }

    public virtual void Load(Transform parent, Action<WearableController> onSuccess, Action<WearableController> onFail)
    {
        bonesRetargeted = false;

        var representation = wearable.GetRepresentation(bodyShapeType);
        var provider = wearable.GetContentProvider(bodyShapeType);

        loader = new RendereableAssetLoadHelper(provider, wearable.baseUrlBundles);

        loader.settings.forceNewInstance = true;
        loader.settings.initialLocalPosition = Vector3.up * 0.75f;
        loader.settings.cachingFlags = MaterialCachingHelper.Mode.CACHE_SHADERS;
        loader.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE;
        loader.settings.parent = parent;

        loader.OnSuccessEvent += (x) =>
        {
            PrepareWearable(x);
            onSuccess.Invoke(this);
        };

        loader.OnFailEvent += () => onFail.Invoke(this);

        loader.Load(representation.mainFile);
    }

    public void SetupDefaultMaterial(Material defaultMaterial, Color skinColor, Color hairColor)
    {
        if (assetContainer == null)
            return;

        if (materials == null)
        {
            materials = AvatarUtils.ReplaceMaterialsWithCopiesOf(assetContainer.transform, defaultMaterial);
        }

        AvatarUtils.SetColorInHierarchy(assetContainer.transform, MATERIAL_FILTER_SKIN, skinColor);
        AvatarUtils.SetColorInHierarchy(assetContainer.transform, MATERIAL_FILTER_HAIR, hairColor);
    }

    public void SetAnimatorBones(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        if (bonesRetargeted) return;

        SkinnedMeshRenderer[] skinnedRenderers = assetContainer.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i1 = 0; i1 < skinnedRenderers.Length; i1++)
        {
            skinnedRenderers[i1].rootBone = skinnedMeshRenderer.rootBone;
            skinnedRenderers[i1].bones = skinnedMeshRenderer.bones;
        }
        bonesRetargeted = true;
    }

    public void CleanUp()
    {
        UnloadMaterials();

        if (loader != null)
        {
            loader.Unload();
        }
    }

    public void SetAssetRenderersEnabled(bool active)
    {
        for (var i = 0; i < assetRenderers.Length; i++)
        {
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
        assetRenderers = assetContainer.GetComponentsInChildren<Renderer>();
    }
}
