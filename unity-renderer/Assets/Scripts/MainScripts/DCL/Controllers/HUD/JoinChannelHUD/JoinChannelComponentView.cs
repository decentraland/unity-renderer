using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinChannelComponentView : BaseComponentView, IJoinChannelComponentView, IComponentModelConfig<JoinChannelComponentModel>
{
    internal const string MODAL_TITLE = "Do you want to join the channel {0}?";

    [Header("Prefab References")]
    [SerializeField] internal Button backgroundButton;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal TMP_Text titleText;
    [SerializeField] internal ButtonComponentView cancelButton;
    [SerializeField] internal ButtonComponentView confirmButton;

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
    }

    public void SetChannel(string channelId)
    {
        model.channelId = channelId;

        if (titleText == null)
            return;

        titleText.text = string.Format(MODAL_TITLE, channelId);
    }

    internal static JoinChannelComponentView Create()
    {
        JoinChannelComponentView joinChannelComponenView = Instantiate(Resources.Load<GameObject>("JoinChannelHUD")).GetComponent<JoinChannelComponentView>();
        joinChannelComponenView.name = "_JoinChannelHUD";

        return joinChannelComponenView;
    }
}
