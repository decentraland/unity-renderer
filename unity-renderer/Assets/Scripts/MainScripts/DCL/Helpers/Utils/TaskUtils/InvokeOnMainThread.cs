using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using System;

namespace DCL.Helpers
{
    public static class InvokeOnMainThreadExtensions
    {
        public static void InvokeOnMainThread<T>([NotNull] this Action<T> action, T param)
        {
            async UniTaskVoid SwitchToMainThreadAndInvoke()
            {
                await UniTask.SwitchToMainThread();
                action(param);
            }

            SwitchToMainThreadAndInvoke().Forget();
        }

        public static void InvokeOnMainThread<T1, T2>([NotNull] this Action<T1, T2> action, T1 param1, T2 param2)
        {
            async UniTaskVoid SwitchToMainThreadAndInvoke()
            {
                await UniTask.SwitchToMainThread();
                action(param1, param2);
            }

            SwitchToMainThreadAndInvoke().Forget();
        }
    }
}
