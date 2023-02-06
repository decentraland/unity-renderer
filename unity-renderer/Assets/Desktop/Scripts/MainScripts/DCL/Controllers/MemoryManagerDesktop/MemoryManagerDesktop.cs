using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace DCL
{
    public class MemoryManagerDesktop : IMemoryManager
    {
        public event System.Action OnCriticalMemory;

        public void Dispose() { }

        public void Initialize() { }
    }
}
