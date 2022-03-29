using DCL.EmotesCustomization;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class EmotesCustomizationComponentViewTests
{
    private EmotesCustomizationComponentView emotesCustomizationComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        emotesCustomizationComponent = BaseComponentView.Create<EmotesCustomizationComponentView>("EmotesCustomization/EmotesCustomizationSection");
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
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
    public void RemoveEmoteCorrectly()
    {
        // Arrange
        EmoteCardComponentModel testEventCardModel = GetTestEmoteCardModel();
        emotesCustomizationComponent.AddEmote(testEventCardModel);

        // Act
        emotesCustomizationComponent.RemoveEmote(testEventCardModel.id);

        // Assert
        List<BaseComponentView> currentEmotes = emotesCustomizationComponent.emotesGrid.GetItems();
        Assert.AreEqual(0, currentEmotes.Count);
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
