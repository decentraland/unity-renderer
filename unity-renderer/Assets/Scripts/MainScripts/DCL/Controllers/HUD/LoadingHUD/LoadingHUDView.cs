using TMPro;
using UnityEngine;
using UnityEngine.UI;

    public class LoadingHUDView : MonoBehaviour
    {
        [SerializeField] internal TextMeshProUGUI text;
        [SerializeField] internal Image loadingBar;
        [SerializeField] internal GameObject tipsContainer;
        [SerializeField] internal GameObject noTipsContainer;

        private bool isDestroyed = false;

        public static LoadingHUDView CreateView()
        {
            LoadingHUDView view = Instantiate(Resources.Load<GameObject>("LoadingHUD")).GetComponent<LoadingHUDView>();
            view.gameObject.name = "_LoadingHUD";
            return view;
        }

        public void Initialize()
        {
            SetMessage("");
            SetPercentage(0);
            SetTips(false);
        }

        public void SetVisible(bool isVisible) { gameObject.SetActive(isVisible); }
        public void SetMessage(string message) { text.text = message; }
        public void SetPercentage(float percentage) { loadingBar.transform.localScale = new Vector3(percentage, 1, 1); }
        public void SetTips(bool showTips)
        {
            tipsContainer.gameObject.SetActive(showTips);
            noTipsContainer.gameObject.SetActive(!showTips);
        }

        private void OnDestroy() { isDestroyed = true; }

        public void Dispose()
        {
            if (isDestroyed)
                return;
            isDestroyed = true;
            Destroy(gameObject);
        }
    }