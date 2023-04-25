using DCL.ECSComponents;
using DCL.ECSRuntime;

public readonly struct SystemsContext
{
    public readonly IECSComponentWriter componentWriter;
    public readonly IInternalECSComponents internalEcsComponents;
    public readonly IComponentGroups componentGroups;
    public readonly ECSComponent<PBBillboard> billboards;
    public readonly ECSComponent<PBVideoEvent> videoEvents;

    public SystemsContext(IECSComponentWriter componentWriter,
        IInternalECSComponents internalEcsComponents,
        IComponentGroups componentGroups,
        ECSComponent<PBBillboard> billboards,
        ECSComponent<PBVideoEvent> videoEvents)
    {
        this.componentWriter = componentWriter;
        this.internalEcsComponents = internalEcsComponents;
        this.componentGroups = componentGroups;
        this.billboards = billboards;
        this.videoEvents = videoEvents;
    }
}
