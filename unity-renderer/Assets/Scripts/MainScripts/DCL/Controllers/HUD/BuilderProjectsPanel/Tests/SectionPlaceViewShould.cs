using System.Collections.Generic;
using System.Linq;
using DCL.Builder;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class SectionPlaceViewShould
    {
        private SectionPlacesView view;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<SectionPlacesView>(SectionPlacesController.VIEW_PREFAB_PATH);
            view = Object.Instantiate(prefab);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(view.gameObject); }

        [Test]
        public void HaveScenesContainerEmptyAtInstantiation() { Assert.AreEqual(0, view.scenesCardContainer.childCount); }

        [Test]
        public void ShowCardsInCorrectSortOrder()
        {
            const string prefabAssetPath =
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/SceneCardView.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<PlaceCardView>(prefabAssetPath);

            Dictionary<string, IPlaceCardView> cardViews = new Dictionary<string, IPlaceCardView>();
            const int cardsCount = 10;
            for (int i = 0; i < cardsCount; i++)
            {
                var card = (IPlaceCardView)Object.Instantiate(prefab);
                card.Setup(new PlaceData() { size = new Vector2Int(i, i), id = i.ToString() });
                cardViews.Add(i.ToString(), card);
            }


            SectionPlacesController controller = new SectionPlacesController(view);
            controller.searchHandler.SetSortType(SectionSearchHandler.SIZE_SORT_TYPE_ASC);

            ((IPlaceListener)controller).SetPlaces(cardViews);

            Assert.AreEqual(cardsCount, view.scenesCardContainer.childCount);

            var prev = (IPlaceCardView)view.scenesCardContainer.GetChild(0).GetComponent<PlaceCardView>();
            for (int i = 1; i < cardsCount; i++)
            {
                var current = (IPlaceCardView)view.scenesCardContainer.GetChild(i).GetComponent<PlaceCardView>();
                Assert.GreaterOrEqual(current.PlaceData.size.x * current.PlaceData.size.y, prev.PlaceData.size.x * prev.PlaceData.size.y);
                prev = current;
            }

            foreach (var card in cardViews.Values)
            {
                card.Dispose();
            }
            controller.Dispose();
        }

        [Test]
        public void ShowEmptyCorrectly()
        {
            view.SetEmpty();
            Assert.IsFalse(view.contentContainer.activeSelf);
            Assert.IsFalse(view.loadingAnimationContainer.activeSelf);
            Assert.IsFalse(view.noSearchResultContainer.activeSelf);
            Assert.IsTrue(view.emptyContainer.activeSelf);
        }

        [Test]
        public void ShowFilledCorrectly()
        {
            view.SetFilled();
            Assert.IsTrue(view.contentContainer.activeSelf);
            Assert.IsFalse(view.loadingAnimationContainer.activeSelf);
            Assert.IsFalse(view.noSearchResultContainer.activeSelf);
            Assert.IsFalse(view.emptyContainer.activeSelf);
        }

        [Test]
        public void ShowNoResultCorrectly()
        {
            view.SetNoSearchResult();
            Assert.IsFalse(view.contentContainer.activeSelf);
            Assert.IsFalse(view.loadingAnimationContainer.activeSelf);
            Assert.IsTrue(view.noSearchResultContainer.activeSelf);
            Assert.IsFalse(view.emptyContainer.activeSelf);
        }

        [Test]
        public void ShowLoadingCorrectly()
        {
            view.SetLoading();
            Assert.IsFalse(view.contentContainer.activeSelf);
            Assert.IsTrue(view.loadingAnimationContainer.activeSelf);
            Assert.IsFalse(view.noSearchResultContainer.activeSelf);
            Assert.IsFalse(view.emptyContainer.activeSelf);
        }

        [Test]
        public void SetSectionStateCorrectly()
        {
            SectionPlacesController controller = new SectionPlacesController(view);
            IPlaceListener listener = controller;

            controller.SetFetchingDataState(true);
            listener.SetPlaces(new Dictionary<string, IPlaceCardView>());
            Assert.IsTrue(view.loadingAnimationContainer.activeSelf);

            controller.SetFetchingDataState(false);
            listener.SetPlaces(new Dictionary<string, IPlaceCardView>());
            Assert.IsTrue(view.emptyContainer.activeSelf);

            listener.SetPlaces(new Dictionary<string, IPlaceCardView>() { { "1", Substitute.For<IPlaceCardView>() }, { "2", Substitute.For<IPlaceCardView>() } });
            Assert.IsTrue(view.contentContainer.activeSelf);

            listener.SetPlaces(new Dictionary<string, IPlaceCardView>() { { "1", Substitute.For<IPlaceCardView>() } });
            Assert.IsTrue(view.contentContainer.activeSelf);

            controller.Dispose();
        }
    }
}