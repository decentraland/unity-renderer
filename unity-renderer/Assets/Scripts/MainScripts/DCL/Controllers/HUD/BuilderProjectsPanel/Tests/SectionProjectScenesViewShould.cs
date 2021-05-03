using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class SectionProjectScenesViewShould
    {
        private SectionProjectScenesView view;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<SectionProjectScenesView>(SectionProjectScenesController.VIEW_PREFAB_PATH);
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

            Dictionary<string, ISceneCardView> cardViews = new Dictionary<string, ISceneCardView>();
            const int cardsCount = 10;
            for (int i = 0; i < cardsCount; i++)
            {
                var card = (ISceneCardView)Object.Instantiate(prefab);
                card.Setup(new SceneData(){size = new Vector2Int(i,i), id = i.ToString()});
                cardViews.Add(i.ToString(), card);
            }


            SectionProjectScenesController controller = new SectionProjectScenesController(view);
            controller.searchHandler.SetSortType(SectionSearchHandler.SIZE_SORT_TYPE_DESC);
            
            ((IProjectSceneListener)controller).OnSetScenes(cardViews);
            
            Assert.AreEqual(cardsCount, view.scenesCardContainer.childCount);

            var prev = (ISceneCardView)view.scenesCardContainer.GetChild(0).GetComponent<SceneCardView>();
            for (int i = 1; i < cardsCount; i++)
            {
                var current = (ISceneCardView)view.scenesCardContainer.GetChild(i).GetComponent<SceneCardView>();
                Assert.LessOrEqual(current.sceneData.size.x * current.sceneData.size.y, prev.sceneData.size.x * prev.sceneData.size.y);
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