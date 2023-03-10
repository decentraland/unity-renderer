using DCL;
using DCL.ECSComponents;
using ECSSystems.BillboardSystem;
using ECSSystems.CameraSystem;
using ECSSystems.ECSSceneBoundsCheckerSystem;
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
    private readonly ECSSceneBoundsCheckerSystem sceneBoundsCheckerSystem;
    private readonly GameObject hoverCanvas;
    private readonly GameObject scenesUi;
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;

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

        BaseVariable<bool> loadingScreenVisible;

        loadingScreenVisible = dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible;

        uiSystem = new ECSScenesUiSystem(scenesUiDocument,
            context.internalEcsComponents.uiContainerComponent,
            DataStore.i.ecs7.scenes, Environment.i.world.state, loadingScreenVisible);

        billboardSystem = new ECSBillboardSystem(context.billboards, DataStore.i.camera);
        videoPlayerSystem = new ECSVideoPlayerSystem(context.internalEcsComponents.videoPlayerComponent, context.internalEcsComponents.videoMaterialComponent);

        cameraEntitySystem = new ECSCameraEntitySystem(context.componentWriter, new PBCameraMode(), new PBPointerLock(),
            DataStore.i.ecs7.scenes, DataStore.i.camera.transform, CommonScriptableObjects.worldOffset, CommonScriptableObjects.cameraMode);

        playerTransformSystem = new ECSPlayerTransformSystem(context.componentWriter, DataStore.i.ecs7.scenes,
            DataStore.i.world.avatarTransform, CommonScriptableObjects.worldOffset);

        uiInputSenderSystem = new ECSUIInputSenderSystem(context.internalEcsComponents.uiInputResultsComponent, context.componentWriter);

        sceneBoundsCheckerSystem = new ECSSceneBoundsCheckerSystem(
            DataStore.i.ecs7.scenes,
            context.internalEcsComponents.sceneBoundsCheckComponent,
            context.internalEcsComponents.visibilityComponent,
            context.internalEcsComponents.renderersComponent,
            context.internalEcsComponents.onPointerColliderComponent,
            context.internalEcsComponents.physicColliderComponent,
            context.internalEcsComponents.audioSourceComponent,
            DataStore.i.debugConfig.isDebugMode.Get());

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
                context.pointerEvents,
                interactionHoverCanvas,
                Environment.i.world.state,
                DataStore.i.ecs7),
            ECSInputSenderSystem.CreateSystem(context.internalEcsComponents.inputEventResultsComponent, context.componentWriter),
            billboardSystem.Update,
            videoPlayerSystem.Update,
        };

        lateUpdateSystems = new ECS7System[]
        {
            uiInputSenderSystem.Update, // Input detection happens during Update() so this system has to run in LateUpdate()
            cameraEntitySystem.Update,
            playerTransformSystem.Update,
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
