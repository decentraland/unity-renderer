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

        private readonly AsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotWorldInfo.WorldInfo>> worlds =
            new (Array.Empty<IHotScenesController.HotWorldInfo.WorldInfo>());

        public HotScenesFetcher(float foregroundUpdateInterval, float backgroundUpdateInterval)
        {
            this.foregroundUpdateInterval = foregroundUpdateInterval;
            this.backgroundUpdateInterval = backgroundUpdateInterval;
        }

        public IReadOnlyAsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>> ScenesInfo => scenes;
        public IReadOnlyAsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotWorldInfo.WorldInfo>> WorldsInfo => worlds;

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
            worlds.AddTo(cts.Token);

            updateInterval = backgroundUpdateInterval;

            UpdateLoop(cts.Token).Forget();
        }

        public void SetUpdateMode(IHotScenesFetcher.UpdateMode mode)
        {
            if (mode is IHotScenesFetcher.UpdateMode.IMMEDIATELY_ONCE)
                updateInterval = 0;
            else
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
                    worlds.Value = await hotScenesController.Ref.GetHotWorldsListAsync(ct);

                    // We set back `updateInterval` to BACKGROUND after IMMEDIATELY_ONCE
                    if (updateInterval == 0)
                        updateInterval = backgroundUpdateInterval;
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
