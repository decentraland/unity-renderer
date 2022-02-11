using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public class GenericPopUpView : BaseComponentView, IGenericPopUp
    {
        public event Action OnOkPressed;
        public event Action OnCancelPressed;

        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Button okButton;

        [SerializeField] internal TextMeshProUGUI subtitleText;

        [SerializeField] internal TextMeshProUGUI okButtonText;
        [SerializeField] internal TextMeshProUGUI cancelButtonText;

        [SerializeField] internal ModalComponentView modalView;
        [SerializeField] internal ShowHideAnimator showHideAnimator;

        public override void Awake()
        {
            base.Awake();
            cancelButton.onClick.AddListener( () =>
            {
                Hide();
                OnCancelPressed?.Invoke();
            });
            okButton.onClick.AddListener( () =>
            {
                Hide();
                OnOkPressed?.Invoke();
            });

            showHideAnimator.OnWillFinishHide += (x) =>
            {
                gameObject.SetActive(false);
            };
        }

        public void ShowPopUpWithoutTitle(string subtitle, string okButtonText, string cancelButtonText, Action OnOk, Action OnCancel)
        {
            RemoveAllListener();
            SetSubtitleText(subtitle);
            SetOkButtonText(okButtonText);
            SetCancelButtonText(cancelButtonText);
            Show();

            OnOkPressed += () => OnOk?.Invoke();

            OnCancelPressed += () => OnCancel?.Invoke();
        }

        public override void Dispose()
        {
            base.Dispose();
            cancelButton.onClick.RemoveAllListeners();
            okButton.onClick.RemoveAllListeners();
        }

        public void RemoveAllListener()
        {
            OnOkPressed = null;
            OnCancelPressed = null;
        }

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);
            base.Show(instant);
            modalView.Show(instant);
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            modalView.Hide(instant);
        }

        public void SetSubtitleText(string text) { subtitleText.text = text; }

        public void SetOkButtonText(string text) { okButtonText.text = text; }

        public void SetCancelButtonText(string text) { cancelButtonText.text = text; }

        public override void RefreshControl() { }
    }
}