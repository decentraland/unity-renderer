using UnityEngine;

namespace DCL
{
    public static class PoolManagerFactory
    {
        public const string EMPTY_GO_POOL_NAME = "Empty";

        public static void EnsureEntityPool(bool prewarm) // TODO: Move to PoolManagerFactory
        {
            if (PoolManager.i.ContainsPool(EMPTY_GO_POOL_NAME))
                return;

            GameObject go = new GameObject();
            Pool pool = PoolManager.i.AddPool(EMPTY_GO_POOL_NAME, go, maxPrewarmCount: 1000, isPersistent: true);

            if (prewarm)
                pool.ForcePrewarm();
        }
    }
}