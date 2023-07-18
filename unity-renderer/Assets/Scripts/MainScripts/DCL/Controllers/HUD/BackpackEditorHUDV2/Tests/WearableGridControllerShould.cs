using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Backpack
{
    public class WearableGridControllerShould
    {
        private string OWN_USER_ID = "ownUserId";

        private WearableGridController controller;
        private IWearableGridView view;
        private IUserProfileBridge userProfileBridge;
        private IWearablesCatalogService wearablesCatalogService;
        private DataStore_BackpackV2 dataStore;
        private BackpackFiltersController backpackFiltersController;
        private UserProfile ownUserProfile;
        private IBrowserBridge browserBridge;
        private AvatarSlotsHUDController avatarSlotsHUDController;
        private IBackpackFiltersComponentView filtersView;
        private IAvatarSlotsView slotsView;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IWearableGridView>();

            ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = "ownUserName",
            });

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            userProfileBridge.GetOwn().Returns(ownUserProfile);

            wearablesCatalogService = AvatarAssetsTestHelpers.CreateTestCatalogLocal();

            dataStore = new DataStore_BackpackV2();

            browserBridge = Substitute.For<IBrowserBridge>();

            filtersView = Substitute.For<IBackpackFiltersComponentView>();

            backpackFiltersController = new BackpackFiltersController(filtersView,
                Substitute.For<IWearablesCatalogService>());

            IBackpackAnalyticsService backpackAnalyticsService = Substitute.For<IBackpackAnalyticsService>();

            slotsView = Substitute.For<IAvatarSlotsView>();

            IBaseVariable<FeatureFlag> ffBaseVariable = Substitute.For<IBaseVariable<FeatureFlag>>();
            var featureFlag = new FeatureFlag();
            ffBaseVariable.Get().Returns(featureFlag);
            avatarSlotsHUDController = new AvatarSlotsHUDController(slotsView, backpackAnalyticsService, ffBaseVariable);

            controller = new WearableGridController(view,
                userProfileBridge,
                wearablesCatalogService,
                dataStore,
                browserBridge,
                backpackFiltersController,
                avatarSlotsHUDController,
                backpackAnalyticsService);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator SetWearableBreadcrumbWhenLoadingWearables()
        {
            const int WEARABLE_TOTAL_AMOUNT = 18;

            IReadOnlyList<WearableItem> wearableList = Array.Empty<WearableItem>();

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, WEARABLE_TOTAL_AMOUNT)));

            controller.LoadWearablesWithFilters();
            yield return null;

            view.Received(1)
                .SetWearableBreadcrumb(Arg.Is<NftBreadcrumbModel>(n => n.Current == 0
                                                                       && n.ResultCount == 0
                                                                       && n.Path[0].Name == "All"
                                                                       && n.Path[0].Filter == "all"));
        }

        [UnityTest]
        public IEnumerator RequestFirstPageWhenLoadingWearables()
        {
            const int WEARABLE_TOTAL_AMOUNT = 18;

            IReadOnlyList<WearableItem> wearableList = Array.Empty<WearableItem>();

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, WEARABLE_TOTAL_AMOUNT)));

            controller.LoadWearablesWithFilters();
            yield return null;

            wearablesCatalogService.Received(1)
                                   .RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>());
        }

        [UnityTest]
        [TestCase(14, 1, ExpectedResult = null)]
        [TestCase(0, 0, ExpectedResult = null)]
        [TestCase(16, 2, ExpectedResult = null)]
        [TestCase(87, 6, ExpectedResult = null)]
        [TestCase(1356, 91, ExpectedResult = null)]
        public IEnumerator SetWearablePagesWhenLoadingWearables(int wearableTotalAmount, int expectedTotalPages)
        {
            IReadOnlyList<WearableItem> wearableList = Array.Empty<WearableItem>();

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, wearableTotalAmount)));

            controller.LoadWearablesWithFilters();
            yield return null;

            view.Received(1).SetWearablePages(1, expectedTotalPages);
        }

        [UnityTest]
        public IEnumerator ShowListOfWearablesWhenLoadingWearables()
        {
            const int WEARABLE_TOTAL_AMOUNT = 3;

            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "common",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                    i18n = new[]
                    {
                        new i18n
                        {
                            text = "idk",
                            code = "wtf",
                        }
                    },
                    data = new WearableItem.Data
                    {
                        replaces = new[] { "category1", "category2", "category3" },
                        representations = new[]
                        {
                            new WearableItem.Representation
                            {
                                bodyShapes = new[] { "bodyShape1" },
                                overrideReplaces = new[] { "override1", "override2", "override3" },
                            }
                        }
                    }
                },
                new WearableItem
                {
                    id = "w2",
                    rarity = "rare",
                    description = "wo wah iii",
                    thumbnail = "w2thumbnail",
                    baseUrl = "http://localimages",
                    i18n = new[]
                    {
                        new i18n
                        {
                            text = "123",
                            code = "567",
                        }
                    },
                    data = new WearableItem.Data
                    {
                        representations = new[]
                        {
                            new WearableItem.Representation
                            {
                                bodyShapes = new[] { "bodyShape1" },
                                overrideReplaces = null,
                            }
                        }
                    }
                },
                new WearableItem
                {
                    id = "w3",
                    rarity = "unique",
                    description = "lalal lolo",
                    thumbnail = "w3thumbnail",
                    baseUrl = "http://localimages",
                    i18n = new[]
                    {
                        new i18n
                        {
                            text = "23p2",
                            code = "oi3j2",
                        }
                    },
                    data = new WearableItem.Data
                    {
                        representations = new[]
                        {
                            new WearableItem.Representation
                            {
                                bodyShapes = new[] { "bodyShape2" },
                                overrideReplaces = new[] { "override1", "override2", "override3" },
                            }
                        }
                    }
                }
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, WEARABLE_TOTAL_AMOUNT)));

            controller.LoadWearablesWithFilters();
            yield return null;

            view.Received(1).ClearWearables();

            view.Received(1)
                .ShowWearables(Arg.Is<IEnumerable<WearableGridItemModel>>(i =>
                     i.ElementAt(0).WearableId == "w1"
                     && i.ElementAt(1).WearableId == "w2"
                     && i.ElementAt(2).WearableId == "w3"
                     && i.Count() == 3));
        }

        [UnityTest]
        public IEnumerator ShowOnEquippedWearableWhenLoadingWearables()
        {
            const int WEARABLE_TOTAL_AMOUNT = 1;

            dataStore.previewEquippedWearables.Add("w1");

            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "common",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                    i18n = new[]
                    {
                        new i18n
                        {
                            text = "idk",
                            code = "wtf",
                        }
                    },
                    data = new WearableItem.Data
                    {
                        replaces = new[] { "category1", "category2", "category3" },
                        representations = new[]
                        {
                            new WearableItem.Representation
                            {
                                bodyShapes = new[] { "bodyShape1" },
                                overrideReplaces = new[] { "override1", "override2", "override3" },
                            }
                        }
                    }
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, WEARABLE_TOTAL_AMOUNT)));

            controller.LoadWearablesWithFilters();
            yield return null;

            view.Received(1).ClearWearables();

            view.Received(1)
                .ShowWearables(Arg.Is<IEnumerable<WearableGridItemModel>>(i =>
                     i.ElementAt(0).WearableId == "w1"
                     && i.ElementAt(0).IsEquipped
                     && i.ElementAt(0).IsSelected == false
                     && i.ElementAt(0).IsNew == false
                     && i.ElementAt(0).ImageUrl == "http://localimagesw1thumbnail"
                     && i.Count() == 1));
        }

        [UnityTest]
        public IEnumerator EquipWearable()
        {
            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "epic",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                    data = new WearableItem.Data()
                    {
                        category = "skin"
                    }
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 1)));

            controller.LoadWearablesWithFilters();
            yield return null;
            view.ClearReceivedCalls();

            controller.Equip("w1");

            view.Received(1)
                .SetWearable(Arg.Is<WearableGridItemModel>(w => w.WearableId == "w1"
                                                                && w.Rarity == NftRarity.Epic
                                                                && w.IsSelected == false
                                                                && w.IsEquipped == true
                                                                && w.ImageUrl == "http://localimagesw1thumbnail"));
            view.Received(1).RefreshAllWearables();
        }

        [UnityTest]
        public IEnumerator DontEquipWearableWhenNotBeingShown()
        {
            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "epic",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                    data = new WearableItem.Data()
                    {
                        category = "skin"
                    }
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 1)));

            controller.LoadWearablesWithFilters();
            yield return null;
            view.ClearReceivedCalls();

            controller.Equip("w2");

            view.DidNotReceiveWithAnyArgs().SetWearable(default(WearableGridItemModel));
        }

        [UnityTest]
        public IEnumerator UnEquipWearable()
        {
            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "epic",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                    data = new WearableItem.Data()
                    {
                        category = "skin"
                    }
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 1)));

            controller.LoadWearablesWithFilters();
            yield return null;
            view.ClearReceivedCalls();

            controller.UnEquip("w1");

            view.Received(1)
                .SetWearable(Arg.Is<WearableGridItemModel>(w => w.WearableId == "w1"
                                                                && w.Rarity == NftRarity.Epic
                                                                && w.IsSelected == false
                                                                && w.IsEquipped == false
                                                                && w.ImageUrl == "http://localimagesw1thumbnail"));
            view.Received(1).RefreshWearable("w1");
        }

        [UnityTest]
        public IEnumerator DontUnEquipWearableWhenNotBeingShown()
        {
            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "epic",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                    data = new WearableItem.Data()
                    {
                        category = "skin"
                    }
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 1)));

            controller.LoadWearablesWithFilters();
            yield return null;
            view.ClearReceivedCalls();

            controller.UnEquip("w2");

            view.DidNotReceiveWithAnyArgs().SetWearable(default(WearableGridItemModel));
        }

        [Test]
        public void ChangePageWhenViewRequestsIt()
        {
            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 3, 15,
                                        Arg.Any<CancellationToken>(),
                                        Arg.Any<string>(),
                                        Arg.Any<NftRarity>(),
                                        Arg.Any<NftCollectionType>(),
                                        Arg.Any<ICollection<string>>(),
                                        Arg.Any<string>(),
                                        Arg.Any<(NftOrderByOperation type, bool directionAscendent)?>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((Array.Empty<WearableItem>(), 50)));

            view.OnWearablePageChanged += Raise.Event<Action<int>>(3);

            wearablesCatalogService.Received(1)
                                   .RequestOwnedWearablesAsync(OWN_USER_ID, 3, 15,
                                        Arg.Any<CancellationToken>(),
                                        Arg.Any<string>(),
                                        Arg.Any<NftRarity>(),
                                        Arg.Any<NftCollectionType>(),
                                        Arg.Any<ICollection<string>>(),
                                        Arg.Any<string>(),
                                        Arg.Any<(NftOrderByOperation type, bool directionAscendent)?>());

            view.Received(1).SetWearablePages(3, 4);
        }

        [Test]
        public void SelectWearableAndFillInfoCard()
        {
            wearablesCatalogService.WearablesCatalog.Returns(new BaseDictionary<string, WearableItem>(new[]
            {
                ("w1", new WearableItem
                {
                    id = "w1",
                    baseUrl = "http://localimages/",
                    data = new WearableItem.Data
                    {
                        category = "upper_body",
                        hides = new[] { "lower_body" },
                        replaces = new[] { "eyes" },
                    },
                    rarity = "legendary",
                    thumbnail = "w1.png",
                    description = "awesome wearable",
                    i18n = new[]
                    {
                        new i18n
                        {
                            code = "en",
                            text = "w1name",
                        }
                    }
                }),
            }));

            dataStore.previewEquippedWearables.Add("w1");

            string wearableSelectedId = null;
            controller.OnWearableSelected += wearableId => wearableSelectedId = wearableId;

            view.OnWearableSelected += Raise.Event<Action<WearableGridItemModel>>(new WearableGridItemModel
            {
                WearableId = "w1",
                ImageUrl = "http://localimages/w1.png",
                IsEquipped = false,
                IsNew = false,
                IsSelected = false,
                Rarity = NftRarity.Legendary,
            });

            view.Received(1).ClearWearableSelection();
            view.Received(1).SelectWearable("w1");

            view.Received(1)
                .FillInfoCard(Arg.Is<InfoCardComponentModel>(i =>
                     i.category == "upper_body"
                     && i.rarity == "legendary"
                     && i.description == "awesome wearable"
                     && i.isEquipped == true
                     && i.name == "w1name"
                     && i.hiddenBy == null
                     && i.hideList[0] == "lower_body"
                     && i.removeList[0] == "eyes"));

            Assert.AreEqual("w1", wearableSelectedId);
        }

        [UnityTest]
        public IEnumerator FilterAllWearablesFromBreadcrumb()
        {
            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>(),
                                        collectionTypeMask: NftCollectionType.Base | NftCollectionType.OnChain)
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((Array.Empty<WearableItem>(), 50)));

            view.OnFilterSelected += Raise.Event<Action<string>>("all");
            yield return null;

            view.Received(1)
                .SetWearableBreadcrumb(Arg.Is<NftBreadcrumbModel>(n =>
                     n.ResultCount == 0
                     && n.Current == 0
                     && n.Path[0].Filter == "all"
                     && n.Path[0].Name == "All"));

            wearablesCatalogService.Received(1)
                                   .RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>(),
                                        collectionTypeMask: NftCollectionType.Base | NftCollectionType.OnChain,
                                        orderBy: Arg.Is<(NftOrderByOperation type, bool directionAscendent)>(arg =>
                                            arg.type == NftOrderByOperation.Date && arg.directionAscendent == false));
        }

        [UnityTest]
        public IEnumerator FilterWearablesByCategoryAndNameFromBreadcrumb()
        {
            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>(),
                                        category: "upper_body",
                                        name: "festival",
                                        collectionTypeMask: NftCollectionType.Base | NftCollectionType.OnChain)
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((Array.Empty<WearableItem>(), 50)));

            view.OnFilterSelected += Raise.Event<Action<string>>("all&category=upper_body&name=festival");
            yield return null;

            view.Received(1)
                .SetWearableBreadcrumb(Arg.Is<NftBreadcrumbModel>(n =>
                     n.ResultCount == 0
                     && n.Current == 2
                     && n.Path[0].Filter == "all"
                     && n.Path[0].Name == "All"
                     && n.Path[0].Removable == false
                     && n.Path[1].Filter == "all&category=upper_body"
                     && n.Path[1].Name == "upper_body"
                     && n.Path[1].Removable == true
                     && n.Path[2].Filter == "all&category=upper_body&name=festival"
                     && n.Path[2].Name == "festival"
                     && n.Path[2].Removable == true));

            wearablesCatalogService.Received(1)
                                   .RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>(),
                                        collectionTypeMask: NftCollectionType.Base | NftCollectionType.OnChain,
                                        category: "upper_body",
                                        name: "festival",
                                        orderBy: Arg.Is<(NftOrderByOperation type, bool directionAscendent)>(arg =>
                                            arg.type == NftOrderByOperation.Date && arg.directionAscendent == false));
        }

        [UnityTest]
        public IEnumerator RemoveNameFilterFromBreadcrumb()
        {
            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>(),
                                        category: "upper_body",
                                        collectionTypeMask: NftCollectionType.Base | NftCollectionType.OnChain)
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((Array.Empty<WearableItem>(), 50)));

            view.OnFilterSelected += Raise.Event<Action<string>>("all&category=upper_body&name=festival");
            yield return null;

            view.ClearReceivedCalls();
            wearablesCatalogService.ClearReceivedCalls();
            filtersView.ClearReceivedCalls();

            view.OnFilterRemoved += Raise.Event<Action<string>>("all&category=upper_body&name=festival");
            yield return null;

            view.Received(1)
                .SetWearableBreadcrumb(Arg.Is<NftBreadcrumbModel>(n =>
                     n.ResultCount == 0
                     && n.Current == 1
                     && n.Path[0].Filter == "all"
                     && n.Path[0].Name == "All"
                     && n.Path[0].Removable == false
                     && n.Path[1].Filter == "all&category=upper_body"
                     && n.Path[1].Name == "upper_body"
                     && n.Path[1].Removable == true
                     && n.Path.Length == 2));

            filtersView.Received(1).SetSearchText(null, false);

            wearablesCatalogService.Received(1)
                                   .RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>(),
                                        collectionTypeMask: NftCollectionType.Base | NftCollectionType.OnChain,
                                        category: "upper_body",
                                        orderBy: Arg.Is<(NftOrderByOperation type, bool directionAscendent)>(arg =>
                                            arg.type == NftOrderByOperation.Date && arg.directionAscendent == false));
        }

        [UnityTest]
        public IEnumerator RemoveCategoryFilterFromBreadcrumb()
        {
            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>(),
                                        name: "festival",
                                        collectionTypeMask: NftCollectionType.Base | NftCollectionType.OnChain)
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((Array.Empty<WearableItem>(), 50)));

            view.OnFilterSelected += Raise.Event<Action<string>>("all&name=festival&category=upper_body");
            yield return null;

            view.ClearReceivedCalls();
            wearablesCatalogService.ClearReceivedCalls();

            view.OnFilterRemoved += Raise.Event<Action<string>>("all&name=festival&category=upper_body");
            yield return null;

            view.Received()
                .SetWearableBreadcrumb(Arg.Is<NftBreadcrumbModel>(n =>
                     n.ResultCount == 0
                     && n.Current == 1
                     && n.Path[0].Filter == "all"
                     && n.Path[0].Name == "All"
                     && n.Path[0].Removable == false
                     && n.Path[1].Filter == "all&name=festival"
                     && n.Path[1].Name == "festival"
                     && n.Path[1].Removable == true
                     && n.Path.Length == 2));

            wearablesCatalogService.Received(1)
                                   .RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>(),
                                        collectionTypeMask: NftCollectionType.Base | NftCollectionType.OnChain,
                                        name: "festival",
                                        orderBy: Arg.Is<(NftOrderByOperation type, bool directionAscendent)>(arg =>
                                            arg.type == NftOrderByOperation.Date && arg.directionAscendent == false));
        }

        [UnityTest]
        public IEnumerator LoadWearablesWithFilters()
        {
            const int WEARABLE_TOTAL_AMOUNT = 18;
            const string CATEGORY = "shoes";
            const string ON_CHAIN_COLLECTION = "urn:decentraland:on:chain";
            const string BASE_OFF_CHAIN_COLLECTION = "urn:decentraland:off-chain:base-avatars:male";
            const string THIRD_PARTY_COLLECTION = "urn:collections-thirdparty:woah";
            const string NAME = "awesome";

            IReadOnlyList<WearableItem> wearableList = Array.Empty<WearableItem>();

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, WEARABLE_TOTAL_AMOUNT)));

            controller.LoadWearablesWithFilters(CATEGORY,
                NftCollectionType.Base | NftCollectionType.OnChain | NftCollectionType.ThirdParty,
                new[] { ON_CHAIN_COLLECTION, BASE_OFF_CHAIN_COLLECTION, THIRD_PARTY_COLLECTION },
                NAME,
                (NftOrderByOperation.Name, false));

            yield return null;

            wearablesCatalogService.Received(1)
                                   .RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>(),
                                        CATEGORY,
                                        NftRarity.None,
                                        NftCollectionType.Base | NftCollectionType.OnChain | NftCollectionType.ThirdParty,
                                        Arg.Is<ICollection<string>>(i =>
                                            i.ElementAt(0) == ON_CHAIN_COLLECTION
                                            && i.ElementAt(1) == BASE_OFF_CHAIN_COLLECTION
                                            && i.ElementAt(2) == THIRD_PARTY_COLLECTION),
                                        NAME,
                                        Arg.Is<(NftOrderByOperation type, bool directionAscendent)>(orderBy =>
                                            orderBy.type == NftOrderByOperation.Name && orderBy.directionAscendent == false));
        }

        [Test]
        public void OpenWearableMarketplaceWhenRegisteredUser()
        {
            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = "ownUserName",
                hasConnectedWeb3 = true,
            });

            view.OnGoToMarketplace += Raise.Event<Action>();

            browserBridge.Received(1).OpenUrl("https://market.decentraland.org/browse?section=wearables");
        }

        [Test]
        public void OpenConnectWalletWhenGuestUser()
        {
            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = "ownUserName",
                hasConnectedWeb3 = false,
            });

            view.OnGoToMarketplace += Raise.Event<Action>();

            browserBridge.Received(1).OpenUrl("https://docs.decentraland.org/get-a-wallet");
        }

        [UnityTest]
        public IEnumerator ShowWearableAsNewWhenHasBeenTransferredBetweenOneDay()
        {
            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "common",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                    i18n = new[]
                    {
                        new i18n
                        {
                            text = "idk",
                            code = "wtf",
                        }
                    },
                    data = new WearableItem.Data
                    {
                        replaces = new[] { "category1", "category2", "category3" },
                        representations = new[]
                        {
                            new WearableItem.Representation
                            {
                                bodyShapes = new[] { "bodyShape1" },
                                overrideReplaces = new[] { "override1", "override2", "override3" },
                            }
                        }
                    },
                    MostRecentTransferredDate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(20)),
                }
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 1)));

            controller.LoadWearablesWithFilters();
            yield return null;

            view.Received(1)
                .ShowWearables(Arg.Is<IEnumerable<WearableGridItemModel>>(i =>
                     i.ElementAt(0).IsNew == true));
        }

        [UnityTest]
        public IEnumerator UpdateBodyShapeCompatibility()
        {
            const string BODY_SHAPE = "bodyShape1";

            var w1 = new WearableItem
            {
                id = "w1",
                rarity = "common",
                description = "super wearable",
                thumbnail = "w1thumbnail",
                baseUrl = "http://localimages",
                i18n = new[]
                {
                    new i18n
                    {
                        text = "idk",
                        code = "wtf",
                    }
                },
                data = new WearableItem.Data
                {
                    replaces = new[] { "category1", "category2", "category3" },
                    representations = new[]
                    {
                        new WearableItem.Representation
                        {
                            bodyShapes = new[] { BODY_SHAPE },
                            overrideReplaces = new[] { "override1", "override2", "override3" },
                        }
                    }
                },
                MostRecentTransferredDate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(20)),
            };

            var w2 = new WearableItem
            {
                id = "w2",
                rarity = "common",
                description = "super wearable",
                thumbnail = "w1thumbnail",
                baseUrl = "http://localimages",
                i18n = new[]
                {
                    new i18n
                    {
                        text = "idk",
                        code = "wtf",
                    }
                },
                data = new WearableItem.Data
                {
                    replaces = Array.Empty<string>(),
                    representations = new[]
                    {
                        new WearableItem.Representation
                        {
                            bodyShapes = new[] { "bodyShape2" },
                            overrideReplaces = Array.Empty<string>(),
                        }
                    }
                },
                MostRecentTransferredDate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(20)),
            };

            var wearableCatalog = new BaseDictionary<string, WearableItem>
            {
                { "w1", w1 },
                { "w2", w2 },
            };

            wearablesCatalogService.WearablesCatalog.Returns(wearableCatalog);

            IReadOnlyList<WearableItem> wearableList = new[]
            {
                w1,
                w2,
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 2)));

            controller.LoadWearablesWithFilters();
            yield return null;
            view.ClearReceivedCalls();

            controller.UpdateBodyShapeCompatibility(BODY_SHAPE);

            view.Received(1)
                .SetWearable(Arg.Is<WearableGridItemModel>(w =>
                     w.IsCompatibleWithBodyShape == true
                     && w.WearableId == "w1"));

            view.Received(1)
                .SetWearable(Arg.Is<WearableGridItemModel>(w =>
                     w.IsCompatibleWithBodyShape == false
                     && w.WearableId == "w2"));
        }

        [UnityTest]
        public IEnumerator MarkBodyShapeAsEquipped()
        {
            const string BODY_SHAPE = "bodyShape1";

            var w1 = new WearableItem
            {
                id = BODY_SHAPE,
                rarity = "common",
                description = "super wearable",
                thumbnail = "w1thumbnail",
                baseUrl = "http://localimages",
                MostRecentTransferredDate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(20)),
                data = new WearableItem.Data
                {
                    category = WearableLiterals.Categories.BODY_SHAPE,
                },
            };

            var w2 = new WearableItem
            {
                id = "w2",
                rarity = "common",
                description = "super wearable",
                thumbnail = "w1thumbnail",
                baseUrl = "http://localimages",
                MostRecentTransferredDate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(20)),
                data = new WearableItem.Data
                {
                    category = WearableLiterals.Categories.BODY_SHAPE,
                },
            };

            dataStore.previewBodyShape.Set(BODY_SHAPE);

            IReadOnlyList<WearableItem> wearableList = new[] { w1, w2, };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 2)));

            controller.LoadWearablesWithFilters();
            yield return null;

            view.Received(1)
                .ShowWearables(Arg.Is<IEnumerable<WearableGridItemModel>>(i =>
                     i.ElementAt(0).WearableId == BODY_SHAPE
                     && i.ElementAt(0).IsEquipped == true
                     && i.ElementAt(1).WearableId == "w2"
                     && i.ElementAt(1).IsEquipped == false));
        }

        [UnityTest]
        [TestCase(WearableLiterals.Categories.BODY_SHAPE, false, ExpectedResult = null)]
        [TestCase(WearableLiterals.Categories.EYES, false, ExpectedResult = null)]
        [TestCase(WearableLiterals.Categories.MOUTH, false, ExpectedResult = null)]
        [TestCase(WearableLiterals.Categories.HAIR, true, ExpectedResult = null)]
        [TestCase(WearableLiterals.Categories.LOWER_BODY, true, ExpectedResult = null)]
        [TestCase(WearableLiterals.Categories.FACIAL_HAIR, true, ExpectedResult = null)]
        public IEnumerator CanWearableBeUnequipped(string category, bool canBeUnequipped)
        {
            var w1 = new WearableItem
            {
                id = "w",
                rarity = "common",
                description = "super wearable",
                thumbnail = "w1thumbnail",
                baseUrl = "http://localimages",
                MostRecentTransferredDate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(20)),
                data = new WearableItem.Data
                {
                    category = category,
                },
            };

            IReadOnlyList<WearableItem> wearableList = new[] { w1, };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 1)));

            controller.LoadWearablesWithFilters();
            yield return null;

            view.Received(1)
                .ShowWearables(Arg.Is<IEnumerable<WearableGridItemModel>>(i =>
                     i.ElementAt(0).WearableId == "w"
                     && i.ElementAt(0).UnEquipAllowed == canBeUnequipped));
        }

        [Test]
        public void ClearSearchTextWhenRemoveTextFilteringFromBreadcrumb()
        {
            IReadOnlyList<WearableItem> wearableList = Array.Empty<WearableItem>();

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 0)));

            view.OnFilterRemoved += Raise.Event<Action<string>>("name=bleh");

            filtersView.Received(1).SetSearchText(null, false);
        }

        [UnityTest]
        public IEnumerator ClearCategorySlotFilterWhenRemoveCategoryFilterFromBreadcrumb()
        {
            slotsView.OnToggleAvatarSlot += Raise.Event<IAvatarSlotsView.ToggleAvatarSlotDelegate>(
                "upper_body", false, PreviewCameraFocus.DefaultEditing, true);

            IReadOnlyList<WearableItem> wearableList = Array.Empty<WearableItem>();

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 0)));

            view.OnFilterRemoved += Raise.Event<Action<string>>("category=upper_body");

            yield return null;

            slotsView.Received(1).DisablePreviousSlot("upper_body");
        }

        [UnityTest]
        public IEnumerator ClearTextFilterWhenCategorySlotIsSelected()
        {
            filtersView.OnSearchTextChanged += Raise.Event<Action<string>>("festival");
            filtersView.ClearReceivedCalls();

            slotsView.OnToggleAvatarSlot += Raise.Event<IAvatarSlotsView.ToggleAvatarSlotDelegate>("upper_body", false, PreviewCameraFocus.DefaultEditing, true);

            yield return null;

            filtersView.Received(1).SetSearchText(null, false);
        }

        [Test]
        public void UpdateOrderByFilterViewWhenLoadWearables()
        {
            controller.LoadWearables();

            filtersView.Received(1).SetSorting(NftOrderByOperation.Date, false, false);
        }

        [UnityTest]
        public IEnumerator UpdateOrderByFilterViewWhenChanges()
        {
            filtersView.OnSortByChanged += Raise.Event<Action<(NftOrderByOperation type, bool directionAscendent)>>(
                (NftOrderByOperation.Name, true));

            yield return null;

            filtersView.Received(1).SetSorting(NftOrderByOperation.Name, true, false);
        }

        [Test]
        public void UpdateCollectionsWhenLoadWearables()
        {
            controller.LoadWearables();

            filtersView.Received(1).SelectDropdownCollections(Arg.Is<HashSet<string>>(h => h.Contains("decentraland")),
                false);
        }
    }
}
