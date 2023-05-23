using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class LeaveChannelConfirmationWindowComponentView : BaseComponentView, ILeaveChannelConfirmationWindowComponentView, IComponentModelConfig<LeaveChannelConfirmationWindowComponentModel>
    {
        [Header("Prefab References")]
        [SerializeField] internal Button backgroundButton;
        [SerializeField] internal Button closeButton;
        [SerializeField] internal ButtonComponentView cancelButton;
        [SerializeField] internal ButtonComponentView confirmButton;

        [Header("Configuration")]
        [SerializeField] internal LeaveChannelConfirmationWindowComponentModel model;

        public RectTransform Transform => (RectTransform)transform;

        public event Action OnCancelLeave;
        public event Action<string> OnConfirmLeave;

        public override void Awake()
        {
            base.Awake();

            backgroundButton.onClick.AddListener(() => OnCancelLeave?.Invoke());
            closeButton.onClick.AddListener(() => OnCancelLeave?.Invoke());
            cancelButton.onClick.AddListener(() => OnCancelLeave?.Invoke());
            confirmButton.onClick.AddListener(() => OnConfirmLeave?.Invoke(model.channelId));
        }

        public override void Dispose()
        {
            backgroundButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            confirmButton.onClick.RemoveAllListeners();

            base.Dispose();
        }

        public void Configure(LeaveChannelConfirmationWindowComponentModel newModel)
        {
            model = newModel;
            RefreshControl();
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetChannel(model.channelId);
        }

        public void SetChannel(string channelId) => model.channelId = channelId;

        public override void Show(bool instant = false) => gameObject.SetActive(true);

        public override void Hide(bool instant = false) => gameObject.SetActive(false);
    }
}
