using NUnit.Framework;
using UnityEngine;

namespace DCL.EmotesCustomization.Tests
{
    public class EmoteSlotCardComponentViewTests
    {
        private EmoteSlotCardComponentView emoteSlotCardComponent;
        private Texture2D testTexture;
        private Sprite testSprite;

        [SetUp]
        public void SetUp()
        {
            emoteSlotCardComponent = BaseComponentView.Create<EmoteSlotCardComponentView>("EmotesCustomization/EmoteSlotCard");
            testTexture = new Texture2D(20, 20);
            testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
        }

        [TearDown]
        public void TearDown()
        {
            emoteSlotCardComponent.Dispose();
            GameObject.Destroy(testTexture);
            GameObject.Destroy(testSprite);
        }

        [Test]
        public void ConfigureEmoteCardCorrectly()
        {
            // Arrange
            EmoteSlotCardComponentModel testModel = new EmoteSlotCardComponentModel
            {
                emoteId = "TestId",
                emoteName = "Test Name",
                hasSeparator = true,
                isSelected = false,
                pictureSprite = testSprite,
                pictureUri = "",
                rarity = "epic",
                slotNumber = 2
            };

            // Act
            emoteSlotCardComponent.Configure(testModel);

            // Assert
            Assert.AreEqual(testModel, emoteSlotCardComponent.model, "The model does not match after configuring the button.");
        }

        [Test]
        public void RaiseOnFocusCorrectly()
        {
            // Arrange
            emoteSlotCardComponent.model.isSelected = false;
            emoteSlotCardComponent.defaultBackgroundImage.color = Color.black;
            emoteSlotCardComponent.slotNumberText.color = Color.black;
            emoteSlotCardComponent.emoteNameText.color = Color.black;
            emoteSlotCardComponent.slotViewerImage.gameObject.SetActive(true);

            // Act
            emoteSlotCardComponent.OnFocus();

            // Assert
            SetSelectedVisualsForHoveringAssert(true);
        }

        [Test]
        public void RaiseOnLoseFocusCorrectly()
        {
            // Arrange
            emoteSlotCardComponent.model.isSelected = false;
            emoteSlotCardComponent.defaultBackgroundImage.color = Color.black;
            emoteSlotCardComponent.slotNumberText.color = Color.black;
            emoteSlotCardComponent.emoteNameText.color = Color.black;
            emoteSlotCardComponent.slotViewerImage.gameObject.SetActive(false);

            // Act
            emoteSlotCardComponent.OnLoseFocus();

            // Assert
            SetSelectedVisualsForHoveringAssert(false);
        }

        [Test]
        [TestCase("TestId")]
        [TestCase("")]
        public void SetEmoteIdCorrectly(string testId)
        {
            // Arrange
            emoteSlotCardComponent.model.emoteId = testId;

            // Act
            emoteSlotCardComponent.SetEmoteId(testId);

            // Assert
            Assert.AreEqual(testId, emoteSlotCardComponent.model.emoteId, "The id does not match in the model.");
            Assert.AreEqual(string.IsNullOrEmpty(testId), emoteSlotCardComponent.nonEmoteImage.gameObject.activeSelf);
            Assert.AreEqual(!string.IsNullOrEmpty(testId), emoteSlotCardComponent.emoteImage.gameObject.activeSelf);
        }

        [Test]
        [TestCase("Test Name")]
        [TestCase("")]
        public void SetEmoteNameCorrectly(string testName)
        {
            // Act
            emoteSlotCardComponent.SetEmoteName(testName);

            // Assert
            Assert.AreEqual(testName, emoteSlotCardComponent.model.emoteName, "The name does not match in the model.");
            Assert.AreEqual(string.IsNullOrEmpty(testName) ? EmoteSlotCardComponentView.EMPTY_SLOT_TEXT : testName, emoteSlotCardComponent.emoteNameText.text, "The name text does not match.");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetEmotePictureWithSpriteCorrectly(bool useNullSprite)
        {
            // Act
            emoteSlotCardComponent.SetEmotePicture(useNullSprite ? null : testSprite);

            // Assert
            if (useNullSprite)
                Assert.AreEqual(emoteSlotCardComponent.defaultEmotePicture, emoteSlotCardComponent.model.pictureSprite, "The picture sprite does not match in the model.");
            else
                Assert.AreEqual(testSprite, emoteSlotCardComponent.model.pictureSprite, "The picture sprite does not match in the model.");
        }

        [Test]
        [TestCase("")]
        [TestCase("TestUri")]
        public void SetEmotePictureWithUriCorrectly(string testUri)
        {
            // Act
            emoteSlotCardComponent.SetEmotePicture(testUri);

            // Assert
            if (string.IsNullOrEmpty(testUri))
                Assert.AreEqual(emoteSlotCardComponent.defaultEmotePicture, emoteSlotCardComponent.model.pictureSprite, "The picture sprite does not match in the model.");
            else
                Assert.AreEqual(testUri, emoteSlotCardComponent.model.pictureUri, "The picture uri does not match in the model.");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetEmoteAsSelectedCorrectly(bool isSelected)
        {
            // Arrange
            emoteSlotCardComponent.model.isSelected = !isSelected;

            // Act
            emoteSlotCardComponent.SetEmoteAsSelected(isSelected);

            // Assert
            Assert.AreEqual(isSelected, emoteSlotCardComponent.model.isSelected, "The isSelected does not match in the model.");
            SetSelectedVisualsForClickingAssert(isSelected);
        }

        [Test]
        public void SetSlotNumberCorrectly()
        {
            // Arrange
            int testSlotNumber = 5;
            emoteSlotCardComponent.model.slotNumber = 0;

            // Act
            emoteSlotCardComponent.SetSlotNumber(testSlotNumber);

            // Assert
            Assert.AreEqual(testSlotNumber, emoteSlotCardComponent.model.slotNumber, "The slotNumber does not match in the model.");
            Assert.AreEqual(testSlotNumber.ToString(), emoteSlotCardComponent.slotNumberText.text);
            Assert.AreEqual(Quaternion.Euler(0, 0, -testSlotNumber * EmoteSlotCardComponentView.SLOT_VIEWER_ROTATION_ANGLE), emoteSlotCardComponent.slotViewerImage.transform.rotation);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetSeparatorActiveCorrectly(bool isActive)
        {
            // Arrange
            emoteSlotCardComponent.model.hasSeparator = !isActive;
            emoteSlotCardComponent.separatorGO.SetActive(!isActive);

            // Act
            emoteSlotCardComponent.SetSeparatorActive(isActive);

            // Assert
            Assert.AreEqual(isActive, emoteSlotCardComponent.model.hasSeparator, "The hasSeparator does not match in the model.");
            Assert.AreEqual(isActive, emoteSlotCardComponent.separatorGO.activeSelf);
        }

        [Test]
        [TestCase("epic")]
        [TestCase("non-exist-rarity")]
        public void SetRarityCorrectly(string testRarity)
        {
            // Act
            emoteSlotCardComponent.SetRarity(testRarity);

            // Assert
            Assert.AreEqual(testRarity, emoteSlotCardComponent.model.rarity, "The rarity does not match in the model.");
            Assert.AreEqual(testRarity != "non-exist-rarity", emoteSlotCardComponent.rarityMark.gameObject.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RaiseOnEmoteImageLoadedCorrectly(bool useNullSprite)
        {
            // Act
            emoteSlotCardComponent.OnEmoteImageLoaded(useNullSprite ? null : testSprite);

            // Assert
            if (useNullSprite)
                Assert.AreEqual(emoteSlotCardComponent.defaultEmotePicture, emoteSlotCardComponent.model.pictureSprite, "The pciture sprite does not match in the model.");
            else
                Assert.AreEqual(testSprite, emoteSlotCardComponent.model.pictureSprite, "The pciture sprite does not match in the model.");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetSelectedVisualsForClickingCorrectly(bool isSelected)
        {
            // Arrange
            emoteSlotCardComponent.defaultBackgroundImage.gameObject.SetActive(isSelected);
            emoteSlotCardComponent.defaultBackgroundImage.color = Color.black;
            emoteSlotCardComponent.selectedBackgroundImage.gameObject.SetActive(!isSelected);
            emoteSlotCardComponent.selectedBackgroundImage.color = Color.black;
            emoteSlotCardComponent.slotNumberText.color = Color.black;
            emoteSlotCardComponent.emoteNameText.color = Color.black;
            emoteSlotCardComponent.slotViewerImage.gameObject.SetActive(!isSelected);

            // Act
            emoteSlotCardComponent.SetSelectedVisualsForClicking(isSelected);

            // Assert
            SetSelectedVisualsForClickingAssert(isSelected);
        }

        private void SetSelectedVisualsForClickingAssert(bool isSelected)
        {
            Assert.AreEqual(!isSelected, emoteSlotCardComponent.defaultBackgroundImage.gameObject.activeSelf);
            Assert.AreEqual(emoteSlotCardComponent.defaultBackgroundColor, emoteSlotCardComponent.defaultBackgroundImage.color);
            Assert.AreEqual(isSelected, emoteSlotCardComponent.selectedBackgroundImage.gameObject.activeSelf);
            Assert.AreEqual(emoteSlotCardComponent.selectedBackgroundColor, emoteSlotCardComponent.selectedBackgroundImage.color);
            Assert.AreEqual(isSelected ? emoteSlotCardComponent.selectedSlotNumberColor : emoteSlotCardComponent.defaultSlotNumberColor, emoteSlotCardComponent.slotNumberText.color);
            Assert.AreEqual(isSelected ? emoteSlotCardComponent.selectedEmoteNameColor : emoteSlotCardComponent.defaultEmoteNameColor, emoteSlotCardComponent.emoteNameText.color);
            Assert.AreEqual(isSelected, emoteSlotCardComponent.slotViewerImage.gameObject.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetSelectedVisualsForHoveringCorrectly(bool isSelected)
        {
            // Arrange
            emoteSlotCardComponent.defaultBackgroundImage.color = Color.black;
            emoteSlotCardComponent.slotNumberText.color = Color.black;
            emoteSlotCardComponent.emoteNameText.color = Color.black;
            emoteSlotCardComponent.slotViewerImage.gameObject.SetActive(!isSelected);

            // Act
            emoteSlotCardComponent.SetSelectedVisualsForHovering(isSelected);

            // Assert
            SetSelectedVisualsForHoveringAssert(isSelected);
        }

        private void SetSelectedVisualsForHoveringAssert(bool isSelected)
        {
            Assert.AreEqual(isSelected ? emoteSlotCardComponent.selectedBackgroundColor : emoteSlotCardComponent.defaultBackgroundColor, emoteSlotCardComponent.defaultBackgroundImage.color);
            Assert.AreEqual(isSelected ? emoteSlotCardComponent.selectedSlotNumberColor : emoteSlotCardComponent.defaultSlotNumberColor, emoteSlotCardComponent.slotNumberText.color);
            Assert.AreEqual(isSelected ? emoteSlotCardComponent.selectedEmoteNameColor : emoteSlotCardComponent.defaultEmoteNameColor, emoteSlotCardComponent.emoteNameText.color);
            Assert.AreEqual(isSelected, emoteSlotCardComponent.slotViewerImage.gameObject.activeSelf);
        }
    }
}