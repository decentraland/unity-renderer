using System;

namespace DCL
{
    public interface IPhysicsSyncController : IService
    {
        bool isDirty { get; }
        void MarkDirty();
        void Sync();
    }
}