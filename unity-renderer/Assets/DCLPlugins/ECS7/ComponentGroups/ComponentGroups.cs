using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

public record ComponentGroups : IComponentGroups
{
    public IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> texturizableGroup { get; }
    public IECSReadOnlyComponentsGroup<InternalRenderers, InternalVisibility> visibilityGroup { get; }
    public IECSReadOnlyComponentsGroup<InternalUiContainer, InternalPointerEvents> UnregisteredUiPointerEvents { get; }
    public IECSReadOnlyComponentsGroup<InternalUiContainer, InternalPointerEvents, InternalRegisteredUiPointerEvents> RegisteredUiPointerEvents { get; }
    public IECSReadOnlyComponentsGroup<InternalRegisteredUiPointerEvents> RegisteredUiPointerEventsWithUiRemoved { get; }
    public IECSReadOnlyComponentsGroup<InternalUiContainer, InternalRegisteredUiPointerEvents> RegisteredUiPointerEventsWithPointerEventsRemoved { get; }

    public ComponentGroups(ECSComponentsManager componentsManager)
    {
        texturizableGroup = componentsManager.CreateComponentGroup<InternalMaterial, InternalTexturizable>
            ((int)InternalECSComponentsId.MATERIAL, (int)InternalECSComponentsId.TEXTURIZABLE);

        visibilityGroup = componentsManager.CreateComponentGroup<InternalRenderers, InternalVisibility>
            ((int)InternalECSComponentsId.RENDERERS, (int)InternalECSComponentsId.VISIBILITY);

        UnregisteredUiPointerEvents = componentsManager.CreateComponentGroupWithoutComponent<InternalUiContainer, InternalPointerEvents>
        ((int)InternalECSComponentsId.UI_CONTAINER,
            (int)InternalECSComponentsId.POINTER_EVENTS,
            (int)InternalECSComponentsId.REGISTERED_UI_POINTER_EVENTS);

        RegisteredUiPointerEvents = componentsManager.CreateComponentGroup<InternalUiContainer, InternalPointerEvents, InternalRegisteredUiPointerEvents>
        ((int)InternalECSComponentsId.UI_CONTAINER,
            (int)InternalECSComponentsId.POINTER_EVENTS,
            (int)InternalECSComponentsId.REGISTERED_UI_POINTER_EVENTS);

        RegisteredUiPointerEventsWithUiRemoved = componentsManager.CreateComponentGroupWithoutComponent<InternalRegisteredUiPointerEvents>
        ((int)InternalECSComponentsId.REGISTERED_UI_POINTER_EVENTS,
            (int)InternalECSComponentsId.UI_CONTAINER);

        RegisteredUiPointerEventsWithPointerEventsRemoved = componentsManager.CreateComponentGroupWithoutComponent<InternalUiContainer, InternalRegisteredUiPointerEvents>
        ((int)InternalECSComponentsId.UI_CONTAINER,
            (int)InternalECSComponentsId.REGISTERED_UI_POINTER_EVENTS,
            (int)InternalECSComponentsId.POINTER_EVENTS);
    }
}
