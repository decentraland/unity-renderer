using System;
using System.Collections;

namespace DCL
{
    public interface IMemoryManager : IService
    {
        public event System.Action OnCriticalMemory;
    }
}