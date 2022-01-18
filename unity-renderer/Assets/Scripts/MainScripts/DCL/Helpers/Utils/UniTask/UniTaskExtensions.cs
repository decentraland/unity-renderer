using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL.Helpers
{
    public struct UniTaskDCL
    {
        private static BaseVariable<bool> multithreading => DataStore.i.multithreading.enabled;
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
    }
}