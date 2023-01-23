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

        public static ListPoolRent<T> RentList<T>() =>
            new ListPoolRent<T>(ListPool<T>.Get());
    }
}
