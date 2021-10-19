using System.Collections.Generic;
using DCL.Builder;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class SectionProjectViewShould
    {
        private SectionProjectView view;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<SectionProjectView>(SectionProjectController.VIEW_PREFAB_PATH);
            view = Object.Instantiate(prefab);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(view.gameObject); }

        [Test]
        [Explicit("Not implemented yet")]
        [Category("Explicit")]
        public void ShowCardsInCorrectSortOrder()
        {
            const string prefabAssetPath =
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/ProjectCardView.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<PlaceCardView>(prefabAssetPath);

            Dictionary<string, IProjectCardView> cardViews = new Dictionary<string, IProjectCardView>();
            const int cardsCount = 10;
            for (int i = 0; i < cardsCount; i++)
            {
                var card = (IProjectCardView)Object.Instantiate(prefab);
                card.Setup(new ProjectData());
                cardViews.Add(i.ToString(), card);
            }

            SectionProjectController controller = new SectionProjectController(view);
            controller.searchHandler.SetSortType(SectionSearchHandler.SIZE_SORT_TYPE_DESC);

            ((IProjectsListener)controller).OnSetProjects(cardViews);

            Assert.AreEqual(cardsCount, view.contentContainer.transform.childCount);

            var prev = (IPlaceCardView)view.contentContainer.transform.GetChild(0).GetComponent<PlaceCardView>();
            for (int i = 1; i < cardsCount; i++)
            {
                var current = (IPlaceCardView)view.contentContainer.transform.GetChild(i).GetComponent<PlaceCardView>();
                Assert.LessOrEqual(current.PlaceData.size.x * current.PlaceData.size.y, prev.PlaceData.size.x * prev.PlaceData.size.y);
                prev = current;
            }

            foreach (var card in cardViews.Values)
            {
                card.Dispose();
            }
            controller.Dispose();
        }
    }
}