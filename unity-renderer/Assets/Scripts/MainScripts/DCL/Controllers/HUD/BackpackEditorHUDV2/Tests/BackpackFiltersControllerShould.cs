using Cysharp.Threading.Tasks;
using DCLServices.WearablesCatalogService;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Backpack
{
    public class BackpackFiltersControllerShould
    {
        private BackpackFiltersController backpackFiltersController;
        private IBackpackFiltersComponentView backpackFiltersComponentView;
        private IWearablesCatalogService wearablesCatalogService;

        [SetUp]
        public void SetUp()
        {
            backpackFiltersComponentView = Substitute.For<IBackpackFiltersComponentView>();
            wearablesCatalogService = Substitute.For<IWearablesCatalogService>();
            backpackFiltersController = new BackpackFiltersController(backpackFiltersComponentView, wearablesCatalogService);
        }

        [TearDown]
        public void TearDown()
        {
            backpackFiltersController.Dispose();
        }

        [Test]
        public void LoadCollectionsCorrectly()
        {
            // Arrange
            WearableCollectionsAPIData.Collection[] testCollections = {
                new() { urn = "testUrn1", name = "testName1" },
                new() { urn = "testUrn2", name = "testName2" },
                new() { urn = "testUrn3", name = "testName3" },
            };

            wearablesCatalogService.GetThirdPartyCollectionsAsync(Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult(testCollections));

            // Act
            backpackFiltersController.LoadCollections();

            // Assert
            backpackFiltersComponentView.Received(1).LoadCollectionDropdown(
                testCollections,
                Arg.Is<WearableCollectionsAPIData.Collection>(x => x.urn == "decentraland" && x.name == "Decentraland"));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TriggerOnlyCollectiblesFilterFromViewCorrectly(bool onlyOnChainWearables)
        {
            // Arrange
            NftCollectionType testCollectionType = NftCollectionType.None;
            backpackFiltersController.OnCollectionTypeChanged += (collectionType) => testCollectionType = collectionType;

            // Act
            backpackFiltersComponentView.OnOnlyCollectiblesChanged += Raise.Event<Action<bool>>(onlyOnChainWearables);

            // Assert
            bool containsBaseWearables = (testCollectionType & NftCollectionType.Base) != 0;
            Assert.AreNotEqual(onlyOnChainWearables, containsBaseWearables);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TriggerCollectionsFilterFromViewCorrectly(bool containsDecentralandCollection)
        {
            // Arrange
            NftCollectionType testCollectionType = NftCollectionType.None;
            backpackFiltersController.OnCollectionTypeChanged += collectionType => testCollectionType = collectionType;

            HashSet<string> testThirdPartyColections = null;
            backpackFiltersController.OnThirdPartyCollectionChanged += thirdPartyCollections => testThirdPartyColections = thirdPartyCollections;

            // Act
            backpackFiltersComponentView.OnCollectionChanged += Raise.Event<Action<HashSet<string>>>(
                new HashSet<string> { containsDecentralandCollection ? "decentraland" : "testCollection1", "testCollection2", "testCollection3" });

            // Assert
            Assert.AreEqual(containsDecentralandCollection, (testCollectionType & NftCollectionType.OnChain) != 0);
            Assert.IsTrue((testCollectionType & NftCollectionType.ThirdParty) == NftCollectionType.ThirdParty);
            Assert.IsNotNull(testThirdPartyColections);
            Assert.AreEqual(!containsDecentralandCollection, testThirdPartyColections.Contains("testCollection1"));
            Assert.AreEqual(true, testThirdPartyColections.Contains("testCollection2"));
            Assert.AreEqual(true, testThirdPartyColections.Contains("testCollection3"));
        }

        [Test]
        [TestCase(NftOrderByOperation.Rarity, true)]
        [TestCase(NftOrderByOperation.Rarity, false)]
        [TestCase(NftOrderByOperation.Date, true)]
        [TestCase(NftOrderByOperation.Date, false)]
        [TestCase(NftOrderByOperation.Name, true)]
        [TestCase(NftOrderByOperation.Name, false)]
        public void TriggerSortByFilterFromViewCorrectly(NftOrderByOperation type, bool directionAscendent)
        {
            // Arrange
            (NftOrderByOperation type, bool directionAscendent) testSorting = (NftOrderByOperation.Rarity, true);
            backpackFiltersController.OnSortByChanged += newSorting => { testSorting = newSorting; };

            // Act
            backpackFiltersComponentView.OnSortByChanged += Raise.Event<Action<(NftOrderByOperation type, bool directionAscendent)>>((type, directionAscendent));

            // Assert
            Assert.AreEqual(type, testSorting.type);
            Assert.AreEqual(directionAscendent, testSorting.directionAscendent);
        }

        [Test]
        public void TriggerSearchFilterFromViewCorrectly()
        {
            // Arrange
            var testSearch = "";
            backpackFiltersController.OnSearchTextChanged += newText => { testSearch = newText; };

            // Act
            backpackFiltersComponentView.OnSearchTextChanged += Raise.Event<Action<string>>("test text");

            // Assert
            Assert.AreEqual("test text", testSearch);
        }
    }
}
