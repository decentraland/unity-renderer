using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSyncController
{
    public bool isDirty { get; private set; } = false;

    public PhysicsSyncController()
    {
        Physics.autoSimulation = false;
        Physics.autoSyncTransforms = false;
    }

    public void MarkDirty()
    {
        isDirty = true;
    }

    public void Sync()
    {
        if (!isDirty)
            return;

        isDirty = false;
        Physics.SyncTransforms();
    }
}