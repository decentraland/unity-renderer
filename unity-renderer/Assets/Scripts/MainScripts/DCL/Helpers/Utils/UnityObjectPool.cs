using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace MainScripts.DCL.Helpers.Utils
{
    public interface IUnityObjectPool<T> : IObjectPool<T> where T : Component
    {
        T Prefab { get; }

        void Prewarm(int count);

        UniTask PrewarmAsync(int count, int createPerFrame, CancellationToken ct);
    }

    public class UnityObjectPool<T> : IUnityObjectPool<T> where T: Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Vector3 defaultPosition;
        private readonly Quaternion defaultRot;
        private readonly Action<T> actionOnCreate;

        private readonly ObjectPool<T> internalPool;

        public UnityObjectPool(
            T prefab,
            Transform parent,
            Vector3 defaultPosition = default,
            Quaternion defaultRot = default,
            Action<T> actionOnCreate = null,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            int defaultCapacity = 10,
            int maxSize = 1000)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.defaultPosition = defaultPosition;
            this.defaultRot = defaultRot;
            this.actionOnCreate = actionOnCreate;

            internalPool = new ObjectPool<T>(
                Instantiate,
                actionOnGet ?? EnableObject,
                actionOnRelease ?? DisableObject,
                actionOnDestroy ?? Destroy,
                Application.isEditor,
                defaultCapacity,
                maxSize
            );
        }

        T IUnityObjectPool<T>.Prefab => prefab;

        public void Prewarm(int count)
        {
            for (var i = 0; i < count; i++)
                Release(Instantiate());
        }

        public async UniTask PrewarmAsync(int count, int createPerFrame, CancellationToken ct)
        {
            while (count > 0)
            {
                for (var i = 0; i < createPerFrame && count > 0; i++)
                {
                    Release(Instantiate());
                    count--;
                }

                await UniTask.DelayFrame(1, cancellationToken: ct);
            }
        }

        private T Instantiate()
        {
            var obj = Object.Instantiate(prefab, defaultPosition, defaultRot, parent);
            actionOnCreate?.Invoke(obj);
            return obj;
        }

        private static void DisableObject(T obj)
        {
            obj.gameObject.SetActive(false);
        }

        private static void EnableObject(T obj)
        {
            obj.gameObject.SetActive(true);
        }

        private void Destroy(T obj)
        {
            if (obj)
                global::DCL.Helpers.Utils.SafeDestroy(obj.gameObject);
        }

        public T Get() =>
            internalPool.Get();

        public PooledObject<T> Get(out T v) =>
            internalPool.Get(out v);

        public void Release(T element)
        {
            internalPool.Release(element);
        }

        public void Clear()
        {
            internalPool.Clear();
        }

        public int CountInactive => internalPool.CountInactive;
    }
}
