using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldChatWindowComponentView : BaseComponentView, IWorldChatWindowView, IComponentModelConfig<WorldChatWindowModel>
{
    private const int CREATION_AMOUNT_PER_FRAME = 5;
    private const int AVATAR_SNAPSHOTS_PER_FRAME = 5;
    private const float REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD = 0.005f;
    private const float MIN_TIME_TO_REQUIRE_MORE_ENTRIES = 0.5f;

    [SerializeField] internal CollapsablePublicChannelListComponentView publicChannelList;
    [SerializeField] internal CollapsableDirectChatListComponentView directChatList;
    [SerializeField] internal CollapsableChatSearchListComponentView searchResultsList;
    [SerializeField] internal GameObject searchLoading;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal GameObject directChatsLoadingContainer;
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
    private int currentAvatarSnapshotIndex;
    private bool isLoadingPrivateChannels;
    private bool isSearchMode;
    private Vector2 lastScrollPosition;
    private float loadMoreEntriesRestrictionTime;
    private Coroutine requireMoreEntriesRoutine;

    public event Action OnClose;
    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChannel;

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
        scroll.onValueChanged.AddListener(RequestMorePrivateChats);
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
        searchResultsList.Remove(userId);
        UpdateHeaders();
        UpdateLayout();
    }

    public void SetPublicChannel(PublicChatChannelModel model)
    {
        var entryModel = new PublicChannelEntry.PublicChannelEntryModel(model.channelId, name = model.name);

        if (isSearchMode)
            searchResultsList.Set(entryModel);
        else
            publicChannelList.Set(model.channelId, entryModel);
            
        UpdateLayout();
    }

    public void Configure(WorldChatWindowModel newModel)
    {
        model = newModel;
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

    public void DisableSearchMode()
    {
        isSearchMode = false;
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
        searchResultsList.Clear();

        UpdateHeaders();
    }

    public void EnableSearchMode()
    {
        isSearchMode = true;

        searchResultsList.Clear();
        searchResultsList.Show();
        searchResultsList.Sort();
        publicChannelList.Hide();
        directChatList.Hide();
        directChannelHeader.SetActive(false);
        searchResultsHeader.SetActive(true);

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
            model.recentMessage != null ? model.recentMessage.body : string.Empty,
            user.face256SnapshotURL,
            model.isBlocked,
            model.isOnline,
            model.recentMessage != null ? model.recentMessage.timestamp : 0);

        if (isSearchMode)
            searchResultsList.Set(entry);
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
        
        if (visible)
            directChatList.Hide();
        else if (!isSearchMode)
            directChatList.Show();
        
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
        
        HideMoreChatsLoading();
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

    private void RequestMorePrivateChats(Vector2 position)
    {
        if (!loadMoreEntriesContainer.activeInHierarchy ||
            loadMoreEntriesLoading.activeInHierarchy ||
            Time.realtimeSinceStartup - loadMoreEntriesRestrictionTime < MIN_TIME_TO_REQUIRE_MORE_ENTRIES) return;

        if (position.y < REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD && lastScrollPosition.y >= REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD)
        {
            if (requireMoreEntriesRoutine != null)
                StopCoroutine(requireMoreEntriesRoutine);

            ShowMoreChatsLoading();
            requireMoreEntriesRoutine = StartCoroutine(WaitThenRequireMoreEntries());

            loadMoreEntriesRestrictionTime = Time.realtimeSinceStartup;
        }

        lastScrollPosition = position;
    }
    
    private IEnumerator WaitThenRequireMoreEntries()
    {
        yield return new WaitForSeconds(1f);
        OnRequireMorePrivateChats?.Invoke();
    }
}