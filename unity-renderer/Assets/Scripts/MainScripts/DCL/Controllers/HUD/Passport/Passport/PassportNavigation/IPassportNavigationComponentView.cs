using System;

namespace DCL.Social.Passports
{
    public interface IPassportNavigationComponentView
    {
        event Action<string> OnClickBuyNft;
        void InitializeView();
        void SetGuestUser(bool isGuest);
        void SetName(string username);
        void SetDescription(string description);
        void SetEquippedWearables(WearableItem[] wearables);
        void SetCollectibleWearables(WearableItem[] wearables);
        void SetCollectibleEmotes(WearableItem[] emotes);
        void SetInitialTab();
    }
}
