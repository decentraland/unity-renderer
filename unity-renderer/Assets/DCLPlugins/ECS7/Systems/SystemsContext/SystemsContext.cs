using System.Collections.Generic;
using DCL.ECSRuntime;
using DCLPlugins.ECSComponents.Events;

public readonly struct SystemsContext
{
    public readonly IECSComponentWriter componentWriter;
    public readonly IInternalECSComponents internalEcsComponents;
    public readonly IComponentGroups componentGroups;
    public readonly Queue<PointerEvent> pendingResolvingPointerEvents;

    public SystemsContext(IECSComponentWriter componentWriter,
        IInternalECSComponents internalEcsComponents,
        IComponentGroups componentGroups,
        Queue<PointerEvent> pendingResolvingPointerEvents)
    {
        this.componentWriter = componentWriter;
        this.internalEcsComponents = internalEcsComponents;
        this.componentGroups = componentGroups;
        this.pendingResolvingPointerEvents = pendingResolvingPointerEvents;
    }
}