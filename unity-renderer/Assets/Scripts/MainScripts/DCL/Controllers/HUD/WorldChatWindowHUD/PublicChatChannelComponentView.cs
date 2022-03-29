using System;
using DCL.Interface;
using TMPro;
using UnityEngine;

public class PublicChatChannelComponentView : BaseComponentView, IChannelChatWindowView
{
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private ChatHUDView chatView;
    [SerializeField] private Model model;
    
    public event Action OnClose;
    public event Action<string> OnMessageUpdated;
    public event Action<ChatMessage> OnSendMessage;
    public event Action OnDeactivatePreview;
    public event Action OnActivatePreview;

    public bool IsActive => gameObject.activeInHierarchy;
    public bool IsPreview => false;
    public IChatHUDComponentView ChatHUD => chatView;
    public RectTransform Transform => (RectTransform) transform;

    public static PublicChatChannelComponentView Create()
    {
        return Instantiate(Resources.Load<PublicChatChannelComponentView>("SocialBarV1/PublicChatChannelHUD"));
    }
    
    public override void RefreshControl()
    {
        nameLabel.text = model.name;
        descriptionLabel.text = model.description;
    }

    public void ActivatePreview()
    {
    }

    public void DeactivatePreview()
    {
    }

    public void Hide() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(false);
    
    public void Setup(string channelId, string name, string description)
    {
        model.name = name;
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