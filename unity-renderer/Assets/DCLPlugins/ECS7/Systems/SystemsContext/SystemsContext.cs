using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using System.Collections.Generic;

public record SystemsContext
{
    public readonly IReadOnlyDictionary<int, ComponentWriter> ComponentWriters;
    public readonly IInternalECSComponents InternalEcsComponents;
    public readonly IComponentGroups ComponentGroups;
    public readonly ECSComponent<PBBillboard> Billboards;
    public readonly ECSComponent<ECSTransform> TransformComponent;
    public readonly WrappedComponentPool<IWrappedComponent<PBCameraMode>> CameraModePool;
    public readonly WrappedComponentPool<IWrappedComponent<PBPointerLock>> PointerLockPool;
    public readonly WrappedComponentPool<IWrappedComponent<ECSTransform>> TransformPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBVideoEvent>> VideoEventPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBRaycastResult>> RaycastResultPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBGltfContainerLoadingState>> GltfContainerLoadingStatePool;
    public readonly WrappedComponentPool<IWrappedComponent<PBEngineInfo>> EngineInfoPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBUiCanvasInformation>> UiCanvasInformationPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBPointerEventsResult>> PointerEventsResultPool;
    // FD:: add here pools -->
    public readonly WrappedComponentPool<IWrappedComponent<PBMeshRenderer>> MeshRendererPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBMeshCollider>> MeshColliderPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBMaterial>> MaterialPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBAnimator>> AnimatorPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBAudioSource>> AudioSourcePool;
    public readonly WrappedComponentPool<IWrappedComponent<PBAudioStream>> AudioStreamPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBAvatarShape>> AvatarShapePool;
    public readonly WrappedComponentPool<IWrappedComponent<PBBillboard>> BillboardPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBGltfContainer>> GltfContainerPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBNftShape>> NftShapePool;
    public readonly WrappedComponentPool<IWrappedComponent<PBPointerEvents>> PointerEventsPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBRaycast>> RaycastPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBTextShape>> TextShapePool;
    public readonly WrappedComponentPool<IWrappedComponent<PBUiBackground>> UIBackgroundPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBUiDropdown>> UIDropdownPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBUiInput>> UIInputPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBUiText>> UITextPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBUiTransform>> UITransformPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBVideoPlayer>> VideoPlayerPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBVisibilityComponent>> VisibilityComponentPool;

    public SystemsContext(
        IReadOnlyDictionary<int, ComponentWriter> componentWriters,
        IInternalECSComponents internalEcsComponents,
        IComponentGroups componentGroups,
        ECSComponent<PBBillboard> billboards,
        ECSComponent<ECSTransform> transformComponent,
        WrappedComponentPool<IWrappedComponent<PBCameraMode>> cameraModePool,
        WrappedComponentPool<IWrappedComponent<PBPointerLock>> pointerLockPool,
        WrappedComponentPool<IWrappedComponent<ECSTransform>> transformPool,
        WrappedComponentPool<IWrappedComponent<PBVideoEvent>> videoEventPool,
        WrappedComponentPool<IWrappedComponent<PBRaycastResult>> raycastResultPool,
        WrappedComponentPool<IWrappedComponent<PBGltfContainerLoadingState>> gltfContainerLoadingStatePool,
        WrappedComponentPool<IWrappedComponent<PBEngineInfo>> engineInfoPool,
        WrappedComponentPool<IWrappedComponent<PBUiCanvasInformation>> uiCanvasInformationPool,
        WrappedComponentPool<IWrappedComponent<PBPointerEventsResult>> pointerEventsResultPool,
        WrappedComponentPool<IWrappedComponent<PBMeshRenderer>> meshRendererPool,
        WrappedComponentPool<IWrappedComponent<PBMeshCollider>> meshColliderPool,
        WrappedComponentPool<IWrappedComponent<PBMaterial>> materialPool,
        WrappedComponentPool<IWrappedComponent<PBAnimator>> animatorPool,
        WrappedComponentPool<IWrappedComponent<PBAudioSource>> audioSourcePool,
        WrappedComponentPool<IWrappedComponent<PBAudioStream>> audioStreamPool,
        WrappedComponentPool<IWrappedComponent<PBAvatarShape>> avatarShapePool,
        WrappedComponentPool<IWrappedComponent<PBBillboard>> billboardPool,
        WrappedComponentPool<IWrappedComponent<PBGltfContainer>> gltfContainerPool,
        WrappedComponentPool<IWrappedComponent<PBNftShape>> nftShapePool,
        WrappedComponentPool<IWrappedComponent<PBPointerEvents>> pointerEventsPool,
        WrappedComponentPool<IWrappedComponent<PBRaycast>> raycastPool,
        WrappedComponentPool<IWrappedComponent<PBTextShape>> textShapePool,
        WrappedComponentPool<IWrappedComponent<PBUiBackground>> uiBackgroundPool,
        WrappedComponentPool<IWrappedComponent<PBUiDropdown>> uiDropdownPool,
        WrappedComponentPool<IWrappedComponent<PBUiInput>> uiInputPool,
        WrappedComponentPool<IWrappedComponent<PBUiText>> uiTextPool,
        WrappedComponentPool<IWrappedComponent<PBUiTransform>> uiTransformPool,
        WrappedComponentPool<IWrappedComponent<PBVideoPlayer>> videoPlayerPool,
        WrappedComponentPool<IWrappedComponent<PBVisibilityComponent>> visibilityComponentPool)
    {
        this.InternalEcsComponents = internalEcsComponents;
        this.ComponentGroups = componentGroups;
        this.Billboards = billboards;
        this.TransformComponent = transformComponent;
        this.CameraModePool = cameraModePool;
        this.PointerLockPool = pointerLockPool;
        this.TransformPool = transformPool;
        this.ComponentWriters = componentWriters;
        this.VideoEventPool = videoEventPool;
        this.RaycastResultPool = raycastResultPool;
        this.GltfContainerLoadingStatePool = gltfContainerLoadingStatePool;
        this.EngineInfoPool = engineInfoPool;
        this.UiCanvasInformationPool = uiCanvasInformationPool;
        this.PointerEventsResultPool = pointerEventsResultPool;
        this.MeshRendererPool = meshRendererPool;
        this.MeshColliderPool = meshColliderPool;
        this.MaterialPool = materialPool;
        this.AnimatorPool = animatorPool;
        this.AudioSourcePool = audioSourcePool;
        this.AudioStreamPool = audioStreamPool;
        this.AvatarShapePool = avatarShapePool;
        this.BillboardPool = billboardPool;
        this.GltfContainerPool = gltfContainerPool;
        this.NftShapePool = nftShapePool;
        this.PointerEventsPool = pointerEventsPool;
        this.RaycastPool = raycastPool;
        this.TextShapePool = textShapePool;
        this.UIBackgroundPool = uiBackgroundPool;
        this.UIDropdownPool = uiDropdownPool;
        this.UIInputPool = uiInputPool;
        this.UITextPool = uiTextPool;
        this.UITransformPool = uiTransformPool;
        this.VideoPlayerPool = videoPlayerPool;
        this.VisibilityComponentPool = visibilityComponentPool;
    }
}
