using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("TaskUtilsTests")]

namespace DCL.Helpers
{
    public unsafe class UniTaskUtils
    {
        public static UniTask WaitForBoolean(ref bool reference, bool targetValue = true, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            // fixes the reference in memory within the block only
            fixed (bool* pointer = &reference)
            {
                // But Unity GC is non-compacting so it does not move references
                // Thus, it is safe to pass it and save as a class field
                return new UniTask(WaitForBooleanPromise.Create(pointer, targetValue, timing, cancellationToken, out var token), token);
            }
        }

        internal sealed class WaitForBooleanPromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<WaitForBooleanPromise>
        {
            internal static TaskPool<WaitForBooleanPromise> pool;
            private WaitForBooleanPromise nextNode;

            ref WaitForBooleanPromise ITaskPoolNode<WaitForBooleanPromise>.NextNode => ref nextNode;

            private CancellationToken cancellationToken;

            private bool* pointer;
            private bool targetValue;

            private UniTaskCompletionSourceCore<object> core;

            static WaitForBooleanPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitForBooleanPromise), () => pool.Size);
            }

            private WaitForBooleanPromise() { }

            public static IUniTaskSource Create(bool* pointer, bool targetValue, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);

                if (!pool.TryPop(out var result))
                    result = new WaitForBooleanPromise();

                result.pointer = pointer;
                result.targetValue = targetValue;
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try { core.GetResult(token); }
                finally { TryReturn(); }
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                try
                {
                    if (*pointer != targetValue)
                        return true;
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                pointer = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }
    }
}
