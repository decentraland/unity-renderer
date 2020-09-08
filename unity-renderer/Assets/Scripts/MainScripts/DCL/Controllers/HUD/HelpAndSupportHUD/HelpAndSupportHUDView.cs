using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.HelpAndSupportHUD
{
    public class HelpAndSupportHUDView : MonoBehaviour
    {
        public bool isOpen { get; private set; } = false;

        public event System.Action OnClose;

        private const string PATH = "HelpAndSupportHUD";
        private const string VIEW_OBJECT_NAME = "_HelpAndSupportHUD";
        private const string JOIN_DISCORD_URL = "https://discord.com/invite/k5ydeZp";
        private const string FAQ_URL = "https://docs.decentraland.org/decentraland/faq/";

        [SerializeField] private ShowHideAnimator helpAndSupportAnimator;
        [SerializeField] private Button joinDiscordButton;
        [SerializeField] private Button visitFAQButton;
        [SerializeField] private Button closeButton;

        private void Initialize()
        {
            gameObject.name = VIEW_OBJECT_NAME;

            joinDiscordButton.onClick.AddListener(() =>
            {
                WebInterface.OpenURL(JOIN_DISCORD_URL);
            });

            visitFAQButton.onClick.AddListener(() =>
            {
                WebInterface.OpenURL(FAQ_URL);
            });

            closeButton.onClick.AddListener(() =>
            {
                SetVisibility(false);
            });
        }

        public static HelpAndSupportHUDView Create()
        {
            HelpAndSupportHUDView view = Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<HelpAndSupportHUDView>();
            view.Initialize();
            return view;
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
                helpAndSupportAnimator.Show();
            else
                helpAndSupportAnimator.Hide();

            if (!visible && isOpen)
                OnClose?.Invoke();

            isOpen = visible;
        }
    }
}