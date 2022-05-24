using System;
using System.Collections.Generic;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldChatWindowComponentView : BaseComponentView, IWorldChatWindowView, IComponentModelConfig
{
    [SerializeField] private CollapsablePublicChannelListComponentView publicChannelList;
    [SerializeField] private CollapsableDirectChatListComponentView directChatList;
    [SerializeField] private CollapsableChatSearchListComponentView searchResultsList;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject directChatsLoadingContainer;
    [SerializeField] private GameObject directChatsContainer;
    [SerializeField] private GameObject directChannelHeader;
    [SerializeField] private GameObject searchResultsHeader;
    [SerializeField] private TMP_Text directChatsHeaderLabel;
    [SerializeField] private TMP_Text searchResultsHeaderLabel;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private SearchBarComponentView searchBar;
    [SerializeField] private WorldChatWindowModel model;

    private string lastSearch;
    private bool privateChatsSortingDirty;

    public event Action OnClose;
    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChannel;

    public event Action<string> OnUnfriend
    {
        add => directChatList.OnUnfriend += value;
        remove => directChatList.OnUnfriend -= value;
    }

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
        directChatList.SortingMethod = (a, b) => b.Model.lastMessageTimestamp.CompareTo(a.Model.lastMessageTimestamp);
        directChatList.OnOpenChat += entry => OnOpenPrivateChat?.Invoke(entry.Model.userId);
        publicChannelList.OnOpenChat += entry => OnOpenPublicChannel?.Invoke(entry.Model.channelId);
        searchBar.OnSearchText += Filter;
        UpdateHeaders();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        UpdateLayout();
    }

    public void Initialize(IChatController chatController, ILastReadMessagesService lastReadMessagesService)
    {
        directChatList.Initialize(chatController, lastReadMessagesService);
        publicChannelList.Initialize(chatController, lastReadMessagesService);
        searchResultsList.Initialize(chatController, lastReadMessagesService);
    }

    public override void Update()
    {
        base.Update();
        
        if (privateChatsSortingDirty)
            directChatList.Sort();
        privateChatsSortingDirty = false;
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void SetPrivateChat(PrivateChatModel model)
    {
        // var user = model.user;
        // directChatList.Set(user.userId, new PrivateChatEntry.PrivateChatEntryModel(
        //     user.userId,
        //     user.userName,
        //     model.recentMessage.body,
        //     user.face256SnapshotURL,
        //     model.isBlocked,
        //     model.isOnline,
        //     model.recentMessage.timestamp));
        // directChatList.Sort();
        // UpdateHeaders();
        // UpdateLayout();
        // privateChatsSortingDirty = true;
    }

    public void RemovePrivateChat(string userId)
    {
        directChatList.Remove(userId);
        UpdateHeaders();
        UpdateLayout();
    }

    public void SetPublicChannel(PublicChatChannelModel model)
    {
        publicChannelList.Set(model.channelId,
            new PublicChannelEntry.PublicChannelEntryModel(model.channelId, name = model.name));
        UpdateLayout();
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (WorldChatWindowModel) newModel;
        RefreshControl();
    }

    public void ShowPrivateChatsLoading() => SetPrivateChatLoadingVisibility(true);

    public void HidePrivateChatsLoading() => SetPrivateChatLoadingVisibility(false);

    public void RefreshBlockedDirectMessages(List<string> blockedUsers)
    {
        if (blockedUsers == null) return;
        directChatList.RefreshBlockedEntries(blockedUsers);
    }

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

    private void Filter(string search)
    {
        if (string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(lastSearch))
        {
            searchResultsList.Export(publicChannelList, directChatList);
            searchResultsList.Hide();
            publicChannelList.Show();
            publicChannelList.Sort();
            directChatList.Show();
            directChatList.Sort();
            directChannelHeader.SetActive(true);
            searchResultsHeader.SetActive(false);
        }

        if (!string.IsNullOrEmpty(search) && string.IsNullOrEmpty(lastSearch))
        {
            searchResultsList.Import(publicChannelList, directChatList);
            searchResultsList.Show();
            searchResultsList.Sort();
            publicChannelList.Hide();
            directChatList.Hide();
            directChannelHeader.SetActive(false);
            searchResultsHeader.SetActive(true);
        }

        searchResultsList.Filter(search);
        publicChannelList.Filter(search);
        directChatList.Filter(search);
        lastSearch = search;
        UpdateHeaders();
    }

    private void SetPrivateChatLoadingVisibility(bool visible)
    {
        model.isLoadingDirectChats = visible;
        directChatsLoadingContainer.SetActive(visible);
        directChatsContainer.SetActive(!visible);
        scroll.enabled = !visible;
    }

    private void UpdateHeaders()
    {
        directChatsHeaderLabel.text = $"Direct Messages ({directChatList.Count()})";
        searchResultsHeaderLabel.text = $"Results ({searchResultsList.Count()})";
    }
    
    private void UpdateLayout() => ((RectTransform) scroll.transform).ForceUpdateLayout();
}