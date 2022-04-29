using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class PhysicsSyncController : IPhysicsSyncController
    {
        public bool isDirty { get; private set; } = false;

        public PhysicsSyncController()
        {
            Physics.autoSimulation = false;
            Physics.autoSyncTransforms = false;

            PoolManager.i.OnGet -= MarkDirty;
            PoolManager.i.OnGet += MarkDirty;
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

        public void Dispose()
        {
            PoolManager.i.OnGet -= MarkDirty;
        }

        public void Initialize()
        {
        }
    }
}