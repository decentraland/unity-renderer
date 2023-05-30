using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;

public readonly struct SystemsContext
{
    public readonly IECSComponentWriter componentWriter;
    public readonly IInternalECSComponents internalEcsComponents;
    public readonly IComponentGroups componentGroups;
    public readonly ISceneStateHandler sceneStateHandler;
    public readonly ECSComponent<PBBillboard> billboards;

    public SystemsContext(IECSComponentWriter componentWriter,
        ISceneStateHandler sceneStateHandler,
        IInternalECSComponents internalEcsComponents,
        IComponentGroups componentGroups,
        ECSComponent<PBBillboard> billboards)
    {
        this.componentWriter = componentWriter;
        this.sceneStateHandler = sceneStateHandler;
        this.internalEcsComponents = internalEcsComponents;
        this.componentGroups = componentGroups;
        this.billboards = billboards;
    }
}
