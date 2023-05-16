using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

public interface IComponentGroups
{
    IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> texturizableGroup { get; }
    IECSReadOnlyComponentsGroup<InternalRenderers, InternalVisibility> visibilityGroup { get; }
    IECSReadOnlyComponentsGroup<InternalUiContainer, InternalPointerEvents> UnregisteredUiPointerEvents { get; }
    IECSReadOnlyComponentsGroup<InternalUiContainer, InternalPointerEvents, InternalRegisteredUiPointerEvents> RegisteredUiPointerEvents { get; }
    IECSReadOnlyComponentsGroup<InternalRegisteredUiPointerEvents> RegisteredUiPointerEventsWithUiRemoved { get; }
    IECSReadOnlyComponentsGroup<InternalUiContainer, InternalRegisteredUiPointerEvents> RegisteredUiPointerEventsWithPointerEventsRemoved { get; }
}
