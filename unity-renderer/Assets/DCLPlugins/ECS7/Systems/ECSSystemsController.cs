using System;
using System.Collections.Generic;
using DCL;
using ECSSystems.CameraSystem;
using ECSSystems.InputSenderSystem;
using ECSSystems.MaterialSystem;
using ECSSystems.PlayerSystem;
using ECSSystems.PointerInputSystem;
using ECSSystems.ScenesUiSystem;
using ECSSystems.VisibilitySystem;
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
    private readonly GameObject hoverCanvas;
    private readonly GameObject scenesUi;

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

        uiSystem = new ECSScenesUiSystem(scenesUiDocument,
            context.internalEcsComponents.uiContainerComponent,
            DataStore.i.ecs7.scenes, Environment.i.world.state);

        updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

        updateSystems = new ECS7System[]
        {
            ECSTransformParentingSystem.Update,
            ECSMaterialSystem.CreateSystem(context.componentGroups.texturizableGroup,
                context.internalEcsComponents.texturizableComponent, context.internalEcsComponents.materialComponent),
            ECSVisibilitySystem.CreateSystem(context.componentGroups.visibilityGroup,
                context.internalEcsComponents.renderersComponent, context.internalEcsComponents.visibilityComponent),
            ECSPointerInputSystem.CreateSystem(
                context.internalEcsComponents.onPointerColliderComponent,
                context.internalEcsComponents.inputEventResultsComponent,
                context.pointerEvents,
                interactionHoverCanvas,
                Environment.i.world.state,
                DataStore.i.ecs7),
            ECSInputSenderSystem.CreateSystem(context.internalEcsComponents.inputEventResultsComponent, context.componentWriter),
            uiSystem.Update
        };

        lateUpdateSystems = new ECS7System[]
        {
            ECSCameraEntitySystem.CreateSystem(context.componentWriter),
            ECSPlayerTransformSystem.CreateSystem(context.componentWriter)
        };
    }

    public void Dispose()
    {
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
        uiSystem.Dispose();
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