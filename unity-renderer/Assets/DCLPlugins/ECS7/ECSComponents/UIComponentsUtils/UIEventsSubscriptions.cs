using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace DCL.ECSComponents
{
    public class UIEventsSubscriptions
    {
        internal List<IDisposable> list { get; private set; } = ListPool<IDisposable>.Get();

        public void Dispose()
        {
            foreach (IDisposable subscription in list)
            {
                subscription.Dispose();
            }

            ListPool<IDisposable>.Release(list);
            list = null;
        }
    }
}
