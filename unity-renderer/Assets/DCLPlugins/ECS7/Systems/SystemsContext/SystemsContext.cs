using DCL.ECSRuntime;

public readonly struct SystemsContext
{
    public readonly IECSComponentWriter componentWriter;
    public readonly IInternalECSComponents internalEcsComponents;
    public readonly IComponentGroups componentGroups;

    public SystemsContext(IECSComponentWriter componentWriter,
        IInternalECSComponents internalEcsComponents,
        IComponentGroups componentGroups)
    {
        this.componentWriter = componentWriter;
        this.internalEcsComponents = internalEcsComponents;
        this.componentGroups = componentGroups;
    }
}