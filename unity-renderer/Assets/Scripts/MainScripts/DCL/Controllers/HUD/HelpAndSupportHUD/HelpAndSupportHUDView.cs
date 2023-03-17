using DCL.Interface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DCL.HelpAndSupportHUD
{
    public class HelpAndSupportHUDView : MonoBehaviour
    {
        public bool isOpen { get; private set; } = false;

        public event System.Action OnClose;

        [SerializeField] private ShowHideAnimator helpAndSupportAnimator;
        [SerializeField] private Button contactSupportButton;
        [SerializeField] private Button joinDiscordButton;
        [SerializeField] private Button visitFAQButton;
        [SerializeField] private Button closeButton;
        [SerializeField] internal InputAction_Trigger closeAction;

        private InputAction_Trigger.Triggered closeActionDelegate;

        private void Awake() { closeActionDelegate = (x) => SetVisibility(false); }

        // private void Initialize_OLD()
        // {
        //     gameObject.name = VIEW_OBJECT_NAME;
        //
        //     joinDiscordButton.onClick.AddListener(() =>
        //     {
        //         WebInterface.OpenURL(JOIN_DISCORD_URL);
        //     });
        //
        //     visitFAQButton.onClick.AddListener(() =>
        //     {
        //         WebInterface.OpenURL(FAQ_URL);
        //     });
        //
        //     closeButton.onClick.AddListener(() =>
        //     {
        //         SetVisibility(false);
        //     });
        // }

        private void Initialize(string viewObjectName, UnityAction contactDiscordAction, UnityAction joinDiscordAction, UnityAction visitFaqAction)
        {
            gameObject.name = viewObjectName;

            contactSupportButton.onClick.AddListener(contactDiscordAction);
            joinDiscordButton.onClick.AddListener(joinDiscordAction);
            visitFAQButton.onClick.AddListener(visitFaqAction);

            closeButton.onClick.AddListener(() =>
            {
                SetVisibility(false);
            });
        }

        public static HelpAndSupportHUDView Create(string resourcePath, string viewObjectName, UnityAction contactDiscordAction, UnityAction joinDiscordAction, UnityAction visitFAQAction)
        {
            HelpAndSupportHUDView view = Instantiate(Resources.Load<GameObject>(resourcePath)).GetComponent<HelpAndSupportHUDView>();
            view.Initialize(viewObjectName, contactDiscordAction, joinDiscordAction, visitFAQAction);
            return view;
        }

        public void SetVisibility(bool visible)
        {
            DataStore.i.exploreV2.isSomeModalOpen.Set(visible);

            closeAction.OnTriggered -= closeActionDelegate;
            if (visible)
            {
                helpAndSupportAnimator.Show();
                closeAction.OnTriggered += closeActionDelegate;
            }
            else
                helpAndSupportAnimator.Hide();

            if (!visible && isOpen)
                OnClose?.Invoke();

            isOpen = visible;
        }
    }
}
