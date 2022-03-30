using NUnit.Framework;
using UnityEngine;

namespace DCL.EmotesCustomization.Tests
{
    public class EmoteCardComponentViewTests
    {
        private EmoteCardComponentView emoteCardComponent;
        private Texture2D testTexture;
        private Sprite testSprite;

        [SetUp]
        public void SetUp()
        {
            emoteCardComponent = BaseComponentView.Create<EmoteCardComponentView>("EmotesCustomization/EmoteCard");
            testTexture = new Texture2D(20, 20);
            testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
        }

        [TearDown]
        public void TearDown()
        {
            emoteCardComponent.Dispose();
            GameObject.Destroy(testTexture);
            GameObject.Destroy(testSprite);
        }

        [Test]
        public void ConfigureEmoteCardCorrectly()
        {
            // Arrange
            EmoteCardComponentModel testModel = new EmoteCardComponentModel
            {
                assignedSlot = -1,
                description = "Test description",
                id = "TestId",
                isAssignedInSelectedSlot = false,
                isCollectible = false,
                isInL2 = false,
                isLoading = false,
                isSelected = false,
                name = "Test Name",
                pictureSprite = testSprite,
                pictureUri = "",
                rarity = "epic"
            };

            // Act
            emoteCardComponent.Configure(testModel);

            // Assert
            Assert.AreEqual(testModel, emoteCardComponent.model, "The model does not match after configuring the button.");
        }

        [Test]
        public void RaiseOnFocusCorrectly()
        {
            // Arrange
            emoteCardComponent.emoteNameContainer.SetActive(false);

            // Act
            emoteCardComponent.OnFocus();

            // Assert
            Assert.IsTrue(emoteCardComponent.emoteNameContainer.activeSelf);
        }

        [Test]
        public void RaiseOnLoseFocusCorrectly()
        {
            // Arrange
            emoteCardComponent.emoteNameContainer.SetActive(true);

            // Act
            emoteCardComponent.OnLoseFocus();

            // Assert
            Assert.IsFalse(emoteCardComponent.emoteNameContainer.activeSelf);
        }

        [Test]
        public void SetEmoteIdCorrectly()
        {
            // Arrange
            string testId = "TestId";

            // Act
            emoteCardComponent.SetEmoteId(testId);

            // Assert
            Assert.AreEqual(testId, emoteCardComponent.model.id, "The id does not match in the model.");
        }

        [Test]
        public void SetEmoteNameCorrectly()
        {
            // Arrange
            string testName = "Test Name";

            // Act
            emoteCardComponent.SetEmoteName(testName);

            // Assert
            Assert.AreEqual(testName, emoteCardComponent.model.name, "The name does not match in the model.");
            Assert.AreEqual(testName, emoteCardComponent.emoteNameText.text, "The name text does not match.");
        }

        [Test]
        public void SetEmoteDescriptionCorrectly()
        {
            // Arrange
            string testDesc = "Test Description";

            // Act
            emoteCardComponent.SetEmoteDescription(testDesc);

            // Assert
            Assert.AreEqual(testDesc, emoteCardComponent.model.description, "The description does not match in the model.");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetEmotePictureWithSpriteCorrectly(bool useNullSprite)
        {
            // Act
            emoteCardComponent.SetEmotePicture(useNullSprite ? null : testSprite);

            // Assert
            if (useNullSprite)
                Assert.AreEqual(emoteCardComponent.defaultEmotePicture, emoteCardComponent.model.pictureSprite, "The picture sprite does not match in the model.");
            else
                Assert.AreEqual(testSprite, emoteCardComponent.model.pictureSprite, "The picture sprite does not match in the model.");
        }

        [Test]
        [TestCase("")]
        [TestCase("TestUri")]
        public void SetEmotePictureWithUriCorrectly(string testUri)
        {
            // Act
            emoteCardComponent.SetEmotePicture(testUri);

            // Assert
            if (string.IsNullOrEmpty(testUri))
                Assert.AreEqual(emoteCardComponent.defaultEmotePicture, emoteCardComponent.model.pictureSprite, "The picture sprite does not match in the model.");
            else
                Assert.AreEqual(testUri, emoteCardComponent.model.pictureUri, "The picture uri does not match in the model.");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetEmoteAsAssignedInSelectedSlotCorrectly(bool isAssigned)
        {
            // Act
            emoteCardComponent.SetEmoteAsAssignedInSelectedSlot(isAssigned);

            // Assert
            Assert.AreEqual(isAssigned, emoteCardComponent.model.isAssignedInSelectedSlot, "The isAssignedInSelectedSlot does not match in the model.");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void AssignSlotCorrectly(int slotNumber)
        {
            // Act
            emoteCardComponent.AssignSlot(slotNumber);

            // Assert
            Assert.AreEqual(slotNumber, emoteCardComponent.model.assignedSlot, "The assignedSlot does not match in the model.");
            Assert.AreEqual(slotNumber.ToString(), emoteCardComponent.assignedSlotNumberText.text, "The name text does not match.");
        }

        [Test]
        public void UnassignSlotCorrectly()
        {
            // Arrange
            emoteCardComponent.AssignSlot(1);

            // Act
            emoteCardComponent.UnassignSlot();

            // Assert
            Assert.AreEqual(-1, emoteCardComponent.model.assignedSlot, "The assignedSlot does not match in the model.");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetEmoteAsSelectedCorrectly(bool isSelected)
        {
            // Arrange
            string testId = "TestId";
            string receivedTestId = "...";
            emoteCardComponent.model.id = testId;
            emoteCardComponent.onEmoteSelected += (id) => receivedTestId = id;

            // Act
            emoteCardComponent.SetEmoteAsSelected(isSelected);

            // Assert
            Assert.AreEqual(isSelected, emoteCardComponent.model.isSelected, "The isSelected does not match in the model.");

            if (isSelected)
                Assert.AreEqual(testId, receivedTestId);
            else
                Assert.IsNull(receivedTestId);
        }

        [Test]
        [TestCase("epic")]
        [TestCase("non-exist-rarity")]
        public void SetRarityCorrectly(string testRarity)
        {
            // Act
            emoteCardComponent.SetRarity(testRarity);

            // Assert
            Assert.AreEqual(testRarity, emoteCardComponent.model.rarity, "The rarity does not match in the model.");
            Assert.AreEqual(testRarity != "non-exist-rarity", emoteCardComponent.rarityMark.gameObject.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetIsInL2Correctly(bool isInL2)
        {
            // Act
            emoteCardComponent.SetIsInL2(isInL2);

            // Assert
            Assert.AreEqual(isInL2, emoteCardComponent.model.isInL2, "The isInL2 does not match in the model.");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetIsCollectibleCorrectly(bool isCollectible)
        {
            // Act
            emoteCardComponent.SetIsCollectible(isCollectible);

            // Assert
            Assert.AreEqual(isCollectible, emoteCardComponent.model.isCollectible, "The isCollectible does not match in the model.");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetAsLoadingCorrectly(bool isloading)
        {
            // Act
            emoteCardComponent.SetAsLoading(isloading);

            // Assert
            Assert.AreEqual(isloading, emoteCardComponent.model.isLoading, "The isCollectible does not match in the model.");
            Assert.AreEqual(isloading, emoteCardComponent.loadingSpinnerGO.activeSelf, "The loadingSpinnerGO active property does not match.");
            Assert.AreEqual(!isloading, emoteCardComponent.mainButton.gameObject.activeSelf, "The mainButton active property does not match.");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RefreshAssignedSlotTextVisibilityCorrectly(bool hasSlotAssigned)
        {
            // Arrange
            emoteCardComponent.assignedSlotNumberText.gameObject.SetActive(!hasSlotAssigned);
            emoteCardComponent.model.assignedSlot = hasSlotAssigned ? 0 : -1;

            // Act
            emoteCardComponent.RefreshAssignedSlotTextVisibility();

            // Assert
            Assert.AreEqual(hasSlotAssigned, emoteCardComponent.assignedSlotNumberText.gameObject.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RefreshSelectionFrameVisibilityCorrectly(bool isSelected)
        {
            // Arrange
            emoteCardComponent.cardSelectionFrame.SetActive(!isSelected);
            emoteCardComponent.model.isSelected = isSelected;

            // Act
            emoteCardComponent.RefreshSelectionFrameVisibility();

            // Assert
            Assert.AreEqual(isSelected, emoteCardComponent.cardSelectionFrame.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RefreshCardButtonsVisibilityCorrectly(bool isCollectible)
        {
            // Arrange
            emoteCardComponent.infoButton.gameObject.SetActive(!isCollectible);
            emoteCardComponent.model.isCollectible = isCollectible;

            // Act
            emoteCardComponent.RefreshCardButtonsVisibility();

            // Assert
            Assert.AreEqual(isCollectible, emoteCardComponent.infoButton.gameObject.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RaiseOnEmoteImageLoadedCorrectly(bool useNullSprite)
        {
            // Act
            emoteCardComponent.OnEmoteImageLoaded(useNullSprite ? null : testSprite);

            // Assert
            if (useNullSprite)
                Assert.AreEqual(emoteCardComponent.defaultEmotePicture, emoteCardComponent.model.pictureSprite, "The pciture sprite does not match in the model.");
            else
                Assert.AreEqual(testSprite, emoteCardComponent.model.pictureSprite, "The pciture sprite does not match in the model.");
        }
    }
}