using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class SectionDeployedScenesViewShould
    {
        private SectionDeployedScenesView view;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<SectionDeployedScenesView>(SectionDeployedScenesController.VIEW_PREFAB_PATH);
            view = Object.Instantiate(prefab);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(view.gameObject);
        }

        [Test]
        public void HaveScenesContainerEmptyAtInstantiation()
        {
            Assert.AreEqual(0, view.scenesCardContainer.childCount);
        }

        [Test]
        public void ShowCardsInCorrectSortOrder()
        {
            const string prefabAssetPath =
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/SceneCardView.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<SceneCardView>(prefabAssetPath);

            Dictionary<string, SceneCardView> cardViews = new Dictionary<string, SceneCardView>();
            const int cardsCount = 10;
            for (int i = 0; i < cardsCount; i++)
            {
                var card = Object.Instantiate(prefab);
                card.Setup(new SceneData(){size = new Vector2Int(i,i), id = i.ToString()});
                cardViews.Add(i.ToString(), card);
            }


            SectionDeployedScenesController controller = new SectionDeployedScenesController(view);
            controller.searchHandler.SetSortType(SceneSearchHandler.SIZE_SORT_TYPE);
            controller.searchHandler.SetSortOrder(false);
            
            ((IDeployedSceneListener)controller).OnSetScenes(cardViews);
            
            Assert.AreEqual(cardsCount, view.scenesCardContainer.childCount);

            var prev = view.scenesCardContainer.GetChild(0).GetComponent<SceneCardView>();
            for (int i = 1; i < cardsCount; i++)
            {
                var current = view.scenesCardContainer.GetChild(i).GetComponent<SceneCardView>();
                Assert.GreaterOrEqual(current.sceneData.size.x * current.sceneData.size.y, prev.sceneData.size.x * prev.sceneData.size.y);
                prev = current;
            }

            foreach (var card in cardViews.Values)
            {
                Object.Destroy(card);
            }
            controller.Dispose();
        }
    }
}