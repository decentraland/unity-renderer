using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace DCL
{
    public class MemoryManager : IMemoryManager
    {
        private const uint MAX_USED_MEMORY = 1300 * 1024 * 1024;
        private const float TIME_FOR_NEW_MEMORY_CHECK = 1.0f;

        private Coroutine autoCleanupCoroutine;

        private uint memoryThresholdForCleanup = 0;
        private float cleanupInterval;

        public event System.Action OnCriticalMemory;

        public MemoryManager (uint memoryThresholdForCleanup, float cleanupInterval)
        {
            this.memoryThresholdForCleanup = this.memoryThresholdForCleanup;
            this.cleanupInterval = this.cleanupInterval;
            autoCleanupCoroutine = CoroutineStarter.Start(AutoCleanup());
        }

        public MemoryManager ()
        {
            this.memoryThresholdForCleanup = MAX_USED_MEMORY;
            this.cleanupInterval = TIME_FOR_NEW_MEMORY_CHECK;
            autoCleanupCoroutine = CoroutineStarter.Start(AutoCleanup());
        }

        public void Dispose()
        {
            PoolManager.i.Cleanup();

            if (autoCleanupCoroutine != null)
                CoroutineStarter.Stop(autoCleanupCoroutine);

            autoCleanupCoroutine = null;
        }

        bool NeedsMemoryCleanup()
        {
            long usedMemory = Profiler.GetTotalAllocatedMemoryLong() + Profiler.GetMonoUsedSizeLong() + Profiler.GetAllocatedMemoryForGraphicsDriver();
            return usedMemory >= MAX_USED_MEMORY;
        }

        IEnumerator AutoCleanup()
        {
            while (true)
            {
                if (NeedsMemoryCleanup())
                {
                    OnCriticalMemory?.Invoke();
                    yield return CleanPoolManager();
                    Resources.UnloadUnusedAssets();
                }

                yield return new WaitForSecondsRealtime(TIME_FOR_NEW_MEMORY_CHECK);
            }
        }

        public IEnumerator CleanPoolManager(bool forceCleanup = false, bool immediate = false)
        {
            bool unusedOnly = true;
            bool nonPersistentOnly = true;

            if ( forceCleanup )
            {
                unusedOnly = false;
                nonPersistentOnly = false;
            }

            if ( immediate )
            {
                PoolManager.i.Cleanup(unusedOnly, nonPersistentOnly);
            }
            else
            {
                yield return PoolManager.i.CleanupAsync(unusedOnly, nonPersistentOnly, false);
            }
        }
    }
}