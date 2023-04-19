
using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;

public class LoadingScreenTimeoutController : IDisposable
{

    private const int LOAD_SCENE_TIMEOUT = 100;
    private CancellationTokenSource timeoutCTS;

    private LoadingScreenTimeoutView view;

    public LoadingScreenTimeoutController(LoadingScreenTimeoutView loadingScreenTimeoutView)
    {
        this.view = loadingScreenTimeoutView;
    }

    private async UniTaskVoid StartTimeoutCounter(CancellationToken ct)
    {
        if (!await UniTask.Delay(TimeSpan.FromMilliseconds(LOAD_SCENE_TIMEOUT), cancellationToken: ct).SuppressCancellationThrow())
            DoTimeout();
    }

    private void DoTimeout()
    {
        view.ShowSceneTimeout();
    }

    public void Dispose() =>
        timeoutCTS?.SafeCancelAndDispose();

    public void StartTimeout()
    {
        timeoutCTS = timeoutCTS.SafeRestart();
        StartTimeoutCounter(timeoutCTS.Token);
    }

    public void StopTimeout()
    {
        view.HideSceneTimeout();
        timeoutCTS.SafeCancelAndDispose();
    }
}
