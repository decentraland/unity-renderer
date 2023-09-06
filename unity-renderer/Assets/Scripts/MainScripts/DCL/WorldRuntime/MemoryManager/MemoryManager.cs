using DCL.Interface;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace DCL
{
    public class MemoryManager : IMemoryManager
    {
        // TODO: Increase this since the top is now 4GB instead of 2?
        private const ulong MAX_USED_MEMORY = (ulong)2600 * 1024 * 1024; // 2.6GB

        private const float TIME_FOR_NEW_MEMORY_CHECK = 60.0f;

        private Coroutine autoCleanupCoroutine;

        private ulong memoryThresholdForCleanup = 0;
        private float cleanupInterval;

        public event System.Action OnCriticalMemory;

        private const string DISABLE_MEMORY_MANAGER = "DISABLE_MEMORY_MANAGER";
        private const string OVERRIDE_TIMESCALE = "OVERRIDE_TIMESCALE";

        public MemoryManager()
        {
// #if UNITY_WEBGL && !UNITY_EDITOR
            if (WebInterface.CheckURLParam(OVERRIDE_TIMESCALE) && float.TryParse(WebInterface.GetURLParam(OVERRIDE_TIMESCALE), out float result))
            {
                Debug.Log($"PRAVS - OVERRIDING TIMESCALE...{result}");
                Time.timeScale = result;
            }
            if (WebInterface.CheckURLParam(DISABLE_MEMORY_MANAGER))
            {
                Debug.Log("PRAVS - DISABLED MEMORY MANAGER...");
                return;
            }
// #endif

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

        private bool NeedsMemoryCleanup()
        {
            ulong usedMemory = (ulong)Profiler.GetTotalAllocatedMemoryLong() + (ulong)Profiler.GetMonoUsedSizeLong() +
                               (ulong)Profiler.GetAllocatedMemoryForGraphicsDriver();

            bool returnValue = usedMemory >= this.memoryThresholdForCleanup;
            if(returnValue)
                Debug.Log($"PRAVS - MEMORY MANAGER CLEANUP - used memory: {usedMemory} / {this.memoryThresholdForCleanup}");

            return returnValue;
        }

        private IEnumerator AutoCleanup()
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
