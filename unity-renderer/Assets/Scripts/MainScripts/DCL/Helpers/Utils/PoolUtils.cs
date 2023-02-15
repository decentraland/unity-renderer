using System;
using System.Collections.Generic;
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
        public static void Prewarm<T>(this ObjectPool<T> pool, int count) where T: class
        {
            T[] instances = new T[count];

            for (int i = 0; i < count; i++) { instances[i] = pool.Get(); }

            for (int i = 0; i < count; i++) { pool.Release(instances[i]); }
        }

        /// <summary>
        /// By providing the Create function, this method can Prewarm a pool without extra allocation.
        /// Watchout for inconsistencies between the pool's create method and the one provided
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="builder"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        public static void Prewarm<T>(this ObjectPool<T> pool, Func<T> builder, int count) where T: class
        {
            for (int i = 0; i < count; i++) { pool.Release(builder()); }
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
