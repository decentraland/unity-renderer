using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using System;
using System.Collections.Generic;

public class InternalECSComponents : IDisposable, IInternalECSComponents
{
    internal readonly KeyValueSet<ComponentIdentifier, ComponentWriteData> markAsDirtyComponents =
        new KeyValueSet<ComponentIdentifier, ComponentWriteData>(100);

    private readonly KeyValueSet<ComponentIdentifier, ComponentWriteData> removeAsDirtyComponents =
        new KeyValueSet<ComponentIdentifier, ComponentWriteData>(100);

    private readonly IReadOnlyDictionary<int, ICRDTExecutor> crdtExecutors;

    public IInternalECSComponent<InternalTexturizable> texturizableComponent { get; }
    public IInternalECSComponent<InternalMaterial> materialComponent { get; }
    public IInternalECSComponent<InternalVideoMaterial> videoMaterialComponent { get; }
    public IInternalECSComponent<InternalVideoPlayer> videoPlayerComponent { get; }
    public IInternalECSComponent<InternalColliders> onPointerColliderComponent { get; }
    public IInternalECSComponent<InternalColliders> physicColliderComponent { get; }
    public IInternalECSComponent<InternalColliders> customLayerColliderComponent { get; }
    public IInternalECSComponent<InternalInputEventResults> inputEventResultsComponent { get; }
    public IInternalECSComponent<InternalRenderers> renderersComponent { get; }
    public IInternalECSComponent<InternalVisibility> visibilityComponent { get; }
    public IInternalECSComponent<InternalUiContainer> uiContainerComponent { get; }
    public IInternalECSComponent<InternalUIInputResults> uiInputResultsComponent { get; }
    public IInternalECSComponent<InternalSceneBoundsCheck> sceneBoundsCheckComponent { get; }
    public IInternalECSComponent<InternalAudioSource> audioSourceComponent { get; }
    public IInternalECSComponent<InternalPointerEvents> PointerEventsComponent { get; }
    public IInternalECSComponent<InternalRegisteredUiPointerEvents> RegisteredUiPointerEventsComponent { get; }
    public IInternalECSComponent<InternalRaycast> raycastComponent { get; }
    public IInternalECSComponent<InternalGltfContainerLoadingState> GltfContainerLoadingStateComponent { get; }
    public IInternalECSComponent<InternalEngineInfo> EngineInfo { get; }

    public InternalECSComponents(ECSComponentsManager componentsManager, ECSComponentsFactory componentsFactory,
        IReadOnlyDictionary<int, ICRDTExecutor> crdtExecutors)
    {
        this.crdtExecutors = crdtExecutors;

        texturizableComponent = new InternalECSComponent<InternalTexturizable>(
            InternalECSComponentsId.TEXTURIZABLE,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalTexturizable>(
                () => texturizableComponent, model => model.renderers.Count == 0),
            markAsDirtyComponents,
            crdtExecutors);

        materialComponent = new InternalECSComponent<InternalMaterial>(
            InternalECSComponentsId.MATERIAL,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors);

        onPointerColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_POINTER,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalColliders>(
                () => onPointerColliderComponent, model => model.colliders.Count == 0),
            markAsDirtyComponents,
            crdtExecutors);

        physicColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_PHYSICAL,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalColliders>(
                () => physicColliderComponent, model => model.colliders.Count == 0),
            markAsDirtyComponents,
            crdtExecutors);

        customLayerColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_CUSTOM,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalColliders>(
                () => customLayerColliderComponent, model => model.colliders.Count == 0),
            markAsDirtyComponents,
            crdtExecutors);

        renderersComponent = new InternalECSComponent<InternalRenderers>(
            InternalECSComponentsId.RENDERERS,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalRenderers>(
                () => renderersComponent, model => model.renderers.Count == 0),
            markAsDirtyComponents,
            crdtExecutors);

        visibilityComponent = new InternalECSComponent<InternalVisibility>(
            InternalECSComponentsId.VISIBILITY,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors);

        inputEventResultsComponent = new InternalECSComponent<InternalInputEventResults>(
            InternalECSComponentsId.INPUT_EVENTS_RESULT,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors);

        uiContainerComponent = new InternalECSComponent<InternalUiContainer>(
            InternalECSComponentsId.UI_CONTAINER,
            componentsManager,
            componentsFactory,
            () => new UiContainerHandler(() => uiContainerComponent),
            markAsDirtyComponents,
            crdtExecutors);

        uiInputResultsComponent = new InternalECSComponent<InternalUIInputResults>(
            InternalECSComponentsId.UI_INPUT_EVENTS_RESULT,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors
        );

        videoPlayerComponent = new InternalECSComponent<InternalVideoPlayer>(
            InternalECSComponentsId.VIDEO_PLAYER,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors);

        videoMaterialComponent = new InternalECSComponent<InternalVideoMaterial>(
            InternalECSComponentsId.VIDEO_MATERIAL,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors);

        sceneBoundsCheckComponent = new InternalECSComponent<InternalSceneBoundsCheck>(
            InternalECSComponentsId.SCENE_BOUNDS_CHECK,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors
        );

        audioSourceComponent = new InternalECSComponent<InternalAudioSource>(
            InternalECSComponentsId.AUDIO_SOURCE,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors
        );

        PointerEventsComponent = new InternalECSComponent<InternalPointerEvents>(
            InternalECSComponentsId.POINTER_EVENTS,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors
        );

        RegisteredUiPointerEventsComponent = new InternalECSComponent<InternalRegisteredUiPointerEvents>(
            InternalECSComponentsId.REGISTERED_UI_POINTER_EVENTS,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors
        );

        raycastComponent = new InternalECSComponent<InternalRaycast>(
            InternalECSComponentsId.RAYCAST,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors
        );

        GltfContainerLoadingStateComponent = new InternalECSComponent<InternalGltfContainerLoadingState>(
            InternalECSComponentsId.GLTF_CONTAINER_LOADING_STATE,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors
        );

        EngineInfo = new InternalECSComponent<InternalEngineInfo>(
            InternalECSComponentsId.ENGINE_INFO,
            componentsManager,
            componentsFactory,
            null,
            markAsDirtyComponents,
            crdtExecutors
        );
    }

    public void Dispose()
    {
        markAsDirtyComponents.Clear();
        removeAsDirtyComponents.Clear();

        texturizableComponent.Dispose();
        materialComponent.Dispose();
        onPointerColliderComponent.Dispose();
        physicColliderComponent.Dispose();
        renderersComponent.Dispose();
        inputEventResultsComponent.Dispose();
        videoPlayerComponent.Dispose();
        videoMaterialComponent.Dispose();
        visibilityComponent.Dispose();
        uiContainerComponent.Dispose();
        uiInputResultsComponent.Dispose();
        sceneBoundsCheckComponent.Dispose();
        audioSourceComponent.Dispose();
        PointerEventsComponent.Dispose();
        RegisteredUiPointerEventsComponent.Dispose();
        GltfContainerLoadingStateComponent.Dispose();
        EngineInfo.Dispose();
    }

    public void MarkDirtyComponentsUpdate()
    {
        var markAsDirty = markAsDirtyComponents.Pairs;

        for (int i = 0; i < markAsDirty.Count; i++)
        {
            int sceneNumber = markAsDirty[i].key.SceneNumber;
            long entityId = markAsDirty[i].key.EntityId;
            int componentId = markAsDirty[i].key.ComponentId;
            InternalComponent data = markAsDirty[i].value.Data;
            bool isRemoval = markAsDirty[i].value.IsDelayedRemoval;

            if (!crdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor crdtExecutor))
            {
                continue;
            }

            data._dirty = true;
            crdtExecutor.ExecuteWithoutStoringState(entityId, componentId, data);
            removeAsDirtyComponents[markAsDirty[i].key] = new ComponentWriteData(data, isRemoval);
        }

        markAsDirtyComponents.Clear();
    }

    public void ResetDirtyComponentsUpdate()
    {
        var resetDirtyComponents = removeAsDirtyComponents.Pairs;

        for (int i = 0; i < resetDirtyComponents.Count; i++)
        {
            int sceneNumber = resetDirtyComponents[i].key.SceneNumber;
            long entityId = resetDirtyComponents[i].key.EntityId;
            int componentId = resetDirtyComponents[i].key.ComponentId;
            InternalComponent data = resetDirtyComponents[i].value.Data;
            bool isRemoval = resetDirtyComponents[i].value.IsDelayedRemoval;

            if (!crdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor crdtExecutor))
            {
                continue;
            }

            if (isRemoval)
            {
                crdtExecutor.ExecuteWithoutStoringState(entityId, componentId, null);
            }
            else
            {
                data._dirty = false;
                crdtExecutor.ExecuteWithoutStoringState(entityId, componentId, data);
            }
        }

        removeAsDirtyComponents.Clear();
    }
}
