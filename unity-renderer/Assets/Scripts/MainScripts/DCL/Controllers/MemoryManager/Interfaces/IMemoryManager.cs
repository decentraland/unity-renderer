using System;
using System.Collections;

namespace DCL
{
    public interface IMemoryManager : IDisposable
    {
        IEnumerator CleanPoolManager(bool forceCleanup = false, bool immediate = false);
    }
}