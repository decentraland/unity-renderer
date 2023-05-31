using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Backpack
{
    public class BackpackPreviewPanelTests
    {
        private BackpackPreviewPanel backpackPreviewPanel;
        private ICharacterPreviewController characterPreviewController;
        private ICharacterPreviewFactory characterPreviewFactory;
        private IPreviewCameraRotationController previewCameraRotationController;
        private IPreviewCameraPanningController previewCameraPanningController;
        private IPreviewCameraZoomController previewCameraZoomController;

        [SetUp]
        public void SetUp()
        {
            backpackPreviewPanel = BaseComponentView.Create<BackpackPreviewPanel>("BackpackPreviewPanel");
            characterPreviewController = Substitute.For<ICharacterPreviewController>();
            characterPreviewFactory = Substitute.For<ICharacterPreviewFactory>();
            characterPreviewFactory.Configure().Create(
                                        Arg.Any<CharacterPreviewMode>(),
                                        Arg.Any<RenderTexture>(),
                                        Arg.Any<bool>(),
                                        Arg.Any<PreviewCameraFocus>(),
                                        Arg.Any<bool>())
                                   .Returns(characterPreviewController);
            previewCameraRotationController = Substitute.For<IPreviewCameraRotationController>();
            previewCameraPanningController = Substitute.For<IPreviewCameraPanningController>();
            previewCameraZoomController = Substitute.For<IPreviewCameraZoomController>();

            backpackPreviewPanel.Initialize(
                characterPreviewFactory,
                previewCameraRotationController,
                previewCameraPanningController,
                previewCameraZoomController);
        }

        [TearDown]
        public void TearDown()
        {
            backpackPreviewPanel.Dispose();
        }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            characterPreviewController.Received(1).SetCameraLimits(Arg.Any<Bounds>());
            characterPreviewController.Received(1).ConfigureZoom(1.4f, 1.1f, 0.3f);
            characterPreviewController.Received(1).SetFocus(PreviewCameraFocus.DefaultEditing);

            previewCameraRotationController.Received(1).Configure(
                backpackPreviewPanel.firstClickAction,
                backpackPreviewPanel.rotationFactor,
                backpackPreviewPanel.slowDownTime,
                backpackPreviewPanel.characterPreviewInputDetector,
                backpackPreviewPanel.rotateCursorTexture);

            previewCameraPanningController.Received(1).Configure(
                backpackPreviewPanel.secondClickAction,
                backpackPreviewPanel.middleClickAction,
                backpackPreviewPanel.panSpeed,
                backpackPreviewPanel.allowVerticalPanning,
                backpackPreviewPanel.allowHorizontalPanning,
                backpackPreviewPanel.panningInertiaDuration,
                backpackPreviewPanel.characterPreviewInputDetector,
                backpackPreviewPanel.panningCursorTexture);

            previewCameraZoomController.Received(1).Configure(
                backpackPreviewPanel.mouseWheelAction,
                backpackPreviewPanel.zoomSpeed,
                backpackPreviewPanel.smoothTime,
                backpackPreviewPanel.characterPreviewInputDetector);

        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPreviewEnabledCorrectly(bool isEnabled)
        {
            // Act
            backpackPreviewPanel.SetPreviewEnabled(isEnabled);

            // Assert
            characterPreviewController.Received(1).SetEnabled(isEnabled);
        }

        [Test]
        [TestCase(false, 0)]
        [TestCase(true, 1000)]
        public void PlayPreviewEmoteCorrectly(bool withTimestamp, long timestamp)
        {
            // Arrange
            var testEmoteId = "testEmote";

            // Act
            if (withTimestamp)
                backpackPreviewPanel.PlayPreviewEmote(testEmoteId, timestamp);
            else
                backpackPreviewPanel.PlayPreviewEmote(testEmoteId);

            // Assert
            characterPreviewController.Received(1).PlayEmote(testEmoteId, withTimestamp ? timestamp : (long)Time.realtimeSinceStartup);
        }

        [Test]
        public void ResetPreviewEmoteCorrectly()
        {
            // Act
            backpackPreviewPanel.ResetPreviewEmote();

            // Assert
            characterPreviewController.Received(1).PlayEmote("Idle", (long)Time.realtimeSinceStartup);
        }

        [Test]
        public void ResetPreviewRotationCorrectly()
        {
            // Act
            backpackPreviewPanel.ResetPreviewRotation();

            // Assert
            characterPreviewController.Received(1).ResetRotation();
        }

        [UnityTest]
        public IEnumerator TryUpdatePreviewModelCorrectly()
        {
            // Arrange
            AvatarModel testAvatarModel = new AvatarModel();

            // Act
            yield return backpackPreviewPanel.TryUpdatePreviewModelAsync(testAvatarModel, CancellationToken.None);

            // Assert
            characterPreviewController.Received(1).TryUpdateModelAsync(testAvatarModel, CancellationToken.None);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetLoadingActiveCorrectly(bool isActive)
        {
            // Act
            backpackPreviewPanel.SetLoadingActive(isActive);

            // Assert
            Assert.AreEqual(isActive, backpackPreviewPanel.avatarPreviewLoadingSpinner.activeSelf);
        }

        [Test]
        public void TakeSnapshotsCorrectly()
        {
            // Act
            backpackPreviewPanel.TakeSnapshots(null, null);

            // Assert
            characterPreviewController.Received(1).TakeSnapshots(
                Arg.Any<CharacterPreviewController.OnSnapshotsReady>(),
                Arg.Any<Action>());
        }

        [Test]
        [TestCase(PreviewCameraFocus.DefaultEditing, true)]
        [TestCase(PreviewCameraFocus.FaceEditing, false)]
        public void SetFocusCorrectly(PreviewCameraFocus focus, bool useTransition)
        {
            // Act
            backpackPreviewPanel.SetFocus(focus, useTransition);

            // Assert
            characterPreviewController.Received().SetFocus(focus, useTransition);
        }

        [Test]
        [TestCase(1.2f)]
        [TestCase(5f)]
        public void RotateCorrectly(float angularVelocity)
        {
            // Act
            previewCameraRotationController.OnHorizontalRotation += Raise.Event<Action<float>>(angularVelocity);

            // Assert
            characterPreviewController.Received(1).Rotate(angularVelocity);
        }

        [Test]
        public void PanCorrectly()
        {
            // Arrange
            Vector3 testPositionDelta = new Vector3(4, 5, 7);

            // Act
            previewCameraPanningController.OnPanning += Raise.Event<Action<Vector3>>(testPositionDelta);

            // Assert
            characterPreviewController.Received(1).MoveCamera(testPositionDelta, true);
        }

        [Test]
        public void ZoomCorrectly()
        {
            // Arrange
            Vector3 testPositionDelta = new Vector3(4, 5, 7);

            // Act
            previewCameraZoomController.OnZoom += Raise.Event<Action<Vector3>>(testPositionDelta);

            // Assert
            characterPreviewController.Received(1).MoveCamera(testPositionDelta, true);
        }
    }
}
