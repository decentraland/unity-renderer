using UnityEngine;

namespace DCL
{
    public class PoolableObject : MonoBehaviour
    {
        public Pool pool;

        public bool isAlive
        {
            get;
            private set;
        }

        public void Init()
        {
            pool.OnReleaseAll += this.OnRelease;
            isAlive = true;
        }

        public void Release()
        {
            pool.OnReleaseAll -= this.OnRelease;

            if (pool && isAlive)
                pool.Release(this);

            isAlive = false;
        }

        public void OnCleanup(DCL.ICleanableEventDispatcher sender)
        {
            sender.OnCleanupEvent -= this.OnCleanup;
            Release();
        }

        public void OnRelease(Pool pool)
        {
            Release();
        }

        void OnDestroy()
        {
            if (pool)
            {
                pool.OnReleaseAll -= this.OnRelease;
                pool.Unregister(this);
            }

            isAlive = false;
        }
    }
}