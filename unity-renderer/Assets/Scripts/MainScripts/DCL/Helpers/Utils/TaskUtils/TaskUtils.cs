using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL.Helpers
{
    public static class TaskUtils
    {
        private static BaseVariable<bool> multithreading => DataStore.i.performance.multithreading;
        public static async UniTask Run(Action action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (multithreading.Get())
            {
                await UniTask.RunOnThreadPool(action, configureAwait, cancellationToken);
            }
            else
            {
                await UniTask.Yield();
                action();
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        public static async UniTask Run(Func<UniTask> action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (multithreading.Get())
            {
                await UniTask.RunOnThreadPool(action, configureAwait, cancellationToken);
            }
            else
            {
                await UniTask.Create(action).AttachExternalCancellation(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        public static async UniTask RunThrottledCoroutine(IEnumerator enumerator, Action<Exception> onFail, Func<double, bool> timeBudget = null)
        {
            IEnumerator routine = DCLCoroutineRunner.Run(enumerator, onFail, timeBudget);
            await routine.ToUniTask(CoroutineStarter.instance);
        }
    }
}