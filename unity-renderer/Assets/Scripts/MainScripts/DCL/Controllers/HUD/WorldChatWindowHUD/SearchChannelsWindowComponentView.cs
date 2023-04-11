using System;
using System.Collections;
using DCL.Chat.Channels;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class SearchChannelsWindowComponentView : BaseComponentView, ISearchChannelsWindowView
    {
        [SerializeField] internal CollapsablePublicChannelListComponentView channelList;
        [SerializeField] internal GameObject resultsHeaderLabelContainer;
        [SerializeField] internal TMP_Text resultsHeaderLabel;
        [SerializeField] internal GameObject loadingContainer;
        [SerializeField] internal ScrollRect scroll;
        [SerializeField] internal SearchBarComponentView searchBar;
        [SerializeField] internal Button backButton;
        [SerializeField] internal Button closeButton;
        [SerializeField] internal GameObject loadMoreContainer;
        [SerializeField] internal GameObject loadMoreContent;
        [SerializeField] internal GameObject loadMoreSpinner;
        [SerializeField] internal GameObject createChannelOnSearchContainer;
        [SerializeField] internal GameObject createChannelOnSearchContent;
        [SerializeField] internal Button[] createChannelButtons;

        private bool isLayoutDirty;
        private bool isSortDirty;
        private Vector2 lastScrollPosition;
        private Coroutine requireMoreEntriesRoutine;
        private bool shouldCreationBeActive;
        private bool isLoading;

        public event Action OnBack;
        public event Action OnClose;
        public event Action<string> OnSearchUpdated;
        public event Action OnRequestMoreChannels;
        public event Action<string> OnJoinChannel;
        public event Action<string> OnLeaveChannel;
        public event Action OnCreateChannel;
        public event Action<string> OnOpenChannel;

        public RectTransform Transform => (RectTransform) transform;
        public int EntryCount => channelList.Count();
        public bool IsActive => gameObject.activeInHierarchy;

        public override void Awake()
        {
            base.Awake();
            backButton.onClick.AddListener(() => OnBack?.Invoke());
            closeButton.onClick.AddListener(() => OnClose?.Invoke());
            searchBar.OnSearchText += s => OnSearchUpdated?.Invoke(s);
            channelList.SortingMethod = (a, b) => b.Model.memberCount.CompareTo(a.Model.memberCount);
            scroll.onValueChanged.AddListener(LoadMoreEntries);

            foreach (var button in createChannelButtons)
                button.onClick.AddListener(() => OnCreateChannel?.Invoke());
        }

        public void Update()
        {
            if (isLayoutDirty)
                ((RectTransform) scroll.transform).ForceUpdateLayout();
            isLayoutDirty = false;

            if (isSortDirty)
                channelList.Sort();
            isSortDirty = false;
        }

        public override void Dispose()
        {
            if (!this) return;
            if (!gameObject) return;
            base.Dispose();
        }

        public static SearchChannelsWindowComponentView Create()
        {
            return Instantiate(Resources.Load<SearchChannelsWindowComponentView>("SocialBarV1/ChannelSearchHUD"));
        }

        public void ClearAllEntries()
        {
            channelList.Clear();
            UpdateLayout();
            UpdateHeaders();
        }

        public void ShowLoading()
        {
            isLoading = true;
            loadingContainer.SetActive(true);
            channelList.gameObject.SetActive(false);
            resultsHeaderLabel.gameObject.SetActive(false);
            createChannelOnSearchContent.SetActive(false);
            loadMoreContent.SetActive(false);
        }

        public void Set(Channel channel)
        {
            channelList.Set(channel.ChannelId,
                new PublicChatEntryModel(channel.ChannelId, channel.Name, channel.Joined, channel.MemberCount,
                    showOnlyOnlineMembers: false, channel.Muted));

            var entry = channelList.Get(channel.ChannelId);
            entry.OnJoin -= HandleJoinRequest;
            entry.OnJoin += HandleJoinRequest;
            entry.OnOpenChat -= HandleOpenChat;
            entry.OnOpenChat += HandleOpenChat;
            entry.OnLeave -= HandleLeaveRequest;
            entry.OnLeave += HandleLeaveRequest;

            UpdateLayout();
            Sort();
            UpdateHeaders();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            searchBar.SetFocus();
        }

        public void Hide() => gameObject.SetActive(false);

        public void ClearSearchInput() => searchBar.ClearSearch();

        public void HideLoading()
        {
            isLoading = false;
            loadingContainer.SetActive(false);
            channelList.gameObject.SetActive(true);
            resultsHeaderLabel.gameObject.SetActive(true);
            createChannelOnSearchContent.SetActive(shouldCreationBeActive);
            loadMoreContent.SetActive(true);
        }

        public void ShowLoadingMore() => loadMoreContainer.SetActive(true);

        public void HideLoadingMore() => loadMoreContainer.SetActive(false);

        public void ShowResultsHeader() => resultsHeaderLabelContainer.SetActive(true);

        public void HideResultsHeader() => resultsHeaderLabelContainer.SetActive(false);

        public void ShowCreateChannelOnSearch() => createChannelOnSearchContainer.SetActive(true);

        public void HideCreateChannelOnSearch() => createChannelOnSearchContainer.SetActive(false);

        public void SetCreateChannelButtonsActive(bool isActive)
        {
            shouldCreationBeActive = isActive;

            foreach (var button in createChannelButtons)
                button.gameObject.SetActive(isActive);

            createChannelOnSearchContent.SetActive(isActive && !isLoading);
        }

        public override void RefreshControl()
        {
            throw new NotImplementedException();
        }

        private void UpdateLayout() => isLayoutDirty = true;

        private void Sort() => isSortDirty = true;

        private void UpdateHeaders()
        {
            var text = $"Results ({channelList.Count()})";

            if (!string.IsNullOrEmpty(searchBar.Text))
                text = "Did you mean?";

            resultsHeaderLabel.text = text;
        }

        private void HandleJoinRequest(PublicChatEntry entry) => OnJoinChannel?.Invoke(entry.Model.channelId);

        private void HandleOpenChat(PublicChatEntry entry) => OnOpenChannel?.Invoke(entry.Model.channelId);

        private void HandleLeaveRequest(PublicChatEntry entry) => OnLeaveChannel?.Invoke(entry.Model.channelId);

        private void LoadMoreEntries(Vector2 scrollPosition)
        {
            if (scrollPosition.y < 0.005f && lastScrollPosition.y >= 0.005f)
            {
                if (requireMoreEntriesRoutine != null)
                    StopCoroutine(requireMoreEntriesRoutine);

                loadMoreSpinner.SetActive(true);
                requireMoreEntriesRoutine = StartCoroutine(WaitThenRequireMoreEntries());
            }

            lastScrollPosition = scrollPosition;
        }

        private IEnumerator WaitThenRequireMoreEntries()
        {
            yield return new WaitForSeconds(1f);
            loadMoreSpinner.SetActive(false);
            OnRequestMoreChannels?.Invoke();
        }
    }
}
