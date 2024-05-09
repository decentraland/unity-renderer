using System;

namespace DCL.MyAccount
{
    public interface IBlockedListComponentView
    {
        event Action<string> OnUnblockUser;
        void SetLoadingActive(bool isActive);
        void Set(BlockedUserEntryModel user);
        void Remove(string userId);
        void ClearAllEntries();
        void SetupBlockedList();
    }
}
