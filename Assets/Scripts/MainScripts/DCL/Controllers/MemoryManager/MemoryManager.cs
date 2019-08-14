using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MemoryManager : MonoBehaviour
    {
        private const float TIME_TO_POOL_CLEANUP = 60.0f;
        private const float MIN_TIME_BETWEEN_UNLOAD_ASSETS = 10.0f;
        private float lastTimeUnloadUnusedAssets = 0;

        void Start()
        {
            StartCoroutine(AutoCleanup());
        }

        // TODO: here we'll define cleanup criteria
        bool NeedsMemoryCleanup()
        {
            return true;
        }

        IEnumerator AutoCleanup()
        {
            while (true)
            {
                if (NeedsMemoryCleanup())
                {
                    yield return CleanupPoolsIfNeeded();
                }

                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        private bool NeedsCleanup(Pool pool)
        {
            return Time.realtimeSinceStartup - pool.lastGetTime >= TIME_TO_POOL_CLEANUP && pool.activeCount == 0;
        }

        private IEnumerator CleanupPoolsIfNeeded()
        {
            using (var iterator = PoolManager.i.pools.GetEnumerator())
            {
                List<object> idsToCleanup = new List<object>();

                while (iterator.MoveNext())
                {
                    Pool pool = iterator.Current.Value;

                    if (NeedsCleanup(pool))
                    {
                        idsToCleanup.Add(pool.id);
                    }
                }

                int count = idsToCleanup.Count;

                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        PoolManager.i.CleanupPool(idsToCleanup[i]);
                        yield return null;
                    }

                    if (Time.realtimeSinceStartup - lastTimeUnloadUnusedAssets >= MIN_TIME_BETWEEN_UNLOAD_ASSETS)
                    {
                        lastTimeUnloadUnusedAssets = Time.realtimeSinceStartup;
                        Resources.UnloadUnusedAssets();
                    }
                }
            }
        }
    }
}