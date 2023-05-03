using DCLServices.WearablesCatalogService;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DCL.Backpack
{
    public class BackpackFiltersComponentViewTests
    {
        private BackpackFiltersComponentView backpackFiltersComponentView;

        [SetUp]
        public void SetUp()
        {
            BackpackFiltersComponentView prefab = AssetDatabase.LoadAssetAtPath<BackpackFiltersComponentView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BackpackEditorHUDV2/Prefabs/BackpackFilters.prefab");

            backpackFiltersComponentView = Object.Instantiate(prefab);
        }

        [TearDown]
        public void TearDown()
        {
            backpackFiltersComponentView.Dispose();
            Object.Destroy(backpackFiltersComponentView.gameObject);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void LoadCollectionDropdownCorrectly(bool includeDefaultCollection)
        {
            // Arrange
            WearableCollectionsAPIData.Collection testDefaultCollection = null;
            if (includeDefaultCollection)
                testDefaultCollection = new () { urn = "testDefaultCollectionId", name = "testDefaultCollection" };

            WearableCollectionsAPIData.Collection[] testCollections =
            {
                new () { urn = "testCollectionId1", name = "testCollection1" },
                new () { urn = "testCollectionId2", name = "testCollection2" },
                new () { urn = "testCollectionId3", name = "testCollection3" },
            };

            // Act
            backpackFiltersComponentView.LoadCollectionDropdown(testCollections, testDefaultCollection);

            // Assert
            if (includeDefaultCollection)
                Assert.IsTrue(backpackFiltersComponentView.loadedFilters.Select(x => x.id).Contains(testDefaultCollection.urn));

            foreach (var collection in testCollections)
                Assert.IsTrue(backpackFiltersComponentView.loadedFilters.Select(x => x.id).Contains(collection.urn));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ChangeOnlyCollectiblesToggleCorrectly(bool isOn)
        {
            // Arrange
            var resultIsOn = false;
            backpackFiltersComponentView.OnOnlyCollectiblesChanged += newIsOn => resultIsOn = newIsOn;

            // Act
            backpackFiltersComponentView.onlyCollectiblesToggle.isOn = isOn;

            // Assert
            Assert.AreEqual(resultIsOn, isOn);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ChangeCollectionDropdownCorrectly(int optionIndex)
        {
            // Arrange
            LoadCollectionDropdownCorrectly(false);
            HashSet<string> resultSelectedCollections = null;
            backpackFiltersComponentView.OnCollectionChanged += selectedCollections => resultSelectedCollections = selectedCollections;

            // Act
            backpackFiltersComponentView.collectionDropdown.GetOption(optionIndex).isOn = false;
            backpackFiltersComponentView.collectionDropdown.GetOption(optionIndex).isOn = true;

            // Assert
            Assert.IsTrue(resultSelectedCollections.Contains(backpackFiltersComponentView.collectionDropdown.GetOption(optionIndex).id));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void ChangeSortByDropdownCorrectly(int optionIndex)
        {
            // Arrange
            (NftOrderByOperation type, bool directionAscendent) resultSorting = (NftOrderByOperation.Date, true);
            backpackFiltersComponentView.OnSortByChanged += newSorting => resultSorting = newSorting;

            // Act
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = false;
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = true;

            // Assert
            switch (optionIndex)
            {
                case 0:
                    // Newest
                    Assert.AreEqual(NftOrderByOperation.Date, resultSorting.type);
                    Assert.AreEqual(false, resultSorting.directionAscendent);
                    break;
                case 1:
                    // Oldest
                    Assert.AreEqual(NftOrderByOperation.Date, resultSorting.type);
                    Assert.AreEqual(true, resultSorting.directionAscendent);
                    break;
                case 2:
                    // Rarest
                    Assert.AreEqual(NftOrderByOperation.Rarity, resultSorting.type);
                    Assert.AreEqual(false, resultSorting.directionAscendent);
                    break;
                case 3:
                    // Less rare
                    Assert.AreEqual(NftOrderByOperation.Rarity, resultSorting.type);
                    Assert.AreEqual(true, resultSorting.directionAscendent);
                    break;
                case 4:
                    // Name A-Z
                    Assert.AreEqual(NftOrderByOperation.Name, resultSorting.type);
                    Assert.AreEqual(true, resultSorting.directionAscendent);
                    break;
                case 5:
                    // Name Z-A
                    Assert.AreEqual(NftOrderByOperation.Name, resultSorting.type);
                    Assert.AreEqual(false, resultSorting.directionAscendent);
                    break;
            }
        }

        [Test]
        public void ChangeSearchBarTextCorrectly()
        {
            // Arrange
            var testText = "test text";
            var resultText = "";
            backpackFiltersComponentView.OnSearchTextChanged += newText => resultText = newText;

            // Act
            backpackFiltersComponentView.searchBar.SubmitSearch(testText);

            // Assert
            Assert.AreEqual(resultText, testText);
        }
    }
}
