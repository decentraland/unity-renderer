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
            Debug.Log("Action Run");
            await UniTask.Yield();
            action();
            Debug.Log("Finish Action Run");
            return;
#endif
            Debug.Log("Thread Run");

            await UniTask.RunOnThreadPool(action, configureAwait, cancellationToken);
        }

        public static async UniTask Run(Func<UniTask> action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
#if !UNITY_STANDALONE || UNITY_EDITOR
            Debug.Log("Run task");
            await UniTask.Yield();
            await action();
            Debug.Log("Finish Run task");
            return;
#endif
            Debug.Log("Thread Run");

            await UniTask.RunOnThreadPool(action, configureAwait, cancellationToken);
        }
        
    }
}