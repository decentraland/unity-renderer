using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GPUSkinning
{
    public interface IGPUSkinningThrottler : IDisposable
    {
        void Bind(IGPUSkinning gpuSkinning);
        void SetThrottling(int framesBetweenUpdates);
        void Start();
        void Stop();
    }

    public class GPUSkinningThrottler : IGPUSkinningThrottler
    {
        internal static int startingFrame = 0;

        internal IGPUSkinning gpuSkinning;
        internal int framesBetweenUpdates;
        internal int currentFrame;
        internal CancellationTokenSource updateCts = new CancellationTokenSource();

        public GPUSkinningThrottler()
        {
            framesBetweenUpdates = 1;
            currentFrame = startingFrame++;
        }

        public void Bind(IGPUSkinning gpuSkinning) { this.gpuSkinning = gpuSkinning; }
        public void SetThrottling(int newFramesBetweenUpdates) { framesBetweenUpdates = newFramesBetweenUpdates; }

        public void Start()
        {
            updateCts?.Cancel();
            updateCts?.Dispose();
            updateCts = new CancellationTokenSource();
            UpdateTask(updateCts.Token);
        }

        private async UniTaskVoid UpdateTask(CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                currentFrame++;
                if (currentFrame % framesBetweenUpdates == 0)
                    gpuSkinning.Update();
                // AttachExternalCancellation is needed because WaitForEndOfFrame cancellation would take a frame
                await UniTask.DelayFrame(1, PlayerLoopTiming.PostLateUpdate, ct).AttachExternalCancellation(ct);
            }
        }
        public void Stop()
        {
            updateCts?.Cancel();
            updateCts?.Dispose();
            updateCts = null;
        }

        public void Dispose()
        {
            updateCts?.Cancel();
            updateCts?.Dispose();
            updateCts = null;
        }
    }
}