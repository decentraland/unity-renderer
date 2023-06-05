using DCLServices.WearablesCatalogService;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public class BackpackAnalyticsService : IBackpackAnalyticsService
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
        private const string FORCE_SHOW_WEARABLE = "show_wearable";
        private const string FORCE_HIDE_WEARABLE = "hide_wearable";

        private readonly IAnalytics analytics;
        private readonly INewUserExperienceAnalytics newUserExperienceAnalytics;
        private const int NORMAL_OUTFIT_SLOTS_COUNT = 3;

        public BackpackAnalyticsService(
            IAnalytics analytics,
            INewUserExperienceAnalytics newUserExperienceAnalytics)
        {
            this.analytics = analytics;
            this.newUserExperienceAnalytics = newUserExperienceAnalytics;
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

        public void SendWearableSortedBy(NftOrderByOperation order, bool asc) =>
            analytics.SendAnalytic(WEARABLE_SORTED_BY, new () { { "sorted_by", CalculateSorting(order, asc) } });

        public void SendWearablePreviewRotated() =>
            analytics.SendAnalytic(WEARABLE_PREVIEW_ROTATED, null);

        public void SendWearableCreatorGoTo(string creatorName) =>
            analytics.SendAnalytic(WEARABLE_CREATOR_GO_TO, new () {{"creator_name", creatorName}});

        public void SendAvatarEditSuccessNuxAnalytic() =>
            newUserExperienceAnalytics.AvatarEditSuccessNux();

        public void SendEquipWearableAnalytic(string category, string rarity, EquipWearableSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "rarity", rarity },
                { "category", category },
            };

            data.Add("source", source.ToString());

            analytics.SendAnalytic(EQUIP_WEARABLE_METRIC, data);
        }

        public void SendUnequippedWearableAnalytic(string category, string rarity, UnequipWearableSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "rarity", rarity },
                { "category", category },
            };
            data.Add("source", source.ToString());

            analytics.SendAnalytic(UNEQUIP_WEARABLE_METRIC, data);
        }

        public void SendAvatarColorPick() =>
            analytics.SendAnalytic(AVATAR_COLOR_PICK, new Dictionary<string, string>());

        public void SendForceHideWearable(string category)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "category", category },
            };

            analytics.SendAnalytic(FORCE_HIDE_WEARABLE, data);
        }

        public void SendForceShowWearable(string category)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "category", category },
            };

            analytics.SendAnalytic(FORCE_SHOW_WEARABLE, data);
        }

        private string CalculateSorting(NftOrderByOperation order, bool asc)
        {
            if (order == NftOrderByOperation.Date)
                return asc ? "newest" : "oldest";

            if (order == NftOrderByOperation.Name)
                return asc ? "nameaz" : "nameza";

            if (order == NftOrderByOperation.Rarity)
                return asc ? "rarest" : "less_rare";

            return "";
        }
    }
}
