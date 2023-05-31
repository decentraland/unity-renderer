using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DCL.Backpack
{
    public class BackpackEditorHUDV2ComponentViewShould
    {
        private BackpackEditorHUDV2ComponentView backpackEditorHUDV2ComponentView;
        private ICharacterPreviewFactory characterPreviewFactory;
        private ICharacterPreviewController characterPreviewController;

        [SetUp]
        public void SetUp()
        {
            characterPreviewController = Substitute.For<ICharacterPreviewController>();
            characterPreviewFactory = Substitute.For<ICharacterPreviewFactory>();
            characterPreviewFactory.Configure().Create(
                Arg.Any<CharacterPreviewMode>(),
                Arg.Any<RenderTexture>(),
                Arg.Any<bool>(),
                Arg.Any<PreviewCameraFocus>(),
                Arg.Any<bool>()).Returns(characterPreviewController);


            backpackEditorHUDV2ComponentView = BackpackEditorHUDV2ComponentView.Create();
            backpackEditorHUDV2ComponentView.Initialize(
                characterPreviewFactory,
                Substitute.For<IPreviewCameraRotationController>(),
                Substitute.For<IPreviewCameraPanningController>(),
                Substitute.For<IPreviewCameraZoomController>());
        }

        [TearDown]
        public void TearDown()
        {
            backpackEditorHUDV2ComponentView.Dispose();
            Object.Destroy(backpackEditorHUDV2ComponentView.gameObject);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void OpenCloseWearablesSectionCorrectly(bool isOpen)
        {
            // Act
            backpackEditorHUDV2ComponentView.sectionSelector.GetSection(0).onSelect.Invoke(isOpen);

            // Assert
            Assert.AreEqual(isOpen, backpackEditorHUDV2ComponentView.wearablesSection.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void OpenCloseEmotesSectionCorrectly(bool isOpen)
        {
            // Act
            backpackEditorHUDV2ComponentView.sectionSelector.GetSection(1).onSelect.Invoke(isOpen);

            // Assert
            Assert.AreEqual(isOpen, backpackEditorHUDV2ComponentView.emotesSection.activeSelf);
        }

        [Test]
        public void ShowCorrectly()
        {
            // Arrange
            backpackEditorHUDV2ComponentView.gameObject.SetActive(false);

            // Act
            backpackEditorHUDV2ComponentView.Show();

            // Assert
            Assert.IsTrue(backpackEditorHUDV2ComponentView.gameObject.activeSelf);
            characterPreviewController.Received(1).SetEnabled(true);
        }

        [Test]
        public void HideCorrectly()
        {
            // Arrange
            backpackEditorHUDV2ComponentView.gameObject.SetActive(true);

            // Act
            backpackEditorHUDV2ComponentView.Hide();

            // Assert
            Assert.IsFalse(backpackEditorHUDV2ComponentView.gameObject.activeSelf);
            characterPreviewController.Received(1).SetEnabled(false);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetColorPickerVisibilityCorrectly(bool isVisible)
        {
            // Arrange
            backpackEditorHUDV2ComponentView.colorPickerComponentView.gameObject.SetActive(!isVisible);

            // Act
            backpackEditorHUDV2ComponentView.SetColorPickerVisibility(isVisible);

            // Assert
            Assert.AreEqual(isVisible, backpackEditorHUDV2ComponentView.colorPickerComponentView.gameObject.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetColorPickerAsSkinModeCorrectly(bool isSkinMode)
        {
            // Act
            backpackEditorHUDV2ComponentView.SetColorPickerAsSkinMode(isSkinMode);

            // Assert
            Assert.AreEqual(isSkinMode, backpackEditorHUDV2ComponentView.colorPickerComponentView.IsShowingOnlyPresetColors);
            Assert.AreEqual(isSkinMode ?
                backpackEditorHUDV2ComponentView.skinColorPresetsSO.colors :
                backpackEditorHUDV2ComponentView.colorPresetsSO.colors,
                backpackEditorHUDV2ComponentView.colorPickerComponentView.ColorList);
        }

        [Test]
        public void SetColorPickerValueCorrectly()
        {
            // Arrange
            Color testColor = Color.red;

            // Act
            backpackEditorHUDV2ComponentView.SetColorPickerValue(testColor);

            // Assert
            Assert.AreEqual(testColor, backpackEditorHUDV2ComponentView.colorPickerComponentView.CurrentSelectedColor);
        }

        [Test]
        [TestCase(null)]
        [TestCase(long.MaxValue)]
        public void PlayPreviewEmoteCorrectly(long? timestamp)
        {
            // Act
            if (timestamp == null)
                backpackEditorHUDV2ComponentView.PlayPreviewEmote("test");
            else
                backpackEditorHUDV2ComponentView.PlayPreviewEmote("test", timestamp.Value);

            // Assert
            characterPreviewController.Received(1).PlayEmote("test", timestamp ?? (long)Time.realtimeSinceStartup);
        }

        [Test]
        public void ResetPreviewPanelCorrectly()
        {
            // Act
            backpackEditorHUDV2ComponentView.ResetPreviewPanel();

            // Assert
            characterPreviewController.Received(1).PlayEmote("Idle", (long)Time.realtimeSinceStartup);
            characterPreviewController.Received(1).ResetRotation();
            characterPreviewController.Received(1).SetFocus(PreviewCameraFocus.DefaultEditing, false);
        }

        [UnityTest]
        public IEnumerator UpdateAvatarPreviewCorrectly()
        {
            // Arrange
            AvatarModel testAvatarModel = new AvatarModel { wearables = new List<string> { "testWearable" },};

            // Act
            backpackEditorHUDV2ComponentView.UpdateAvatarPreview(testAvatarModel);
            yield return null;

            // Assert
            characterPreviewController.Received(1).TryUpdateModelAsync(
                Arg.Is<AvatarModel>(avatarModel => avatarModel.wearables.Contains("testWearable")),
                Arg.Any<CancellationToken>());
        }

        [Test]
        [TestCase(PreviewCameraFocus.DefaultEditing, true)]
        [TestCase(PreviewCameraFocus.FaceEditing, false)]
        public void SetAvatarPreviewFocusCorrectly(PreviewCameraFocus focus, bool useTransition)
        {
            // Act
            backpackEditorHUDV2ComponentView.SetAvatarPreviewFocus(focus, useTransition);

            // Assert
            characterPreviewController.Received().SetFocus(focus, useTransition);
        }
    }
}
