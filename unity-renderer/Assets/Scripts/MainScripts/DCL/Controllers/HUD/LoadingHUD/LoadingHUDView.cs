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
        [SerializeField] internal TextMeshProUGUI text;
        [SerializeField] internal Image loadingBar;
        [SerializeField] internal GameObject walletPrompt;
        [SerializeField] internal GameObject tipsContainer;
        [SerializeField] internal GameObject noTipsContainer;

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
            SetTips(false);
        }

        public void SetVisible(bool isVisible) { gameObject.SetActive(isVisible); }
        public void SetMessage(string message) { text.text = message; }
        public void SetPercentage(float percentage) { loadingBar.transform.localScale = new Vector3(percentage, 1, 1); }
        public void SetWalletPrompt(bool showWalletPrompt) { walletPrompt.gameObject.SetActive(showWalletPrompt); }
        public void SetTips(bool showTips)
        {
            tipsContainer.gameObject.SetActive(showTips);
            noTipsContainer.gameObject.SetActive(!showTips);
        }
    }
}