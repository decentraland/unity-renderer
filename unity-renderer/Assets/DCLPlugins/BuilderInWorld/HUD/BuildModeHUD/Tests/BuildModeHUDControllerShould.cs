using DCL.Controllers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Helpers;
using UnityEngine;

namespace Tests.BuildModeHUDControllers
{
    public class BuildModeHUDControllerShould
    {
        private BuilderEditorHUDController builderEditorHudController;

        [SetUp]
        public void SetUp()
        {
            BuildModeHUDInitializationModel testControllers = new BuildModeHUDInitializationModel
            {
                tooltipController = Substitute.For<ITooltipController>(),
                sceneCatalogController = Substitute.For<ISceneCatalogController>(),
                quickBarController = Substitute.For<IQuickBarController>(),
                entityInformationController = Substitute.For<IEntityInformationController>(),
                firstPersonModeController = Substitute.For<IFirstPersonModeController>(),
                shortcutsController = Substitute.For<IShortcutsController>(),
                dragAndDropSceneObjectController = Substitute.For<IDragAndDropSceneObjectController>(),
                publishBtnController = Substitute.For<IPublishBtnController>(),
                inspectorBtnController = Substitute.For<IInspectorBtnController>(),
                catalogBtnController = Substitute.For<ICatalogBtnController>(),
                inspectorController = Substitute.For<IInspectorController>(),
                buildModeConfirmationModalController = Substitute.For<IBuildModeConfirmationModalController>(),
                topActionsButtonsController = Substitute.For<ITopActionsButtonsController>(),
                saveHUDController = Substitute.For<ISaveHUDController>(),
                newProjectDetailsController = Substitute.For<INewProjectDetailController>(),
                feedbackTooltipController =  Substitute.For<ITooltipController>()
            };

            builderEditorHudController = Substitute.ForPartsOf<BuilderEditorHUDController>();
            builderEditorHudController.Configure().CreateView().Returns(info => Substitute.For<IBuildModeHUDView>());
            builderEditorHudController.Initialize(testControllers, BIWTestUtils.CreateMockedContext());
        }

        [TearDown]
        public void TearDown() { builderEditorHudController.Dispose(); }

        [Test]
        public void CreateBuildModeControllersCorrectly()
        {
            // Arrange
            builderEditorHudController.controllers.tooltipController = null;
            builderEditorHudController.controllers.sceneCatalogController = null;
            builderEditorHudController.controllers.quickBarController = null;
            builderEditorHudController.controllers.entityInformationController = null;
            builderEditorHudController.controllers.firstPersonModeController = null;
            builderEditorHudController.controllers.shortcutsController = null;
            builderEditorHudController.controllers.dragAndDropSceneObjectController = null;
            builderEditorHudController.controllers.publishBtnController = null;
            builderEditorHudController.controllers.inspectorBtnController = null;
            builderEditorHudController.controllers.catalogBtnController = null;
            builderEditorHudController.controllers.inspectorController = null;
            builderEditorHudController.controllers.topActionsButtonsController = null;
            builderEditorHudController.controllers.saveHUDController = null;
            builderEditorHudController.controllers.newProjectDetailsController = null;

            // Act
            builderEditorHudController.CreateBuildModeControllers();

            // Assert
            Assert.NotNull(builderEditorHudController.controllers.tooltipController, "The tooltipController is null!");
            Assert.NotNull(builderEditorHudController.controllers.sceneCatalogController, "The sceneCatalogController is null!");
            Assert.NotNull(builderEditorHudController.controllers.quickBarController, "The quickBarController is null!");
            Assert.NotNull(builderEditorHudController.controllers.entityInformationController, "The entityInformationController is null!");
            Assert.NotNull(builderEditorHudController.controllers.firstPersonModeController, "The firstPersonModeController is null!");
            Assert.NotNull(builderEditorHudController.controllers.shortcutsController, "The shortcutsController is null!");
            Assert.NotNull(builderEditorHudController.controllers.dragAndDropSceneObjectController, "The dragAndDropSceneObjectController is null!");
            Assert.NotNull(builderEditorHudController.controllers.publishBtnController, "The publishBtnController is null!");
            Assert.NotNull(builderEditorHudController.controllers.inspectorBtnController, "The inspectorBtnController is null!");
            Assert.NotNull(builderEditorHudController.controllers.catalogBtnController, "The catalogBtnController is null!");
            Assert.NotNull(builderEditorHudController.controllers.inspectorController, "The inspectorController is null!");
            Assert.NotNull(builderEditorHudController.controllers.topActionsButtonsController, "The topActionsButtonsController is null!");
            Assert.NotNull(builderEditorHudController.controllers.saveHUDController, "The saveHUDController is null!");
            Assert.NotNull(builderEditorHudController.controllers.newProjectDetailsController, "The newProjectDetailsController is null!");
        }

        [Test]
        public void CreateMainViewCorrectly()
        {
            // Arrange
            builderEditorHudController.view = null;

            // Act
            builderEditorHudController.CreateMainView();

            // Assert
            Assert.NotNull(builderEditorHudController.view, "The view is null!");
            builderEditorHudController.view.Received(1).Initialize(builderEditorHudController.controllers);
        }

        [Test]
        public void StartNewProjectFlowCorrectly()
        {
            // Arrange
            Texture2D testScreenshot = new Texture2D(10, 10);

            // Act
            builderEditorHudController.NewSceneForLand(Substitute.For<IBuilderScene>());

            // Assert
            // TODO: This is temporal until we add the Welcome panel where the user will be able to edit the project info

            //builderEditorHudController.controllers.newProjectDetailsController.Received(1).SetPublicationScreenshot(testScreenshot);
            //builderEditorHudController.controllers.newProjectDetailsController.Received(1).SetActive(true);

            Object.Destroy(testScreenshot);
        }

        [Test]
        public void ConfirmNewProjectDetailsCorrectly()
        {
            //TODO: Re-Implement when the welcome panel has been implemented
            // Arrange
            bool newProjectDetailsConfirmed = false;
            // builderEditorHudController.OnSaveSceneInfoAction += (name, desc, image) =>
            // {
            //     newProjectDetailsConfirmed = true;
            // };
            //builderEditorHudController.context.cameraController.Configure().GetLastScreenshot().Returns(new Texture2D(120, 120));

            // Act
            // builderEditorHudController.SaveSceneInfo();

            // Assert
            // builderEditorHudController.controllers.newProjectDetailsController.Received(1).GetSceneName();
            // builderEditorHudController.controllers.newProjectDetailsController.Received(1).GetSceneDescription();
            // builderEditorHudController.controllers.publicationDetailsController.Received(1).SetCustomPublicationInfo(Arg.Any<string>(), Arg.Any<string>());
            // builderEditorHudController.controllers.newProjectDetailsController.Received(1).SetActive(false);
            // Assert.IsTrue(newProjectDetailsConfirmed);
        }

        [Test]
        public void CancelNewProjectDetailsCorrectly()
        {
            // Act
            builderEditorHudController.CancelNewProjectDetails();

            // Assert
            builderEditorHudController.controllers.newProjectDetailsController.Received(1).SetActive(false);
        }

        [Test]
        public void ExitStartCorrectly()
        {
            // Act
            builderEditorHudController.ExitStart();

            // Assert
            builderEditorHudController.controllers.buildModeConfirmationModalController.Received(1).SetActive(true, BuildModeModalType.EXIT);
        }

        [Test]
        public void CancelExitModalCorrectly()
        {
            // Act
            builderEditorHudController.CancelExitModal(BuildModeModalType.EXIT);

            // Assert
            builderEditorHudController.controllers.buildModeConfirmationModalController.Received(1).SetActive(false, BuildModeModalType.EXIT);
        }

        [Test]
        public void ConfirmExitModalCorrectly()
        {
            // Arrange
            bool exitConfirmed = false;
            builderEditorHudController.OnLogoutAction += () => { exitConfirmed = true; };

            // Act
            builderEditorHudController.ConfirmExitModal(BuildModeModalType.EXIT);

            // Assert
            Assert.IsTrue(exitConfirmed, "exitConfirmed is false!");
        }

        [Test]
        public void SetParcelSceneCorrectly()
        {
            // Arrange
            ParcelScene testParcelScene = TestUtils.CreateComponentWithGameObject<ParcelScene>("_ParcelScene");

            // Act
            builderEditorHudController.SetParcelScene(testParcelScene);

            // Assert
            builderEditorHudController.controllers.inspectorController.sceneLimitsController.Received(1).SetParcelScene(testParcelScene);

            Object.Destroy(testParcelScene.gameObject);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPublishBtnAvailabilityCorrectly(bool isAvailable)
        {
            // Act
            builderEditorHudController.SetPublishBtnAvailability(isAvailable);

            // Assert
            builderEditorHudController.view.Received(1).SetPublishBtnAvailability(isAvailable);
        }

        [Test]
        public void RefreshCatalogAssetPackCorrectly()
        {
            // Act
            builderEditorHudController.RefreshCatalogAssetPack();

            // Assert
            builderEditorHudController.view.Received(1).RefreshCatalogAssetPack();
        }

        [Test]
        public void RefreshCatalogContentCorrectly()
        {
            // Act
            builderEditorHudController.RefreshCatalogContent();

            // Assert
            builderEditorHudController.view.Received(1).RefreshCatalogContent();
        }

        [Test]
        public void CatalogItemSelectedCorrectly()
        {
            // Arrange
            CatalogItem returnedCatalogItem = null;
            CatalogItem testCatalogItem = new CatalogItem();
            builderEditorHudController.OnCatalogItemSelected += (item) => { returnedCatalogItem = item; };

            // Act
            builderEditorHudController.CatalogItemSelected(testCatalogItem);

            // Assert
            Assert.AreEqual(testCatalogItem, returnedCatalogItem, "The catalog item does not march!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfCatalogCorrectly(bool isVisible)
        {
            // Arrange
            bool catalogOpened = false;
            builderEditorHudController.isCatalogOpen = !isVisible;
            builderEditorHudController.OnCatalogOpen += () => { catalogOpened = true; };

            // Act
            builderEditorHudController.SetVisibilityOfCatalog(isVisible);

            // Assert
            Assert.AreEqual(isVisible, builderEditorHudController.isCatalogOpen, "The isCatalogOpen does not match!");
            builderEditorHudController.view.Received(1).SetVisibilityOfCatalog(builderEditorHudController.isCatalogOpen);

            if (isVisible)
                Assert.IsTrue(catalogOpened, "catalogOpened is false!");
        }

        [Test]
        public void ChangeVisibilityOfCatalogCorrectly()
        {
            // Arrange
            builderEditorHudController.isCatalogOpen = builderEditorHudController.controllers.sceneCatalogController.IsCatalogOpen();

            // Act
            builderEditorHudController.ChangeVisibilityOfCatalog();

            // Assert
            Assert.AreEqual(
                !builderEditorHudController.controllers.sceneCatalogController.IsCatalogOpen(),
                builderEditorHudController.isCatalogOpen,
                "The isCatalogOpen does not match!");
        }

        [Test]
        public void UpdateSceneLimitInfoCorrectly()
        {
            // Act
            builderEditorHudController.UpdateSceneLimitInfo();

            // Assert
            builderEditorHudController.controllers.inspectorController.sceneLimitsController.Received(1).UpdateInfo();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ChangeVisibilityOfSceneInfoCorrectly(bool shouldBeVisible)
        {
            // Arrange
            builderEditorHudController.isSceneLimitInfoVisibile = !shouldBeVisible;

            // Act
            builderEditorHudController.ChangeVisibilityOfSceneInfo(shouldBeVisible);

            // Assert
            Assert.AreEqual(shouldBeVisible, builderEditorHudController.isSceneLimitInfoVisibile, "The isSceneLimitInfoVisibile does not match!");
            builderEditorHudController.view.Received(1).SetVisibilityOfSceneInfo(builderEditorHudController.isSceneLimitInfoVisibile);
        }

        [Test]
        public void ChangeVisibilityOfSceneInfoCorrectly()
        {
            // Arrange
            builderEditorHudController.isSceneLimitInfoVisibile = false;

            // Act
            builderEditorHudController.ChangeVisibilityOfSceneInfo();

            // Assert
            Assert.IsTrue(builderEditorHudController.isSceneLimitInfoVisibile, "The isSceneLimitInfoVisibile is false!");
            builderEditorHudController.view.Received(1).SetVisibilityOfSceneInfo(builderEditorHudController.isSceneLimitInfoVisibile);
        }

        [Test]
        public void ActivateFirstPersonModeUICorrectly()
        {
            // Act
            builderEditorHudController.ActivateFirstPersonModeUI();

            // Assert
            builderEditorHudController.view.Received(1).SetFirstPersonView();
        }

        [Test]
        public void ActivateGodModeUICorrectly()
        {
            // Act
            builderEditorHudController.ActivateGodModeUI();

            // Assert
            builderEditorHudController.view.Received(1).SetGodModeView();
        }

        [Test]
        public void EntityInformationSetEntityCorrectly()
        {
            // Arrange
            BIWEntity testEntity = new BIWEntity();
            ParcelScene testScene = TestUtils.CreateComponentWithGameObject<ParcelScene>("_ParcelScene");

            // Act
            builderEditorHudController.EntityInformationSetEntity(testEntity, testScene);

            // Assert
            builderEditorHudController.controllers.entityInformationController.Received(1).SetEntity(testEntity, testScene);

            Object.Destroy(testScene.gameObject);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShowEntityInformationCorrectly(bool activateTransparencyMode)
        {
            // Act
            builderEditorHudController.ShowEntityInformation(activateTransparencyMode);

            // Assert
            builderEditorHudController.controllers.entityInformationController.Received(1).Enable();
            builderEditorHudController.controllers.entityInformationController.Received(1).SetTransparencyMode(activateTransparencyMode);
            builderEditorHudController.controllers.sceneCatalogController.Received(1).CloseCatalog();
            builderEditorHudController.controllers.tooltipController.Received(1).HideTooltip();

            if (activateTransparencyMode)
                builderEditorHudController.controllers.catalogBtnController.Received(1).SetActive(false);
        }

        [Test]
        public void HideEntityInformationCorrectly()
        {
            // Act
            builderEditorHudController.HideEntityInformation();

            // Assert
            builderEditorHudController.controllers.entityInformationController.Received(1).Disable();
            builderEditorHudController.controllers.catalogBtnController.Received(1).SetActive(true);

            if (builderEditorHudController.isCatalogOpen)
                builderEditorHudController.controllers.sceneCatalogController.Received(1).SetActive(true);
        }

        [Test]
        public void SetEntityListCorrectly()
        {
            // Arrange
            List<BIWEntity> testEntityList = new List<BIWEntity>();
            testEntityList.Add(new BIWEntity());
            testEntityList.Add(new BIWEntity());
            testEntityList.Add(new BIWEntity());

            // Act
            builderEditorHudController.SetEntityList(testEntityList);

            // Assert
            builderEditorHudController.controllers.inspectorController.Received(1).SetEntityList(testEntityList);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ChangeVisibilityOfEntityListCorrectly(bool isVisible)
        {
            // Arrange
            bool isEntityListVisible = false;
            builderEditorHudController.isEntityListVisible = isVisible;
            builderEditorHudController.OnEntityListVisible += () => { isEntityListVisible = true; };

            // Act
            builderEditorHudController.ChangeVisibilityOfEntityList();

            // Assert
            if (builderEditorHudController.isEntityListVisible)
            {
                Assert.IsTrue(isEntityListVisible, "isEntityListVisible is false!");
                builderEditorHudController.controllers.inspectorController.Received(1).OpenEntityList();
            }
            else
            {
                builderEditorHudController.controllers.inspectorController.Received(1).CloseList();
            }
        }

        [Test]
        public void ClearEntityListCorrectly()
        {
            // Act
            builderEditorHudController.ClearEntityList();

            // Assert
            builderEditorHudController.controllers.inspectorController.Received(1).ClearList();
        }

        [Test]
        public void ChangeVisibilityOfControlsCorrectly()
        {
            // Arrange
            builderEditorHudController.isControlsVisible = false;

            // Act
            builderEditorHudController.ChangeVisibilityOfControls();

            // Assert
            Assert.IsTrue(builderEditorHudController.isControlsVisible, "The isControlsVisible is false!");
            builderEditorHudController.view.Received(1).SetVisibilityOfControls(builderEditorHudController.isControlsVisible);
        }

        [Test]
        public void ChangeVisibilityOfExtraBtnsCorrectly()
        {
            // Arrange
            builderEditorHudController.areExtraButtonsVisible = false;

            // Act
            builderEditorHudController.ChangeVisibilityOfExtraBtns();

            // Assert
            Assert.IsTrue(builderEditorHudController.areExtraButtonsVisible, "The areExtraButtonsVisible is false!");
            builderEditorHudController.view.Received(1).SetVisibilityOfExtraBtns(builderEditorHudController.areExtraButtonsVisible);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfInspectorCorrectly(bool isVisible)
        {
            // Act
            builderEditorHudController.SetVisibilityOfInspector(isVisible);

            // Assert
            Assert.AreEqual(isVisible, builderEditorHudController.isEntityListVisible);
            builderEditorHudController.view.Received(1).SetVisibilityOfInspector(isVisible);
        }
    }
}