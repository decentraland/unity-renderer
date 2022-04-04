using System;
using DCL.Components;

namespace DCL
{
    public interface IRuntimeComponentFactory : IService
    {
        IComponent CreateComponent(int classId);

        void RegisterBuilder(int classId, Func<IComponent> builder);

        void UnregisterBuilder(int classId);
    }
}