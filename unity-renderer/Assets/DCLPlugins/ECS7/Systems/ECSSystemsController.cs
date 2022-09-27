using System;
using System.Collections.Generic;
using DCL;
using ECSSystems.CameraSystem;
using ECSSystems.InputSenderSystem;
using ECSSystems.MaterialSystem;
using ECSSystems.PlayerSystem;
using ECSSystems.PointerInputSystem;
using ECSSystems.VisibilitySystem;
using ECS7System = System.Action;
using Environment = DCL.Environment;

public class ECSSystemsController : IDisposable
{
    private readonly IList<ECS7System> updateSystems;
    private readonly IList<ECS7System> lateUpdateSystems;
    private readonly IUpdateEventHandler updateEventHandler;
    private readonly ECS7System componentWriteSystem;

    public ECSSystemsController(ECS7System componentWriteSystem, SystemsContext context)
    {
        this.updateEventHandler = Environment.i.platform.updateEventHandler;
        this.componentWriteSystem = componentWriteSystem;

        updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

        updateSystems = new ECS7System[]
        {
            context.internalEcsComponents.writeInternalComponentsSystem,
            ECSTransformParentingSystem.Update,
            ECSMaterialSystem.CreateSystem(context.componentGroups.texturizableGroup,
                context.internalEcsComponents.texturizableComponent, context.internalEcsComponents.materialComponent),
            ECSVisibilitySystem.CreateSystem(context.componentGroups.visibilityGroup,
                context.internalEcsComponents.renderersComponent, context.internalEcsComponents.visibilityComponent)
        };

        lateUpdateSystems = new ECS7System[]
        {
            ECSPointerInputSystem.CreateSystem(
                context.internalEcsComponents.onPointerColliderComponent,
                context.internalEcsComponents.inputEventResultsComponent,
                Environment.i.world.state,
                DataStore.i.ecs7),
            ECSInputSenderSystem.CreateSystem(context.internalEcsComponents.inputEventResultsComponent, context.componentWriter),
            ECSCameraEntitySystem.CreateSystem(context.componentWriter),
            ECSPlayerTransformSystem.CreateSystem(context.componentWriter)
        };
    }

    public void Dispose()
    {
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
    }

    private void Update()
    {
        int count = updateSystems.Count;
        for (int i = 0; i < count; i++)
        {
            updateSystems[i].Invoke();
        }
    }

    private void LateUpdate()
    {
        int count = lateUpdateSystems.Count;
        for (int i = 0; i < count; i++)
        {
            lateUpdateSystems[i].Invoke();
        }
        componentWriteSystem.Invoke();
    }
}