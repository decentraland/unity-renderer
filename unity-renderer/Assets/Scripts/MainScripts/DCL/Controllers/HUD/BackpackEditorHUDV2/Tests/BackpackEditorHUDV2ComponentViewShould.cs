using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDV2ComponentViewShould
    {
        private BackpackEditorHUDV2ComponentView backpackEditorHUDV2ComponentView;

        [SetUp]
        public void SetUp()
        {
            backpackEditorHUDV2ComponentView = BackpackEditorHUDV2ComponentView.Create();
            backpackEditorHUDV2ComponentView.Initialize(Substitute.For<ICharacterPreviewFactory>());
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
    }
}
