using System;
using System.Collections.Generic;
using DCL;
using DCL.ECSRuntime;
using ECSSystems.CameraSystem;
using ECSSystems.PlayerSystem;
using ECS7System = System.Action;

public class ECSSystemsController : IDisposable
{
    private readonly IList<ECS7System> updateSystems;
    private readonly IList<ECS7System> lateUpdateSystems;
    private readonly IUpdateEventHandler updateEventHandler;
    private readonly ECS7System componentWriteSystem;

    public ECSSystemsController(IUpdateEventHandler updateEventHandler, IECSComponentWriter componentWriter, ECS7System componentWriteSystem)
    {
        this.updateEventHandler = updateEventHandler;
        this.componentWriteSystem = componentWriteSystem;

        updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

        updateSystems = new ECS7System[]
        {
            ECSTransformParentingSystem.Update
        };

        lateUpdateSystems = new ECS7System[]
        {
            ECSCameraTransformSystem.CreateSystem(componentWriter),
            ECSPlayerTransformSystem.CreateSystem(componentWriter)
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