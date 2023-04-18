using DCLServices.WearablesCatalogService;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public class BackpackAnalytics
    {
        private const string EQUIP_WEARABLE_METRIC = "equip_wearable";

        private readonly IAnalytics analytics;
        private readonly INewUserExperienceAnalytics newUserExperienceAnalytics;
        private readonly IWearablesCatalogService wearablesCatalogService;

        public BackpackAnalytics(
            IAnalytics analytics,
            INewUserExperienceAnalytics newUserExperienceAnalytics,
            IWearablesCatalogService wearablesCatalogService)
        {
            this.analytics = analytics;
            this.newUserExperienceAnalytics = newUserExperienceAnalytics;
            this.wearablesCatalogService = wearablesCatalogService;
        }

        public void SendNewEquippedWearablesAnalytics(List<string> oldWearables, List<string> newWearables)
        {
            foreach (string t in newWearables)
            {
                if (oldWearables.Contains(t))
                    continue;

                wearablesCatalogService.WearablesCatalog.TryGetValue(t, out WearableItem newEquippedEmote);
                if (newEquippedEmote != null && !newEquippedEmote.IsEmote())
                    SendEquipWearableAnalytic(newEquippedEmote);
            }
        }

        public void SendAvatarEditSuccessNuxAnalytics() =>
            newUserExperienceAnalytics.AvatarEditSuccessNux();

        private void SendEquipWearableAnalytic(WearableItem equippedWearable)
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

            analytics.SendAnalytic(EQUIP_WEARABLE_METRIC, data);
        }
    }
}
