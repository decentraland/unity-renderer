using System;
using System.Collections.Generic;
using DCL;
using ECSSystems.CameraSystem;

public delegate void ECS7System();

public class ECSSystemsController : IDisposable
{
    private readonly IList<ECS7System> runningSystems;
    private readonly IUpdateEventHandler updateEventHandler;

    public ECSSystemsController(IUpdateEventHandler updateEventHandler)
    {
        this.updateEventHandler = updateEventHandler;
        updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);

        runningSystems = new List<ECS7System>(1)
        {
            ECSCameraSystem.Update
        };
    }

    public void Dispose()
    {
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
    }

    private void Update()
    {
        int count = runningSystems.Count;
        for (int i = 0; i < count; i++)
        {
            runningSystems[i].Invoke();
        }
    }
}