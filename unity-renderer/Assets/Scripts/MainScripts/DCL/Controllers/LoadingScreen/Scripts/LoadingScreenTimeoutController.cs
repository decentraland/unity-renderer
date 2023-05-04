using Cysharp.Threading.Tasks;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.NotificationModel;
using DCL.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Type = System.Type;

namespace DCL.LoadingScreen
{
    public class LoadingScreenTimeoutController : IDisposable
    {
        private const int LOAD_SCENE_TIMEOUT = 120000;
        private const int WEBSOCKET_TIMEOUT = 15000;

        internal int currentEvaluatedTimeout;

        private LoadingScreenController loadingScreenController;
        private CancellationTokenSource timeoutCTS;
        internal ILoadingScreenTimeoutView view;
        private IWorldState worldState;
        private Vector2Int currentDestination;

        internal bool goHomeRequested;

        public LoadingScreenTimeoutController(ILoadingScreenTimeoutView loadingScreenTimeoutView, IWorldState worldState, LoadingScreenController loadingScreenController)
        {
            this.view = loadingScreenTimeoutView;
            this.loadingScreenController = loadingScreenController;

            view.OnExitButtonClicked += ExitClicked;
            view.OnJumpHomeButtonClicked += GoBackHomeClicked;

            this.worldState = worldState;

            currentEvaluatedTimeout = Application.platform == RuntimePlatform.WebGLPlayer ? LOAD_SCENE_TIMEOUT : WEBSOCKET_TIMEOUT;
        }

        private async UniTaskVoid StartTimeoutCounter(CancellationToken ct)
        {
            if (!await UniTask.Delay(TimeSpan.FromMilliseconds(currentEvaluatedTimeout), cancellationToken: ct).SuppressCancellationThrow())
                DoTimeout();
        }

        private void DoTimeout()
        {
            IParcelScene destinationScene = worldState.GetScene(worldState.GetSceneNumberByCoords(currentDestination));

            if (destinationScene != null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "sceneID", destinationScene.sceneData?.id },
                    { "contentServer", destinationScene.contentProvider?.baseUrl },
                    { "contentServerBundlesUrl", destinationScene.contentProvider?.assetBundlesBaseUrl },
                };

                GenericAnalytics.SendAnalytic("scene_loading_failed", variables);
            }

            if (goHomeRequested)
                loadingScreenController.RandomPositionRequested();
            else
                view.ShowSceneTimeout();

            goHomeRequested = false;
        }

        public void StartTimeout(Vector2Int newDestination)
        {
            timeoutCTS = timeoutCTS.SafeRestart();
            currentDestination = newDestination;
            StartTimeoutCounter(timeoutCTS.Token).Forget();
        }

        public void StopTimeout()
        {
            //Once the websocket has connected and the first fadeout has been done, its always LOAD_SCENE_TIMEOUT
            currentEvaluatedTimeout = LOAD_SCENE_TIMEOUT;

            view.HideSceneTimeout();
            timeoutCTS.SafeCancelAndDispose();
        }

        private void ExitClicked()
        {
            Utils.QuitApplication();
        }

        internal void GoBackHomeClicked()
        {
            WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = "/goto home",
            });
            goHomeRequested = true;
            view.HideSceneTimeout();
        }

        public void Dispose()
        {
            timeoutCTS?.SafeCancelAndDispose();
            view.Dispose();
        }
    }
}
