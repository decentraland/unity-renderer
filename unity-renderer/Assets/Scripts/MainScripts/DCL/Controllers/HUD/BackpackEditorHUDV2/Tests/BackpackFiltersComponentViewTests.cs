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
        public void ChangeSortByNewestCorrectly()
        {
            // Arrange
            (NftOrderByOperation type, bool directionAscendent) resultSorting = (NftOrderByOperation.Name, true);
            backpackFiltersComponentView.OnSortByChanged += newSorting => resultSorting = newSorting;
            var optionIndex = 0;

            // Act
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = false;
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = true;

            // Assert
            Assert.AreEqual(NftOrderByOperation.Date, resultSorting.type);
            Assert.AreEqual(false, resultSorting.directionAscendent);
        }

        [Test]
        public void ChangeSortByOldestCorrectly()
        {
            // Arrange
            (NftOrderByOperation type, bool directionAscendent) resultSorting = (NftOrderByOperation.Name, true);
            backpackFiltersComponentView.OnSortByChanged += newSorting => resultSorting = newSorting;
            var optionIndex = 1;

            // Act
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = false;
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = true;

            // Assert
            Assert.AreEqual(NftOrderByOperation.Date, resultSorting.type);
            Assert.AreEqual(true, resultSorting.directionAscendent);
        }

        [Test]
        public void ChangeSortByRarestCorrectly()
        {
            // Arrange
            (NftOrderByOperation type, bool directionAscendent) resultSorting = (NftOrderByOperation.Name, true);
            backpackFiltersComponentView.OnSortByChanged += newSorting => resultSorting = newSorting;
            var optionIndex = 2;

            // Act
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = false;
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = true;

            // Assert
            Assert.AreEqual(NftOrderByOperation.Rarity, resultSorting.type);
            Assert.AreEqual(false, resultSorting.directionAscendent);
        }

        [Test]
        public void ChangeSortByLessRareCorrectly()
        {
            // Arrange
            (NftOrderByOperation type, bool directionAscendent) resultSorting = (NftOrderByOperation.Name, true);
            backpackFiltersComponentView.OnSortByChanged += newSorting => resultSorting = newSorting;
            var optionIndex = 3;

            // Act
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = false;
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = true;

            // Assert
            Assert.AreEqual(NftOrderByOperation.Rarity, resultSorting.type);
            Assert.AreEqual(true, resultSorting.directionAscendent);
        }

        [Test]
        public void ChangeSortByNameA_ZCorrectly()
        {
            // Arrange
            (NftOrderByOperation type, bool directionAscendent) resultSorting = (NftOrderByOperation.Name, true);
            backpackFiltersComponentView.OnSortByChanged += newSorting => resultSorting = newSorting;
            var optionIndex = 4;

            // Act
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = false;
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = true;

            // Assert
            Assert.AreEqual(NftOrderByOperation.Name, resultSorting.type);
            Assert.AreEqual(true, resultSorting.directionAscendent);
        }

        [Test]
        public void ChangeSortByNameZ_ACorrectly()
        {
            // Arrange
            (NftOrderByOperation type, bool directionAscendent) resultSorting = (NftOrderByOperation.Name, true);
            backpackFiltersComponentView.OnSortByChanged += newSorting => resultSorting = newSorting;
            var optionIndex = 5;

            // Act
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = false;
            backpackFiltersComponentView.sortByDropdown.GetOption(optionIndex).isOn = true;

            // Assert
            Assert.AreEqual(NftOrderByOperation.Name, resultSorting.type);
            Assert.AreEqual(false, resultSorting.directionAscendent);
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
