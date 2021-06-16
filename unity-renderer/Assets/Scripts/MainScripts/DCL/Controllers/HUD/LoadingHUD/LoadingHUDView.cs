using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoadingHUD
{
    public interface ILoadingHUDView
    {
        void SetVisible(bool isVisible);
        void SetMessage(string message);
        void SetPercentage(float percentage);
        void SetWalletPrompt(bool showWalletPrompt);
        void SetTips(bool showTips);
    }

    public class LoadingHUDView : MonoBehaviour, ILoadingHUDView
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image loadingBar;
        [SerializeField] private GameObject walletPrompt;
        [SerializeField] private GameObject tips;

        public static ILoadingHUDView CreateView()
        {
            LoadingHUDView view = Instantiate(Resources.Load<GameObject>("LoadingHUD")).GetComponent<LoadingHUDView>();
            view.gameObject.name = "_LoadingHUD";
            return view;
        }

        private void Awake()
        {
            SetMessage("");
            SetPercentage(0);
            SetWalletPrompt(false);
        }

        public void SetVisible(bool isVisible) { gameObject.SetActive(isVisible); }
        public void SetMessage(string message) { text.text = message; }
        public void SetPercentage(float percentage) { loadingBar.fillAmount = percentage; }
        public void SetWalletPrompt(bool showWalletPrompt) { walletPrompt.gameObject.SetActive(showWalletPrompt); }
        public void SetTips(bool showTips) { tips.gameObject.SetActive(showTips); }
    }
}