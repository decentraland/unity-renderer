using System;
using System.Collections.Generic;
using DCL;
using UnityEngine;
using Object = UnityEngine.Object;

public class WearableController
{
    private const string MATERIAL_FILTER_HAIR = "hair";
    private const string MATERIAL_FILTER_SKIN = "skin";

    public readonly WearableItem wearable;
    private AssetPromise_GLTF promise;
    private readonly string bodyShapeType;

    public string id => wearable.id;
    public string category => wearable.category;

    protected GameObject assetContainer => promise?.asset?.container;
    public bool isReady => promise != null && promise.state == AssetPromiseState.FINISHED;

    protected Renderer[] assetRenderers;
    
    List<Material> materials = null;

    public WearableController(WearableItem wearableItem, string bodyShapeType)
    {
        this.wearable = wearableItem;
        this.bodyShapeType = bodyShapeType;
    }

    protected WearableController(WearableController original)
    {
        wearable = original.wearable;
        promise = original.promise;
        bodyShapeType = original.bodyShapeType;
        assetRenderers = original.assetRenderers;
    }

    public void Load(Transform parent, Action<WearableController> onSuccess, Action<WearableController> onFail)
    {
        var representation = wearable.GetRepresentation(bodyShapeType);
        var provider = wearable.GetContentProvider(bodyShapeType);

        promise = new AssetPromise_GLTF(provider, representation.mainFile);

        promise.settings.forceNewInstance = true;
        promise.settings.initialLocalPosition = Vector3.up * 0.75f;
        promise.settings.visibleFlags = AssetPromise_GLTF.VisibleFlags.INVISIBLE;
        promise.OnSuccessEvent += (x) =>
        {
            assetRenderers = x.container.GetComponentsInChildren<Renderer>();
            onSuccess.Invoke(this);
        };
        promise.OnFailEvent += (x) => onFail.Invoke(this);
        promise.settings.parent = parent;

        AssetPromiseKeeper_GLTF.i.Keep(promise);
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
        SkinnedMeshRenderer[] skinnedRenderers = assetContainer.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i1 = 0; i1 < skinnedRenderers.Length; i1++)
        {
            skinnedRenderers[i1].rootBone = skinnedMeshRenderer.rootBone;
            skinnedRenderers[i1].bones = skinnedMeshRenderer.bones;
        }
    }

    public void CleanUp()
    {
        UnloadMaterials();
        if (promise != null)
        {
            promise.ClearEvents();
            AssetPromiseKeeper_GLTF.i.Forget(this.promise);
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
}