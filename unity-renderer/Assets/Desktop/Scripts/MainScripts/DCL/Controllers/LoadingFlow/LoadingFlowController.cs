using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MainScripts.DCL.Controllers.LoadingFlow
{
    public class LoadingFlowController : IDisposable
    {
        private const float GENERAL_TIMEOUT_IN_SECONDS = 100;
        private const int WEB_SOCKET_TIMEOUT = 15;

        private readonly ILoadingFlowView view;
        private readonly BaseVariable<bool> loadingHudVisible;
        private readonly RendererState rendererState;
        private readonly BaseVariable<bool> websocketCommunicationEstablished;
        private float timerStart;

        private CancellationTokenSource disposeToken;

        public LoadingFlowController(
            BaseVariable<bool> loadingHudVisible,
            RendererState rendererState,
            BaseVariable<bool> websocketCommunicationEstablished)
        {
            this.loadingHudVisible = loadingHudVisible;
            this.rendererState = rendererState;
            this.websocketCommunicationEstablished = websocketCommunicationEstablished;

            view = CreateView();
            view.Hide();

            this.loadingHudVisible.OnChange += OnLoadingHudVisibleChanged;
            this.rendererState.OnChange += OnRendererStateChange;

            StartWatching();
        }

        private ILoadingFlowView CreateView()
        {
            return Object.Instantiate(Resources.Load<LoadingFlowView>("LoadingFlow"));
        }

        private void OnLoadingHudVisibleChanged(bool current, bool previous)
        {
            if (current)
                StartWatching();
            else
            {
                view.Hide();
                StopWatching();
            }
        }

        private void StartWatching()
        {
            //Means we are already watching
            if (disposeToken != null) return;

            timerStart = Time.unscaledTime;
            disposeToken = new CancellationTokenSource();
            ListenForTimeout().Forget();
        }

        private void StopWatching() =>
            DisposeCancellationToken();

        private bool IsTimeToShowTimeout()
        {
            var elapsedLoadingTime = Time.unscaledTime - timerStart;

            return elapsedLoadingTime > GENERAL_TIMEOUT_IN_SECONDS
                   || (!websocketCommunicationEstablished.Get() && elapsedLoadingTime > WEB_SOCKET_TIMEOUT);
        }

        private void OnRendererStateChange(bool current, bool previous)
        {
            if (current)
            {
                view.Hide();
                StopWatching();
            }
        }

        public void Dispose()
        {
            loadingHudVisible.OnChange -= OnLoadingHudVisibleChanged;
            rendererState.OnChange -= OnRendererStateChange;
            DisposeCancellationToken();
        }

        private async UniTask ListenForTimeout()
        {
            while (true)
            {
                if (IsTimeToShowTimeout())
                {
                    ShowTimeout();
                    StopWatching();
                }

                await UniTask.Yield(cancellationToken: disposeToken.Token);
            }
        }

        private void ShowTimeout()
        {
            view.Show();
            StopWatching();
        }

        private void DisposeCancellationToken()
        {
            if (disposeToken == null) return;

            disposeToken.Cancel();
            disposeToken.Dispose();
            disposeToken = null;
        }
    }
}
