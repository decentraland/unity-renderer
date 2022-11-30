using System.Collections.Generic;

namespace DCL.Social.Passports
{
    public interface IPassportNavigationComponentView
    {
        void InitializeView();
        void SetGuestUser(bool isGuest);
        void SetName(string username);
        void SetDescription(string description);
        void SetEquippedWearables(WearableItem[] wearables);
    }
}
