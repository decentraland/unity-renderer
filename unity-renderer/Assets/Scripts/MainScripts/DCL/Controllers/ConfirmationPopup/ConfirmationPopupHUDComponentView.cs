using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.ConfirmationPopup
{
    public class ConfirmationPopupHUDComponentView : BaseComponentView<ConfirmationPopupHUDViewModel>, IConfirmationPopupHUDView
    {
        [SerializeField] internal TMP_Text titleLabel;
        [SerializeField] internal TMP_Text bodyLabel;
        [SerializeField] internal TMP_Text confirmLabel;
        [SerializeField] internal TMP_Text cancelLabel;
        [SerializeField] internal ButtonComponentView confirmButton;
        [SerializeField] internal ButtonComponentView cancelButton;

        public event Action OnConfirm;
        public event Action OnCancel;

        public static ConfirmationPopupHUDComponentView Create() =>
            Instantiate(Resources.Load<ConfirmationPopupHUDComponentView>("GenericConfirmationPopup"));

        public override void Awake()
        {
            base.Awake();

            confirmButton.onClick.AddListener(() => OnConfirm?.Invoke());
            cancelButton.onClick.AddListener(() => OnCancel?.Invoke());
        }

        public override void RefreshControl()
        {
            titleLabel.text = model.Title;
            bodyLabel.text = model.Body;
            confirmLabel.text = model.ConfirmButton;
            cancelLabel.text = model.CancelButton;
        }
    }
}
