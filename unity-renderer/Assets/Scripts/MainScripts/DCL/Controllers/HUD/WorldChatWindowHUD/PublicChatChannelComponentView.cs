using System;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PublicChatChannelComponentView : BaseComponentView, IChannelChatWindowView
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private ChatHUDView chatView;
    [SerializeField] private Model model;
    
    public event Action OnClose;
    public event Action OnBack;

    public bool IsActive => gameObject.activeInHierarchy;
    public IChatHUDComponentView ChatHUD => chatView;
    public RectTransform Transform => (RectTransform) transform;
    public string Channel => model.name;

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
        nameLabel.text = model.name;
        descriptionLabel.text = model.description;
    }

    public void Hide() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(true);
    
    public void Setup(string channelId, string name, string description)
    {
        model.name = $"#{name}";
        model.description = description;
        RefreshControl();
    }

    [Serializable]
    private struct Model
    {
        public string name;
        public string description;
    }
}