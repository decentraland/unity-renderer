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
        void SendWearableSortedBy(string sortMethod);
        void SendWearablePreviewRotated();
        void SendWearableCreatorGoTo(string creatorName);
        void SendNewEquippedWearablesAnalytics(List<string> oldWearables, List<string> newWearables);
        void SendAvatarEditSuccessNuxAnalytic();
        void SendEquipWearableAnalytic(WearableItem equippedWearable, EquipWearableSource source);
        void SendUnequippedWearableAnalytic(WearableItem unequippedWearable, UnequipWearableSource source);
        void SendAvatarColorPick();
    }
}
