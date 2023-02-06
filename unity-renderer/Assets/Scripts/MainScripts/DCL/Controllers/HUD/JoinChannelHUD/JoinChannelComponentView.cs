using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Chat.Channels
{
    public class JoinChannelComponentView : BaseComponentView, IJoinChannelComponentView, IComponentModelConfig<JoinChannelComponentModel>
    {
        internal const string MODAL_TITLE = "Do you want to join the channel {0}?";

        [Header("Prefab References")]
        [SerializeField] internal Button backgroundButton;
        [SerializeField] internal Button closeButton;
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal ButtonComponentView cancelButton;
        [SerializeField] internal ButtonComponentView confirmButton;
        [SerializeField] internal GameObject loadingSpinnerContainer;

        [Header("Configuration")]
        [SerializeField] internal JoinChannelComponentModel model;

        public event Action OnCancelJoin;
        public event Action<string> OnConfirmJoin;

        public override void Awake()
        {
            base.Awake();

            backgroundButton.onClick.AddListener(() => OnCancelJoin?.Invoke());
            closeButton.onClick.AddListener(() => OnCancelJoin?.Invoke());
            cancelButton.onClick.AddListener(() => OnCancelJoin?.Invoke());
            confirmButton.onClick.AddListener(() => OnConfirmJoin?.Invoke(model.channelId));
        }

        public override void Dispose()
        {
            backgroundButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            confirmButton.onClick.RemoveAllListeners();

            base.Dispose();
        }

        public void Configure(JoinChannelComponentModel newModel)
        {
            model = newModel;
            RefreshControl();
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetChannel(model.channelId);

            if (model.isLoading)
                ShowLoading();
            else
                HideLoading();
        }

        public void SetChannel(string channelName)
        {
            model.channelId = channelName;

            if (titleText == null)
                return;

            titleText.text = string.Format(MODAL_TITLE, channelName);
        }

        public void ShowLoading()
        {
            model.isLoading = true;
            cancelButton.SetInteractable(false);
            confirmButton.SetInteractable(false);
            loadingSpinnerContainer.SetActive(true);
        }

        public void HideLoading()
        {
            model.isLoading = false;
            cancelButton.SetInteractable(true);
            confirmButton.SetInteractable(true);
            loadingSpinnerContainer.SetActive(false);
        }

        internal static JoinChannelComponentView Create()
        {
            JoinChannelComponentView joinChannelComponenView = Instantiate(Resources.Load<GameObject>("JoinChannelHUD")).GetComponent<JoinChannelComponentView>();
            joinChannelComponenView.name = "_JoinChannelHUD";

            return joinChannelComponenView;
        }
    }
}
