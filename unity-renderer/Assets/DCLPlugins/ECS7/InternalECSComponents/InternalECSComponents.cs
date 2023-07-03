using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using System;
using System.Collections.Generic;

public class InternalECSComponents : IDisposable, IInternalECSComponents, IComponentDirtySystem
{
    public event Action MarkComponentsAsDirty;
    public event Action RemoveComponentsAsDirty;

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
        texturizableComponent = new InternalECSComponent<InternalTexturizable>(
            InternalECSComponentsId.TEXTURIZABLE,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalTexturizable>(
                () => texturizableComponent, model => model.renderers.Count == 0),
            crdtExecutors,
            this);

        materialComponent = new InternalECSComponent<InternalMaterial>(
            InternalECSComponentsId.MATERIAL,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this);

        onPointerColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_POINTER,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalColliders>(
                () => onPointerColliderComponent, model => model.colliders.Count == 0),
            crdtExecutors,
            this);

        physicColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_PHYSICAL,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalColliders>(
                () => physicColliderComponent, model => model.colliders.Count == 0),
            crdtExecutors,
            this);

        customLayerColliderComponent = new InternalECSComponent<InternalColliders>(
            InternalECSComponentsId.COLLIDER_CUSTOM,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalColliders>(
                () => customLayerColliderComponent, model => model.colliders.Count == 0),
            crdtExecutors,
            this);

        renderersComponent = new InternalECSComponent<InternalRenderers>(
            InternalECSComponentsId.RENDERERS,
            componentsManager,
            componentsFactory,
            () => new RemoveOnConditionHandler<InternalRenderers>(
                () => renderersComponent, model => model.renderers.Count == 0),
            crdtExecutors,
            this);

        visibilityComponent = new InternalECSComponent<InternalVisibility>(
            InternalECSComponentsId.VISIBILITY,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this);

        inputEventResultsComponent = new InternalECSComponent<InternalInputEventResults>(
            InternalECSComponentsId.INPUT_EVENTS_RESULT,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this);

        uiContainerComponent = new InternalECSComponent<InternalUiContainer>(
            InternalECSComponentsId.UI_CONTAINER,
            componentsManager,
            componentsFactory,
            () => new UiContainerHandler(() => uiContainerComponent),
            crdtExecutors,
            this);

        uiInputResultsComponent = new InternalECSComponent<InternalUIInputResults>(
            InternalECSComponentsId.UI_INPUT_EVENTS_RESULT,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this
        );

        videoPlayerComponent = new InternalECSComponent<InternalVideoPlayer>(
            InternalECSComponentsId.VIDEO_PLAYER,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this);

        videoMaterialComponent = new InternalECSComponent<InternalVideoMaterial>(
            InternalECSComponentsId.VIDEO_MATERIAL,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this);

        sceneBoundsCheckComponent = new InternalECSComponent<InternalSceneBoundsCheck>(
            InternalECSComponentsId.SCENE_BOUNDS_CHECK,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this
        );

        audioSourceComponent = new InternalECSComponent<InternalAudioSource>(
            InternalECSComponentsId.AUDIO_SOURCE,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this
        );

        PointerEventsComponent = new InternalECSComponent<InternalPointerEvents>(
            InternalECSComponentsId.POINTER_EVENTS,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this
        );

        RegisteredUiPointerEventsComponent = new InternalECSComponent<InternalRegisteredUiPointerEvents>(
            InternalECSComponentsId.REGISTERED_UI_POINTER_EVENTS,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this
        );

        raycastComponent = new InternalECSComponent<InternalRaycast>(
            InternalECSComponentsId.RAYCAST,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this
        );

        GltfContainerLoadingStateComponent = new InternalECSComponent<InternalGltfContainerLoadingState>(
            InternalECSComponentsId.GLTF_CONTAINER_LOADING_STATE,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this
        );

        EngineInfo = new InternalECSComponent<InternalEngineInfo>(
            InternalECSComponentsId.ENGINE_INFO,
            componentsManager,
            componentsFactory,
            null,
            crdtExecutors,
            this
        );
    }

    public void Dispose()
    {
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
        MarkComponentsAsDirty?.Invoke();
    }

    public void ResetDirtyComponentsUpdate()
    {
        RemoveComponentsAsDirty?.Invoke();
    }
}
