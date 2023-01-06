using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// View responsible of showing the corresponding loading percentage and message provided by LoadingScreenPercentageController
    /// </summary>
    public class LoadingScreenPercentageView : MonoBehaviour
    {
        [SerializeField] internal TMP_Text loadingMessage;
        [SerializeField] internal Image loadingPercentage;

        public void SetLoadingPercentage(int percentage)
        {
            loadingMessage.text = GetLoadingStringText(percentage);
            loadingPercentage.fillAmount = percentage / 100f;
        }

        internal string GetLoadingStringText(int percentage) =>
            $"Loading scenes, 3D models, and sounds... {percentage}% complete";
    }
}
