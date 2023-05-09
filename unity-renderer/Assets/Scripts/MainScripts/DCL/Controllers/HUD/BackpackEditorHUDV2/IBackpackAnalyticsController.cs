using DCLServices.WearablesCatalogService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackAnalyticsController
    {
        void SendAvatarScreenshot(AvatarScreenshotSource source);
        void SendOutfitEquipped(int slotNumber);
        void SendOutfitSave(int slotNumber);
        void SendOutfitDelete(int slotNumber);
        void SendOutfitBuySlot(int slotNumber);
        void SendWearableSearch(string searchTerms);
        void SendWearableFilter(bool onlyNft);
        void SendWearableSortedBy(NftOrderByOperation order, bool asc);
        void SendWearablePreviewRotated();
        void SendWearableCreatorGoTo(string creatorName);
        void SendAvatarEditSuccessNuxAnalytic();
        void SendEquipWearableAnalytic(string category, string rarity, EquipWearableSource source);
        void SendUnequippedWearableAnalytic(string category, string rarity, UnequipWearableSource source);
        void SendAvatarColorPick();
    }
}
