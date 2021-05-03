using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests
{
    public class SearchBarViewShould
    {
        private SearchBarView view;

        [SetUp]
        public void SetUp()
        {
            const string prefabAssetPath =
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/SearchBar/SearchBarView.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<SearchBarView>(prefabAssetPath);
            view = Object.Instantiate(prefab);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(view.gameObject);
        }

        [UnityTest]
        public IEnumerator TriggerSearchWhenUserStopTyping()
        {
            const string searchValue = "Something";
            const float idleTimeToTriggerSearch = 1;

            bool searchTriggered = false;
            string searchValueReceived = "";
            float searchTriggerTime = 0;

            var handler = Substitute.For<ISectionSearchHandler>();
            handler.WhenForAnyArgs(a => a.SetSearchString(""))
                .Do(info =>
                {
                    searchTriggered = true;
                    searchValueReceived = info.Arg<string>();
                    searchTriggerTime = Time.unscaledTime;
                });

            view.SetSearchBar(handler, null);

            float searchTime = Time.unscaledTime;

            view.inputField.inputField.text = searchValue;
            yield return new WaitForSeconds(idleTimeToTriggerSearch);

            Assert.IsTrue(searchTriggered);
            Assert.AreEqual(searchValue, searchValueReceived);
            Assert.GreaterOrEqual(searchTriggerTime, searchTime + idleTimeToTriggerSearch);
        }

        [Test]
        public void ShowSpinnerAndClearSearchButton()
        {
            Assert.IsFalse(view.inputField.searchSpinner.activeSelf);
            Assert.IsFalse(view.inputField.clearSearchButton.gameObject.activeSelf);

            view.inputField.inputField.text = "searchValue";

            Assert.IsTrue(view.inputField.searchSpinner.activeSelf);
            Assert.IsFalse(view.inputField.clearSearchButton.gameObject.activeSelf);

            view.inputField.inputField.onSubmit.Invoke("searchValue");

            Assert.IsFalse(view.inputField.searchSpinner.activeSelf);
            Assert.IsTrue(view.inputField.clearSearchButton.gameObject.activeSelf);

            view.inputField.clearSearchButton.onClick.Invoke();

            Assert.IsFalse(view.inputField.searchSpinner.activeSelf);
            Assert.IsFalse(view.inputField.clearSearchButton.gameObject.activeSelf);
            Assert.AreEqual(string.Empty, view.inputField.inputField.text);
        }

        [Test]
        public void ShowFilters()
        {
            view.ShowFilters(true,false,false);

            Assert.IsTrue(view.ownerToggle.gameObject.activeSelf);
            Assert.IsFalse(view.operatorToggle.gameObject.activeSelf);
            Assert.IsFalse(view.contributorToggle.gameObject.activeSelf);

            view.ShowFilters(false,false,false);

            Assert.IsFalse(view.ownerToggle.gameObject.activeSelf);
            Assert.IsFalse(view.operatorToggle.gameObject.activeSelf);
            Assert.IsFalse(view.contributorToggle.gameObject.activeSelf);
        }

        [Test]
        public void TriggerFiltersCallback()
        {
            bool ownerON = false;
            bool operatorON = false;
            bool contributorON = false;

            var handler = Substitute.For<ISectionSearchHandler>();
            handler.WhenForAnyArgs(a => a.SetFilter(false, false, false))
                .Do(info =>
                {
                    ownerON = info.ArgAt<bool>(0);
                    operatorON = info.ArgAt<bool>(1);
                    contributorON = info.ArgAt<bool>(2);
                });

            view.SetSearchBar(handler, null);

            view.ownerToggle.isOn = true;
            Assert.IsTrue(ownerON);
            Assert.IsFalse(operatorON);
            Assert.IsFalse(contributorON);

            view.operatorToggle.isOn = true;
            Assert.IsFalse(ownerON);
            Assert.IsTrue(operatorON);
            Assert.IsFalse(contributorON);

            view.contributorToggle.isOn = true;
            Assert.IsFalse(ownerON);
            Assert.IsFalse(operatorON);
            Assert.IsTrue(contributorON);

            view.ownerToggle.isOn = false;
            view.operatorToggle.isOn = false;
            view.contributorToggle.isOn = false;
            Assert.IsFalse(ownerON);
            Assert.IsFalse(operatorON);
            Assert.IsFalse(contributorON);
        }

        [Test]
        public void ShowSortDropdown()
        {
            string[] sortTypes = new[] {"Type1", "Type2", "Type3"};
            view.SetSortTypes(sortTypes);

            Assert.IsTrue(view.sortDropdown.activeButtons.TrueForAll(button => sortTypes.Contains(button.label.text)));
            Assert.IsFalse(view.sortDropdown.gameObject.activeSelf);

            view.sortButton.onClick.Invoke();
            Assert.IsTrue(view.sortDropdown.gameObject.activeSelf);
        }

        [Test]
        public void NotShowSortDropdownWhenLessThanTwoSortTypes()
        {
            string[] sortTypes = new[] {"Type1"};
            view.SetSortTypes(sortTypes);
            Assert.IsFalse(view.sortDropdown.gameObject.activeSelf);

            view.sortButton.onClick.Invoke();
            Assert.IsFalse(view.sortDropdown.gameObject.activeSelf);
        }

        [Test]
        public void TriggerSortTypeCallback()
        {
            string selectedSort = "";

            var handler = Substitute.For<ISectionSearchHandler>();
            handler.WhenForAnyArgs(a => a.SetSortType(""))
                .Do(info =>
                {
                    selectedSort = info.Arg<string>();
                });

            view.SetSearchBar(handler, null);

            string[] sortTypes = new[] {"Type1"};
            view.SetSortTypes(sortTypes);
            ((IPointerDownHandler)view.sortDropdown.activeButtons[0]).OnPointerDown(null);

            Assert.AreEqual(sortTypes[0], selectedSort);
            Assert.AreEqual(sortTypes[0], view.sortTypeLabel.text);
        }
    }
}