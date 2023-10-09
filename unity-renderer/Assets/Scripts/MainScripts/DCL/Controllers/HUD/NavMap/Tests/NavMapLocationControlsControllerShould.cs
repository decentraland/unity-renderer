using DCL;
using DCL.Helpers;
using DCLServices.MapRendererV2.MapCameraController;
using ExploreV2Analytics;
using NSubstitute;
using NUnit.Framework;
using System;
using UnityEngine;

namespace HUD.NavMap.Tests
{
    public class NavMapLocationControlsControllerShould
    {
        private NavMapLocationControlsController controller;
        private INavMapLocationControlsView view;
        private INavmapZoomViewController zoomViewController;
        private INavmapToastViewController toastViewController;
        private IMapCameraController cameraController;
        private IExploreV2Analytics exploreV2Analytics;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<INavMapLocationControlsView>();
            zoomViewController = Substitute.For<INavmapZoomViewController>();
            toastViewController = Substitute.For<INavmapToastViewController>();
            cameraController = Substitute.For<IMapCameraController>();
            exploreV2Analytics = Substitute.For<IExploreV2Analytics>();
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void Activate_SetsUpListeners()
        {
            controller = new NavMapLocationControlsController(view, exploreV2Analytics, zoomViewController, toastViewController, new BaseVariable<Vector2Int>(), new BaseVariable<Vector3>());

            controller.Activate(cameraController);

            view.HomeButtonClicked += Raise.Event<Action>();
            toastViewController.Received().CloseCurrentToast();

            view.CenterToPlayerButtonClicked += Raise.Event<Action>();
            toastViewController.Received(2).CloseCurrentToast();
        }

        [Test]
        public void Deactivate_RemovesListeners()
        {
            controller = new NavMapLocationControlsController(view, exploreV2Analytics, zoomViewController, toastViewController, new BaseVariable<Vector2Int>(), new BaseVariable<Vector3>());

            controller.Activate(cameraController);
            controller.Deactivate();

            view.HomeButtonClicked += Raise.Event<Action>();
            toastViewController.DidNotReceive().CloseCurrentToast();

            view.CenterToPlayerButtonClicked += Raise.Event<Action>();
            toastViewController.DidNotReceive().CloseCurrentToast();
        }

        [Test]
        public void Hide_DeactivatesAndHidesView()
        {
            controller = new NavMapLocationControlsController(view, exploreV2Analytics, zoomViewController, toastViewController, new BaseVariable<Vector2Int>(), new BaseVariable<Vector3>());

            controller.Hide();
            view.Received().Hide();
        }

        [Test]
        public void InjectHomeAndPlayerLocations()
        {
            // Arrange
            Vector2Int homeLocation = Vector2Int.one;
            Vector3 playerPosition = Vector3.zero;

            // Act
            controller = new NavMapLocationControlsController(view, exploreV2Analytics, zoomViewController, toastViewController, new BaseVariable<Vector2Int>(homeLocation), new BaseVariable<Vector3>(playerPosition));

            // Assert
            Assert.That(controller.homePoint.Get(), Is.EqualTo(homeLocation));
            Assert.That(controller.playerPlayerWorldPosition.Get(), Is.EqualTo(playerPosition));
        }

        [Test]
        public void FocusOnHomeLocation_CallsExpectedMethods()
        {
            // Arrange
            Vector2Int homeLocation = Vector2Int.one;
            Vector3 playerPosition = Vector3.zero;
            controller = new NavMapLocationControlsController(view, exploreV2Analytics, zoomViewController, toastViewController, new BaseVariable<Vector2Int>(homeLocation), new BaseVariable<Vector3>(playerPosition));
            controller.Activate(cameraController);

            // Act
            view.HomeButtonClicked += Raise.Event<Action>();

            // Assert
            cameraController.Received()
                            .TranslateTo(Arg.Any<Vector2>(),
                                 Arg.Is(NavMapLocationControlsController.TRANSLATION_DURATION), Arg.Any<Action>());
        }

        [Test]
        public void CenterToPlayerLocation_CallsExpectedMethods()
        {
            // Arrange
            Vector2Int homeLocation = Vector2Int.one;
            Vector3 playerPosition = Vector3.zero;
            controller = new NavMapLocationControlsController(view, exploreV2Analytics, zoomViewController, toastViewController, new BaseVariable<Vector2Int>(homeLocation), new BaseVariable<Vector3>(playerPosition));
            controller.Activate(cameraController);

            // Act
            view.CenterToPlayerButtonClicked += Raise.Event<Action>();

            // Assert
            cameraController.Received()
                            .TranslateTo(
                                 Arg.Any<Vector2>(),
                                 Arg.Is(NavMapLocationControlsController.TRANSLATION_DURATION));
        }
    }
}
