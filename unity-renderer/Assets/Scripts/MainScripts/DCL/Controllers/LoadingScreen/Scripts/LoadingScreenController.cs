using DCL.Helpers;
using System;
using System.IO;
using UnityEngine;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Controls the state of the loading screen. It's responsibility is to update the view depending on the SceneController state
    /// Creates and provides the controllers associated to the LoadingScreen: TipsController and PercentageController
    /// </summary>
    public class LoadingScreenController : IDisposable
    {
        private readonly ILoadingScreenView view;
        private readonly ISceneController sceneController;
        private readonly DataStore_Player playerDataStore;
        private readonly DataStore_Common commonDataStore;
        private readonly DataStore_LoadingScreen loadingScreenDataStore;
        private readonly IWorldState worldState;

        private Vector2Int currentDestination;
        private readonly LoadingScreenTipsController tipsController;
        private readonly LoadingScreenPercentageController percentageController;

        public LoadingScreenController(ILoadingScreenView view, ISceneController sceneController, DataStore_Player playerDataStore, DataStore_Common commonDataStore, DataStore_LoadingScreen loadingScreenDataStore,
            IWorldState worldState)
        {
            this.view = view;
            this.sceneController = sceneController;
            this.playerDataStore = playerDataStore;
            this.commonDataStore = commonDataStore;
            this.worldState = worldState;
            this.loadingScreenDataStore = loadingScreenDataStore;

            tipsController = new LoadingScreenTipsController(view.GetTipsView());
            percentageController = new LoadingScreenPercentageController(sceneController, view.GetPercentageView());

            this.playerDataStore.lastTeleportPosition.OnChange += TeleportRequested;
            this.commonDataStore.isSignUpFlow.OnChange += OnSignupFlow;
            this.sceneController.OnReadyScene += ReadyScene;
            view.OnFadeInFinish += FadeInFinished;

            loadingScreenDataStore.decoupledLoadingHUD.visible.Set(true);
            view.FadeIn(true);

            tipsController.StartTips();
        }

        public void Dispose()
        {
            view.Dispose();
            percentageController.Dispose();
            playerDataStore.lastTeleportPosition.OnChange -= TeleportRequested;
            commonDataStore.isSignUpFlow.OnChange -= OnSignupFlow;
            sceneController.OnReadyScene -= ReadyScene;
            view.OnFadeInFinish -= FadeInFinished;
        }

        private void FadeInFinished(ShowHideAnimator obj)
        {
            loadingScreenDataStore.decoupledLoadingHUD.visible.Set(true);
        }

        private void ReadyScene(int obj)
        {
            //We have to check that the latest scene loaded is the one from our current destination
            if (worldState.GetSceneNumberByCoords(currentDestination).Equals(obj))
                FadeOutView();
        }

        private void OnSignupFlow(bool current, bool previous)
        {
            if (current)
                FadeOutView();
            else

                //Blit not necessary since we wont be hiding the Terms&Condition menu until full fade in
                view.FadeIn(false);
        }

        private void TeleportRequested(Vector3 current, Vector3 previous)
        {
            Vector2Int currentDestinationCandidate = Utils.WorldToGridPosition(current);

            //The teleport is request both on the POSITION_SETTLED and POSITION_UNSETTLED events from kernel. So, if the positions are not the same, then it means we are calling to a POSITION_UNSETTLED event and we are indeed teleporting
            //Also, we need to check that the scene that has just been unsettled is not loaded. Only then, we show the loading screen
            if (!current.Equals(previous) && worldState.GetSceneNumberByCoords(currentDestinationCandidate).Equals(-1))
            {
                currentDestination = currentDestinationCandidate;

                //if (Camera.main != null)
                //{
                    BlitTexture();
                    /*
                    ((LoadingScreenView)view).blitRenderTexture = new RenderTexture(mCamera.pixelWidth, mCamera.pixelHeight, 24);
                    ((LoadingScreenView)view).SetRenderTexture();
                    Graphics.Blit(GetTextureFromCamera(mCamera), ((LoadingScreenView)view).blitRenderTexture);

                    Debug.Log("AAAA " + mCamera.pixelWidth);
                    Debug.Log("BBBB " + mCamera.pixelHeight);

                    Screen
                    ScreenCapture.CaptureScreenshotIntoRenderTexture(((LoadingScreenView)view).blitRenderTexture);
                    */
                //}


                //TODO: The blit to avoid the flash of the empty camera/the unloaded scene
                view.FadeIn(true);


                //On a teleport, to copy previos behaviour, we disable tips entirely and show the teleporting screen
                //This is probably going to change with the integration of WORLDS loading screen
                tipsController.StopTips();
                percentageController.StartLoading(currentDestination);
            }
        }

        private void BlitTexture()
        {
            //Graphics.Blit(null, ((LoadingScreenView)view).blitRenderTexture);
        }

        /*
        private static Texture2D GetTextureFromCamera(Camera mCamera)
        {
            Rect rect = new Rect(0, 0, mCamera.pixelWidth, mCamera.pixelHeight);
            RenderTexture renderTexture = new RenderTexture(mCamera.pixelWidth, mCamera.pixelHeight, 24);
            Texture2D screenShot = new Texture2D(mCamera.pixelWidth, mCamera.pixelHeight, TextureFormat.RGBA32, false);

            mCamera.targetTexture = renderTexture;
            mCamera.Render();

            RenderTexture.active = renderTexture;

            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();


            mCamera.targetTexture = null;
            RenderTexture.active = null;
            return screenShot;
        }
*/
        private void FadeOutView()
        {
            view.FadeOut();
            loadingScreenDataStore.decoupledLoadingHUD.visible.Set(false);
        }
    }
}
