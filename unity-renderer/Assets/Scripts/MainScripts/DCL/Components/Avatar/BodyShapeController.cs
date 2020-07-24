using System;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

public class BodyShapeController : WearableController
{
    public string bodyShapeId => wearable.id;
    private Transform animationTarget;

    public BodyShapeController(WearableItem wearableItem) : base(wearableItem, wearableItem?.id)
    {
    }

    protected BodyShapeController(WearableController original) : base(original)
    {
    }

    public SkinnedMeshRenderer skinnedMeshRenderer { get; private set; }

    public override void Load(Transform parent, Action<WearableController> onSuccess, Action<WearableController> onFail)
    {
        animationTarget = parent;
        skinnedMeshRenderer = null;
        base.Load(parent, onSuccess, onFail);
    }

    public void RemoveUnusedParts(HashSet<string> usedCategories)
    {
        bool lowerBodyActive = false;
        bool upperBodyActive = false;
        bool feetActive = false;

        foreach (var category in usedCategories)
        {
            switch (category)
            {
                case WearableLiterals.Categories.LOWER_BODY:
                    lowerBodyActive = true;
                    break;
                case WearableLiterals.Categories.UPPER_BODY:
                    upperBodyActive = true;
                    break;
                case WearableLiterals.Categories.FEET:
                    feetActive = true;
                    break;
            }
        }

        lowerBodyRenderer.gameObject.SetActive(lowerBodyActive);
        upperBodyRenderer.gameObject.SetActive(upperBodyActive);
        feetRenderer.gameObject.SetActive(feetActive);
    }

    public void SetupEyes(Material material, Texture texture, Texture mask, Color color)
    {
        AvatarUtils.MapSharedMaterialsRecursively(assetContainer.transform,
            (mat) =>
            {
                material.SetTexture(AvatarUtils._EyesTexture, texture);
                material.SetTexture(AvatarUtils._IrisMask, mask);
                material.SetColor(AvatarUtils._EyeTint, color);
                return material;
            },
            "eyes");
    }

    public void SetupEyebrows(Material material, Texture texture, Color color)
    {
        AvatarUtils.MapSharedMaterialsRecursively(assetContainer.transform,
            (mat) =>
            {
                material.SetTexture(AvatarUtils._BaseMap, texture);

                //NOTE(Brian): This isn't an error, we must also apply hair color to this mat
                material.SetColor(AvatarUtils._BaseColor, color);

                return material;
            },
            "eyebrows");
    }

    public void SetupMouth(Material material, Texture texture, Color color)
    {
        AvatarUtils.MapSharedMaterialsRecursively(assetContainer.transform,
            (mat) =>
            {
                material.SetTexture(AvatarUtils._BaseMap, texture);

                //NOTE(Brian): This isn't an error, we must also apply skin color to this mat
                material.SetColor(AvatarUtils._BaseColor, color);
                return material;
            },
            "mouth");
    }

    private Animation PrepareAnimation(GameObject container)
    {
        Animation createdAnimation = null;

        //NOTE(Brian): Fix to support hierarchy difference between AssetBundle and GLTF wearables.
        Utils.ForwardTransformChildTraversal<Transform>((x) =>
            {
                if (x.name.Contains("Armature"))
                {
                    createdAnimation = x.parent.gameObject.GetOrCreateComponent<Animation>();
                    return false; //NOTE(Brian): If we return false the traversal is stopped.
                }

                return true;
            },
            container.transform);

        createdAnimation.cullingType = AnimationCullingType.BasedOnRenderers;
        return createdAnimation;
    }

    protected override void PrepareWearable(GameObject assetContainer)
    {
        skinnedMeshRenderer = assetContainer.GetComponentInChildren<SkinnedMeshRenderer>();

        var animation = PrepareAnimation(assetContainer);
        var animator = animationTarget.GetComponent<AvatarAnimatorLegacy>();
        animator.BindBodyShape(animation, bodyShapeId, animationTarget);

        var allRenderers = assetContainer.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (var r in allRenderers)
        {
            string parentName = r.transform.parent.name.ToLower();

            if (parentName.Contains("ubody"))
                upperBodyRenderer = r;
            else if (parentName.Contains("lbody"))
                lowerBodyRenderer = r;
            else if (parentName.Contains("feet"))
                feetRenderer = r;
        }
    }

    public SkinnedMeshRenderer feetRenderer { get; private set; }
    public SkinnedMeshRenderer upperBodyRenderer { get; private set; }
    public SkinnedMeshRenderer lowerBodyRenderer { get; private set; }

    public override void UpdateVisibility()
    {
        SetAssetRenderersEnabled(!hiddenList.Contains(WearableLiterals.Misc.HEAD));
    }
}