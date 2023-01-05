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
        [SerializeField] private TMP_Text loadingMessage;
        [SerializeField] private Image loadingPercentage;

        public void SetLoadingPercentage(int percentage)
        {
            loadingMessage.text = $"Loading scenes, 3D models, and sounds... {percentage}% complete";
            loadingPercentage.fillAmount = percentage / 100f;
        }
    }
}
