using System;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IBodyShapeController
{
    bool isReady { get; }
    string bodyShapeId { get; }
    void SetupEyes(Material material, Texture texture, Texture mask, Color color);
    void SetupEyebrows(Material material, Texture texture, Color color);
    void SetupMouth(Material material, Texture texture, Texture mask, Color color);
    void SetFacialFeaturesVisible(bool visible, bool updateVisibility = false);
}

public class BodyShapeController : WearableController, IBodyShapeController
{
    public string bodyShapeId => wearable.id;
    private Transform animationTarget;
    private bool facialFeaturesVisible = true;
    private HashSet<string> currentHiddenList = new HashSet<string>();
    public Transform[] bones;
    public Transform rootBone;

    private bool lowerBodyActive = true;
    private bool upperBodyActive = true;
    private bool feetActive = true;

    private AvatarAnimationEventHandler animEventHandler;

    public BodyShapeController(WearableItem wearableItem) : base(wearableItem) { }

    protected BodyShapeController(BodyShapeController original) : base(original)
    {
        headRenderer = original.headRenderer;
        eyebrowsRenderer = original.eyebrowsRenderer;
        eyesRenderer = original.eyesRenderer;
        mouthRenderer = original.mouthRenderer;
        feetRenderer = original.feetRenderer;
        upperBodyRenderer = original.upperBodyRenderer;
        lowerBodyRenderer = original.lowerBodyRenderer;
        bones = original.bones;
        rootBone = original.rootBone;
        upperBodyActive = original.upperBodyActive;
        lowerBodyActive = original.lowerBodyActive;
        feetActive = original.feetActive;
    }

    public override void Load(string bodyShapeId, Transform parent, Action<WearableController> onSuccess, Action<WearableController, Exception> onFail)
    {
        animationTarget = parent;
        base.Load(bodyShapeId, parent, onSuccess, onFail);
    }

    public void SetActiveParts(bool lowerBodyActive, bool upperBodyActive, bool feetActive)
    {
        this.lowerBodyActive = lowerBodyActive;
        this.upperBodyActive = upperBodyActive;
        this.feetActive = feetActive;
    }

    public void SetupEyes(Material material, Texture texture, Texture mask, Color color)
    {
        if (assetContainer == null || assetContainer.transform == null)
        {
            Debug.LogWarning("Tried to setup eyes when the asset not ready");
            return;
        }

        material.SetTexture(AvatarUtils._EyesTexture, texture);
        material.SetTexture(AvatarUtils._IrisMask, mask);
        material.SetColor(AvatarUtils._EyeTint, color);

        AvatarUtils.MapSharedMaterialsRecursively(assetContainer.transform,
            (mat) => material,
            "eyes");
    }

    public void SetupEyebrows(Material material, Texture texture, Color color)
    {
        if (assetContainer == null || assetContainer.transform == null)
        {
            Debug.LogWarning("Tried to setup eyebrows when the asset not ready");
            return;
        }

        material.SetTexture(AvatarUtils._BaseMap, texture);

        //NOTE(Brian): This isn't an error, we must also apply hair color to this mat
        material.SetColor(AvatarUtils._BaseColor, color);

        AvatarUtils.MapSharedMaterialsRecursively(assetContainer.transform,
            (mat) => material,
            "eyebrows");
    }

    public void SetupMouth(Material material, Texture texture, Texture mask, Color color)
    {
        if (assetContainer == null || assetContainer.transform == null)
        {
            Debug.LogWarning("Tried to setup mouth when the asset not ready");
            return;
        }

        material.SetTexture(AvatarUtils._BaseMap, texture);
        material.SetTexture(AvatarUtils._TintMask, mask);

        //NOTE(Brian): This isn't an error, we must also apply skin color to this mat
        material.SetColor(AvatarUtils._BaseColor, color);

        AvatarUtils.MapSharedMaterialsRecursively(assetContainer.transform,
            (mat) => material,
            "mouth");
    }

    public void SetFacialFeaturesVisible(bool visible, bool updateVisibility = false)
    {
        facialFeaturesVisible = visible;

        if ( updateVisibility )
            RefreshFacialFeaturesVisibility();
    }

    void RefreshFacialFeaturesVisibility()
    {
        bool headIsVisible = !currentHiddenList.Contains(WearableLiterals.Misc.HEAD);
        eyebrowsRenderer.enabled = headIsVisible && facialFeaturesVisible;
        eyesRenderer.enabled = headIsVisible && facialFeaturesVisible;
        mouthRenderer.enabled = headIsVisible && facialFeaturesVisible;
    }

    protected override void PrepareWearable(GameObject assetContainer)
    {
        Animation animation = null;

        //NOTE(Brian): Fix to support hierarchy difference between AssetBundle and GLTF wearables.
        Utils.ForwardTransformChildTraversal<Transform>((x) =>
            {
                if (x.name.Contains("Armature"))
                {
                    var armatureGO = x.parent;
                    animation = armatureGO.gameObject.GetOrCreateComponent<Animation>();
                    armatureGO.gameObject.GetOrCreateComponent<StickerAnimationListener>();
                    return false; //NOTE(Brian): If we return false the traversal is stopped.
                }

                return true;
            },
            assetContainer.transform);

        // BasedOnRenderers only acts on child renderers. As all child renderers are deactivated, this feature doesn't work.
        //
        // Adding the merged avatar as child require toggling the animation enabled state on/off and is delayed by a frame,
        // So the avatar can be seen as gigantic in this lapse.
        //
        // TODO(Brian): Find a better way to cull animations
        animation.cullingType = AnimationCullingType.AlwaysAnimate;

        SkinnedMeshRenderer firstRenderer = null;
        foreach (var r in GetRenderers())
        {
            string parentName = r.transform.parent.name.ToLower();

            if ( firstRenderer == null )
                firstRenderer = r;

            if (parentName.Contains("ubody"))
                upperBodyRenderer = r;
            else if (parentName.Contains("lbody"))
                lowerBodyRenderer = r;
            else if (parentName.Contains("feet"))
                feetRenderer = r;
            else if (parentName.Contains("head"))
                headRenderer = r;
            else if (parentName.Contains("eyebrows"))
                eyebrowsRenderer = r;
            else if (parentName.Contains("eyes"))
                eyesRenderer = r;
            else if (parentName.Contains("mouth"))
                mouthRenderer = r;
        }

        if ( firstRenderer != null )
        {
            bones = firstRenderer.bones;
            rootBone = firstRenderer.rootBone;
        }

        var animator = animationTarget.GetComponent<AvatarAnimatorLegacy>();
        animator.BindBodyShape(animation, bodyShapeId, animationTarget);

        InitializeAvatarAudioAndParticleHandlers(assetContainer, animation);
    }

    public SkinnedMeshRenderer headRenderer { get; private set; }
    public SkinnedMeshRenderer eyebrowsRenderer { get; private set; }
    public SkinnedMeshRenderer eyesRenderer { get; private set; }
    public SkinnedMeshRenderer mouthRenderer { get; private set; }
    public SkinnedMeshRenderer feetRenderer { get; private set; }
    public SkinnedMeshRenderer upperBodyRenderer { get; private set; }
    public SkinnedMeshRenderer lowerBodyRenderer { get; private set; }

    public override void UpdateVisibility(HashSet<string> hiddenList)
    {
        currentHiddenList = hiddenList;

        bool headIsVisible = !currentHiddenList.Contains(WearableLiterals.Misc.HEAD);

        headRenderer.enabled = headIsVisible;

        RefreshFacialFeaturesVisibility();

        feetRenderer.enabled = feetActive && !currentHiddenList.Contains(WearableLiterals.Categories.FEET);
        upperBodyRenderer.enabled = upperBodyActive && !currentHiddenList.Contains(WearableLiterals.Categories.UPPER_BODY);
        lowerBodyRenderer.enabled = lowerBodyActive && !currentHiddenList.Contains(WearableLiterals.Categories.LOWER_BODY);
    }

    private void InitializeAvatarAudioAndParticleHandlers(GameObject container, Animation createdAnimation)
    {
        //NOTE(Mordi): Adds handler for animation events, and passes in the audioContainer for the avatar
        AvatarAnimationEventHandler animationEventHandler = createdAnimation.gameObject.AddComponent<AvatarAnimationEventHandler>();

        //Temporary ugly fix to avoid backpack avatar preview loading failure, this part will vanish with the avatar refactor merge
        AudioContainer audioContainer = null;
        if (container.transform.parent.parent.parent != null)
        {
            audioContainer = container.transform.parent.parent.parent.GetComponentInChildren<AudioContainer>();
        }
         
        if (audioContainer != null)
        {
            animationEventHandler.Init(audioContainer);

            //NOTE(Mordi): If this is a remote avatar, pass the animation component so we can keep track of whether it is culled (off-screen) or not
            AvatarAudioHandlerRemote audioHandlerRemote = audioContainer.GetComponent<AvatarAudioHandlerRemote>();
            if (audioHandlerRemote != null)
            {
                audioHandlerRemote.Init(createdAnimation.gameObject);
            }
        }

        animEventHandler = animationEventHandler;
    }

    public override void CleanUp()
    {
        Object.Destroy(animEventHandler);
        facialFeaturesVisible = true;
        base.CleanUp();
    }
}