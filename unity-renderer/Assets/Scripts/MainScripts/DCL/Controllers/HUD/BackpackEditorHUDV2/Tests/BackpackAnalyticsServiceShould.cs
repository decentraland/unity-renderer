using DCLServices.WearablesCatalogService;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public class BackpackAnalyticsServiceShould
    {
        private IAnalytics analytics;
        private INewUserExperienceAnalytics newUserExperienceAnalytics;
        private BackpackAnalyticsService backpackAnalyticsService;

        [SetUp]
        public void SetUp()
        {
            analytics = Substitute.For<IAnalytics>();
            newUserExperienceAnalytics = Substitute.For<INewUserExperienceAnalytics>();
            backpackAnalyticsService = new BackpackAnalyticsService(analytics, newUserExperienceAnalytics);
        }

        [Test]
        public void SendAvatarEditSuccessNuxAnalyticCorrectly()
        {
            // Act
            backpackAnalyticsService.SendAvatarEditSuccessNuxAnalytic();

            // Assert
            newUserExperienceAnalytics.Received(1).AvatarEditSuccessNux();
        }

        [Test]
        public void SendEquipAnalyticCorrectly()
        {
            backpackAnalyticsService.SendEquipWearableAnalytic("testcat", "rare", EquipWearableSource.Wearable);

            analytics.Received(1).SendAnalytic("equip_wearable", Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void SendUnEquipAnalyticCorrectly()
        {
            backpackAnalyticsService.SendUnequippedWearableAnalytic("testcat", "rare", UnequipWearableSource.AvatarSlot);

            analytics.Received(1).SendAnalytic("unequip_wearable", Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void SendWearableFilter()
        {
            backpackAnalyticsService.SendWearableFilter(false);

            analytics.Received(1).SendAnalytic("wearable_filter", Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void SendWearableSort()
        {
            backpackAnalyticsService.SendWearableSortedBy(NftOrderByOperation.Date, false);

            analytics.Received(1).SendAnalytic("wearable_sorted_by", Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void SendWearableSearch()
        {
            backpackAnalyticsService.SendWearableSearch("testsearch");

            analytics.Received(1).SendAnalytic("wearable_search", Arg.Any<Dictionary<string, string>>());
        }
    }
}
