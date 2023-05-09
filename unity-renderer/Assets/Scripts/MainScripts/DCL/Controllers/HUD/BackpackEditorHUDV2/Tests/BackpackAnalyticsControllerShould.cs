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
        public void SendNewEquippedWearablesAnalyticsCorrectly()
        {
            // Arrange
            var testWearable1 = new WearableItem { id = "newWearable1", i18n = new[] { new i18n() }, data = new WearableItem.Data { category = "test" } };
            var testWearable2 = new WearableItem { id = "newWearable2", i18n = new[] { new i18n() }, data = new WearableItem.Data { category = "test" } };
            var testWearable3 = new WearableItem { id = "newWearable3", i18n = new[] { new i18n() }, data = new WearableItem.Data { category = "test" } };
            var oldWearables = new List<string> { testWearable1.id };
            var newWearables = new List<string> { testWearable1.id, testWearable2.id, testWearable3.id };

            wearablesCatalogService
               .Configure()
               .WearablesCatalog.Returns(new BaseDictionary<string, WearableItem>
                {
                    { testWearable1.id, testWearable1 },
                    { testWearable2.id, testWearable2 },
                    { testWearable3.id, testWearable3 },
                });

            // Act
            backpackAnalyticsController.SendNewEquippedWearablesAnalytics(oldWearables, newWearables);

            // Assert
            analytics.Received(2).SendAnalytic("equip_wearable", Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void SendAvatarEditSuccessNuxAnalyticCorrectly()
        {
            // Act
            backpackAnalyticsController.SendAvatarEditSuccessNuxAnalytic();

            // Assert
            newUserExperienceAnalytics.Received(1).AvatarEditSuccessNux();
        }
    }
}
