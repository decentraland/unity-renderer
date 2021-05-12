using System;
using System.Collections;

namespace DCL
{
    public interface IMemoryManager : IDisposable
    {
        void Initialize(IParcelScenesCleaner parcelScenesCleaner);
        IEnumerator CleanupPoolsIfNeeded(bool forceCleanup = false, bool immediate = false);
    }
}