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
        void SetOwnUserTexts(bool isOwnUser);
        void SetDescription(string description);
        void SetEquippedWearables(WearableItem[] wearables, string bodyShapeId);
        void SetCollectibleWearables(WearableItem[] wearables);
        void SetCollectibleWearablesLoadingActive(bool isActive);
        void SetCollectibleEmotes(WearableItem[] emotes);
        void SetCollectibleEmotesLoadingActive(bool isActive);
        void SetCollectibleNames(NamesResponse.NameEntry[] names);
        void SetCollectibleNamesLoadingActive(bool isActive);
        void SetCollectibleLands(LandsResponse.LandEntry[] lands);
        void SetCollectibleLandsLoadingActive(bool isActive);
        void SetHasBlockedOwnUser(bool isBlocked);
        void CloseAllNFTItemInfos();
        void SetInitialPage();
    }
}
