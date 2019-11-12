using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class PoolableObject : MonoBehaviour
    {
        public Pool pool;
        public LinkedListNode<PoolableObject> node;

        public bool isInsidePool { get { return node == null; } }

        public System.Action OnRelease;

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
                pool.RemoveFromPool(this);
            }
        }
    }
}
