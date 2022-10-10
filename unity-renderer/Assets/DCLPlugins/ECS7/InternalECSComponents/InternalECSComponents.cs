using System;
using System.Collections.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;

public class InternalECSComponents : IDisposable, IInternalECSComponents
{
    internal readonly IList<InternalComponentWriteData> scheduledWrite = new List<InternalComponentWriteData>(50);

    public IInternalECSComponent<InternalTexturizable> texturizableComponent { get; }
    public IInternalECSComponent<InternalMaterial> materialComponent { get; }
    public IInternalECSComponent<InternalColliders> onPointerColliderComponent { get; }
    public IInternalECSComponent<InternalColliders> physicColliderComponent { get; }
    public IInternalECSComponent<InternalInputEventResults> inputEventResultsComponent { get; }
    public IInternalECSComponent<InternalRenderers> renderersComponent { get; }
    public IInternalECSComponent<InternalVisibility> visibilityComponent { get; }
    public IInternalECSComponent<InternalUiContainer> uiContainerComponent { get; }

    public InternalECSComponents(ECSComponentsManager componentsManager, ECSComponentsFactory componentsFactory)
    {
        texturizableComponent = new InternalECSComponent<InternalTexturizable>(
            InternalECSComponentsId.TEXTURIZABLE,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalTexturizable>(
                () => texturizableComponent, (_, __, model) => model.renderers.Count == 0),
            scheduledWrite);

        materialComponent = new InternalECSComponent<InternalMaterial>(
            InternalECSComponentsId.MATERIAL,
            componentsManager,
            componentsFactory,
            null,
            scheduledWrite);

        onPointerColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_POINTER,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalColliders>(
                () => onPointerColliderComponent, (_, __, model) => model.colliders.Count == 0),
            scheduledWrite);

        physicColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_PHYSICAL,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalColliders>(
                () => physicColliderComponent, (_, __, model) => model.colliders.Count == 0),
            scheduledWrite);

        renderersComponent = new InternalECSComponent<InternalRenderers>(
            InternalECSComponentsId.RENDERERS,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalRenderers>(
                () => renderersComponent, (_, __, model) => model.renderers.Count == 0),
            scheduledWrite);

        visibilityComponent = new InternalECSComponent<InternalVisibility>(
            InternalECSComponentsId.VISIBILITY,
            componentsManager,
            componentsFactory,
            null,
            scheduledWrite);

        inputEventResultsComponent = new InternalECSComponent<InternalInputEventResults>(
            InternalECSComponentsId.INPUT_EVENTS_RESULT,
            componentsManager,
            componentsFactory,
            null,
            scheduledWrite);

        uiContainerComponent = new InternalECSComponent<InternalUiContainer>(
            InternalECSComponentsId.UI_CONTAINER,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalUiContainer>(
                () => uiContainerComponent,
                (_, entity, model) => model.rootElement.childCount == 0 && model.components.Count == 0 && entity.entityId != SpecialEntityId.SCENE_ROOT_ENTITY),
            scheduledWrite);
    }

    public void Dispose()
    {
        scheduledWrite.Clear();

        texturizableComponent.Dispose();
        materialComponent.Dispose();
        onPointerColliderComponent.Dispose();
        physicColliderComponent.Dispose();
        renderersComponent.Dispose();
        inputEventResultsComponent.Dispose();
    }

    public void WriteSystemUpdate()
    {
        for (int i = 0; i < scheduledWrite.Count; i++)
        {
            var writeData = scheduledWrite[i];
            if (writeData.scene == null)
                continue;

            InternalComponent data = writeData.data;
            if (data != null)
            {
                data._dirty = false;
            }
            else
            {
                writeData.scene.crdtExecutor.ExecuteWithoutStoringState(writeData.entityId, writeData.componentId, null);
            }
        }
        scheduledWrite.Clear();
    }
}