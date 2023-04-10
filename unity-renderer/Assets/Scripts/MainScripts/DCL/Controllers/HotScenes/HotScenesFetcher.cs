using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HotScenes
{
    public class HotScenesFetcher : IHotScenesFetcher
    {
        private readonly float foregroundUpdateInterval;
        private readonly float backgroundUpdateInterval;

        private float updateInterval;

        private CancellationTokenSource cts;

        private Service<IHotScenesController> hotScenesController;

        private readonly AsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>> scenes =
            new (Array.Empty<IHotScenesController.HotSceneInfo>());

        public HotScenesFetcher(float foregroundUpdateInterval, float backgroundUpdateInterval)
        {
            this.foregroundUpdateInterval = foregroundUpdateInterval;
            this.backgroundUpdateInterval = backgroundUpdateInterval;
        }

        public IReadOnlyAsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>> ScenesInfo => scenes;

        public void Dispose()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        public void Initialize()
        {
            // dirty hack to prevent any logic from execution as it can be invoked in tests that are using the realtime ServiceLocatorFactory
            if (hotScenesController.Ref == null)
                return;

            cts = new CancellationTokenSource();
            scenes.AddTo(cts.Token);

            updateInterval = backgroundUpdateInterval;

            UpdateLoop(cts.Token).Forget();
        }

        public void SetUpdateMode(IHotScenesFetcher.UpdateMode mode)
        {
            updateInterval = mode == IHotScenesFetcher.UpdateMode.BACKGROUND ? backgroundUpdateInterval : foregroundUpdateInterval;
        }

        private async UniTaskVoid UpdateLoop(CancellationToken ct)
        {
            try
            {
                while (true)
                {
                    float time = Time.realtimeSinceStartup;

                    // We can't use UniTask.Delay as `updateInterval` can be changed in the process of waiting
                    while (Time.realtimeSinceStartup - time < updateInterval)
                        await UniTask.NextFrame(ct);

                    scenes.Value = await hotScenesController.Ref.GetHotScenesListAsync(ct);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
