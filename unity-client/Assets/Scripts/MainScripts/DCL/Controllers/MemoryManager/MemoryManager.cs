using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MemoryManager : Singleton<MemoryManager>
    {
        private const float TIME_TO_POOL_CLEANUP = 60.0f;

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
                    MemoryManager.i.CleanupPoolsIfNeeded();
                    ParcelScene.parcelScenesCleaner.ForceCleanup();
                    Resources.UnloadUnusedAssets();
                }
            };
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

        private bool NeedsCleanup(Pool pool, bool forceCleanup = false)
        {
            if (forceCleanup)
                return true;

            if (pool.persistent)
                return false;

            bool timeout = Time.unscaledTime - pool.lastGetTime >= TIME_TO_POOL_CLEANUP;
            return timeout && pool.usedObjectsCount == 0;
        }

        public IEnumerator CleanupPoolsIfNeeded(bool forceCleanup = false)
        {
            using (var iterator = PoolManager.i.pools.GetEnumerator())
            {
                List<object> idsToCleanup = new List<object>();

                while (iterator.MoveNext())
                {
                    Pool pool = iterator.Current.Value;

                    if (NeedsCleanup(pool, forceCleanup))
                    {
                        idsToCleanup.Add(pool.id);
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
}