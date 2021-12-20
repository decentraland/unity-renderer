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

    public class GPUSkinningThrottler_New : IGPUSkinningThrottler
    {
        internal static int startingFrame = 0;

        internal IGPUSkinning gpuSkinning;
        internal int framesBetweenUpdates;
        internal int currentFrame;
        internal CancellationTokenSource updateCts = new CancellationTokenSource();

        public GPUSkinningThrottler_New()
        {
            framesBetweenUpdates = 1;
            currentFrame = startingFrame++;
        }

        public void Bind(IGPUSkinning gpuSkinning) { this.gpuSkinning = gpuSkinning; }
        public void SetThrottling(int newFramesBetweenUpdates) { framesBetweenUpdates = newFramesBetweenUpdates; }

        public void Start()
        {
            updateCts?.Cancel();
            updateCts = new CancellationTokenSource();
            UpdateTask(updateCts.Token);
        }

        private async UniTaskVoid UpdateTask(CancellationToken ct)
        {
            while (true)
            {
                if (ct.IsCancellationRequested)
                    return;
                currentFrame++;
                if (currentFrame % framesBetweenUpdates == 0)
                    gpuSkinning.Update();
                await UniTask.WaitForEndOfFrame();
            }
        }
        public void Stop() { updateCts?.Cancel(); }
        public void Dispose()
        {
            updateCts?.Cancel();
            updateCts?.Dispose();
            updateCts = null;
        }
    }

    public class GPUSkinningThrottler
    {
        internal static int startingFrame = 0;

        internal readonly IGPUSkinning gpuSkinning;
        internal int framesBetweenUpdates;
        internal int currentFrame;

        public GPUSkinningThrottler(IGPUSkinning gpuSkinning)
        {
            this.gpuSkinning = gpuSkinning;
            framesBetweenUpdates = 1;
            currentFrame = startingFrame++;
        }

        public void SetThrottling(int newFramesBetweenUpdates) { framesBetweenUpdates = newFramesBetweenUpdates; }

        public void TryUpdate()
        {
            currentFrame++;
            if (currentFrame % framesBetweenUpdates == 0)
                gpuSkinning.Update();
        }
    }
}