using DCLServices.WearablesCatalogService;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public class BackpackAnalyticsControllerShould
    {
        private IAnalytics analytics;
        private INewUserExperienceAnalytics newUserExperienceAnalytics;
        private IWearablesCatalogService wearablesCatalogService;
        private BackpackAnalyticsController backpackAnalyticsController;

        [SetUp]
        public void SetUp()
        {
            analytics = Substitute.For<IAnalytics>();
            newUserExperienceAnalytics = Substitute.For<INewUserExperienceAnalytics>();
            wearablesCatalogService = Substitute.For<IWearablesCatalogService>();
            backpackAnalyticsController = new BackpackAnalyticsController(analytics, newUserExperienceAnalytics, wearablesCatalogService);
        }

        [Test]
        public void SendAvatarEditSuccessNuxAnalyticCorrectly()
        {
            // Act
            backpackAnalyticsController.SendAvatarEditSuccessNuxAnalytic();

            // Assert
            newUserExperienceAnalytics.Received(1).AvatarEditSuccessNux();
        }

        [Test]
        public void SendEquipAnalyticCorrectly()
        {
            backpackAnalyticsController.SendEquipWearableAnalytic("testcat", "rare", EquipWearableSource.Wearable);

            analytics.Received(1).SendAnalytic("equip_wearable", Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void SendUnEquipAnalyticCorrectly()
        {
            backpackAnalyticsController.SendUnequippedWearableAnalytic("testcat", "rare", UnequipWearableSource.AvatarSlot);

            analytics.Received(1).SendAnalytic("unequip_wearable", Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void SendWearableFilter()
        {
            backpackAnalyticsController.SendWearableFilter(false);

            analytics.Received(1).SendAnalytic("wearable_filter", Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void SendWearableSort()
        {
            backpackAnalyticsController.SendWearableSortedBy(NftOrderByOperation.Date, false);

            analytics.Received(1).SendAnalytic("wearable_sorted_by", Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void SendWearableSearch()
        {
            backpackAnalyticsController.SendWearableSearch("testsearch");

            analytics.Received(1).SendAnalytic("wearable_search", Arg.Any<Dictionary<string, string>>());
        }
    }
}
