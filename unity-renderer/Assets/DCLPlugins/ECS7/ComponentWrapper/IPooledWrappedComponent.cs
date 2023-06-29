using System;

namespace DCL.ECS7.ComponentWrapper
{
    public interface IPooledWrappedComponent : IDisposable
    {
        IWrappedComponent WrappedComponentBase { get; }
    }
}
