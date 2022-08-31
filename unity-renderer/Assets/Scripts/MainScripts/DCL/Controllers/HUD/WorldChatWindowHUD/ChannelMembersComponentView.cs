using DCL.Helpers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class ChannelMembersComponentView : BaseComponentView, IChannelMembersComponentView
    {
        [SerializeField] internal CollapsableChannelMemberListComponentView memberList;
        [SerializeField] internal TMP_Text resultsHeaderLabel;
        [SerializeField] internal GameObject loadingContainer;
        [SerializeField] internal ScrollRect scroll;
        [SerializeField] internal SearchBarComponentView searchBar;
        [SerializeField] internal GameObject loadMoreContainer;
        [SerializeField] internal GameObject loadMoreSpinner;

        private bool isLayoutDirty;
        private bool isSortDirty;
        private Vector2 lastScrollPosition;
        private Coroutine requireMoreEntriesRoutine;

        public event Action<string> OnSearchUpdated;
        public event Action OnRequestMoreMembers;

        public int EntryCount => memberList.Count();
        public bool IsActive => gameObject.activeInHierarchy;

        public override void Awake()
        {
            base.Awake();

            searchBar.OnSearchText += s => OnSearchUpdated?.Invoke(s);
            memberList.SortingMethod = (a, b) => b.Model.userName.CompareTo(a.Model.userName);
            scroll.onValueChanged.AddListener(LoadMoreEntries);
        }

        public override void Update()
        {
            base.Update();

            if (isLayoutDirty)
                ((RectTransform)scroll.transform).ForceUpdateLayout();

            isLayoutDirty = false;

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
            memberList.Clear(true);
            UpdateLayout();
            UpdateHeaders();
        }

        public void ShowLoading()
        {
            loadingContainer.SetActive(true);
            memberList.gameObject.SetActive(false);
            resultsHeaderLabel.gameObject.SetActive(false);
        }

        public void Set(ChannelMemberEntryModel user)
        {
            memberList.Set(user.userId, user);

            UpdateLayout();
            Sort();
            UpdateHeaders();
        }

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);
            searchBar.SetFocus();
        }

        public override void Hide(bool instant = false) => gameObject.SetActive(false);

        public void ClearSearchInput() => searchBar.ClearSearch();

        public void HideLoading()
        {
            loadingContainer.SetActive(false);
            memberList.gameObject.SetActive(true);
            resultsHeaderLabel.gameObject.SetActive(true);
        }

        public void ShowLoadingMore() => loadMoreContainer.SetActive(true);

        public void HideLoadingMore() => loadMoreContainer.SetActive(false);

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
            OnRequestMoreMembers?.Invoke();
        }
    }
}