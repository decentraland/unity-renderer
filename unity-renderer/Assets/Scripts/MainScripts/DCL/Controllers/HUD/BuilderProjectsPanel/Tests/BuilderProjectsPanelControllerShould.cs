using System;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class BuilderProjectsPanelControllerShould
    {
        private BuilderProjectsPanelController controller;
        private ISectionsController sectionsController;
        private IScenesViewController scenesViewController;
        private ILandController landsController;

        [SetUp]
        public void SetUp()
        {
            controller = new BuilderProjectsPanelController();

            sectionsController = Substitute.For<ISectionsController>();
            scenesViewController = Substitute.For<IScenesViewController>();
            landsController = Substitute.For<ILandController>();

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
                landsController, theGraph, catalyst);
        }

        [TearDown]
        public void TearDown() { controller.Dispose(); }

        [Test]
        public void ViewCreatedCorrectly() { Assert.IsNotNull(controller.view); }

        [Test]
        public void ViewVisibleCorrectly()
        {
            BuilderProjectsPanelView view = (BuilderProjectsPanelView)controller.view;
            Assert.IsFalse(view.gameObject.activeSelf);

            controller.SetVisibility(true);
            Assert.IsTrue(view.gameObject.activeSelf);
        }

        [Test]
        public void ViewHideCorrectly()
        {
            BuilderProjectsPanelView view = (BuilderProjectsPanelView)controller.view;

            controller.SetVisibility(true);
            Assert.IsTrue(view.gameObject.activeSelf);

            controller.SetVisibility(false);
            Assert.IsFalse(view.showHideAnimator.isVisible);
        }

        [Test]
        public void ViewHideCorrectlyOnClosePressed()
        {
            BuilderProjectsPanelView view = (BuilderProjectsPanelView)controller.view;

            controller.SetVisibility(true);
            Assert.IsTrue(view.gameObject.activeSelf);

            view.closeButton.onClick.Invoke();
            Assert.IsFalse(view.showHideAnimator.isVisible);
        }

        [Test]
        public void ViewHideAndShowCorrectlyOnEvent()
        {
            BuilderProjectsPanelView view = (BuilderProjectsPanelView)controller.view;

            DataStore.i.HUDs.builderProjectsPanelVisible.Set(true);
            Assert.IsTrue(view.showHideAnimator.isVisible);

            DataStore.i.HUDs.builderProjectsPanelVisible.Set(false);
            Assert.IsFalse(view.showHideAnimator.isVisible);
        }

        [Test]
        public void AddScenesListenerOnSectionShow()
        {
            var section = Substitute.For<SectionDeployedScenesController>();
            sectionsController.OnSectionShow += Raise.Event<Action<SectionBase>>(section);
            scenesViewController.Received(1).AddListener(section);
        }

        [Test]
        public void RemoveScenesListenerOnSectionHide()
        {
            var section = Substitute.For<SectionDeployedScenesController>();
            sectionsController.OnSectionHide += Raise.Event<Action<SectionBase>>(section);
            scenesViewController.Received(1).RemoveListener(section);
        }

        [Test]
        public void CallOpenSectionWhenSceneSelected()
        {
            var cardView = UnityEngine.Object.Instantiate(controller.view.GetCardViewPrefab());
            ((ISceneCardView)cardView).Setup(new SceneData());
            scenesViewController.OnSceneSelected += Raise.Event<Action<ISceneCardView>>(cardView);
            sectionsController.Received(1).OpenSection(Arg.Any<SectionId>());
            UnityEngine.Object.DestroyImmediate(cardView.gameObject);
        }

        [Test]
        public void HandleLeftMenuCorrectly()
        {
            BuilderProjectsPanelView view = (BuilderProjectsPanelView)controller.view;

            sectionsController.OnOpenSectionId += Raise.Event<Action<SectionId>>(SectionId.SCENES_DEPLOYED);
            Assert.IsTrue(view.leftPanelMain.activeSelf);
            Assert.IsFalse(view.leftPanelProjectSettings.activeSelf);

            sectionsController.OnOpenSectionId += Raise.Event<Action<SectionId>>(SectionId.SETTINGS_PROJECT_GENERAL);
            Assert.IsTrue(view.leftPanelProjectSettings.activeSelf);
            Assert.IsFalse(view.leftPanelMain.activeSelf);

            view.backToMainPanelButton.onClick.Invoke();
            sectionsController.Received(1).OpenSection(SectionId.SCENES_DEPLOYED);
        }

        [Test]
        public void SetSettingsLeftMenuCorrectly()
        {
            Vector2Int coords = new Vector2Int(0, 1);
            const string author = "Temptation Creator";

            var cardView = UnityEngine.Object.Instantiate(controller.view.GetCardViewPrefab());
            ((ISceneCardView)cardView).Setup(new SceneData()
            {
                isDeployed = true,
                coords = coords,
                authorName = author
            });
            scenesViewController.OnSceneSelected += Raise.Event<Action<ISceneCardView>>(cardView);

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