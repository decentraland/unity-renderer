using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class PoolableObject : MonoBehaviour
    {
        public Pool pool;

        public bool isInsidePool { get { return pool.IsInPool(this); } }
        public bool isOutsidePool { get { return pool.IsOutOfPool(this); } }

        public System.Action OnRelease;

        void OnEnable()
        {
            if (pool != null)
            {
                pool.OnReleaseAll -= this.Release;
                pool.OnReleaseAll += this.Release;
            }
        }

        private void OnDisable()
        {
            if (pool != null)
                pool.OnReleaseAll -= this.Release;
        }

        public void Release(Pool releasePool = null)
        {
            if (this == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Release == null??! This shouldn't happen");
#endif
                return;
            }

            if (pool != null)
            {
                pool.Release(this);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("Pool is null upon release!");
#endif
            }

            OnRelease?.Invoke();
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
                pool.OnReleaseAll -= this.Release;
                pool.RemoveFromPool(this);
            }
        }
    }
}
