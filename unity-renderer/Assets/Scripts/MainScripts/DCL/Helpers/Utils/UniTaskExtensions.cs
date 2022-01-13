using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DCL.Helpers
{
    public struct UniTaskDCL
    {
        public static async UniTask Run(Action action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            action();
            return;
#endif
            await UniTask.RunOnThreadPool(action, configureAwait, cancellationToken);
        }

        public static async UniTask Run(Func<UniTask> action, bool configureAwait = true, CancellationToken cancellationToken = default)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            await UniTask.Create(action);
            return;
#endif
            await UniTask.RunOnThreadPool(action, configureAwait, cancellationToken);
        }
        
    }
}