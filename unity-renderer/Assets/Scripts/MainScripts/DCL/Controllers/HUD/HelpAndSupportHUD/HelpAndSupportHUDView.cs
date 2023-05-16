using DCL.Interface;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DCL.HelpAndSupportHUD
{
    public class HelpAndSupportHUDView : MonoBehaviour, IHelpAndSupportHUDView
    {
        [SerializeField] private ShowHideAnimator helpAndSupportAnimator;
        [SerializeField] private Button contactSupportButton;
        [SerializeField] private Button joinDiscordButton;
        [SerializeField] private Button visitFAQButton;
        [SerializeField] private Button closeButton;
        [SerializeField] internal InputAction_Trigger closeAction;

        private InputAction_Trigger.Triggered closeActionDelegate;

        public event Action OnDiscordButtonPressed;
        public event Action OnFaqButtonPressed;
        public event Action OnSupportButtonPressed;

        public void Initialize()
        {
            closeActionDelegate = (x) => SetVisibility(false);

            contactSupportButton.onClick.AddListener(() => OnSupportButtonPressed?.Invoke());
            joinDiscordButton.onClick.AddListener(() => OnDiscordButtonPressed?.Invoke());
            visitFAQButton.onClick.AddListener(() => OnFaqButtonPressed?.Invoke());

            closeButton.onClick.AddListener(() => { SetVisibility(false); });
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
        }

        public void Dispose()
        {
            contactSupportButton.onClick.RemoveAllListeners();
            joinDiscordButton.onClick.RemoveAllListeners();
            visitFAQButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();

            closeAction.OnTriggered -= closeActionDelegate;
            closeActionDelegate = null;

            Destroy(gameObject);
        }
    }
}
