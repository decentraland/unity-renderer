using System;

namespace DCL
{
    public interface IAvatarsLODController : IDisposable
    {
        void Update(bool forceUpdate = false);
    }
}