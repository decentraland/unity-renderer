using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.EmotesCustomization.Tests
{
    public class EmoteSlotSelectorComponentViewTests
    {
        private EmoteSlotSelectorComponentView emoteSlotSelectorComponent;
        private Texture2D testTexture;
        private Sprite testSprite;

        [SetUp]
        public void SetUp()
        {
            emoteSlotSelectorComponent = BaseComponentView.Create<EmoteSlotSelectorComponentView>("EmotesCustomization/EmoteSlotSelector");
            testTexture = new Texture2D(20, 20);
            testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
        }

        [TearDown]
        public void TearDown()
        {
            emoteSlotSelectorComponent.Dispose();
            GameObject.Destroy(testTexture);
            GameObject.Destroy(testSprite);
        }

        [Test]
        public void ConfigureEmoteSlotSelectorCorrectly()
        {
            // Arrange
            EmoteSlotSelectorComponentModel testModel = new EmoteSlotSelectorComponentModel
            {
                selectedSlot = 0
            };

            // Act
            emoteSlotSelectorComponent.Configure(testModel);

            // Assert
            Assert.AreEqual(testModel, emoteSlotSelectorComponent.model, "The model does not match after configuring the button.");
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(20)]
        public void SelectSlotCorrectly(int slotNumber)
        {
            // Arrange
            List<EmoteSlotCardComponentView> currentSlots = emoteSlotSelectorComponent.GetAllSlots();

            int slotNumberReceived = -1;
            string emoteIdReceived = "";
            emoteSlotSelectorComponent.onSlotSelected += (slotNumber, emoteId) =>
            {
                slotNumberReceived = slotNumber;
                emoteIdReceived = emoteId;
            };

            // Act
            emoteSlotSelectorComponent.SelectSlot(slotNumber);

            // Assert
            if (slotNumber <= 9)
            {
                Assert.IsTrue(currentSlots[slotNumber - 1].model.isSelected);
                Assert.AreEqual(slotNumber, slotNumberReceived);
                Assert.AreEqual(currentSlots[slotNumber - 1].model.emoteId, emoteIdReceived);
            }
            else
            {
                Assert.AreEqual(-1, slotNumberReceived);
                Assert.AreEqual("", emoteIdReceived);
            }
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public void AssignEmoteIntoSlotCorrectly(int slotNumber)
        {
            // Arrange
            List<EmoteSlotCardComponentView> currentSlots = emoteSlotSelectorComponent.GetAllSlots();
            string testEmoteId = "TestId";
            string testEmoteName = "TestName";
            string testEmoteRarity = "Epic";

            int slotNumberReceived = -1;
            string emoteIdReceived = "";
            emoteSlotSelectorComponent.onSlotSelected += (slotNumber, emoteId) =>
            {
                slotNumberReceived = slotNumber;
                emoteIdReceived = emoteId;
            };

            // Act
            emoteSlotSelectorComponent.AssignEmoteIntoSlot(slotNumber, testEmoteId, testEmoteName, testSprite, "", testEmoteRarity);

            // Assert
            Assert.AreEqual(testEmoteId, currentSlots[slotNumber - 1].model.emoteId);
            Assert.AreEqual(testEmoteName, currentSlots[slotNumber - 1].model.emoteName);
            Assert.AreEqual(testEmoteRarity, currentSlots[slotNumber - 1].model.rarity);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ConfigureSlotButtonsCorrectly(int slotIndex)
        {
            // Arrange
            List<EmoteSlotCardComponentView> currentSlots = emoteSlotSelectorComponent.GetAllSlots();

            bool clickedResponse = false;
            currentSlots[slotIndex].onClick.AddListener(() => clickedResponse = true);

            // Act
            emoteSlotSelectorComponent.ConfigureSlotButtons();
            currentSlots[slotIndex].onClick.Invoke();

            // Assert
            Assert.IsTrue(clickedResponse);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void UnsubscribeSlotButtonsCorrectly(int slotIndex)
        {
            // Arrange
            List<EmoteSlotCardComponentView> currentSlots = emoteSlotSelectorComponent.GetAllSlots();

            bool clickedResponse = false;
            currentSlots[slotIndex].onClick.AddListener(() => clickedResponse = true);

            // Act
            emoteSlotSelectorComponent.UnsubscribeSlotButtons();
            currentSlots[slotIndex].onClick.Invoke();

            // Assert
            Assert.IsFalse(clickedResponse);
        }
    }
}