using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using System;

namespace DCL.Social.Passports
{
    public interface IPassportNavigationComponentView
    {
        event Action<string, string> OnClickBuyNft;
        event Action OnClickCollectibles;
        void InitializeView();
        void SetGuestUser(bool isGuest);
        void SetName(string username);
        void SetDescription(string description);
        void SetEquippedWearables(WearableItem[] wearables, string bodyShapeId);
        void SetCollectibleWearables(WearableItem[] wearables);
        void SetCollectibleEmotes(WearableItem[] emotes);
        void SetCollectibleNames(NamesResponse.NameEntry[] names);
        void SetCollectibleLands(LandsResponse.LandEntry[] lands);
        void SetHasBlockedOwnUser(bool isBlocked);
        void CloseAllNFTItemInfos();
    }
}
