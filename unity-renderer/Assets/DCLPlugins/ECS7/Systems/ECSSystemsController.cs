using DCL;
using DCL.ECSComponents;
using ECSSystems.BillboardSystem;
using ECSSystems.CameraSystem;
using ECSSystems.InputSenderSystem;
using ECSSystems.MaterialSystem;
using ECSSystems.PlayerSystem;
using ECSSystems.PointerInputSystem;
using ECSSystems.ScenesUiSystem;
using ECSSystems.VideoPlayerSystem;
using ECSSystems.UIInputSenderSystem;
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
    private readonly ECS7System internalComponentWriteSystem;
    private readonly ECSScenesUiSystem uiSystem;
    private readonly ECSBillboardSystem billboardSystem;
    private readonly ECSCameraEntitySystem cameraEntitySystem;
    private readonly ECSPlayerTransformSystem playerTransformSystem;
    private readonly ECSVideoPlayerSystem videoPlayerSystem;
    private readonly ECSUIInputSenderSystem uiInputSenderSystem;
    private readonly GameObject hoverCanvas;
    private readonly GameObject scenesUi;
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;


    public ECSSystemsController(ECS7System componentWriteSystem, SystemsContext context)
    {
        this.updateEventHandler = Environment.i.platform.updateEventHandler;
        this.componentWriteSystem = componentWriteSystem;
        this.internalComponentWriteSystem = context.internalEcsComponents.WriteSystemUpdate;

        var canvas = Resources.Load<GameObject>("ECSInteractionHoverCanvas");
        hoverCanvas = Object.Instantiate(canvas);
        hoverCanvas.name = "_ECSInteractionHoverCanvas";
        IECSInteractionHoverCanvas interactionHoverCanvas = hoverCanvas.GetComponent<IECSInteractionHoverCanvas>();

        var scenesUiResource = Resources.Load<UIDocument>("ScenesUI");
        var scenesUiDocument = Object.Instantiate(scenesUiResource);
        scenesUiDocument.name = "_ECSScenesUI";
        scenesUi = scenesUiDocument.gameObject;

        BaseVariable<bool> loadingScreenVisible;

        if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(DataStore.i.featureFlags.DECOUPLED_LOADING_SCREEN_FF))
            loadingScreenVisible = dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible;
        else
            loadingScreenVisible = dataStoreLoadingScreen.Ref.loadingHUD.visible;


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

        updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

        updateSystems = new ECS7System[]
        {
            ECSTransformParentingSystem.Update,
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
            uiInputSenderSystem.Update,
            billboardSystem.Update,
            videoPlayerSystem.Update,
        };

        lateUpdateSystems = new ECS7System[]
        {
            cameraEntitySystem.Update,
            playerTransformSystem.Update
        };
    }

    public void Dispose()
    {
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
        uiSystem.Dispose();
        cameraEntitySystem.Dispose();
        playerTransformSystem.Dispose();
        Object.Destroy(hoverCanvas);
        Object.Destroy(scenesUi);
    }

    private void Update()
    {
        componentWriteSystem.Invoke();

        int count = updateSystems.Count;

        for (int i = 0; i < count; i++)
        {
            updateSystems[i].Invoke();
        }

        internalComponentWriteSystem.Invoke();
    }

    private void LateUpdate()
    {
        int count = lateUpdateSystems.Count;

        for (int i = 0; i < count; i++)
        {
            lateUpdateSystems[i].Invoke();
        }
    }
}
