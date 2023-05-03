using DCLServices.WearablesCatalogService;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public class BackpackAnalyticsController
    {
        private const string AVATAR_SCREENSHOT = "avatar_screenshot";
        private const string OUTFIT_EQUIP = "outfit_equip";
        private const string OUTFIT_SAVE = "outfit_save";
        private const string OUTFIT_DELETE = "outfit_delete";
        private const string OUTFIT_BUY_SLOT = "outfit_buy_slot";
        private const string WEARABLE_SEARCH = "wearable_search";
        private const string WEARABLE_FILTER = "wearable_filter";
        private const string WEARABLE_SORTED_BY = "wearable_sorted_by";
        private const string WEARABLE_PREVIEW_ROTATED = "wearable_preview_rotated";
        private const string WEARABLE_CREATOR_GO_TO = "wearable_creator_go_to";
        private const string EQUIP_WEARABLE_METRIC = "equip_wearable";
        private const string UNEQUIP_WEARABLE_METRIC = "unequip_wearable";
        private const string AVATAR_COLOR_PICK = "avatar_color_pick";

        private readonly IAnalytics analytics;
        private readonly INewUserExperienceAnalytics newUserExperienceAnalytics;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private const int NORMAL_OUTFIT_SLOTS_COUNT = 3;

        public BackpackAnalyticsController(
            IAnalytics analytics,
            INewUserExperienceAnalytics newUserExperienceAnalytics,
            IWearablesCatalogService wearablesCatalogService)
        {
            this.analytics = analytics;
            this.newUserExperienceAnalytics = newUserExperienceAnalytics;
            this.wearablesCatalogService = wearablesCatalogService;
        }

        public void SendAvatarScreenshot(AvatarScreenshotSource source) =>
            analytics.SendAnalytic(AVATAR_SCREENSHOT, new Dictionary<string, string>() { {"source", source.ToString()} } );

        public void SendOutfitEquipped(int slotNumber) =>
            analytics.SendAnalytic(OUTFIT_EQUIP,new () { { "slot_number", slotNumber.ToString()} });

        public void SendOutfitSave(int slotNumber) =>
            analytics.SendAnalytic(OUTFIT_SAVE, new ()
            {
                { "slot_number", slotNumber.ToString() },
                { "slot_type", slotNumber > NORMAL_OUTFIT_SLOTS_COUNT ? SlotType.Extra.ToString() : SlotType.Free.ToString() }
            });

        public void SendOutfitDelete(int slotNumber) =>
            analytics.SendAnalytic(OUTFIT_DELETE, new () { { "slot_number", slotNumber.ToString()} });

        public void SendOutfitBuySlot(int slotNumber) =>
            analytics.SendAnalytic(OUTFIT_BUY_SLOT, new () { { "slot_number", slotNumber.ToString()} });

        public void SendWearableSearch(string searchTerms) =>
            analytics.SendAnalytic(WEARABLE_SEARCH, new () {{"search_terms", searchTerms}});

        public void SendWearableFilter(bool onlyNft) =>
            analytics.SendAnalytic(WEARABLE_FILTER, new () {{"only_nft", onlyNft.ToString()}});

        public void SendWearableSortedBy(string sortMethod) =>
            analytics.SendAnalytic(WEARABLE_SORTED_BY, new () {{"sorted_by", sortMethod}});

        public void SendWearablePreviewRotated() =>
            analytics.SendAnalytic(WEARABLE_PREVIEW_ROTATED, null);

        public void SendWearableCreatorGoTo(string creatorName) =>
            analytics.SendAnalytic(WEARABLE_CREATOR_GO_TO, new () {{"creator_name", creatorName}});

        public void SendNewEquippedWearablesAnalytics(List<string> oldWearables, List<string> newWearables)
        {
            foreach (string newWearable in newWearables)
            {
                if (oldWearables.Contains(newWearable))
                    continue;

                wearablesCatalogService.WearablesCatalog.TryGetValue(newWearable, out WearableItem newEquippedEmote);
                if (newEquippedEmote != null && !newEquippedEmote.IsEmote())
                    SendEquipWearableAnalytic(newEquippedEmote, EquipWearableSource.Wearable);
            }
        }

        public void SendAvatarEditSuccessNuxAnalytic() =>
            newUserExperienceAnalytics.AvatarEditSuccessNux();

        public void SendEquipWearableAnalytic(WearableItem equippedWearable, EquipWearableSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "name", equippedWearable.GetName() },
                { "rarity", equippedWearable.rarity },
                { "category", equippedWearable.data.category },
                { "linked_wearable", equippedWearable.IsFromThirdPartyCollection.ToString() },
                { "third_party_collection_id", equippedWearable.ThirdPartyCollectionId },
                { "is_in_l2", equippedWearable.IsInL2().ToString() },
                { "smart_item", equippedWearable.IsSmart().ToString() },
            };

            data.Add("source", source.ToString());

            analytics.SendAnalytic(EQUIP_WEARABLE_METRIC, data);
        }

        public void SendUnequippedWearableAnalytic(WearableItem unequippedWearable, UnequipWearableSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "name", unequippedWearable.GetName() },
                { "rarity", unequippedWearable.rarity },
                { "category", unequippedWearable.data.category },
                { "linked_wearable", unequippedWearable.IsFromThirdPartyCollection.ToString() },
                { "third_party_collection_id", unequippedWearable.ThirdPartyCollectionId },
                { "is_in_l2", unequippedWearable.IsInL2().ToString() },
                { "smart_item", unequippedWearable.IsSmart().ToString() },
            };
            data.Add("source", source.ToString());

            analytics.SendAnalytic(UNEQUIP_WEARABLE_METRIC, data);
        }

        public void SendAvatarColorPick() =>
            analytics.SendAnalytic(AVATAR_COLOR_PICK, null);
    }
}
