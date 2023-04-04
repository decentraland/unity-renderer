using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCL.Helpers
{
    public static class PromiseAsyncExtensions
    {
        public static UniTask<T>.Awaiter GetAwaiter<T>(this Promise<T> promise) =>
            promise.ToUniTask().GetAwaiter();

        public static UniTask<T> WithCancellation<T>(this Promise<T> promise, CancellationToken cancellationToken) =>
            promise.ToUniTask(cancellationToken);

        public static UniTask<T> ToUniTask<T>(this Promise<T> promise, CancellationToken cancellationToken = default)
        {
            if (promise == null)
                throw new ArgumentNullException(nameof(promise));

            if (cancellationToken.IsCancellationRequested) return UniTask.FromCanceled<T>(cancellationToken);
            if (promise.failed) return UniTask.FromException<T>(promise.exception);
            if (promise.resolved) return UniTask.FromResult(promise.value);

            return new UniTask<T>(PromiseCompletionSource<T>.Create(promise, cancellationToken, out short token), token);
        }

        internal sealed class PromiseCompletionSource<T> : IUniTaskSource<T>, ITaskPoolNode<PromiseCompletionSource<T>>
        {
            private static TaskPool<PromiseCompletionSource<T>> pool;

            private PromiseCompletionSource<T> nextNode;

            public ref PromiseCompletionSource<T> NextNode => ref nextNode;

            private Promise<T> innerPromise;
            private CancellationToken cancellationToken;
            private CancellationTokenRegistration cancellationTokenRegistration;

            private UniTaskCompletionSourceCore<T> core;

            static PromiseCompletionSource()
            {
                TaskPool.RegisterSizeGetter(typeof(PromiseCompletionSource<T>), () => pool.Size);
            }

            private PromiseCompletionSource() { }

            private void SetData(Promise<T> innerPromise, CancellationToken cancellationToken)
            {
                this.innerPromise = innerPromise;
                this.cancellationToken = cancellationToken;

                // Subscribe directly to `Promise` instead of calling `MoveNext` to prevent skipping an extra frame
                innerPromise.onSuccess += SetResult;
                innerPromise.onError += SetException;

                cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(SetCancelled);
            }

            private void SetCancelled()
            {
                UnsubscribeBeforeReset();
                core.TrySetCanceled(cancellationToken);
            }

            private void SetResult(T result)
            {
                UnsubscribeBeforeReset();
                core.TrySetResult(result);
            }

            private void SetException(string _)
            {
                UnsubscribeBeforeReset();
                core.TrySetException(innerPromise.exception);
            }

            private void UnsubscribeBeforeReset()
            {
                cancellationTokenRegistration.Dispose();
                innerPromise.onSuccess -= SetResult;
                innerPromise.onError -= SetException;
            }

            public static IUniTaskSource<T> Create(Promise<T> promise, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested) { return AutoResetUniTaskCompletionSource<T>.CreateFromCanceled(cancellationToken, out token); }

                if (!pool.TryPop(out var result)) result = new PromiseCompletionSource<T>();

                result.SetData(promise, cancellationToken);

                TaskTracker.TrackActiveTask(result, 3);

                token = result.core.Version;

                return result;
            }

            public UniTaskStatus GetStatus(short token) =>
                core.GetStatus(token);

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public T GetResult(short token)
            {
                try { return core.GetResult(token); }
                finally { TryReturn(); }
            }

            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            public UniTaskStatus UnsafeGetStatus() =>
                core.UnsafeGetStatus();

            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                innerPromise = null;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }
    }
}
