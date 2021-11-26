using System;
using DCL.Components;

namespace DCL
{
    public interface IRuntimeComponentFactory : IDisposable
    {
        IComponent CreateComponent(int classId);
    }
}