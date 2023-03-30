using System;
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
        private readonly string LOADING_SCENE_MESSAGE = "Downloading scenes: {0}% done, {1} elements out of {2} ready."
                                                        + "\nEstimated time remaining: 23 seconds ({3}Mb out of {4}Mb at 13Mb/s)...";
        private readonly string LOADING_BEGINS_MESSAGE = "Initializing...";

        private string currentMessage;

        public void SetLoadingPercentage(int percentage, int currentLoadedObjects, int totalObjects, float downloadedSize, float totalSizeInMB)
        {
            float currentPercentage = (totalObjects - currentLoadedObjects) / (float)totalObjects * 100;
            loadingMessage.text = GetCurrentLoadingMessage((int)currentPercentage, currentLoadedObjects, totalObjects, downloadedSize, totalSizeInMB);
            loadingPercentage.fillAmount = currentPercentage / 100f;
        }

        internal string GetCurrentLoadingMessage(int percentage, int currentLoadedObjects, int totalObjects, float downloadedSize, float totalSizeInMB) =>
            string.Format(currentMessage, percentage, totalObjects - currentLoadedObjects, totalObjects, downloadedSize, totalSizeInMB);

        public void SetSceneLoadingMessage() =>
            currentMessage = LOADING_SCENE_MESSAGE;

        public void SetPlayerLoadingMessage() =>
            currentMessage = LOADING_AVATAR_MESSAGE;

        public void InitLoading() =>
            currentMessage = LOADING_BEGINS_MESSAGE;
    }
}
