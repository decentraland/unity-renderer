using Cysharp.Threading.Tasks;
using System;
using System.Buffers;
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

            internal ListPoolRent(List<T> list)
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
        /// Calls `IDispose` on elements before returning the list to the pool
        /// </summary>
        public struct ListPoolOfDisposablesRent<T> : IDisposable where T : IDisposable
        {
            private List<T> list;

            public List<T> GetList() =>
                list;

            internal ListPoolOfDisposablesRent(List<T> list)
            {
                this.list = list;
            }

            public void Dispose()
            {
                foreach (T element in list)
                    element.Dispose();

                ListPool<T>.Release(list);
                list = null;
            }
        }

        public struct DictionaryPoolRent<TKey, TValue> : IDisposable
        {
            private Dictionary<TKey, TValue> dictionary;

            public Dictionary<TKey, TValue> GetDictionary() =>
                dictionary;

            internal DictionaryPoolRent(Dictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }

            public void Dispose()
            {
                DictionaryPool<TKey, TValue>.Release(dictionary);
                dictionary = null;
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

        public static ListPoolRent<T> RentList<T>(this IEnumerable<T> copyFrom)
        {
            var list = ListPool<T>.Get();
            list.AddRange(copyFrom);
            return new ListPoolRent<T>(list);
        }

        public static ListPoolOfDisposablesRent<T> RentListOfDisposables<T>() where T : IDisposable =>
            new ListPoolOfDisposablesRent<T>(ListPool<T>.Get());

        public static DictionaryPoolRent<TKey, TValue> RentDictionary<TKey, TValue>() =>
            new DictionaryPoolRent<TKey, TValue>(DictionaryPool<TKey, TValue>.Get());
    }
}
