using DCL.Controllers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.BuildModeHUDControllers
{
    public class BuildModeHUDControllerShould
    {
        private BuildModeHUDController buildModeHUDController;

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
                publishPopupController = Substitute.For<IPublishPopupController>(),
                dragAndDropSceneObjectController = Substitute.For<IDragAndDropSceneObjectController>(),
                publishBtnController = Substitute.For<IPublishBtnController>(),
                inspectorBtnController = Substitute.For<IInspectorBtnController>(),
                catalogBtnController = Substitute.For<ICatalogBtnController>(),
                inspectorController = Substitute.For<IInspectorController>(),
                buildModeConfirmationModalController = Substitute.For<IBuildModeConfirmationModalController>(),
                topActionsButtonsController = Substitute.For<ITopActionsButtonsController>()
            };

            buildModeHUDController = Substitute.ForPartsOf<BuildModeHUDController>();
            buildModeHUDController.Configure().CreateView().Returns(info => Substitute.For<IBuildModeHUDView>());
            buildModeHUDController.Initialize(testControllers);
        }

        [TearDown]
        public void TearDown() { }

        [Test]
        public void CreateBuildModeControllersCorrectly()
        {
            // Arrange
            buildModeHUDController.controllers.tooltipController = null;
            buildModeHUDController.controllers.sceneCatalogController = null;
            buildModeHUDController.controllers.quickBarController = null;
            buildModeHUDController.controllers.entityInformationController = null;
            buildModeHUDController.controllers.firstPersonModeController = null;
            buildModeHUDController.controllers.shortcutsController = null;
            buildModeHUDController.controllers.publishPopupController = null;
            buildModeHUDController.controllers.dragAndDropSceneObjectController = null;
            buildModeHUDController.controllers.publishBtnController = null;
            buildModeHUDController.controllers.inspectorBtnController = null;
            buildModeHUDController.controllers.catalogBtnController = null;
            buildModeHUDController.controllers.inspectorController = null;
            buildModeHUDController.controllers.topActionsButtonsController = null;

            // Act
            buildModeHUDController.CreateBuildModeControllers();

            // Assert
            Assert.NotNull(buildModeHUDController.controllers.tooltipController, "The tooltipController is null!");
            Assert.NotNull(buildModeHUDController.controllers.sceneCatalogController, "The sceneCatalogController is null!");
            Assert.NotNull(buildModeHUDController.controllers.quickBarController, "The quickBarController is null!");
            Assert.NotNull(buildModeHUDController.controllers.entityInformationController, "The entityInformationController is null!");
            Assert.NotNull(buildModeHUDController.controllers.firstPersonModeController, "The firstPersonModeController is null!");
            Assert.NotNull(buildModeHUDController.controllers.shortcutsController, "The shortcutsController is null!");
            Assert.NotNull(buildModeHUDController.controllers.publishPopupController, "The publishPopupController is null!");
            Assert.NotNull(buildModeHUDController.controllers.dragAndDropSceneObjectController, "The dragAndDropSceneObjectController is null!");
            Assert.NotNull(buildModeHUDController.controllers.publishBtnController, "The publishBtnController is null!");
            Assert.NotNull(buildModeHUDController.controllers.inspectorBtnController, "The inspectorBtnController is null!");
            Assert.NotNull(buildModeHUDController.controllers.catalogBtnController, "The catalogBtnController is null!");
            Assert.NotNull(buildModeHUDController.controllers.inspectorController, "The inspectorController is null!");
            Assert.NotNull(buildModeHUDController.controllers.topActionsButtonsController, "The topActionsButtonsController is null!");
        }

        [Test]
        public void CreateMainViewCorrectly()
        {
            // Arrange
            buildModeHUDController.view = null;

            // Act
            buildModeHUDController.CreateMainView();

            // Assert
            Assert.NotNull(buildModeHUDController.view, "The view is null!");
            buildModeHUDController.view.Received(1).Initialize(buildModeHUDController.controllers);
        }

        [Test]
        public void PublishStartCorrectly()
        {
            // Act
            buildModeHUDController.PublishStart();

            // Assert
            buildModeHUDController.controllers.buildModeConfirmationModalController.Received(1)
                                  .Configure(
                                      Arg.Any<string>(),
                                      Arg.Any<string>(),
                                      Arg.Any<string>(),
                                      Arg.Any<string>());
            buildModeHUDController.controllers.buildModeConfirmationModalController.Received(1).SetActive(true, BuildModeModalType.PUBLISH);
        }

        [Test]
        public void CancelPublishModalCorrectly()
        {
            // Act
            buildModeHUDController.CancelPublishModal(BuildModeModalType.PUBLISH);

            // Assert
            buildModeHUDController.controllers.buildModeConfirmationModalController.Received(1).SetActive(false, BuildModeModalType.PUBLISH);
        }

        [Test]
        public void ConfirmPublishModalCorrectly()
        {
            // Arrange
            bool publishConfirmed = false;
            buildModeHUDController.OnConfirmPublishAction += () => { publishConfirmed = true; };

            // Act
            buildModeHUDController.ConfirmPublishModal(BuildModeModalType.PUBLISH);

            // Assert
            buildModeHUDController.controllers.publishPopupController.Received(1).PublishStart();
            Assert.IsTrue(publishConfirmed, "publishConfirmed is false!");
        }

        [Test]
        public void ExitStartCorrectly()
        {
            // Act
            buildModeHUDController.ExitStart();

            // Assert
            buildModeHUDController.controllers.buildModeConfirmationModalController.Received(1)
                                  .Configure(
                                      Arg.Any<string>(),
                                      Arg.Any<string>(),
                                      Arg.Any<string>(),
                                      Arg.Any<string>());
            buildModeHUDController.controllers.buildModeConfirmationModalController.Received(1).SetActive(true, BuildModeModalType.EXIT);
        }

        [Test]
        public void CancelExitModalCorrectly()
        {
            // Act
            buildModeHUDController.CancelExitModal(BuildModeModalType.EXIT);

            // Assert
            buildModeHUDController.controllers.buildModeConfirmationModalController.Received(1).SetActive(false, BuildModeModalType.EXIT);
        }

        [Test]
        public void ConfirmExitModalCorrectly()
        {
            // Arrange
            bool exitConfirmed = false;
            buildModeHUDController.OnLogoutAction += () => { exitConfirmed = true; };

            // Act
            buildModeHUDController.ConfirmExitModal(BuildModeModalType.EXIT);

            // Assert
            Assert.IsTrue(exitConfirmed, "exitConfirmed is false!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void PublishEndCorrectly(bool isOk)
        {
            // Arrange
            string testErrorMessage = "Test text";

            // Act
            buildModeHUDController.PublishEnd(isOk, testErrorMessage);

            // Assert
            buildModeHUDController.controllers.publishPopupController.Received(1).PublishEnd(isOk, testErrorMessage);
        }

        [Test]
        public void SetParcelSceneCorrectly()
        {
            // Arrange
            ParcelScene testParcelScene = new GameObject("_ParcelScene").AddComponent<ParcelScene>();

            // Act
            buildModeHUDController.SetParcelScene(testParcelScene);

            // Assert
            buildModeHUDController.controllers.inspectorController.sceneLimitsController.Received(1).SetParcelScene(testParcelScene);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPublishBtnAvailabilityCorrectly(bool isAvailable)
        {
            // Act
            buildModeHUDController.SetPublishBtnAvailability(isAvailable);

            // Assert
            buildModeHUDController.view.Received(1).SetPublishBtnAvailability(isAvailable);
        }

        [Test]
        public void RefreshCatalogAssetPackCorrectly()
        {
            // Act
            buildModeHUDController.RefreshCatalogAssetPack();

            // Assert
            buildModeHUDController.view.Received(1).RefreshCatalogAssetPack();
        }

        [Test]
        public void RefreshCatalogContentCorrectly()
        {
            // Act
            buildModeHUDController.RefreshCatalogContent();

            // Assert
            buildModeHUDController.view.Received(1).RefreshCatalogContent();
        }

        [Test]
        public void CatalogItemSelectedCorrectly()
        {
            // Arrange
            CatalogItem returnedCatalogItem = null;
            CatalogItem testCatalogItem = new CatalogItem();
            buildModeHUDController.OnCatalogItemSelected += (item) => { returnedCatalogItem = item; };

            // Act
            buildModeHUDController.CatalogItemSelected(testCatalogItem);

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
            buildModeHUDController.isCatalogOpen = !isVisible;
            buildModeHUDController.OnCatalogOpen += () => { catalogOpened = true; };

            // Act
            buildModeHUDController.SetVisibilityOfCatalog(isVisible);

            // Assert
            Assert.AreEqual(isVisible, buildModeHUDController.isCatalogOpen, "The isCatalogOpen does not match!");
            buildModeHUDController.view.Received(1).SetVisibilityOfCatalog(buildModeHUDController.isCatalogOpen);

            if (isVisible)
                Assert.IsTrue(catalogOpened, "catalogOpened is false!");
        }

        [Test]
        public void ChangeVisibilityOfCatalogCorrectly()
        {
            // Arrange
            buildModeHUDController.isCatalogOpen = buildModeHUDController.controllers.sceneCatalogController.IsCatalogOpen();

            // Act
            buildModeHUDController.ChangeVisibilityOfCatalog();

            // Assert
            Assert.AreEqual(
                !buildModeHUDController.controllers.sceneCatalogController.IsCatalogOpen(),
                buildModeHUDController.isCatalogOpen,
                "The isCatalogOpen does not match!");
        }

        [Test]
        public void UpdateSceneLimitInfoCorrectly()
        {
            // Act
            buildModeHUDController.UpdateSceneLimitInfo();

            // Assert
            buildModeHUDController.controllers.inspectorController.sceneLimitsController.Received(1).UpdateInfo();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ChangeVisibilityOfSceneInfoCorrectly(bool shouldBeVisible)
        {
            // Arrange
            buildModeHUDController.isSceneLimitInfoVisibile = !shouldBeVisible;

            // Act
            buildModeHUDController.ChangeVisibilityOfSceneInfo(shouldBeVisible);

            // Assert
            Assert.AreEqual(shouldBeVisible, buildModeHUDController.isSceneLimitInfoVisibile, "The isSceneLimitInfoVisibile does not match!");
            buildModeHUDController.view.Received(1).SetVisibilityOfSceneInfo(buildModeHUDController.isSceneLimitInfoVisibile);
        }

        [Test]
        public void ChangeVisibilityOfSceneInfoCorrectly()
        {
            // Arrange
            buildModeHUDController.isSceneLimitInfoVisibile = false;

            // Act
            buildModeHUDController.ChangeVisibilityOfSceneInfo();

            // Assert
            Assert.IsTrue(buildModeHUDController.isSceneLimitInfoVisibile, "The isSceneLimitInfoVisibile is false!");
            buildModeHUDController.view.Received(1).SetVisibilityOfSceneInfo(buildModeHUDController.isSceneLimitInfoVisibile);
        }

        [Test]
        public void ActivateFirstPersonModeUICorrectly()
        {
            // Act
            buildModeHUDController.ActivateFirstPersonModeUI();

            // Assert
            buildModeHUDController.view.Received(1).SetFirstPersonView();
        }

        [Test]
        public void ActivateGodModeUICorrectly()
        {
            // Act
            buildModeHUDController.ActivateGodModeUI();

            // Assert
            buildModeHUDController.view.Received(1).SetGodModeView();
        }

        [Test]
        public void EntityInformationSetEntityCorrectly()
        {
            // Arrange
            DCLBuilderInWorldEntity testEntity = new GameObject("_DCLBuilderInWorldEntity").AddComponent<DCLBuilderInWorldEntity>();
            ParcelScene testScene = new GameObject("_ParcelScene").AddComponent<ParcelScene>();

            // Act
            buildModeHUDController.EntityInformationSetEntity(testEntity, testScene);

            // Assert
            buildModeHUDController.controllers.entityInformationController.Received(1).SetEntity(testEntity, testScene);
        }

        [Test]
        public void ShowEntityInformationCorrectly()
        {
            // Act
            buildModeHUDController.ShowEntityInformation();

            // Assert
            buildModeHUDController.controllers.entityInformationController.Received(1).Enable();
        }

        [Test]
        public void HideEntityInformationCorrectly()
        {
            // Act
            buildModeHUDController.HideEntityInformation();

            // Assert
            buildModeHUDController.controllers.entityInformationController.Received(1).Disable();
        }

        [Test]
        public void SetEntityListCorrectly()
        {
            // Arrange
            List<DCLBuilderInWorldEntity> testEntityList = new List<DCLBuilderInWorldEntity>();
            testEntityList.Add(new GameObject("_DCLBuilderInWorldEntity1").AddComponent<DCLBuilderInWorldEntity>());
            testEntityList.Add(new GameObject("_DCLBuilderInWorldEntity2").AddComponent<DCLBuilderInWorldEntity>());
            testEntityList.Add(new GameObject("_DCLBuilderInWorldEntity3").AddComponent<DCLBuilderInWorldEntity>());

            // Act
            buildModeHUDController.SetEntityList(testEntityList);

            // Assert
            buildModeHUDController.controllers.inspectorController.Received(1).SetEntityList(testEntityList);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ChangeVisibilityOfEntityListCorrectly(bool isVisible)
        {
            // Arrange
            bool isEntityListVisible = false;
            buildModeHUDController.isEntityListVisible = isVisible;
            buildModeHUDController.OnEntityListVisible += () => { isEntityListVisible = true; };

            // Act
            buildModeHUDController.ChangeVisibilityOfEntityList();

            // Assert
            if (buildModeHUDController.isEntityListVisible)
            {
                Assert.IsTrue(isEntityListVisible, "isEntityListVisible is false!");
                buildModeHUDController.controllers.inspectorController.Received(1).OpenEntityList();
            }
            else
            {
                buildModeHUDController.controllers.inspectorController.Received(1).CloseList();
            }
        }

        [Test]
        public void ClearEntityListCorrectly()
        {
            // Act
            buildModeHUDController.ClearEntityList();

            // Assert
            buildModeHUDController.controllers.inspectorController.Received(1).ClearList();
        }

        [Test]
        public void ChangeVisibilityOfControlsCorrectly()
        {
            // Arrange
            buildModeHUDController.isControlsVisible = false;

            // Act
            buildModeHUDController.ChangeVisibilityOfControls();

            // Assert
            Assert.IsTrue(buildModeHUDController.isControlsVisible, "The isControlsVisible is false!");
            buildModeHUDController.view.Received(1).SetVisibilityOfControls(buildModeHUDController.isControlsVisible);
        }

        [Test]
        public void ChangeVisibilityOfExtraBtnsCorrectly()
        {
            // Arrange
            buildModeHUDController.areExtraButtonsVisible = false;

            // Act
            buildModeHUDController.ChangeVisibilityOfExtraBtns();

            // Assert
            Assert.IsTrue(buildModeHUDController.areExtraButtonsVisible, "The areExtraButtonsVisible is false!");
            buildModeHUDController.view.Received(1).SetVisibilityOfExtraBtns(buildModeHUDController.areExtraButtonsVisible);
        }
    }
}