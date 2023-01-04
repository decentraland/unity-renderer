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


        public void SetLoadingMessage(string message)
        {
            loadingMessage.text = message;
        }

        public void SetLoadingPercentage(float percentage)
        {
            loadingPercentage.fillAmount = percentage;
        }

    }
}
