using System;

namespace DCL
{
    public interface IPhysicsSyncController : IDisposable
    {
        bool isDirty { get; }
        void MarkDirty();
        void Sync();
    }
}