using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class WorldChatWindowComponentView : BaseComponentView, IWorldChatWindowView, IComponentModelConfig<WorldChatWindowModel>
    {
        private const int CREATION_AMOUNT_PER_FRAME = 5;
        private const int AVATAR_SNAPSHOTS_PER_FRAME = 5;
        private const string NEARBY_CHANNEL = "nearby";

        [SerializeField] internal CollapsablePublicChannelListComponentView publicChannelList;
        [SerializeField] internal CollapsableDirectChatListComponentView directChatList;
        [SerializeField] internal CollapsableChatSearchListComponentView searchResultsList;
        [SerializeField] internal GameObject searchLoading;
        [SerializeField] internal Button closeButton;
        [SerializeField] internal GameObject channelsLoadingContainer;
        [SerializeField] internal GameObject directChatsLoadingContainer;
        [SerializeField] internal GameObject directChatsContainer;
        [SerializeField] internal GameObject emptyDirectChatsContainer;
        [SerializeField] internal GameObject channelsHeader;
        [SerializeField] internal GameObject directChannelHeader;
        [SerializeField] internal GameObject searchResultsHeader;
        [SerializeField] internal TMP_Text channelsHeaderLabel;
        [SerializeField] internal TMP_Text directChatsHeaderLabel;
        [SerializeField] internal TMP_Text searchResultsHeaderLabel;
        [SerializeField] internal ScrollRect scroll;
        [SerializeField] internal SearchBarComponentView searchBar;
        [SerializeField] internal Button openChannelSearchButton;
        [SerializeField] internal ChannelContextualMenu channelContextualMenu;
        [SerializeField] internal Button createChannelButton;
        [SerializeField] private WorldChatWindowModel model;

        [Header("Load More Entries")] [SerializeField]
        internal GameObject loadMoreEntriesContainer;

        [SerializeField] internal TMP_Text loadMoreEntriesLabel;
        [SerializeField] internal GameObject loadMoreEntriesLoading;

        private readonly Dictionary<string, PrivateChatModel> privateChatsCreationQueue =
            new Dictionary<string, PrivateChatModel>();

        private readonly Dictionary<string, PublicChatModel> publicChatsCreationQueue =
            new Dictionary<string, PublicChatModel>();

        private bool isSortingDirty;
        private bool isLayoutDirty;
        private int currentAvatarSnapshotIndex;
        private bool isLoadingPrivateChannels;
        private bool isSearchMode;
        private string optionsChannelId;

        public event Action OnClose;
        public event Action<string> OnOpenPrivateChat;
        public event Action<string> OnOpenPublicChat;
        public event Action<string> OnSearchChatRequested;
        public event Action OnRequireMorePrivateChats;
        public event Action OnOpenChannelSearch;
        public event Action<string> OnLeaveChannel;
        public event Action OnCreateChannel;

        public RectTransform Transform => (RectTransform) transform;
        public bool IsActive => gameObject.activeInHierarchy;

        public int PrivateChannelsCount =>
            directChatList.Count() + privateChatsCreationQueue.Keys.Count(s => !directChatList.Contains(s));

        public int PublicChannelsCount =>
            publicChannelList.Count() + publicChatsCreationQueue.Keys.Count(s => !publicChannelList.Contains(s));

        public static WorldChatWindowComponentView Create()
        {
            return Instantiate(Resources.Load<WorldChatWindowComponentView>("SocialBarV1/ConversationListHUD"));
        }

        public override void Awake()
        {
            base.Awake();
        
            int SortByAlphabeticalOrder(PublicChatEntry u1, PublicChatEntry u2)
            {
                if (u1.Model.name == NEARBY_CHANNEL)
                    return -1;
                else if (u2.Model.name == NEARBY_CHANNEL)
                    return 1;

                return string.Compare(u1.Model.name, u2.Model.name, StringComparison.InvariantCultureIgnoreCase);
            }
        
            openChannelSearchButton.onClick.AddListener(() => OnOpenChannelSearch?.Invoke());
            closeButton.onClick.AddListener(() => OnClose?.Invoke());
            directChatList.SortingMethod = (a, b) => b.Model.lastMessageTimestamp.CompareTo(a.Model.lastMessageTimestamp);
            directChatList.OnOpenChat += entry => OnOpenPrivateChat?.Invoke(entry.Model.userId);
            publicChannelList.SortingMethod = SortByAlphabeticalOrder;
            publicChannelList.OnOpenChat += entry => OnOpenPublicChat?.Invoke(entry.Model.channelId);
            searchBar.OnSearchText += text => OnSearchChatRequested?.Invoke(text);
            scroll.onValueChanged.AddListener((scrollPos) =>
            {
                if (scrollPos.y < 0.005f)
                    OnRequireMorePrivateChats?.Invoke();
            });
            channelContextualMenu.OnLeave += () => OnLeaveChannel?.Invoke(optionsChannelId);
            createChannelButton.onClick.AddListener(() => OnCreateChannel?.Invoke());
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

        public void SetPrivateChat(PrivateChatModel model) => privateChatsCreationQueue[model.user.userId] = model;

        public void RemovePrivateChat(string userId)
        {
            directChatList.Remove(userId);
            searchResultsList.Remove(userId);
            UpdateHeaders();
            UpdateLayout();
        }

        public void SetPublicChat(PublicChatModel model) => publicChatsCreationQueue[model.channelId] = model;

        public void RemovePublicChat(string channelId)
        {
            publicChatsCreationQueue.Remove(channelId);
            publicChannelList.Remove(channelId);
            UpdateHeaders();
            UpdateLayout();
        }

        public void Configure(WorldChatWindowModel newModel)
        {
            model = newModel;
            RefreshControl();
        }

        public void ShowChannelsLoading() => SetChannelsLoadingVisibility(true);

        public void HideChannelsLoading() => SetChannelsLoadingVisibility(false);

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
            emptyDirectChatsContainer.SetActive(directChatList.Count() == 0);
            UpdateLayout();
        }

        public void ShowMoreChatsToLoadHint(int count)
        {
            loadMoreEntriesLabel.SetText(
                $"{count} chats hidden. Use the search bar to find them or click below to show more.");
            loadMoreEntriesContainer.SetActive(true);
            emptyDirectChatsContainer.SetActive(false);
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
            isSearchMode = false;
            searchResultsList.Export(publicChannelList, directChatList);
            searchResultsList.Hide();
            publicChannelList.Show();
            publicChannelList.Sort();

            if (!isLoadingPrivateChannels)
            {
                directChatList.Show();
                directChatList.Sort();
                channelsHeader.SetActive(true);
                directChannelHeader.SetActive(true);
            }

            searchResultsHeader.SetActive(false);

            directChatList.Filter(entry => true);
            publicChannelList.Filter(entry => true);

            UpdateHeaders();
        }

        public void Filter(Dictionary<string, PrivateChatModel> privateChats,
            Dictionary<string, PublicChatModel> publicChannels)
        {
            isSearchMode = true;

            searchResultsList.Import(publicChannelList, directChatList);
            searchResultsList.Show();
            searchResultsList.Sort();
            publicChannelList.Hide();
            directChatList.Hide();
            channelsHeader.SetActive(false);
            directChannelHeader.SetActive(false);
            searchResultsHeader.SetActive(true);

            searchResultsList.Filter(entry => privateChats.ContainsKey(entry.Model.userId),
                entry => publicChannels.ContainsKey(entry.Model.channelId));

            UpdateHeaders();
            searchLoading.transform.SetAsLastSibling();
        }

        public bool ContainsPrivateChannel(string userId) => privateChatsCreationQueue.ContainsKey(userId)
                                                             || directChatList.Get(userId) != null;

        public override void RefreshControl()
        {
            publicChannelList.Clear();
            foreach (var entry in model.publicChannels)
                publicChannelList.Set(entry.channelId, entry);
            SetChannelsLoadingVisibility(model.isLoadingChannels);

            directChatList.Clear();
            foreach (var entry in model.privateChats)
                directChatList.Set(entry.userId, entry);
            SetPrivateChatLoadingVisibility(model.isLoadingDirectChats);
        }

        private void Set(PrivateChatModel model)
        {
            var user = model.user;
            var userId = user.userId;

            var entry = new PrivateChatEntryModel(
                user.userId,
                user.userName,
                model.recentMessage != null ? model.recentMessage.body : string.Empty,
                user.face256SnapshotURL,
                model.isBlocked,
                model.isOnline,
                model.recentMessage != null ? model.recentMessage.timestamp : 0);

            if (isSearchMode)
            {
                directChatList.Remove(userId);
                searchResultsList.Set(entry);
            }
            else
            {
                directChatList.Set(userId, entry);
                searchResultsList.Remove(userId);
            }

            UpdateHeaders();
            UpdateLayout();
            SortLists();
        }
    
        private void Set(PublicChatModel model)
        {
            var channelId = model.channelId;
            var entryModel = new PublicChatEntryModel(channelId, model.name, model.joined, model.memberCount, model.muted);
            PublicChatEntry entry;

            if (isSearchMode)
            {
                publicChannelList.Remove(channelId);
                searchResultsList.Set(entryModel);
                entry = (PublicChatEntry) searchResultsList.Get(channelId);
            }
            else
            {
                publicChannelList.Set(channelId, entryModel);
                searchResultsList.Remove(channelId);
                entry = publicChannelList.Get(channelId);
            }

            entry.OnOpenOptions -= OpenChannelOptions;
            entry.OnOpenOptions += OpenChannelOptions;

            UpdateHeaders();
            UpdateLayout();
            SortLists();
        }

        private void OpenChannelOptions(PublicChatEntry entry)
        {
            optionsChannelId = entry.Model.channelId;
            entry.Dock(channelContextualMenu);
            channelContextualMenu.SetHeaderTitle($"#{optionsChannelId.ToLower()}");
            channelContextualMenu.Show();
        }

        private void SortLists() => isSortingDirty = true;

        private void SetChannelsLoadingVisibility(bool visible)
        {
            model.isLoadingChannels = visible;
            channelsLoadingContainer.SetActive(visible);
            publicChannelList.gameObject.SetActive(!visible);
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
            searchResultsHeader.SetActive(searchResultsList.Count() > 0);
            channelsHeaderLabel.text = $"Channels ({publicChannelList.Count()})";
        }

        private void UpdateLayout() => isLayoutDirty = true;

        private void SetQueuedEntries()
        {
            if (privateChatsCreationQueue.Count > 0)
            {
                for (var i = 0; i < CREATION_AMOUNT_PER_FRAME && privateChatsCreationQueue.Count > 0; i++)
                {
                    var (userId, model) = privateChatsCreationQueue.First();
                    privateChatsCreationQueue.Remove(userId);
                    Set(model);
                }
            }

            if (publicChatsCreationQueue.Count > 0)
            {
                for (var i = 0; i < CREATION_AMOUNT_PER_FRAME && publicChatsCreationQueue.Count > 0; i++)
                {
                    var (userId, model) = publicChatsCreationQueue.First();
                    publicChatsCreationQueue.Remove(userId);
                    Set(model);
                }
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
}