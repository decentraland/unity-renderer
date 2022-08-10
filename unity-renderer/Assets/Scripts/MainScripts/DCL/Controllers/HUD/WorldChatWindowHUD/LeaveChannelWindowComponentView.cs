using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class LeaveChannelWindowComponentView : BaseComponentView, ILeaveChannelWindowComponentView, IComponentModelConfig<LeaveChannelWindowComponentModel>
    {
        [Header("Prefab References")]
        [SerializeField] internal Button backgroundButton;
        [SerializeField] internal Button closeButton;
        [SerializeField] internal ButtonComponentView cancelButton;
        [SerializeField] internal ButtonComponentView confirmButton;

        [Header("Configuration")]
        [SerializeField] internal LeaveChannelWindowComponentModel model;

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

        public void Configure(LeaveChannelWindowComponentModel newModel)
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

        public static LeaveChannelWindowComponentView Create()
        {
            LeaveChannelWindowComponentView leaveChannelComponenView = Instantiate(Resources.Load<GameObject>("SocialBarV1/LeaveChannelHUD")).GetComponent<LeaveChannelWindowComponentView>();
            leaveChannelComponenView.name = "_LeaveChannelHUD";

            return leaveChannelComponenView;
        }
    }
}