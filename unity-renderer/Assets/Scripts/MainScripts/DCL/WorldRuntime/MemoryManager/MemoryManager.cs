using DCL.Interface;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace DCL
{
    public class MemoryManager : IMemoryManager
    {
        // TODO: Increase this since the top is now 4GB instead of 2?
        private const long MAX_USED_MEMORY = 1300 * 1024 * 1024; // 1.3GB

        private const float TIME_FOR_NEW_MEMORY_CHECK = 60.0f;

        private Coroutine autoCleanupCoroutine;

        private long memoryThresholdForCleanup = 0;
        private float cleanupInterval;

        public event System.Action OnCriticalMemory;

        private const string DISABLE_MEMORY_MANAGER = "DISABLE_MEMORY_MANAGER";
        private const string OVERRIDE_TIMESCALE = "OVERRIDE_TIMESCALE";
        public MemoryManager(long memoryThresholdForCleanup, float cleanupInterval)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (WebInterface.CheckURLParam(OVERRIDE_TIMESCALE))
            {
                Debug.Log("PRAVS - OVERRIDING TIMESCALE...");
                Time.timeScale = float.Parse(WebInterface.GetURLParam(OVERRIDE_TIMESCALE));
            }
            if (WebInterface.CheckURLParam(DISABLE_MEMORY_MANAGER))
            {
                Debug.Log("PRAVS - DISABLED MEMORY MANAGER...");
                return;
            }
#endif

            this.memoryThresholdForCleanup = memoryThresholdForCleanup;
            this.cleanupInterval = cleanupInterval;
            autoCleanupCoroutine = CoroutineStarter.Start(AutoCleanup());
        }

        public MemoryManager()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (WebInterface.CheckURLParam(OVERRIDE_TIMESCALE))
            {
                Debug.Log("PRAVS - OVERRIDING TIMESCALE...");
                Time.timeScale = float.Parse(WebInterface.GetURLParam(OVERRIDE_TIMESCALE));
            }
            if (WebInterface.CheckURLParam(DISABLE_MEMORY_MANAGER))
            {
                Debug.Log("PRAVS - DISABLED MEMORY MANAGER...");
                return;
            }
#endif

            this.memoryThresholdForCleanup = MAX_USED_MEMORY;
            this.cleanupInterval = TIME_FOR_NEW_MEMORY_CHECK;
            autoCleanupCoroutine = CoroutineStarter.Start(AutoCleanup());
        }

        public void Dispose()
        {
            if (autoCleanupCoroutine != null)
                CoroutineStarter.Stop(autoCleanupCoroutine);

            autoCleanupCoroutine = null;
        }

        public void Initialize()
        {
        }

        bool NeedsMemoryCleanup()
        {
            long usedMemory = Profiler.GetTotalAllocatedMemoryLong() + Profiler.GetMonoUsedSizeLong() +
                              Profiler.GetAllocatedMemoryForGraphicsDriver();

            bool returnValue = usedMemory >= this.memoryThresholdForCleanup;
            if(returnValue)
                Debug.Log($"PRAVS - MEMORY MANAGER CLEANUP - used memory: {usedMemory} / {this.memoryThresholdForCleanup}");

            return returnValue;
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

                yield return new WaitForSecondsRealtime(this.cleanupInterval);
            }
        }

        public IEnumerator CleanPoolManager(bool forceCleanup = false, bool immediate = false)
        {
            bool unusedOnly = true;
            bool nonPersistentOnly = true;

            if (forceCleanup)
            {
                unusedOnly = false;
                nonPersistentOnly = false;
            }

            if (immediate)
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
