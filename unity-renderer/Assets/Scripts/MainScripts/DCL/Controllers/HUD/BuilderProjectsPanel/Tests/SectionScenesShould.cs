using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DCL.Builder;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class SectionScenesShould
    {
        private SectionController sectionController;
        private IPlacesViewController placesController;

        [SetUp]
        public void SetUp()
        {
            const string sceneCardPrefabPath =
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/SceneCardView.prefab";
            var sceneCardPrefab = AssetDatabase.LoadAssetAtPath<PlaceCardView>(sceneCardPrefabPath);

            sectionController = new SectionController();
            placesController = new PlacesViewController(sceneCardPrefab);

            placesController.AddListener((IPlaceListener)sectionController);
            placesController.AddListener((IProjectListener)sectionController);
        }

        [TearDown]
        public void TearDown()
        {
            placesController.Dispose();
            sectionController.Dispose();
        }

        [Test]
        public void HavePrefabSetupCorrectly()
        {
            Assert.AreEqual(0, sectionController.view.deployedSceneContainer.transform.childCount, "InWorldCardsContainer should be empty");
            Assert.AreEqual(0, sectionController.view.projectSceneContainer.transform.childCount, "ProjectsCardsContainer should be empty");
        }

        [Test]
        public void ShowEmptyScreenWhenNoScenes()
        {
            Assert.IsTrue(sectionController.view.emptyScreen.activeSelf);
            Assert.IsFalse(sectionController.view.contentScreen.activeSelf);
        }

        [Test]
        public void ShowAndHideCardsCorrectly()
        {
            List<IPlaceData> scenes = new List<IPlaceData>();
            Assert.IsFalse(sectionController.view.inWorldContainer.activeSelf);
            Assert.IsFalse(sectionController.view.projectsContainer.activeSelf);

            //add project scene
            scenes.Add(new PlaceData() { isDeployed = false, id = "Project1" });
            placesController.SetPlaces(scenes.ToArray());
            Assert.IsFalse(sectionController.view.inWorldContainer.activeSelf);
            Assert.IsTrue(sectionController.view.projectsContainer.activeSelf);

            //add deployed scenes
            scenes.Add(new PlaceData() { isDeployed = true, id = "Deployed1" });
            scenes.Add(new PlaceData() { isDeployed = true, id = "Deployed2" });
            scenes.Add(new PlaceData() { isDeployed = true, id = "Deployed3" });
            placesController.SetPlaces(scenes.ToArray());
            Assert.IsTrue(sectionController.view.inWorldContainer.activeSelf);
            Assert.IsTrue(sectionController.view.projectsContainer.activeSelf);
            Assert.AreEqual(1, sectionController.view.projectSceneContainer.childCount);
            Assert.AreEqual(3, sectionController.view.deployedSceneContainer.childCount);
            Assert.AreEqual(1, GetVisibleChildrenAmount(sectionController.view.projectSceneContainer));
            Assert.AreEqual(3, GetVisibleChildrenAmount(sectionController.view.deployedSceneContainer));
            Assert.AreEqual(sectionController.view.projectSceneContainer.childCount, sectionController.projectViews.Count);
            Assert.AreEqual(sectionController.view.deployedSceneContainer.childCount, sectionController.deployedViews.Count);

            //add deployed scene
            scenes.Add(new PlaceData() { isDeployed = true, id = "Deployed4" });
            placesController.SetPlaces(scenes.ToArray());
            Assert.IsTrue(sectionController.view.inWorldContainer.activeSelf);
            Assert.IsTrue(sectionController.view.projectsContainer.activeSelf);
            Assert.AreEqual(1, sectionController.view.projectSceneContainer.childCount);
            Assert.AreEqual(4, sectionController.view.deployedSceneContainer.childCount);
            Assert.AreEqual(1, GetVisibleChildrenAmount(sectionController.view.projectSceneContainer));
            Assert.AreEqual(3, GetVisibleChildrenAmount(sectionController.view.deployedSceneContainer));
            Assert.AreEqual(sectionController.view.projectSceneContainer.childCount, sectionController.projectViews.Count);
            Assert.AreEqual(sectionController.view.deployedSceneContainer.childCount, sectionController.deployedViews.Count);

            //remove deployed scene
            scenes = scenes.FindAll((data) => data.id != "Deployed3");
            placesController.SetPlaces(scenes.ToArray());
            Assert.IsTrue(sectionController.view.inWorldContainer.activeSelf);
            Assert.IsTrue(sectionController.view.projectsContainer.activeSelf);
            Assert.AreEqual(1, sectionController.view.projectSceneContainer.childCount);
            Assert.AreEqual(3, sectionController.view.deployedSceneContainer.childCount);
            Assert.AreEqual(1, GetVisibleChildrenAmount(sectionController.view.projectSceneContainer));
            Assert.AreEqual(3, GetVisibleChildrenAmount(sectionController.view.deployedSceneContainer));
            Assert.AreEqual(sectionController.view.projectSceneContainer.childCount, sectionController.projectViews.Count);
            Assert.AreEqual(sectionController.view.deployedSceneContainer.childCount, sectionController.deployedViews.Count);

            //remove all deployed
            scenes = new List<IPlaceData>() { new PlaceData() { isDeployed = false, id = "Project1" } };
            placesController.SetPlaces(scenes.ToArray());
            Assert.IsFalse(sectionController.view.inWorldContainer.activeSelf);
            Assert.IsTrue(sectionController.view.projectsContainer.activeSelf);
            Assert.AreEqual(sectionController.view.projectSceneContainer.childCount, sectionController.projectViews.Count);
            Assert.AreEqual(sectionController.view.deployedSceneContainer.childCount, sectionController.deployedViews.Count);

            //switch project to deployed
            scenes = new List<IPlaceData>() { new PlaceData() { isDeployed = true, id = "Project1" } };
            placesController.SetPlaces(scenes.ToArray());
            Assert.IsTrue(sectionController.view.inWorldContainer.activeSelf);
            Assert.IsFalse(sectionController.view.projectsContainer.activeSelf);
            Assert.AreEqual(sectionController.view.projectSceneContainer.childCount, sectionController.projectViews.Count);
            Assert.AreEqual(sectionController.view.deployedSceneContainer.childCount, sectionController.deployedViews.Count);

            //remove all scenes
            placesController.SetPlaces(new IPlaceData[] { });
            Assert.IsFalse(sectionController.view.inWorldContainer.activeSelf);
            Assert.IsFalse(sectionController.view.projectsContainer.activeSelf);
            Assert.AreEqual(sectionController.view.projectSceneContainer.childCount, sectionController.projectViews.Count);
            Assert.AreEqual(sectionController.view.deployedSceneContainer.childCount, sectionController.deployedViews.Count);
        }

        public int GetVisibleChildrenAmount(Transform parent) { return parent.Cast<Transform>().Count(child => child.gameObject.activeSelf); }
    }
}