using System;
using System.Collections;

namespace DCL
{
    public interface IMemoryManager : IDisposable
    {
        public event System.Action OnCriticalMemory;
    }
}