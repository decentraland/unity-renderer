using DCL.Helpers;
using System;
using UnityEngine;

namespace DCL.MyAccount
{
    public class BlockedListComponentView : BaseComponentView, IBlockedListComponentView
    {
        [Header("General")]
        [SerializeField] internal GameObject mainContainer;
        [SerializeField] internal GameObject loadingContainer;
        [SerializeField] internal RectTransform contentTransform;
        [SerializeField] internal GameObject scrollBar;
        [SerializeField] internal CollapsableSortedBlockedEntryList blockedList;

        public event Action<string> OnUnblockUser;

        public void SetupBlockedList()
        {
            blockedList.OnUnblockUser = OnUnblockUser;
            int SortByAlphabeticalOrder(BlockedUserEntry u1, BlockedUserEntry u2)
            {
                return string.Compare(u1.Model.userName, u2.Model.userName, StringComparison.InvariantCultureIgnoreCase);
            }

            blockedList.SortingMethod = SortByAlphabeticalOrder;
        }

        public void Set(BlockedUserEntryModel user)
        {
            blockedList.Set(user.userId, user);
            RefreshContentLayout();
        }

        public void Remove(string userId)
        {
            blockedList.Remove(userId);
            RefreshContentLayout();
        }

        public void ClearAllEntries()
        {
            blockedList.Clear();
        }

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);

            if (scrollBar != null)
                scrollBar.SetActive(true);
        }

        public override void Hide(bool instant = false)
        {
            gameObject.SetActive(false);

            if (scrollBar != null)
                scrollBar.SetActive(false);
        }

        public void SetLoadingActive(bool isActive)
        {
            loadingContainer.SetActive(isActive);
            mainContainer.SetActive(!isActive);
        }

        public void RefreshContentLayout() =>
            Utils.ForceRebuildLayoutImmediate(contentTransform);

        public override void RefreshControl()
        {
        }
    }
}
