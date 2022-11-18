using DCL.ECSComponents;
using DCL.ECSRuntime;

public readonly struct SystemsContext
{
    public readonly IECSComponentWriter componentWriter;
    public readonly IInternalECSComponents internalEcsComponents;
    public readonly IComponentGroups componentGroups;
    public readonly ECSComponent<PBPointerHoverFeedback> pointerEvents;
    public readonly ECSComponent<PBBillboard> billboards;

    public SystemsContext(IECSComponentWriter componentWriter,
        IInternalECSComponents internalEcsComponents,
        IComponentGroups componentGroups,
        ECSComponent<PBPointerHoverFeedback> pointerEvents,
        ECSComponent<PBBillboard> billboards)
    {
        this.componentWriter = componentWriter;
        this.internalEcsComponents = internalEcsComponents;
        this.componentGroups = componentGroups;
        this.pointerEvents = pointerEvents;
        this.billboards = billboards;
    }
}