using System;
using System.Buffers;
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
