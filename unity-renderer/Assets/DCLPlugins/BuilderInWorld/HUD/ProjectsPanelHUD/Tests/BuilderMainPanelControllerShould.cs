using System;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class BuilderMainPanelControllerShould
    {
        private BuilderMainPanelController controller;
        private ISectionsController sectionsController;
        private IScenesViewController scenesViewController;
        private ILandsController landsesController;
        private IProjectsController projectsController;
        private INewProjectFlowController newProjectFlowController;

        private bool condtionMet = false;

        [SetUp]
        public void SetUp()
        {
            controller = new BuilderMainPanelController();

            sectionsController = Substitute.For<ISectionsController>();
            scenesViewController = Substitute.For<IScenesViewController>();
            landsesController = Substitute.For<ILandsController>();
            projectsController = Substitute.For<IProjectsController>();
            newProjectFlowController = Substitute.For<INewProjectFlowController>();

            ITheGraph theGraph = Substitute.For<ITheGraph>();
            theGraph.Query(Arg.Any<string>(), Arg.Any<string>()).Returns(new Promise<string>());
            theGraph.Query(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<QueryVariablesBase>()).Returns(new Promise<string>());
            theGraph.QueryLands(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<float>()).Returns(new Promise<List<Land>>());

            ICatalyst catalyst = Substitute.For<ICatalyst>();
            catalyst.contentUrl.Returns(string.Empty);
            catalyst.Get(Arg.Any<string>()).Returns(new Promise<string>());
            catalyst.GetEntities(Arg.Any<string>(), Arg.Any<string[]>()).Returns(new Promise<string>());
            catalyst.GetDeployedScenes(Arg.Any<string[]>()).Returns(new Promise<CatalystSceneEntityPayload[]>());

            controller.Initialize(sectionsController, scenesViewController,
                landsesController, projectsController, newProjectFlowController, theGraph, catalyst);
        }

        [TearDown]
        public void TearDown()
        {
            controller.OnJumpInOrEdit -= AssertJump;

            controller.Dispose();
        }

        [Test]
        public void GiveErrorOnFailLandsFetched()
        {
            //Arrange
            controller.isFetchingLands = true;
            var error =  "This error is intended";

            //Act
            controller.LandsFetchedError(error);

            //Assert
            Assert.IsFalse(controller.isFetchingLands);
            LogAssert.Expect(LogType.Error, error);
        }

        [Test]
        public void FetchLands()
        {
            //Arrange
            Parcel parcel = new Parcel();
            parcel.x = 0;
            parcel.y = 0;

            Vector2Int parcelCoords = new Vector2Int(0, 0);
            Land land = new Land();
            land.parcels = new List<Parcel>() { parcel };

            LandWithAccess landWithAccess = new LandWithAccess(land);
            Scene scene = new Scene();
            scene.parcelsCoord = new Vector2Int[] { parcelCoords };
            scene.deploymentSource = Scene.Source.SDK;

            landWithAccess.scenes = new List<Scene>() { scene };
            var lands = new LandWithAccess[]
            {
                landWithAccess
            };

            //Act
            controller.LandsFetched(lands);

            //Assert
            landsesController.Received().SetLands(lands);
        }

        [Test]
        public void AddProjectsCorrectly()
        {
            //Arrange
            ProjectData[] projectDatas = new [] { new ProjectData() };
            
            //Act
            controller.ProjectsFetched(projectDatas);
            
            //Assert
            Assert.IsFalse(controller.isFetchingProjects);
        }
        
        [Test]
        public void FailCorrectlyOnProjectFetchedError()
        {
            //Act
            controller.ProjectsFetchedError("Intended error");
            
            //Assert
            Assert.IsFalse(controller.isFetchingProjects);
        }

        [Test]
        public void GoToCoords()
        {
            //Arrange
            condtionMet = false;
            controller.OnJumpInOrEdit += AssertJump;

            //Act
            controller.GoToCoords(new Vector2Int(0, 0));

            //Assert
            Assert.IsTrue(condtionMet);
        }

        [Test]
        public void GoToEditScene()
        {
            //Arrange
            condtionMet = false;
            controller.OnJumpInOrEdit += AssertJump;

            //Act
            controller.OnGoToEditScene(new Vector2Int(0, 0));

            //Assert
            Assert.IsTrue(condtionMet);
        }

        private void AssertJump() { condtionMet = true; }

        [Test]
        public void ViewCreatedCorrectly() { Assert.IsNotNull(controller.view); }

        [Test]
        public void ViewVisibleCorrectly()
        {
            BuilderMainPanelView view = (BuilderMainPanelView)controller.view;
            Assert.IsFalse(view.gameObject.activeSelf);

            controller.SetVisibility(true);
            Assert.IsTrue(view.gameObject.activeSelf);
        }

        [Test]
        public void ViewHideCorrectly()
        {
            BuilderMainPanelView view = (BuilderMainPanelView)controller.view;

            controller.SetVisibility(true);
            Assert.IsTrue(view.gameObject.activeSelf);

            controller.SetVisibility(false);
            Assert.IsFalse(view.showHideAnimator.isVisible);
        }

        [Test]
        public void ViewHideCorrectlyOnClosePressed()
        {
            BuilderMainPanelView view = (BuilderMainPanelView)controller.view;

            controller.SetVisibility(true);
            Assert.IsTrue(view.gameObject.activeSelf);

            view.closeButton.onClick.Invoke();
            Assert.IsFalse(view.showHideAnimator.isVisible);
        }

        [Test]
        public void ViewHideAndShowCorrectlyOnEvent()
        {
            BuilderMainPanelView view = (BuilderMainPanelView)controller.view;

            DataStore.i.HUDs.builderProjectsPanelVisible.Set(true);
            Assert.IsTrue(view.showHideAnimator.isVisible);

            DataStore.i.HUDs.builderProjectsPanelVisible.Set(false);
            Assert.IsFalse(view.showHideAnimator.isVisible);
        }

        [Test]
        public void AddScenesListenerOnSectionShow()
        {
            var section = Substitute.For<SectionScenesController>();
            sectionsController.OnSectionShow += Raise.Event<Action<SectionBase>>(section);
            scenesViewController.Received(1).AddListener(section);
        }

        [Test]
        public void RemoveScenesListenerOnSectionHide()
        {
            var section = Substitute.For<SectionScenesController>();
            sectionsController.OnSectionHide += Raise.Event<Action<SectionBase>>(section);
            scenesViewController.Received(1).RemoveListener(section);
        }

        [Test]
        public void CallOpenSectionWhenSceneSelected()
        {
            var cardView = UnityEngine.Object.Instantiate(controller.view.GetSceneCardViewPrefab());
            ((ISceneCardView)cardView).Setup(new SceneData());
            scenesViewController.OnProjectSelected += Raise.Event<Action<ISceneCardView>>(cardView);
            sectionsController.Received(1).OpenSection(Arg.Any<SectionId>());
            UnityEngine.Object.DestroyImmediate(cardView.gameObject);
        }

        [Test]
        public void HandleLeftMenuCorrectly()
        {
            BuilderMainPanelView view = (BuilderMainPanelView)controller.view;

            sectionsController.OnOpenSectionId += Raise.Event<Action<SectionId>>(SectionId.SCENES);
            Assert.IsTrue(view.leftPanelMain.activeSelf);
            Assert.IsFalse(view.leftPanelProjectSettings.activeSelf);

            sectionsController.OnOpenSectionId += Raise.Event<Action<SectionId>>(SectionId.SETTINGS_PROJECT_GENERAL);
            Assert.IsTrue(view.leftPanelProjectSettings.activeSelf);
            Assert.IsFalse(view.leftPanelMain.activeSelf);

            view.backToMainPanelButton.onClick.Invoke();
            sectionsController.Received(1).OpenSection(SectionId.SCENES);
        }

        [Test]
        public void SetSettingsLeftMenuCorrectly()
        {
            Vector2Int coords = new Vector2Int(0, 1);
            const string author = "Temptation Creator";

            var cardView = UnityEngine.Object.Instantiate(controller.view.GetSceneCardViewPrefab());
            ((ISceneCardView)cardView).Setup(new SceneData()
            {
                isDeployed = true,
                coords = coords,
                authorName = author
            });
            scenesViewController.OnProjectSelected += Raise.Event<Action<ISceneCardView>>(cardView);

            LeftMenuSettingsViewReferences viewReferences = controller.view.GetSettingsViewReferences();

            Assert.AreEqual(LeftMenuSettingsViewHandler.SCENE_TITLE, viewReferences.titleLabel.text);
            Assert.IsTrue(viewReferences.coordsContainer.activeSelf);
            Assert.IsFalse(viewReferences.sizeContainer.activeSelf);
            Assert.AreEqual($"{coords.x},{coords.y}", viewReferences.coordsText.text);
            Assert.AreEqual(author, viewReferences.authorNameText.text);
            Assert.IsTrue(viewReferences.adminsMenuToggle.gameObject.activeSelf);

            UnityEngine.Object.DestroyImmediate(cardView.gameObject);
        }

        [Test]
        public void SceneContextMenuHidesCorrectly()
        {
            var contextMenu = controller.view.GetSceneCardViewContextMenu();
            contextMenu.gameObject.SetActive(true);

            sectionsController.OnRequestContextMenuHide += Raise.Event<Action>();
            Assert.IsFalse(contextMenu.gameObject.activeSelf);
        }
    }
}