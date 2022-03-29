using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldChatWindowComponentView : BaseComponentView, IWorldChatWindowView
{
    [SerializeField] private CollapsablePublicChannelListComponentView publicChannelList;
    [SerializeField] private CollapsableDirectChatListComponentView directChatList;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject directChatsLoadingContainer;
    [SerializeField] private GameObject directChatsContainer;
    [SerializeField] private TMP_Text directChatsHeaderLabel;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private Model model;

    public event Action OnClose;
    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChannel;

    public RectTransform Transform => (RectTransform) transform;
    public bool IsActive => gameObject.activeInHierarchy;
    
    public static WorldChatWindowComponentView Create()
    {
        return Instantiate(Resources.Load<WorldChatWindowComponentView>("SocialBarV1/ConversationListHUD"));
    }

    public override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(() => OnClose?.Invoke());
        directChatList.OnOpenChat += entry => OnOpenPrivateChat?.Invoke(entry.Model.userId);
        publicChannelList.OnOpenChat += entry => OnOpenPublicChannel?.Invoke(entry.Model.channelId);
        UpdateDirectChatsHeader();
    }

    public void Initialize(IChatController chatController)
    {
        directChatList.Initialize(chatController);
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void SetPrivateChat(PrivateChatModel model)
    {
        var user = model.user;
        directChatList.Set(user.userId, new PrivateChatEntry.PrivateChatEntryModel(
            user.userId,
            user.userName,
            model.recentMessage.body,
            user.face256SnapshotURL,
            model.isBlocked,
            model.isOnline));
        UpdateDirectChatsHeader();
    }

    public void SetPublicChannel(PublicChatChannelModel model)
    {
        publicChannelList.Set(model.channelId, new PublicChannelEntry.PublicChannelEntryModel
        {
            channelId = model.channelId,
            name = model.name
        });
    }

    public void ShowPrivateChatsLoading() => SetPrivateChatLoadingVisibility(true);

    public void HidePrivateChatsLoading() => SetPrivateChatLoadingVisibility(false);

    public override void RefreshControl()
    {
        publicChannelList.Clear();
        foreach (var entry in model.publicChannels)
            publicChannelList.Set(entry.channelId, entry);
        directChatList.Clear();
        foreach (var entry in model.privateChats)
            directChatList.Set(entry.userId, entry);
        SetPrivateChatLoadingVisibility(model.isLoadingDirectChats);
    }

    private void SetPrivateChatLoadingVisibility(bool visible)
    {
        model.isLoadingDirectChats = visible;
        directChatsLoadingContainer.SetActive(visible);
        directChatsContainer.SetActive(!visible);
        scroll.enabled = !visible;
    }

    private void UpdateDirectChatsHeader()
    {
        directChatsHeaderLabel.text = $"Direct Messages ({directChatList.Count()})";
    }

    [Serializable]
    private class Model
    {
        public PrivateChatEntry.PrivateChatEntryModel[] privateChats;
        public PublicChannelEntry.PublicChannelEntryModel[] publicChannels;
        public bool isLoadingDirectChats;
    }
}