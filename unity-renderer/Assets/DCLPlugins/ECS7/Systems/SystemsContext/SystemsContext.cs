using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using System.Collections.Generic;

public record SystemsContext
{
    public readonly IReadOnlyDictionary<int, ComponentWriter> ComponentWriters;
    public readonly IInternalECSComponents internalEcsComponents;
    public readonly IComponentGroups componentGroups;
    public readonly ECSComponent<PBBillboard> billboards;
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
        WrappedComponentPool<IWrappedComponent<PBPointerEventsResult>> pointerEventsResultPool)
    {
        this.internalEcsComponents = internalEcsComponents;
        this.componentGroups = componentGroups;
        this.billboards = billboards;
        TransformComponent = transformComponent;
        CameraModePool = cameraModePool;
        PointerLockPool = pointerLockPool;
        TransformPool = transformPool;
        ComponentWriters = componentWriters;
        VideoEventPool = videoEventPool;
        RaycastResultPool = raycastResultPool;
        GltfContainerLoadingStatePool = gltfContainerLoadingStatePool;
        EngineInfoPool = engineInfoPool;
        UiCanvasInformationPool = uiCanvasInformationPool;
        PointerEventsResultPool = pointerEventsResultPool;
    }
}
