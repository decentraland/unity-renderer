using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using System.Collections.Generic;

public record SystemsContext
{
    public readonly IECSComponentWriter componentWriter;
    public readonly IReadOnlyDictionary<int, ComponentWriter> ComponentWriters;
    public readonly IInternalECSComponents internalEcsComponents;
    public readonly IComponentGroups componentGroups;
    public readonly ECSComponent<PBBillboard> billboards;
    public readonly WrappedComponentPool<IWrappedComponent<PBCameraMode>> CameraModePool;
    public readonly WrappedComponentPool<IWrappedComponent<PBPointerLock>> PointerLockPool;
    public readonly WrappedComponentPool<IWrappedComponent<ECSTransform>> TransformPool;
    public readonly WrappedComponentPool<IWrappedComponent<PBVideoEvent>> VideoEventPool;

    public SystemsContext(IECSComponentWriter componentWriter,
        IReadOnlyDictionary<int, ComponentWriter> componentWriters,
        IInternalECSComponents internalEcsComponents,
        IComponentGroups componentGroups,
        ECSComponent<PBBillboard> billboards,
        WrappedComponentPool<IWrappedComponent<PBCameraMode>> cameraModePool,
        WrappedComponentPool<IWrappedComponent<PBPointerLock>> pointerLockPool,
        WrappedComponentPool<IWrappedComponent<ECSTransform>> transformPool,
        WrappedComponentPool<IWrappedComponent<PBVideoEvent>> videoEventPool)
    {
        this.componentWriter = componentWriter;
        this.internalEcsComponents = internalEcsComponents;
        this.componentGroups = componentGroups;
        this.billboards = billboards;
        CameraModePool = cameraModePool;
        PointerLockPool = pointerLockPool;
        TransformPool = transformPool;
        ComponentWriters = componentWriters;
        VideoEventPool = videoEventPool;
    }
}
