using MainScripts.DCL.Helpers.Utils;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.CommonBehavior
{
    /// <summary>
    /// Represents Poolable behaviour of the map object
    /// </summary>
    internal struct MapMarkerPoolableBehavior<T> where T : MonoBehaviour
    {
        internal readonly IUnityObjectPool<T> objectsPool;

        internal T instance { get; private set; }

        internal bool isVisible { get; private set; }

        internal Vector3 currentPosition { get; private set; }

        internal MapMarkerPoolableBehavior(IUnityObjectPool<T> objectsPool) : this()
        {
            this.objectsPool = objectsPool;
        }

        public void SetCurrentPosition(Vector3 pos)
        {
            currentPosition = pos;

            if (isVisible)
                instance.transform.localPosition = pos;
        }

        public T OnBecameVisible()
        {
            instance = objectsPool.Get();
            instance.transform.localPosition = currentPosition;
            isVisible = true;
            return instance;
        }

        public void OnBecameInvisible()
        {
            if (instance)
            {
                objectsPool.Release(instance);
                instance = null;
            }

            isVisible = false;
        }
    }
}
