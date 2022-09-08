using DCL.ECSComponents;
using DCL.ECSRuntime;

public readonly struct SystemsContext
{
    public readonly IECSComponentWriter componentWriter;
    public readonly IInternalECSComponents internalEcsComponents;
    public readonly IComponentGroups componentGroups;
    public readonly ECSComponent<PBOnPointerDown> pointerDownComponent;
    public readonly ECSComponent<PBOnPointerUp> pointerUpComponent;

    public SystemsContext(IECSComponentWriter componentWriter,
        IInternalECSComponents internalEcsComponents,
        IComponentGroups componentGroups,
        ECSComponent<PBOnPointerDown> pointerDownComponent,
        ECSComponent<PBOnPointerUp> pointerUpComponent)
    {
        this.componentWriter = componentWriter;
        this.internalEcsComponents = internalEcsComponents;
        this.componentGroups = componentGroups;
        this.pointerDownComponent = pointerDownComponent;
        this.pointerUpComponent = pointerUpComponent;
    }
}