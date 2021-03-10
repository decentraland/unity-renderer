using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public interface IPhysicsSyncController
    {
        bool isDirty { get; }
        void MarkDirty();
        void Sync();
    }

    public class PhysicsSyncController : IPhysicsSyncController
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
}