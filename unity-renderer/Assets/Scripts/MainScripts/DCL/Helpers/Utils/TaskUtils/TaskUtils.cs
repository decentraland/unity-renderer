using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DCL.Helpers
{
    public static class TaskUtils
    {
        private static BaseVariable<bool> multithreading => DataStore.i.performance.multithreading;
        public static async UniTask Run(Action action, CancellationToken cancellationToken = default, bool returnToMainThread = true)
        {
            if (Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
                action();
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (multithreading.Get())
            {
                await UniTask.RunOnThreadPool(action, returnToMainThread, cancellationToken);
            }
            else
            {
                action();
            }
        }

        public static async UniTask Run(Func<UniTask> action, CancellationToken cancellationToken = default, bool returnToMainThread = true)
        {
            if (Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
                action();
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (multithreading.Get())
            {
                await UniTask.RunOnThreadPool(action, returnToMainThread, cancellationToken);
            }
            else
            {
                await UniTask.Create(action).AttachExternalCancellation(cancellationToken);
            }
        }

        public static async UniTask<T> Run<T>(Func<UniTask<T>> action, CancellationToken cancellationToken = default, bool returnToMainThread = true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (multithreading.Get())
            {
                return await UniTask.RunOnThreadPool(action, returnToMainThread, cancellationToken);
            }

            return await UniTask.Create(action).AttachExternalCancellation(cancellationToken);
        }

        public static async UniTask<T> Run<T>(Func<T> action, CancellationToken cancellationToken = default, bool returnToMainThread = true)
        {
            if (Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
                return action();
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (multithreading.Get())
            {
                return await UniTask.RunOnThreadPool(action, returnToMainThread, cancellationToken);
            }

            return action();
        }

        public static async UniTask RunThrottledCoroutine(IEnumerator enumerator, Action<Exception> onFail, Func<double, bool> timeBudget = null)
        {
            IEnumerator routine = DCLCoroutineRunner.Run(enumerator, onFail, timeBudget);
            await routine.ToUniTask(CoroutineStarter.instance);
        }

    }
}
