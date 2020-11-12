using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DCL.SettingsHUD
{
    public class SettingsHUDView : MonoBehaviour
    {
        public event UnityAction OnClose;
        public event UnityAction OnDone;

        [SerializeField] private ShowHideAnimator settingsAnimator;
        [SerializeField] private GameObject generalSettingsTab;
        [SerializeField] internal InputAction_Trigger closeAction;

        private InputAction_Trigger.Triggered closeActionDelegate;

        public bool isOpen { get; private set; }

        private const string PATH = "SettingsHUD";

        [SerializeField] internal Button closeButton;
        [SerializeField] internal Button doneButton;

        private void Awake()
        {
            closeActionDelegate = (x) => RaiseOnClose();
            closeButton.onClick.AddListener(RaiseOnClose);
            doneButton.onClick.AddListener(RaiseOnDone);

            settingsAnimator.OnWillFinishHide += OnFinishHide;

            isOpen = false;
            settingsAnimator.Hide(true);
        }

        private void OnDestroy()
        {
            if (settingsAnimator)
            {
                settingsAnimator.OnWillFinishHide -= OnFinishHide;
            }
        }

        public static SettingsHUDView Create()
        {
            SettingsHUDView view = Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<SettingsHUDView>();
            view.name = "_SettingsHUD";
            return view;
        }

        public void SetVisibility(bool visible)
        {
            if (visible && !isOpen)
                AudioScriptableObjects.dialogOpen.Play(true);
            else if (isOpen)
                AudioScriptableObjects.dialogClose.Play(true);

            closeAction.OnTriggered -= closeActionDelegate;
            if (visible)
            {
                closeAction.OnTriggered += closeActionDelegate;
                settingsAnimator.Show();
                generalSettingsTab.SetActive(true);
            }
            else
            {
                settingsAnimator.Hide();
            }

            isOpen = visible;
        }

        private void RaiseOnClose()
        {
            OnClose?.Invoke();
        }

        private void RaiseOnDone()
        {
            OnDone?.Invoke();
        }

        private void OnFinishHide(ShowHideAnimator animator)
        {
            generalSettingsTab.SetActive(false);
        }
    }
}