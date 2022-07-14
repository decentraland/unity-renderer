using System;
using System.Collections.Generic;
using DCL;
using ECSSystems.CameraSystem;
using ECSSystems.PlayerSystem;

public delegate void ECS7System();

public class ECSSystemsController : IDisposable
{
    private readonly IList<ECS7System> updateSystems;
    private readonly IList<ECS7System> lateUpdateSystems;
    private readonly IUpdateEventHandler updateEventHandler;
    private readonly ECS7System componentWriteSystem;

    public ECSSystemsController(IUpdateEventHandler updateEventHandler, ECS7System componentWriteSystem)
    {
        this.updateEventHandler = updateEventHandler;
        this.componentWriteSystem = componentWriteSystem;

        updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

        updateSystems = new ECS7System[] { };

        lateUpdateSystems = new ECS7System[]
        {
            ECSCameraSystem.Update,
            ECSPlayerSystem.Update
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