using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace DCL
{
    public class MemoryManager
    {
        private const uint MAX_USED_MEMORY = 1300 * 1024 * 1024;
        private const float TIME_FOR_NEW_MEMORY_CHECK = 1.0f;

        private List<object> idsToCleanup = new List<object>();

        public void Initialize()
        {
            CoroutineStarter.Start(AutoCleanup());
        }

        public MemoryManager()
        {
            CommonScriptableObjects.rendererState.OnChange += (isEnable, prevState) =>
            {
                if (isEnable)
                {
                    Environment.i.parcelScenesCleaner.ForceCleanup();
                    Resources.UnloadUnusedAssets();
                }
            };
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
                    yield return CleanupPoolsIfNeeded();
                }

                yield return new WaitForSecondsRealtime(TIME_FOR_NEW_MEMORY_CHECK);
            }
        }

        private bool NeedsCleanup(Pool pool, bool forceCleanup = false)
        {
            if (forceCleanup)
                return true;

            if (pool.persistent)
                return false;

            return pool.usedObjectsCount == 0;
        }

        public IEnumerator CleanupPoolsIfNeeded(bool forceCleanup = false)
        {
            using (var iterator = PoolManager.i.pools.GetEnumerator())
            {
                idsToCleanup.Clear();

                while (iterator.MoveNext())
                {
                    Pool pool = iterator.Current.Value;

                    if (NeedsCleanup(pool, forceCleanup))
                    {
                        idsToCleanup.Add(pool.id);
                    }
                }
            }

            int count = idsToCleanup.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    PoolManager.i.RemovePool(idsToCleanup[i]);
                    yield return null;
                }
            }
        }
    }
}