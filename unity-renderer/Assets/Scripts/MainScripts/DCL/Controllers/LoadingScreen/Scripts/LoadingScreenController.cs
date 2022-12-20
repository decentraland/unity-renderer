using System;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Controls the state of the loading screen. It's responsibility is to update the view depending on the SceneController state
    /// </summary>
    public class LoadingScreenController : IDisposable
    {
        private readonly ILoadingScreenView view;
        private readonly ISceneController sceneController;

        public LoadingScreenController(ILoadingScreenView view, ISceneController sceneController)
        {
            this.view = view;
            this.sceneController = sceneController;
        }

        private void NewSceneAdded() { }

        private void SceneRemoved() { }

        private void UpdateLoadingMessage()
        {
            view.UpdateLoadingMessage();
        }

        public void Dispose() =>
            view.Dispose();
    }
}
