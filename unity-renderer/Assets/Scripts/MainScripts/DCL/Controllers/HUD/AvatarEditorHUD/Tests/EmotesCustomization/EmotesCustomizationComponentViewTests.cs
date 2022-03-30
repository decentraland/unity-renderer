using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.EmotesCustomization.Tests
{
    public class EmotesCustomizationComponentViewTests
    {
        private EmotesCustomizationComponentView emotesCustomizationComponent;
        private EmoteSlotCardComponentView testEmoteSlotCard;
        private Texture2D testTexture;
        private Sprite testSprite;

        [SetUp]
        public void SetUp()
        {
            emotesCustomizationComponent = BaseComponentView.Create<EmotesCustomizationComponentView>("EmotesCustomization/EmotesCustomizationSection");
            testEmoteSlotCard = BaseComponentView.Create<EmoteSlotCardComponentView>("EmotesCustomization/EmoteSlotCard");
            testTexture = new Texture2D(20, 20);
            testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
        }

        [TearDown]
        public void TearDown()
        {
            testEmoteSlotCard.Dispose();
            emotesCustomizationComponent.Dispose();
            GameObject.Destroy(testTexture);
            GameObject.Destroy(testSprite);
        }

        [Test]
        public void CleanEmotesCorrectly()
        {
            // Arrange
            emotesCustomizationComponent.AddEmote(GetTestEmoteCardModel());

            // Act
            emotesCustomizationComponent.CleanEmotes();

            // Assert
            Assert.AreEqual(0, emotesCustomizationComponent.emotesGrid.GetItems().Count);
        }

        [Test]
        public void AddEmoteCorrectly()
        {
            // Act
            EmoteCardComponentModel testEventCardModel = GetTestEmoteCardModel();
            emotesCustomizationComponent.AddEmote(testEventCardModel);

            // Assert
            List<BaseComponentView> currentEmotes = emotesCustomizationComponent.emotesGrid.GetItems();
            Assert.AreEqual(1, currentEmotes.Count);
            Assert.IsTrue(currentEmotes.Exists(x => (x as EmoteCardComponentView).model.id == testEventCardModel.id));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void EquipEmoteCorrectly(int slotNumber)
        {
            // Arrange
            EmoteCardComponentModel testEventCardModel = GetTestEmoteCardModel();
            EmoteCardComponentView testEventCard = emotesCustomizationComponent.AddEmote(testEventCardModel);

            string equippedEmoteID = "";
            int equippedSlot = -1;
            emotesCustomizationComponent.onEmoteEquipped += (emoteId, slotNumber) =>
            {
                equippedEmoteID = emoteId;
                equippedSlot = slotNumber;
            };

            // Act
            emotesCustomizationComponent.EquipEmote(
                testEventCardModel.id,
                testEventCardModel.name,
                slotNumber);

            // Assert
            Assert.AreEqual(slotNumber, testEventCard.model.assignedSlot);
            Assert.AreEqual(slotNumber, emotesCustomizationComponent.emoteSlotSelector.selectedSlot);
            Assert.AreEqual(testEventCardModel.id, equippedEmoteID);
            Assert.AreEqual(slotNumber, equippedSlot);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void UnequipEmoteCorrectly(int slotNumber)
        {
            // Arrange
            EmoteCardComponentModel testEventCardModel = GetTestEmoteCardModel();
            EmoteCardComponentView testEventCard = emotesCustomizationComponent.AddEmote(testEventCardModel);

            string unequippedEmoteID = "";
            int unequippedSlot = -1;
            emotesCustomizationComponent.onEmoteUnequipped += (emoteId, slotNumber) =>
            {
                unequippedEmoteID = emoteId;
                unequippedSlot = slotNumber;
            };

            // Act
            emotesCustomizationComponent.UnequipEmote(
                testEventCardModel.id,
                slotNumber);

            // Assert
            Assert.AreEqual(-1, testEventCard.model.assignedSlot);
            Assert.AreEqual(testEventCardModel.id, unequippedEmoteID);
            Assert.AreEqual(slotNumber, unequippedSlot);
        }

        [Test]
        public void OpenEmoteInfoPanelCorrectly()
        {
            // Arrange
            EmoteCardComponentModel testEventCardModel = GetTestEmoteCardModel();
            Color testColor = Color.green;
            Transform testAnchorTransform = new GameObject().transform;
            emotesCustomizationComponent.emoteInfoPanel.SetActive(false);

            // Act
            emotesCustomizationComponent.OpenEmoteInfoPanel(testEventCardModel, testColor, testAnchorTransform);

            // Assert
            Assert.AreEqual(testEventCardModel.name, emotesCustomizationComponent.emoteInfoPanel.name.text);
            Assert.AreEqual(testEventCardModel.description, emotesCustomizationComponent.emoteInfoPanel.description.text);
            Assert.AreEqual(testColor, emotesCustomizationComponent.emoteInfoPanel.backgroundImage.color);
            Assert.AreEqual(testEventCardModel.rarity, emotesCustomizationComponent.emoteInfoPanel.rarityName.text);
            Assert.IsTrue(emotesCustomizationComponent.emoteInfoPanel.gameObject.activeSelf);
            Assert.AreEqual(testAnchorTransform, emotesCustomizationComponent.emoteInfoPanel.transform.parent);
            Assert.AreEqual(Vector3.zero, emotesCustomizationComponent.emoteInfoPanel.transform.localPosition);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetEmoteInfoPanelActiveCorrectly(bool isActive)
        {
            // Arrange
            emotesCustomizationComponent.emoteInfoPanel.SetActive(!isActive);

            // Act
            emotesCustomizationComponent.SetEmoteInfoPanelActive(isActive);

            // Assert
            Assert.AreEqual(isActive, emotesCustomizationComponent.emoteInfoPanel.gameObject.activeSelf);
        }

        [Test]
        [TestCase("TestId")]
        [TestCase("NonExistingId")]
        public void GetEmoteCardByIdCorrectly(string emoteId)
        {
            // Arrange
            EmoteCardComponentModel testEventCardModel = GetTestEmoteCardModel();
            EmoteCardComponentView testEventCard = emotesCustomizationComponent.AddEmote(testEventCardModel);

            // Act
            EmoteCardComponentView emoteCardResult = emotesCustomizationComponent.GetEmoteCardById(emoteId);

            // Assert
            if (emoteId == "TestId")
                Assert.AreEqual(testEventCard.model.id, emoteCardResult.model.id);
            else
                Assert.IsNull(emoteCardResult);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            emotesCustomizationComponent.gameObject.SetActive(!isActive);

            // Act
            emotesCustomizationComponent.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, emotesCustomizationComponent.gameObject.activeSelf);
        }

        [Test]
        [TestCase(0)]
        [TestCase(20)]
        public void GetSlotCorrectly(int slotNumber)
        {
            // Act
            EmoteSlotCardComponentView slotCardResult = emotesCustomizationComponent.GetSlot(slotNumber);

            // Assert
            if (slotNumber == 0)
                Assert.IsNotNull(slotCardResult);
            else
                Assert.IsNull(slotCardResult);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClickOnEmoteCorrectly(bool isAssignedInSelectedSlot)
        {
            // Arrange
            int testSlotNumber = 0;
            EmoteCardComponentModel testEventCardModel = GetTestEmoteCardModel();
            EmoteCardComponentView testEventCard = emotesCustomizationComponent.AddEmote(testEventCardModel);

            string equippedEmoteID = "";
            int equippedSlot = -1;
            emotesCustomizationComponent.onEmoteEquipped += (emoteId, slotNumber) =>
            {
                equippedEmoteID = emoteId;
                equippedSlot = slotNumber;
            };

            string unequippedEmoteID = "";
            int unequippedSlot = -1;
            emotesCustomizationComponent.onEmoteUnequipped += (emoteId, slotNumber) =>
            {
                unequippedEmoteID = emoteId;
                unequippedSlot = slotNumber;
            };

            // Act
            emotesCustomizationComponent.ClickOnEmote(
                testEventCardModel.id,
                testEventCardModel.name,
                testSlotNumber,
                isAssignedInSelectedSlot);

            // Assert
            if (!isAssignedInSelectedSlot)
            {
                Assert.AreEqual(testSlotNumber, testEventCard.model.assignedSlot);
                Assert.AreEqual(testSlotNumber, emotesCustomizationComponent.emoteSlotSelector.selectedSlot);
                Assert.AreEqual(testEventCardModel.id, equippedEmoteID);
                Assert.AreEqual(testSlotNumber, equippedSlot);
            }
            else
            {
                Assert.AreEqual(-1, testEventCard.model.assignedSlot);
                Assert.AreEqual(testEventCardModel.id, unequippedEmoteID);
                Assert.AreEqual(emotesCustomizationComponent.selectedSlot, unequippedSlot);
            }

            Assert.IsFalse(emotesCustomizationComponent.emoteInfoPanel.gameObject.activeSelf);
        }

        [Test]
        [TestCase("TestId")]
        [TestCase("NonExistingId")]
        public void RaiseOnEmoteSelectedCorrectly(string emoteId)
        {
            // Arrange
            EmoteCardComponentModel testEventCardModel = GetTestEmoteCardModel();
            EmoteCardComponentView testEventCard = emotesCustomizationComponent.AddEmote(testEventCardModel);

            // Act
            emotesCustomizationComponent.OnEmoteSelected(emoteId);

            // Assert
            if (emoteId == "TestId")
                Assert.AreEqual(testEventCard, emotesCustomizationComponent.selectedCard);
            else
                Assert.IsNull(emotesCustomizationComponent.selectedCard);
        }

        [Test]
        public void InstantiateAndConfigureEmoteCardCorrectly()
        {
            // Arrange
            EmoteCardComponentModel testEventCardModel = GetTestEmoteCardModel();

            // Act
            EmoteCardComponentView emoteCardResult = emotesCustomizationComponent.InstantiateAndConfigureEmoteCard(testEventCardModel);

            // Assert
            Assert.AreEqual(testEventCardModel, emoteCardResult.model);
            Assert.AreEqual(testEventCardModel.name, emoteCardResult.emoteNameText.text);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void RaiseOnSlotSelectedCorrectly(int slotNumber)
        {
            // Arrange
            EmoteCardComponentModel testEventCardModel = GetTestEmoteCardModel();
            testEventCardModel.assignedSlot = slotNumber;
            EmoteCardComponentView testEventCard = emotesCustomizationComponent.AddEmote(testEventCardModel);

            string selectedSlotEmoteID = "";
            int selectedSlotNumber = -1;
            emotesCustomizationComponent.onSlotSelected += (emoteId, slotNumber) =>
            {
                selectedSlotEmoteID = emoteId;
                selectedSlotNumber = slotNumber;
            };

            // Act
            emotesCustomizationComponent.OnSlotSelected(slotNumber, testEventCardModel.id);

            // Assert
            Assert.IsTrue(testEventCard.model.isAssignedInSelectedSlot);
            Assert.AreEqual(testEventCardModel.id, selectedSlotEmoteID);
            Assert.AreEqual(slotNumber, selectedSlotNumber);
            Assert.IsFalse(emotesCustomizationComponent.emoteInfoPanel.gameObject.activeSelf);
        }

        [Test]
        public void GetAllEmoteCardsCorrectly()
        {
            // Arrange
            EmoteCardComponentModel testEventCardModel1 = GetTestEmoteCardModel();
            testEventCardModel1.id = $"{testEventCardModel1.id}_1";
            EmoteCardComponentView testEventCard1 = emotesCustomizationComponent.AddEmote(testEventCardModel1);

            EmoteCardComponentModel testEventCardModel2 = GetTestEmoteCardModel();
            testEventCardModel2.id = $"{testEventCardModel1.id}_2";
            EmoteCardComponentView testEventCard2 = emotesCustomizationComponent.AddEmote(testEventCardModel2);

            // Act
            List<EmoteCardComponentView> emoteCardsResult = emotesCustomizationComponent.GetAllEmoteCards();

            // Assert
            Assert.AreEqual(2, emoteCardsResult.Count);
            Assert.AreEqual(testEventCardModel1.id, emoteCardsResult[0].model.id);
            Assert.AreEqual(testEventCardModel2.id, emoteCardsResult[1].model.id);
        }

        private EmoteCardComponentModel GetTestEmoteCardModel()
        {
            return new EmoteCardComponentModel
            {
                assignedSlot = -1,
                description = "Test Description",
                id = "TestId",
                isAssignedInSelectedSlot = false,
                isCollectible = true,
                isInL2 = true,
                isLoading = false,
                isSelected = false,
                name = "Test Name",
                pictureSprite = testSprite,
                pictureUri = "testUri",
                rarity = "epic"
            };
        }
    }
}