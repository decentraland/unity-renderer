using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Pool;

namespace MainScripts.DCL.Helpers.Utils
{
    public static class PoolUtils
    {
        public struct ListPoolRent<T> : IDisposable
        {
            private List<T> list;

            public List<T> GetList() =>
                list;

            public ListPoolRent(List<T> list)
            {
                this.list = list;
            }

            public void Dispose()
            {
                ListPool<T>.Release(list);
                list = null;
            }
        }

        /// <summary>
        /// This method uses the Create function provided to the pool on construction
        /// to generate instances. Therefore some allocation cannot be avoided.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        public static void Prewarm<T>(this IObjectPool<T> pool, int count) where T: class
        {
            T[] instances = new T[count];

            for (int i = 0; i < count; i++) { instances[i] = pool.Get(); }

            for (int i = 0; i < count; i++) { pool.Release(instances[i]); }
        }

        /// <summary>
        /// By providing the Create function, this method can Prewarm a pool without extra allocation.
        /// Watch out for inconsistencies between the pool's create method and the one provided
        /// </summary>
        public static void Prewarm<T>(this IObjectPool<T> pool, Func<T> builder, int count) where T: class
        {
            for (int i = 0; i < count; i++)
                pool.Release(builder());
        }

        /// <summary>
        /// <inheritdoc cref="Prewarm{T}(UnityEngine.Pool.ObjectPool{T},int)"/>
        /// </summary>
        public static async UniTask PrewarmAsync<T>(this IObjectPool<T> pool, Func<T> builder, int count, int perFrame, CancellationToken ct) where T: class
        {
            while (count > 0)
            {
                for (var i = 0; i < perFrame && count > 0; i++)
                {
                    pool.Release(builder());
                    count--;
                }

                await UniTask.NextFrame(ct);
            }
        }

        public static ObjectPool<T> CreatePool<T>(int prewarmCount, Func<T> createFunc) where T: class
        {
            var pool = new ObjectPool<T>(createFunc, defaultCapacity: prewarmCount);

            for (var i = 0; i < prewarmCount; i++) { pool.Release(createFunc()); }

            return pool;
        }

        public static ListPoolRent<T> RentList<T>() =>
            new ListPoolRent<T>(ListPool<T>.Get());
    }
}
