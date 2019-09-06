using UnityEngine;

namespace DCL
{
    public class PoolableObject : MonoBehaviour
    {
        public Pool pool;

        void OnEnable()
        {
            if (pool != null)
            {
                pool.OnReleaseAll -= this.OnRelease;
                pool.OnReleaseAll += this.OnRelease;
            }
        }

        private void OnDisable()
        {
            if (pool != null)
                pool.OnReleaseAll -= this.OnRelease;
        }

        public void OnRelease(Pool pool)
        {
            Release();
        }

        public void Release()
        {
            if (this == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Release == null??! This shouldn't happen");
#endif
                return;
            }

            if (pool != null && gameObject.activeSelf)
            {
                pool.Release(this);
            }
        }

        public void OnCleanup(DCL.ICleanableEventDispatcher sender)
        {
            sender.OnCleanupEvent -= this.OnCleanup;
            Release();
        }

        void OnDestroy()
        {
            if (pool != null)
            {
                pool.OnReleaseAll -= this.OnRelease;
                pool.RemoveFromPool(this);
            }
        }
    }
}
