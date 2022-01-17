using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL.Helpers
{
    public struct UniTaskDCL
    {
        public static async UniTask Run(Action action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
#if !UNITY_STANDALONE || UNITY_EDITOR
            await UniTask.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            action();
            cancellationToken.ThrowIfCancellationRequested();
            return;
#endif
            await UniTask.RunOnThreadPool(action, configureAwait, cancellationToken);
        }

        public static async UniTask Run(Func<UniTask> action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
#if !UNITY_STANDALONE || UNITY_EDITOR
            await UniTask.Create(action).AttachExternalCancellation(cancellationToken);
            return;
#endif
            await UniTask.RunOnThreadPool(action, configureAwait, cancellationToken);
        }
        
    }
}