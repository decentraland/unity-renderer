using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.LoadingScreen
{
    /// <summary>
    ///     View responsible of showing the corresponding loading percentage and message provided by LoadingScreenPercentageController
    /// </summary>
    public class LoadingScreenPercentageView : MonoBehaviour
    {
        [SerializeField] internal TMP_Text loadingMessage;
        [SerializeField] internal Image loadingPercentage;
        private readonly string LOADING_AVATAR_MESSAGE = "Loading avatar...";
        private readonly string LOADING_SCENE_MESSAGE = "Loading scenes, 3D models, and sounds... {0}% complete";
        private readonly string COMPILING_SHADERS_MESSAGE = "Compiling shaders, this might take a few seconds";
        private string currentMessage;

        public void SetLoadingPercentage(int percentage)
        {
            loadingMessage.text = GetCurrentLoadingMessage(percentage);
            loadingPercentage.fillAmount = percentage / 100f;
        }

        internal string GetCurrentLoadingMessage(int percentage) =>
            string.Format(currentMessage, percentage);

        public void SetSceneLoadingMessage() =>
            currentMessage = LOADING_SCENE_MESSAGE;

        public void SetPlayerLoadingMessage() =>
            currentMessage = LOADING_AVATAR_MESSAGE;

        public void SetShaderCompilingMessage(float progress)
        {
            loadingMessage.text = COMPILING_SHADERS_MESSAGE;
            loadingPercentage.fillAmount = progress;
        }

    }
}
