
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LoadingScreenTimeoutController : IDisposable
{

    private const int LOAD_SCENE_TIMEOUT = 1000;
    private const int WEBSOCKET_TIMEOUT = 1000;

    private int currentEvaluatedTimeout;

    private CancellationTokenSource timeoutCTS;
    private LoadingScreenTimeoutView view;
    private IWorldState worldState;
    private Vector2Int currentDestination;

    public LoadingScreenTimeoutController(LoadingScreenTimeoutView loadingScreenTimeoutView, IWorldState worldState)
    {
        this.view = loadingScreenTimeoutView;
        this.worldState = worldState;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            currentEvaluatedTimeout = LOAD_SCENE_TIMEOUT;
        else
            currentEvaluatedTimeout = WEBSOCKET_TIMEOUT;
    }

    private async UniTaskVoid StartTimeoutCounter(CancellationToken ct)
    {
        if (!await UniTask.Delay(TimeSpan.FromMilliseconds(currentEvaluatedTimeout), cancellationToken: ct).SuppressCancellationThrow())
            DoTimeout();
    }

    private void DoTimeout()
    {
        view.ShowSceneTimeout();

        IParcelScene destinationScene = worldState.GetScene(worldState.GetSceneNumberByCoords(currentDestination));
        if (destinationScene != null)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "sceneID", destinationScene.sceneData.id },
                { "contentServer", destinationScene.contentProvider.baseUrl },
                { "contentServerBundlesUrl", destinationScene.contentProvider.assetBundlesBaseUrl },
            };
            GenericAnalytics.SendAnalytic("scene_loading_failed", variables);
        }
    }

    public void Dispose() =>
        timeoutCTS?.SafeCancelAndDispose();

    public void StartTimeout(Vector2Int newDestination)
    {
        timeoutCTS = timeoutCTS.SafeRestart();
        currentDestination = newDestination;
        StartTimeoutCounter(timeoutCTS.Token);
    }

    public void StopTimeout()
    {
        //Once the websocket has connected and the first fadeout has been done, its always LOAD_SCENE_TIMEOUT
        currentEvaluatedTimeout = LOAD_SCENE_TIMEOUT;

        view.HideSceneTimeout();
        timeoutCTS.SafeCancelAndDispose();
    }
}
