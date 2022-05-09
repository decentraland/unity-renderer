using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PublicChatChannelComponentView : BaseComponentView, IChannelChatWindowView, IComponentModelConfig
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private ChatHUDView chatView;
    [SerializeField] private PublicChatChannelModel model;

    public event Action OnClose;
    public event Action OnBack;

    public bool IsActive => gameObject.activeInHierarchy;
    public IChatHUDComponentView ChatHUD => chatView;
    public RectTransform Transform => (RectTransform) transform;

    public static PublicChatChannelComponentView Create()
    {
        return Instantiate(Resources.Load<PublicChatChannelComponentView>("SocialBarV1/GeneralChatChannelHUD"));
    }

    public override void Awake()
    {
        base.Awake();
        backButton.onClick.AddListener(() => OnBack?.Invoke());
        closeButton.onClick.AddListener(() => OnClose?.Invoke());
    }

    public override void RefreshControl()
    {
        nameLabel.text = $"#{model.name}";
        descriptionLabel.text = model.description;
    }

    public void Hide() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(true);
    
    public void Configure(PublicChatChannelModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public void Configure(BaseComponentModel newModel) => Configure((PublicChatChannelModel) newModel);
}