using Cysharp.Threading.Tasks;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.WearablesCatalogService;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class ViewAllComponentControllerShould
    {
        private ViewAllComponentController controller;
        private IViewAllComponentView view;
        private IWearablesCatalogService wearablesCatalogService;
        private ILandsService landsService;
        private INamesService namesService;
        private NotificationsController notificationsController;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IViewAllComponentView>();
            wearablesCatalogService = Substitute.For<IWearablesCatalogService>();
            landsService = Substitute.For<ILandsService>();
            namesService = Substitute.For<INamesService>();
            notificationsController = NotificationsController.i;

            controller = new ViewAllComponentController(
                view,
                Substitute.For<DataStore_HUDs>(),
                wearablesCatalogService,
                landsService,
                namesService,
                notificationsController);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        [TestCase(PassportSection.Wearables)]
        [TestCase(PassportSection.Names)]
        [TestCase(PassportSection.Lands)]
        [TestCase(PassportSection.Emotes)]
        public void OpenViewAllSectionCorrectly(PassportSection section)
        {
            // Act
            controller.OpenViewAllSection(section);

            // Assert
            view.Received(1).Initialize(section);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetViewAllVisibilityCorrectly(bool isVisible)
        {
            // Act
            controller.SetViewAllVisibility(isVisible);

            // Assert
            view.Received(1).SetVisible(isVisible);

            if (!isVisible)
                view.Received(1).SetLoadingActive(false);
        }

        [Test]
        public void BackFromViewAllCorrectly()
        {
            // Act
            view.OnBackFromViewAll += Raise.Event<Action>();

            // Assert
            view.Received(1).SetVisible(false);
            view.Received(1).SetLoadingActive(false);
            view.Received(1).CloseAllNftItemInfos();
        }

        [Test]
        public void RequestWearablesElementsCorrectly()
        {
            // Arrange
            List<WearableItem> testWearables = new List<WearableItem>
            {
                new()
                {
                    id = "testWearable1",
                    data = new WearableItem.Data { category = "testCategory1" },
                    i18n = new[] { new i18n { code = "en", text = "testName1" } },
                    rarity = "testRarity1",
                    baseUrl = "testBaseUrl1",
                    thumbnail = "testThumbnail1",
                },
                new()
                {
                    id = "testWearable2",
                    data = new WearableItem.Data { category = "testCategory2" },
                    i18n = new[] { new i18n { code = "en", text = "testName2" } },
                    rarity = "testRarity2",
                    baseUrl = "testBaseUrl2",
                    thumbnail = "testThumbnail2",
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(
                                        Arg.Any<string>(),
                                        Arg.Any<int>(),
                                        Arg.Any<int>(),
                                        Arg.Any<bool>(),
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((testWearables, testWearables.Count)));

            // Act
            view.OnRequestCollectibleElements += Raise.Event<Action<PassportSection, int, int>>(PassportSection.Wearables, 1, 1);

            // Assert
            view.Received(1).CloseAllNftItemInfos();
            view.Received(1).ShowNftIcons(Arg.Any<List<(NFTIconComponentModel model, WearableItem w)>>());
            view.Received(1).SetTotalElements(testWearables.Count);
            view.Received(1).SetLoadingActive(false);
        }
    }
}
