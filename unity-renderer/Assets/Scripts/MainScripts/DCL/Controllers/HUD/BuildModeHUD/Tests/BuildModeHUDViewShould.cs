using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class BuildModeHUDViewShould
    {
        private BuildModeHUDView buildModeHUDView;
        private BuildModeHUDInitializationModel testControllers;

        [SetUp]
        public void SetUp()
        {
            testControllers = new BuildModeHUDInitializationModel
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

            buildModeHUDView = BuildModeHUDView.Create();
            buildModeHUDView.Initialize(testControllers);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(buildModeHUDView.gameObject); }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            Assert.AreEqual(testControllers.tooltipController, buildModeHUDView.controllers.tooltipController, "The tooltipController does not match!");
            testControllers.tooltipController.Received(1).Initialize(buildModeHUDView.tooltipView);
            Assert.AreEqual(testControllers.sceneCatalogController, buildModeHUDView.controllers.sceneCatalogController, "The sceneCatalogController does not match!");
            testControllers.sceneCatalogController.Received(1).Initialize(buildModeHUDView.sceneCatalogView, testControllers.quickBarController);
            Assert.AreEqual(testControllers.quickBarController, buildModeHUDView.controllers.quickBarController, "The quickBarController does not match!");
            testControllers.quickBarController.Received(1).Initialize(buildModeHUDView.quickBarView, testControllers.sceneCatalogController);
            Assert.AreEqual(testControllers.entityInformationController, buildModeHUDView.controllers.entityInformationController, "The entityInformationController does not match!");
            testControllers.entityInformationController.Received(1).Initialize(buildModeHUDView.entityInformationView);
            Assert.AreEqual(testControllers.firstPersonModeController, buildModeHUDView.controllers.firstPersonModeController, "The firstPersonModeController does not match!");
            testControllers.firstPersonModeController.Received(1).Initialize(buildModeHUDView.firstPersonModeView, testControllers.tooltipController);
            Assert.AreEqual(testControllers.shortcutsController, buildModeHUDView.controllers.shortcutsController, "The shortcutsController does not match!");
            testControllers.shortcutsController.Received(1).Initialize(buildModeHUDView.shortcutsView);
            Assert.AreEqual(testControllers.publishPopupController, buildModeHUDView.controllers.publishPopupController, "The publishPopupController does not match!");
            testControllers.publishPopupController.Received(1).Initialize(buildModeHUDView.publishPopupView);
            Assert.AreEqual(testControllers.dragAndDropSceneObjectController, buildModeHUDView.controllers.dragAndDropSceneObjectController, "The dragAndDropSceneObjectController does not match!");
            testControllers.dragAndDropSceneObjectController.Received(1).Initialize(buildModeHUDView.dragAndDropSceneObjectView);
            Assert.AreEqual(testControllers.publishBtnController, buildModeHUDView.controllers.publishBtnController, "The publishBtnController does not match!");
            testControllers.publishBtnController.Received(1).Initialize(buildModeHUDView.publishBtnView, testControllers.tooltipController);
            Assert.AreEqual(testControllers.inspectorBtnController, buildModeHUDView.controllers.inspectorBtnController, "The inspectorBtnController does not match!");
            testControllers.inspectorBtnController.Received(1).Initialize(buildModeHUDView.inspectorBtnView, testControllers.tooltipController);
            Assert.AreEqual(testControllers.catalogBtnController, buildModeHUDView.controllers.catalogBtnController, "The catalogBtnController does not match!");
            testControllers.catalogBtnController.Received(1).Initialize(buildModeHUDView.catalogBtnView, testControllers.tooltipController);
            Assert.AreEqual(testControllers.inspectorController, buildModeHUDView.controllers.inspectorController, "The inspectorController does not match!");
            testControllers.inspectorController.Received(1).Initialize(buildModeHUDView.inspectorView);
            Assert.AreEqual(testControllers.topActionsButtonsController, buildModeHUDView.controllers.topActionsButtonsController, "The topActionsButtonsController does not match!");
            testControllers.topActionsButtonsController.Received(1).Initialize(buildModeHUDView.topActionsButtonsView, testControllers.tooltipController, testControllers.buildModeConfirmationModalController);
            Assert.AreEqual(testControllers.buildModeConfirmationModalController, buildModeHUDView.controllers.buildModeConfirmationModalController, "The buildModeConfirmationModalController does not match!");
            testControllers.buildModeConfirmationModalController.Received(1).Initialize(buildModeHUDView.buildModeConfirmationModalView);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPublishBtnAvailabilityCorrectly(bool isAvailable)
        {
            // Act
            buildModeHUDView.SetPublishBtnAvailability(isAvailable);

            // Assert
            testControllers.publishBtnController.Received(1).SetInteractable(isAvailable);
        }

        [Test]
        public void RefreshCatalogAssetPackCorrectly()
        {
            // Act
            buildModeHUDView.RefreshCatalogAssetPack();

            // Assert
            testControllers.sceneCatalogController.Received(1).RefreshAssetPack();
        }

        [Test]
        public void RefreshCatalogContentCorrectly()
        {
            // Act
            buildModeHUDView.RefreshCatalogContent();

            // Assert
            testControllers.sceneCatalogController.Received(1).RefreshCatalog();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfCatalogCorrectly(bool isVisible)
        {
            // Act
            buildModeHUDView.SetVisibilityOfCatalog(isVisible);

            // Assert
            if (isVisible)
                testControllers.sceneCatalogController.Received(1).OpenCatalog();
            else
                testControllers.sceneCatalogController.Received(1).CloseCatalog();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfSceneInfoCorrectly(bool isVisible)
        {
            // Act
            buildModeHUDView.SetVisibilityOfSceneInfo(isVisible);

            // Assert
            if (isVisible)
                testControllers.inspectorController.sceneLimitsController.Received(1).Enable();
            else
                testControllers.inspectorController.sceneLimitsController.Received(1).Disable();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfControlsCorrectly(bool isVisible)
        {
            // Act
            buildModeHUDView.SetVisibilityOfControls(isVisible);

            // Assert
            testControllers.shortcutsController.Received(1).SetActive(isVisible);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfExtraBtnsCorrectly(bool isVisible)
        {
            // Act
            buildModeHUDView.SetVisibilityOfExtraBtns(isVisible);

            // Assert
            testControllers.topActionsButtonsController.Received(1).SetExtraActionsActive(isVisible);
        }

        [Test]
        public void SetFirstPersonViewCorrectly()
        {
            // Arrange
            buildModeHUDView.firstPersonCanvasGO.SetActive(false);
            buildModeHUDView.godModeCanvasGO.SetActive(true);

            // Act
            buildModeHUDView.SetFirstPersonView();

            // Assert
            Assert.IsTrue(buildModeHUDView.firstPersonCanvasGO.activeSelf, "The firstPersonCanvasGO active property is false!");
            Assert.IsFalse(buildModeHUDView.godModeCanvasGO.activeSelf, "The godModeCanvasGO active property is true!");
        }

        [Test]
        public void HideToolTipCorrectly()
        {
            // Act
            buildModeHUDView.HideToolTip();

            // Assert
            testControllers.tooltipController.Received(1).HideTooltip();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            buildModeHUDView.gameObject.SetActive(!isActive);

            // Act
            buildModeHUDView.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, buildModeHUDView.gameObject.activeSelf, "The game object actove property does not match!");
        }
    }
}