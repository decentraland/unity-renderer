using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class ChannelMembersComponentView : BaseComponentView, IChannelMembersComponentView
    {
        private const float REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD = 0.005f;
        private const int ENTRIES_THROTTLING = 30;

        [SerializeField] internal CollapsableChannelMemberListComponentView memberList;
        [SerializeField] internal GameObject resultsHeaderLabelContainer;
        [SerializeField] internal TMP_Text resultsHeaderLabel;
        [SerializeField] internal GameObject loadingContainer;
        [SerializeField] internal ScrollRect scroll;
        [SerializeField] internal SearchBarComponentView searchBar;
        [SerializeField] internal GameObject loadMoreContainer;
        [SerializeField] internal GameObject loadMoreSpinner;
        [SerializeField] internal GameObject emptyStateForCommon;
        [SerializeField] internal GameObject emptyStateForSearch;

        private readonly Queue<ChannelMemberEntryModel> queuedEntries = new Queue<ChannelMemberEntryModel>();
        private bool isLayoutDirty;
        private bool isSortDirty;
        private Vector2 lastScrollPosition;
        private Coroutine requireMoreEntriesRoutine;

        public event Action<string> OnSearchUpdated;
        public event Action OnRequestMoreMembers;

        public int EntryCount => memberList.Count() + queuedEntries.Count;

        public override void Awake()
        {
            base.Awake();

            searchBar.OnSearchText += s =>
            {
                SetSearchModeActive(!string.IsNullOrEmpty(s));
                OnSearchUpdated?.Invoke(s);
            };
            memberList.SortingMethod = (a, b) => a.Model.userName.CompareTo(b.Model.userName);
            scroll.onValueChanged.AddListener(LoadMoreEntries);
        }

        public void Update()
        {
            if (isLayoutDirty)
                ((RectTransform)scroll.transform).ForceUpdateLayout();

            isLayoutDirty = false;

            SetQueuedEntries();

            if (isSortDirty)
                memberList.Sort();

            isSortDirty = false;
        }

        public override void Dispose()
        {
            if (!this) return;
            if (!gameObject) return;
            base.Dispose();
        }

        public void ClearAllEntries()
        {
            memberList.Clear();
            queuedEntries.Clear();
            UpdateLayout();
            UpdateHeaders();
        }

        public void ShowLoading()
        {
            loadingContainer.SetActive(true);
            memberList.gameObject.SetActive(false);
            resultsHeaderLabel.gameObject.SetActive(false);
        }

        public void Set(ChannelMemberEntryModel user) => queuedEntries.Enqueue(user);

        public void Remove(string userId)
        {
            memberList.Remove(userId);
            UpdateLayout();
            UpdateHeaders();
        }

        public override void Show(bool instant = false) => gameObject.SetActive(true);

        public override void Hide(bool instant = false) => gameObject.SetActive(false);

        public void ClearSearchInput(bool notify = true)
        {
            searchBar.ClearSearch(notify);
            SetSearchModeActive(false);
        }

        public void HideLoading()
        {
            loadingContainer.SetActive(false);
            memberList.gameObject.SetActive(true);
            resultsHeaderLabel.gameObject.SetActive(true);
        }

        public void ShowLoadingMore() => loadMoreContainer.SetActive(true);

        public void HideLoadingMore() => loadMoreContainer.SetActive(false);

        public void ShowResultsHeader() => resultsHeaderLabelContainer.SetActive(true);

        public void HideResultsHeader() => resultsHeaderLabelContainer.SetActive(false);

        public override void RefreshControl() { }

        private void UpdateLayout() => isLayoutDirty = true;

        private void Sort() => isSortDirty = true;

        private void UpdateHeaders()
        {
            var text = $"Results ({memberList.Count()})";

            if (!string.IsNullOrEmpty(searchBar.Text))
                text = "Did you mean?";

            resultsHeaderLabel.text = text;
        }

        private void LoadMoreEntries(Vector2 scrollPosition)
        {
            if (scrollPosition.y < REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD &&
                lastScrollPosition.y >= REQUEST_MORE_ENTRIES_SCROLL_THRESHOLD)
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
            OnRequestMoreMembers?.Invoke();
        }

        private void SetQueuedEntries()
        {
            if (queuedEntries.Count <= 0) return;

            for (var i = 0; i < ENTRIES_THROTTLING && queuedEntries.Count > 0; i++)
            {
                var user = queuedEntries.Dequeue();
                memberList.Set(user.userId, user);
            }

            UpdateLayout();
            Sort();
            UpdateHeaders();
        }

        private void SetSearchModeActive(bool isActive)
        {
            if (emptyStateForCommon != null)
                emptyStateForCommon.SetActive(!isActive);

            if (emptyStateForSearch != null)
                emptyStateForSearch.SetActive(isActive);
        }
    }
}
