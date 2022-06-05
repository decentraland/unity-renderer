using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldChatWindowComponentView : BaseComponentView, IWorldChatWindowView, IComponentModelConfig
{
    private const int CREATION_AMOUNT_PER_FRAME = 5;
    private const int AVATAR_SNAPSHOTS_PER_FRAME = 5;

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

    [Header("Load More Entries")] [SerializeField]
    private Button loadMoreEntriesButton;

    [SerializeField] private GameObject loadMoreEntriesContainer;
    [SerializeField] private TMP_Text loadMoreEntriesLabel;

    private readonly Dictionary<string, PrivateChatModel> creationQueue = new Dictionary<string, PrivateChatModel>();
    private bool isSortingDirty;
    private bool isLayoutDirty;
    private Dictionary<string, PrivateChatModel> filteredPrivateChats;
    private int currentAvatarSnapshotIndex;
    private bool isLoadingPrivateChannels;

    public event Action OnClose;
    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChannel;

    public event Action<string> OnUnfriend
    {
        add => directChatList.OnUnfriend += value;
        remove => directChatList.OnUnfriend -= value;
    }

    public event Action<string> OnSearchChannelRequested;
    public event Action OnRequireMorePrivateChats;

    public RectTransform Transform => (RectTransform) transform;
    public bool IsActive => gameObject.activeInHierarchy;
    public int PrivateChannelsCount => directChatList.Count() + creationQueue.Count;

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
        searchBar.OnSearchText += text => OnSearchChannelRequested?.Invoke(text);
        loadMoreEntriesButton.onClick.AddListener(() => OnRequireMorePrivateChats?.Invoke());
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

        SetQueuedEntries();

        if (isSortingDirty)
        {
            directChatList.Sort();
            searchResultsList.Sort();
            publicChannelList.Sort();
        }

        isSortingDirty = false;

        if (isLayoutDirty)
            ((RectTransform) scroll.transform).ForceUpdateLayout();
        isLayoutDirty = false;

        FetchProfilePicturesForVisibleEntries();
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void SetPrivateChat(PrivateChatModel model) => creationQueue[model.user.userId] = model;

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

    public void HideMoreChatsToLoadHint()
    {
        loadMoreEntriesContainer.SetActive(false);
        UpdateLayout();
    }

    public void ShowMoreChatsToLoadHint(int count)
    {
        loadMoreEntriesLabel.SetText(
            $"{count} chats hidden. Use the search bar to find them or click below to show more.");
        ShowMoreChatsToLoadHint();
    }

    public void ClearFilter()
    {
        filteredPrivateChats = null;
        searchResultsList.Export(publicChannelList, directChatList);
        searchResultsList.Hide();
        publicChannelList.Show();
        publicChannelList.Sort();
        
        if (!isLoadingPrivateChannels)
        {
            directChatList.Show();
            directChatList.Sort();
            directChannelHeader.SetActive(true);    
        }
        
        searchResultsHeader.SetActive(false);

        directChatList.Filter(entry => true);
        publicChannelList.Filter(entry => true);

        UpdateHeaders();
    }

    public void Filter(Dictionary<string, PrivateChatModel> privateChats,
        Dictionary<string, PublicChatChannelModel> publicChannels)
    {
        filteredPrivateChats = privateChats;

        foreach (var chat in privateChats)
            if (!directChatList.Contains(chat.Key))
                SetPrivateChat(chat.Value);

        foreach (var channel in publicChannels)
            if (!publicChannelList.Contains(channel.Key))
                SetPublicChannel(channel.Value);

        searchResultsList.Import(publicChannelList, directChatList);
        searchResultsList.Show();
        searchResultsList.Sort();
        publicChannelList.Hide();
        directChatList.Hide();
        directChannelHeader.SetActive(false);
        searchResultsHeader.SetActive(true);

        searchResultsList.Filter(entry => privateChats.ContainsKey(entry.Model.userId),
            entry => publicChannels.ContainsKey(entry.Model.channelId));

        UpdateHeaders();
    }

    public bool ContainsPrivateChannel(string userId) => creationQueue.ContainsKey(userId)
                                                         || directChatList.Get(userId) != null;

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

    private void Set(PrivateChatModel model)
    {
        var user = model.user;
        var userId = user.userId;

        var entry = new PrivateChatEntry.PrivateChatEntryModel(
            user.userId,
            user.userName,
            model.recentMessage.body,
            user.face256SnapshotURL,
            model.isBlocked,
            model.isOnline,
            model.recentMessage.timestamp);

        if (filteredPrivateChats?.ContainsKey(userId) ?? false)
        {
            directChatList.Remove(userId);
            searchResultsList.Set(entry);
        }
        else
            directChatList.Set(userId, entry);

        UpdateHeaders();
        UpdateLayout();
        SortLists();
    }

    private void SortLists() => isSortingDirty = true;

    private void ShowMoreChatsToLoadHint()
    {
        loadMoreEntriesContainer.SetActive(true);
        UpdateLayout();
    }

    private void SetPrivateChatLoadingVisibility(bool visible)
    {
        model.isLoadingDirectChats = visible;
        directChatsLoadingContainer.SetActive(visible);
        directChatsContainer.SetActive(!visible);
        scroll.enabled = !visible;
        isLoadingPrivateChannels = visible;
    }

    private void UpdateHeaders()
    {
        directChatsHeaderLabel.text = $"Direct Messages ({directChatList.Count()})";
        searchResultsHeaderLabel.text = $"Results ({searchResultsList.Count()})";
    }

    private void UpdateLayout() => isLayoutDirty = true;

    private void SetQueuedEntries()
    {
        if (creationQueue.Count == 0) return;

        for (var i = 0; i < CREATION_AMOUNT_PER_FRAME && creationQueue.Count > 0; i++)
        {
            var (userId, model) = creationQueue.First();
            creationQueue.Remove(userId);
            Set(model);
        }
    }

    private void FetchProfilePicturesForVisibleEntries()
    {
        foreach (var entry in directChatList.Entries.Values.Skip(currentAvatarSnapshotIndex)
                     .Take(AVATAR_SNAPSHOTS_PER_FRAME))
        {
            if (entry.IsVisible((RectTransform) scroll.transform))
                entry.EnableAvatarSnapshotFetching();
            else
                entry.DisableAvatarSnapshotFetching();
        }

        currentAvatarSnapshotIndex += AVATAR_SNAPSHOTS_PER_FRAME;

        if (currentAvatarSnapshotIndex >= directChatList.Entries.Count)
            currentAvatarSnapshotIndex = 0;
    }
}