using DCL;
using DCL.ECSComponents;
using ECSSystems.BillboardSystem;
using ECSSystems.CameraSystem;
using ECSSystems.ECSRaycastSystem;
using ECSSystems.ECSSceneBoundsCheckerSystem;
using ECSSystems.ECSUiPointerEventsSystem;
using ECSSystems.InputSenderSystem;
using ECSSystems.MaterialSystem;
using ECSSystems.PlayerSystem;
using ECSSystems.PointerInputSystem;
using ECSSystems.ScenesUiSystem;
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
    private readonly ECS7System componentWriteSystem;
    private readonly ECS7System internalComponentMarkDirtySystem;
    private readonly ECS7System internalComponentRemoveDirtySystem;
    private readonly ECSScenesUiSystem uiSystem;
    private readonly ECSBillboardSystem billboardSystem;
    private readonly ECSCameraEntitySystem cameraEntitySystem;
    private readonly ECSPlayerTransformSystem playerTransformSystem;
    private readonly ECSVideoPlayerSystem videoPlayerSystem;
    private readonly ECSUIInputSenderSystem uiInputSenderSystem;
    private readonly ECSRaycastSystem raycastSystem;
    private readonly ECSSceneBoundsCheckerSystem sceneBoundsCheckerSystem;
    private readonly GameObject hoverCanvas;
    private readonly GameObject scenesUi;

    public ECSSystemsController(ECS7System componentWriteSystem, SystemsContext context)
    {
        this.updateEventHandler = Environment.i.platform.updateEventHandler;
        this.componentWriteSystem = componentWriteSystem;
        this.internalComponentMarkDirtySystem = context.internalEcsComponents.MarkDirtyComponentsUpdate;
        this.internalComponentRemoveDirtySystem = context.internalEcsComponents.ResetDirtyComponentsUpdate;

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
            context.internalEcsComponents.uiContainerComponent,
            DataStore.i.ecs7.scenes,
            Environment.i.world.state,
            CommonScriptableObjects.allUIHidden,
            DataStore.i.HUDs.isSceneUIEnabled);

        billboardSystem = new ECSBillboardSystem(context.billboards, DataStore.i.camera);
        videoPlayerSystem = new ECSVideoPlayerSystem(context.internalEcsComponents.videoPlayerComponent, context.internalEcsComponents.videoMaterialComponent);

        cameraEntitySystem = new ECSCameraEntitySystem(context.componentWriter, new PBCameraMode(), new PBPointerLock(),
            DataStore.i.ecs7.scenes, DataStore.i.camera.transform, CommonScriptableObjects.worldOffset, CommonScriptableObjects.cameraMode);

        playerTransformSystem = new ECSPlayerTransformSystem(context.componentWriter, DataStore.i.ecs7.scenes,
            DataStore.i.world.avatarTransform, CommonScriptableObjects.worldOffset);

        uiInputSenderSystem = new ECSUIInputSenderSystem(context.internalEcsComponents.uiInputResultsComponent, context.componentWriter);

        raycastSystem = new ECSRaycastSystem(
            context.internalEcsComponents.raycastComponent,
            context.internalEcsComponents.physicColliderComponent,
            context.internalEcsComponents.onPointerColliderComponent,
            context.internalEcsComponents.customLayerColliderComponent,
            context.componentWriter);

        sceneBoundsCheckerSystem = new ECSSceneBoundsCheckerSystem(
            DataStore.i.ecs7.scenes,
            context.internalEcsComponents.sceneBoundsCheckComponent,
            context.internalEcsComponents.visibilityComponent,
            context.internalEcsComponents.renderersComponent,
            context.internalEcsComponents.onPointerColliderComponent,
            context.internalEcsComponents.physicColliderComponent,
            context.internalEcsComponents.audioSourceComponent,
            DataStore.i.debugConfig.isDebugMode.Get());

        ECSUiPointerEventsSystem uiPointerEventsSystem = new ECSUiPointerEventsSystem(
            context.internalEcsComponents.RegisteredUiPointerEventsComponent,
            context.internalEcsComponents.inputEventResultsComponent,
            context.componentGroups.UnregisteredUiPointerEvents,
            context.componentGroups.RegisteredUiPointerEvents,
            context.componentGroups.RegisteredUiPointerEventsWithUiRemoved,
            context.componentGroups.RegisteredUiPointerEventsWithPointerEventsRemoved);

        updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

        updateSystems = new ECS7System[]
        {
            ECSTransformParentingSystem.CreateSystem(context.internalEcsComponents.sceneBoundsCheckComponent),
            ECSMaterialSystem.CreateSystem(context.componentGroups.texturizableGroup,
                context.internalEcsComponents.texturizableComponent, context.internalEcsComponents.materialComponent),
            ECSVisibilitySystem.CreateSystem(context.componentGroups.visibilityGroup,
                context.internalEcsComponents.renderersComponent, context.internalEcsComponents.visibilityComponent),
            uiSystem.Update,
            ECSPointerInputSystem.CreateSystem(
                context.internalEcsComponents.onPointerColliderComponent,
                context.internalEcsComponents.inputEventResultsComponent,
                context.internalEcsComponents.PointerEventsComponent,
                interactionHoverCanvas,
                Environment.i.world.state,
                DataStore.i.ecs7,
                DataStore.i.rpc.context.restrictedActions),
            billboardSystem.Update,
            videoPlayerSystem.Update,
        };

        lateUpdateSystems = new ECS7System[]
        {
            uiPointerEventsSystem.Update,
            uiInputSenderSystem.Update, // Input detection happens during Update() so this system has to run in LateUpdate()
            ECSInputSenderSystem.CreateSystem(context.internalEcsComponents.inputEventResultsComponent, context.componentWriter),
            cameraEntitySystem.Update,
            playerTransformSystem.Update,
            raycastSystem.Update, // should always be after player/entity transformations update
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
        Object.Destroy(hoverCanvas);
        Object.Destroy(scenesUi);
    }

    private void Update()
    {
        try
        {
            componentWriteSystem.Invoke();
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
