using DCL.ECSComponents;
using DCL.ECSRuntime;

public readonly struct SystemsContext
{
    public readonly IECSComponentWriter componentWriter;
    public readonly IInternalECSComponents internalEcsComponents;
    public readonly IComponentGroups componentGroups;
    public readonly ECSComponent<PBPointerEvents> pointerEvents;
    public readonly ECSComponent<PBBillboard> billboards;
    public readonly ECSComponent<PBRaycast> raycasts;
    public readonly ECSComponent<PBRaycastResult> raycastResults;
    public readonly ECSComponent<PBMeshCollider> meshCollider;

    public SystemsContext(IECSComponentWriter componentWriter,
        IInternalECSComponents internalEcsComponents,
        IComponentGroups componentGroups,
        ECSComponent<PBPointerEvents> pointerEvents,
        ECSComponent<PBBillboard> billboards,
        ECSComponent<PBRaycast> raycasts,
        ECSComponent<PBRaycastResult> raycastResults,
        ECSComponent<PBMeshCollider> meshCollider)
    {
        this.componentWriter = componentWriter;
        this.internalEcsComponents = internalEcsComponents;
        this.componentGroups = componentGroups;
        this.pointerEvents = pointerEvents;
        this.billboards = billboards;
        this.raycasts = raycasts;
        this.raycastResults = raycastResults;
        this.meshCollider = meshCollider;
    }
}
