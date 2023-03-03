using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using System;
using System.Collections.Generic;

public class InternalECSComponents : IDisposable, IInternalECSComponents
{
    internal readonly IList<InternalComponentWriteData> scheduledWrite = new List<InternalComponentWriteData>(50);
    internal readonly IList<InternalComponentWriteData> scheduledDirty = new List<InternalComponentWriteData>(50);

    public IInternalECSComponent<InternalTexturizable> texturizableComponent { get; }
    public IInternalECSComponent<InternalMaterial> materialComponent { get; }
    public IInternalECSComponent<InternalVideoMaterial> videoMaterialComponent { get; }
    public IInternalECSComponent<InternalVideoPlayer> videoPlayerComponent { get; }
    public IInternalECSComponent<InternalColliders> onPointerColliderComponent { get; }
    public IInternalECSComponent<InternalColliders> physicColliderComponent { get; }
    public IInternalECSComponent<InternalInputEventResults> inputEventResultsComponent { get; }
    public IInternalECSComponent<InternalRenderers> renderersComponent { get; }
    public IInternalECSComponent<InternalVisibility> visibilityComponent { get; }
    public IInternalECSComponent<InternalUiContainer> uiContainerComponent { get; }
    public IInternalECSComponent<InternalUIInputResults> uiInputResultsComponent { get; }
    public IInternalECSComponent<InternalSceneBoundsCheck> sceneBoundsCheckComponent { get; }
    public IInternalECSComponent<InternalAudioSource> audioSourceComponent { get; }

    public InternalECSComponents(ECSComponentsManager componentsManager, ECSComponentsFactory componentsFactory)
    {
        texturizableComponent = new InternalECSComponent<InternalTexturizable>(
            InternalECSComponentsId.TEXTURIZABLE,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalTexturizable>(
                () => texturizableComponent, model => model.renderers.Count == 0),
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
                () => onPointerColliderComponent, model => model.colliders.Count == 0),
            scheduledWrite);

        physicColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_PHYSICAL,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalColliders>(
                () => physicColliderComponent, model => model.colliders.Count == 0),
            scheduledWrite);

        renderersComponent = new InternalECSComponent<InternalRenderers>(
            InternalECSComponentsId.RENDERERS,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalRenderers>(
                () => renderersComponent, model => model.renderers.Count == 0),
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
            () => new UiContainerHandler(() => uiContainerComponent),
            scheduledWrite);

        uiInputResultsComponent = new InternalECSComponent<InternalUIInputResults>(
            InternalECSComponentsId.UI_INPUT_EVENTS_RESULT,
            componentsManager,
            componentsFactory,
            null,
            scheduledWrite
        );

        videoPlayerComponent = new InternalECSComponent<InternalVideoPlayer>(
            InternalECSComponentsId.VIDEO_PLAYER,
            componentsManager,
            componentsFactory,
            null,
            scheduledWrite);

        videoMaterialComponent = new InternalECSComponent<InternalVideoMaterial>(
            InternalECSComponentsId.VIDEO_MATERIAL,
            componentsManager,
            componentsFactory,
            null,
            scheduledWrite);

        sceneBoundsCheckComponent = new InternalECSComponent<InternalSceneBoundsCheck>(
            InternalECSComponentsId.SCENE_BOUNDS_CHECK,
            componentsManager,
            componentsFactory,
            null,
            scheduledWrite
        );

        audioSourceComponent = new InternalECSComponent<InternalAudioSource>(
            InternalECSComponentsId.AUDIO_SOURCE,
            componentsManager,
            componentsFactory,
            null,
            scheduledWrite
        );
    }

    public void Dispose()
    {
        scheduledWrite.Clear();
        scheduledDirty.Clear();

        texturizableComponent.Dispose();
        materialComponent.Dispose();
        onPointerColliderComponent.Dispose();
        physicColliderComponent.Dispose();
        renderersComponent.Dispose();
        inputEventResultsComponent.Dispose();
        videoPlayerComponent.Dispose();
        videoMaterialComponent.Dispose();
    }

    public void WriteSystemUpdate()
    {
        for (int i = 0; i < scheduledWrite.Count; i++)
        {
            var writeData = scheduledWrite[i];

            if (writeData.scene?.crdtExecutor == null)
                continue;

            InternalComponent data = writeData.data;

            if (data != null)
            {
                data._dirty = true;
            }

            writeData.scene.crdtExecutor.ExecuteWithoutStoringState(writeData.entityId, writeData.componentId, data);

            scheduledDirty.Add(new InternalComponentWriteData(writeData.scene, writeData.entityId,
                writeData.componentId, data, writeData.flaggedForRemoval));
        }

        scheduledWrite.Clear();
    }

    public void DirtySystemUpdate()
    {
        for (int i = 0; i < scheduledDirty.Count; i++)
        {
            var writeData = scheduledDirty[i];

            if (writeData.scene?.crdtExecutor == null)
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

        scheduledDirty.Clear();
    }
}
