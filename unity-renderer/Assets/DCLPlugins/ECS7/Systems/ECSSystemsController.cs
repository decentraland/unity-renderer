using DCL;
using ECSSystems.BillboardSystem;
using ECSSystems.CameraSystem;
using ECSSystems.ECSEngineInfoSystem;
using ECSSystems.ECSRaycastSystem;
using ECSSystems.ECSSceneBoundsCheckerSystem;
using ECSSystems.ECSUiPointerEventsSystem;
using ECSSystems.GltfContainerLoadingStateSystem;
using ECSSystems.InputSenderSystem;
using ECSSystems.MaterialSystem;
using ECSSystems.PlayerSystem;
using ECSSystems.PointerInputSystem;
using ECSSystems.ScenesUiSystem;
using ECSSystems.UiCanvasInformationSystem;
using ECSSystems.UIInputSenderSystem;
using ECSSystems.VideoPlayerSystem;
using ECSSystems.VisibilitySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ECS7System = System.Action;
using Environment = DCL.Environment;
using Object = UnityEngine.Object;

public class ECSSystemsController : IDisposable
{
    private readonly IList<ECS7System> updateSystems;
    private readonly IList<ECS7System> lateUpdateSystems;
    private readonly IUpdateEventHandler updateEventHandler;
    private readonly ECS7System internalComponentMarkDirtySystem;
    private readonly ECS7System internalComponentRemoveDirtySystem;
    private readonly ECSScenesUiSystem uiSystem;
    private readonly ECSCameraEntitySystem cameraEntitySystem;
    private readonly ECSPlayerTransformSystem playerTransformSystem;
    private readonly ECSSceneBoundsCheckerSystem sceneBoundsCheckerSystem;
    private readonly ECSUiCanvasInformationSystem uiCanvasInformationSystem;
    private readonly GameObject hoverCanvas;
    private readonly GameObject scenesUi;
    private readonly IWorldState worldState;

    public ECSSystemsController(SystemsContext context)
    {
        this.worldState = Environment.i.world.state;
        this.updateEventHandler = Environment.i.platform.updateEventHandler;
        this.internalComponentMarkDirtySystem = context.InternalEcsComponents.MarkDirtyComponentsUpdate;
        this.internalComponentRemoveDirtySystem = context.InternalEcsComponents.ResetDirtyComponentsUpdate;

        var canvas = Resources.Load<GameObject>("ECSInteractionHoverCanvas");
        hoverCanvas = Object.Instantiate(canvas);
        hoverCanvas.name = "_ECSInteractionHoverCanvas";
        IECSInteractionHoverCanvas interactionHoverCanvas = hoverCanvas.GetComponent<IECSInteractionHoverCanvas>();

        var scenesUiResource = Resources.Load<UIDocument>("ScenesUI");
        var scenesUiDocument = Object.Instantiate(scenesUiResource);
        scenesUiDocument.name = "_ECSScenesUI";
        scenesUi = scenesUiDocument.gameObject;

        uiSystem = new ECSScenesUiSystem(
            scenesUiDocument,
            context.InternalEcsComponents.uiContainerComponent,
            DataStore.i.ecs7.scenes,
            Environment.i.world.state,
            CommonScriptableObjects.allUIHidden,
            DataStore.i.HUDs.isCurrentSceneUiEnabled,
            DataStore.i.HUDs.isSceneUiEnabled);

        ECSBillboardSystem billboardSystem = new ECSBillboardSystem(context.Billboards, DataStore.i.camera);

        ECSVideoPlayerSystem videoPlayerSystem = new ECSVideoPlayerSystem(
            context.InternalEcsComponents.videoPlayerComponent,
            context.InternalEcsComponents.videoMaterialComponent,
            context.InternalEcsComponents.EngineInfo,
            context.ComponentWriters,
            context.VideoEventPool);

        cameraEntitySystem = new ECSCameraEntitySystem(context.ComponentWriters,
            context.CameraModePool, context.PointerLockPool, context.TransformPool, context.TransformComponent,
            DataStore.i.ecs7.scenes, DataStore.i.camera.transform, CommonScriptableObjects.worldOffset, CommonScriptableObjects.cameraMode);

        playerTransformSystem = new ECSPlayerTransformSystem(context.ComponentWriters, context.TransformPool,
            context.TransformComponent, DataStore.i.ecs7.scenes,
            DataStore.i.world.avatarTransform, CommonScriptableObjects.worldOffset);

        ECSUIInputSenderSystem uiInputSenderSystem = new ECSUIInputSenderSystem(
            context.InternalEcsComponents.uiInputResultsComponent,
            context.ComponentWriters);

        ECSRaycastSystem raycastSystem = new ECSRaycastSystem(
            context.InternalEcsComponents.raycastComponent,
            context.InternalEcsComponents.physicColliderComponent,
            context.InternalEcsComponents.onPointerColliderComponent,
            context.InternalEcsComponents.customLayerColliderComponent,
            context.InternalEcsComponents.EngineInfo,
            context.ComponentWriters,
            context.RaycastResultPool);

        sceneBoundsCheckerSystem = new ECSSceneBoundsCheckerSystem(
            DataStore.i.ecs7.scenes,
            context.InternalEcsComponents.sceneBoundsCheckComponent,
            context.InternalEcsComponents.visibilityComponent,
            context.InternalEcsComponents.renderersComponent,
            context.InternalEcsComponents.onPointerColliderComponent,
            context.InternalEcsComponents.physicColliderComponent,
            DataStore.i.debugConfig.isDebugMode.Get());

        ECSUiPointerEventsSystem uiPointerEventsSystem = new ECSUiPointerEventsSystem(
            context.InternalEcsComponents.RegisteredUiPointerEventsComponent,
            context.InternalEcsComponents.inputEventResultsComponent,
            context.ComponentGroups.UnregisteredUiPointerEvents,
            context.ComponentGroups.RegisteredUiPointerEvents,
            context.ComponentGroups.RegisteredUiPointerEventsWithUiRemoved,
            context.ComponentGroups.RegisteredUiPointerEventsWithPointerEventsRemoved);

        ECSPointerInputSystem pointerInputSystem = new ECSPointerInputSystem(
            context.InternalEcsComponents.onPointerColliderComponent,
            context.InternalEcsComponents.inputEventResultsComponent,
            context.InternalEcsComponents.PointerEventsComponent,
            interactionHoverCanvas,
            Environment.i.world.state,
            DataStore.i.ecs7);

        GltfContainerLoadingStateSystem gltfContainerLoadingStateSystem = new GltfContainerLoadingStateSystem(
            context.ComponentWriters,
            context.GltfContainerLoadingStatePool,
            context.InternalEcsComponents.GltfContainerLoadingStateComponent);

        ECSEngineInfoSystem engineInfoSystem = new ECSEngineInfoSystem(
            context.ComponentWriters,
            context.EngineInfoPool,
            context.InternalEcsComponents.EngineInfo);

        uiCanvasInformationSystem = new ECSUiCanvasInformationSystem(
            context.ComponentWriters,
            context.UiCanvasInformationPool,
            DataStore.i.ecs7.scenes
        );

        ECSInputSenderSystem inputSenderSystem = new ECSInputSenderSystem(
            context.InternalEcsComponents.inputEventResultsComponent,
            context.InternalEcsComponents.EngineInfo,
            context.ComponentWriters,
            context.PointerEventsResultPool,
            () => worldState.GetCurrentSceneNumber());

        updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

        updateSystems = new ECS7System[]
        {
            engineInfoSystem.Update,
            ECSTransformParentingSystem.CreateSystem(context.InternalEcsComponents.sceneBoundsCheckComponent),
            ECSMaterialSystem.CreateSystem(context.ComponentGroups.texturizableGroup,
                context.InternalEcsComponents.texturizableComponent, context.InternalEcsComponents.materialComponent),
            ECSVisibilitySystem.CreateSystem(context.ComponentGroups.visibilityGroup,
                context.InternalEcsComponents.renderersComponent, context.InternalEcsComponents.visibilityComponent),
            uiSystem.Update,
            pointerInputSystem.Update,
            billboardSystem.Update,
            videoPlayerSystem.Update,
            uiCanvasInformationSystem.Update
        };

        lateUpdateSystems = new ECS7System[]
        {
            uiPointerEventsSystem.Update,
            uiInputSenderSystem.Update, // Input detection happens during Update() so this system has to run in LateUpdate()
            inputSenderSystem.Update,
            cameraEntitySystem.Update,
            playerTransformSystem.Update,
            gltfContainerLoadingStateSystem.Update,
            raycastSystem.Update, // Should always be after player/entity transformations update
            sceneBoundsCheckerSystem.Update // Should always be the last system
        };
    }

    public void Dispose()
    {
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
        uiSystem.Dispose();
        cameraEntitySystem.Dispose();
        playerTransformSystem.Dispose();
        sceneBoundsCheckerSystem.Dispose();
        uiCanvasInformationSystem.Dispose();
        Object.Destroy(hoverCanvas);
        Object.Destroy(scenesUi);
    }

    private void Update()
    {
        try
        {
            internalComponentMarkDirtySystem.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        int count = updateSystems.Count;

        for (int i = 0; i < count; i++)
        {
            try
            {
                updateSystems[i].Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private void LateUpdate()
    {
        int count = lateUpdateSystems.Count;

        for (int i = 0; i < count; i++)
        {
            try
            {
                lateUpdateSystems[i].Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        try
        {
            internalComponentRemoveDirtySystem.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
