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

    [SerializeField] internal CollapsablePublicChannelListComponentView publicChannelList;
    [SerializeField] internal CollapsableDirectChatListComponentView directChatList;
    [SerializeField] internal CollapsableChatSearchListComponentView searchResultsList;
    [SerializeField] internal GameObject searchLoading;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal GameObject directChatsLoadingContainer;
    [SerializeField] internal GameObject directChatsContainer;
    [SerializeField] internal GameObject directChannelHeader;
    [SerializeField] internal GameObject searchResultsHeader;
    [SerializeField] internal TMP_Text directChatsHeaderLabel;
    [SerializeField] internal TMP_Text searchResultsHeaderLabel;
    [SerializeField] internal ScrollRect scroll;
    [SerializeField] internal SearchBarComponentView searchBar;
    [SerializeField] private WorldChatWindowModel model;

    [Header("Load More Entries")]
    [SerializeField] internal GameObject loadMoreEntriesContainer;
    [SerializeField] internal TMP_Text loadMoreEntriesLabel;
    [SerializeField] internal GameObject loadMoreEntriesLoading;

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
    public int PrivateChannelsCount => directChatList.Count() + creationQueue.Keys.Count(s => !directChatList.Contains(s));

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
        scroll.onValueChanged.AddListener((scrollPos) =>
        {
            if (scrollPos.y < 0.005f)
                OnRequireMorePrivateChats?.Invoke();
        });
        UpdateHeaders();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        UpdateLayout();
    }

    public void Initialize(IChatController chatController)
    {
        directChatList.Initialize(chatController);
        publicChannelList.Initialize(chatController);
        searchResultsList.Initialize(chatController);
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
        loadMoreEntriesLabel.SetText($"{count} chats hidden. Use the search bar to find them or click below to show more.");
        loadMoreEntriesContainer.SetActive(true);
        UpdateLayout();
    }

    public void HideMoreChatsLoading()
    {
        loadMoreEntriesLoading.SetActive(false);
    }

    public void ShowMoreChatsLoading()
    { 
        loadMoreEntriesLoading.SetActive(true);
    }

    public void HideSearchLoading()
    {
        searchLoading.SetActive(false);
    }

    public void ShowSearchLoading()
    {
        searchLoading.SetActive(true);
        searchLoading.transform.SetAsLastSibling();
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
        searchLoading.transform.SetAsLastSibling();
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