using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DCL.Emotes;
using UnityEngine;

namespace DCL.EmotesCustomization.Tests
{
    public class EmotesCustomizationComponentControllerTests
    {
        private EmotesCustomizationComponentController emotesCustomizationComponentController;
        private DataStore_EmotesCustomization emotesCustomizationDataStore;
        private DataStore_Emotes emotesDataStore;
        private DataStore_ExploreV2 exploreV2DataStore;
        private DataStore_HUDs hudsDataStore;
        private IEmotesCustomizationComponentView emotesCustomizationComponentView;

        [SetUp]
        public void SetUp()
        {
            emotesCustomizationDataStore = new DataStore_EmotesCustomization();
            emotesDataStore = new DataStore_Emotes();
            exploreV2DataStore = new DataStore_ExploreV2();
            hudsDataStore = new DataStore_HUDs();
            emotesCustomizationComponentView = Substitute.For<IEmotesCustomizationComponentView>();
            emotesCustomizationComponentController = Substitute.ForPartsOf<EmotesCustomizationComponentController>();
            emotesCustomizationComponentController.Configure().CreateView().Returns(info => emotesCustomizationComponentView);
            emotesCustomizationComponentController.Initialize(emotesCustomizationDataStore,
                emotesDataStore,
                exploreV2DataStore,
                hudsDataStore,
                null);
        }

        [TearDown]
        public void TearDown() { emotesCustomizationComponentController.Dispose(); }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            Assert.AreEqual(emotesCustomizationComponentView, emotesCustomizationComponentController.view);
        }

        [Test]
        public void RestoreEmoteSlotsCorrectly()
        {
            // Arrange
            emotesCustomizationDataStore.unsavedEquippedEmotes.Set(new List<EquippedEmoteData>());
            emotesCustomizationDataStore.equippedEmotes.Set(new List<EquippedEmoteData>
            {
                new EquippedEmoteData { id = "TestId1", cachedThumbnail = null },
                new EquippedEmoteData { id = "TestId2", cachedThumbnail = null }
            });

            // Act
            emotesCustomizationComponentController.RestoreEmoteSlots();

            // Assert
            Assert.AreEqual(emotesCustomizationDataStore.equippedEmotes.Get(), emotesCustomizationDataStore.unsavedEquippedEmotes.Get());
        }

        [Test]
        public void RaiseOnEquippedEmotesSetCorrectly()
        {
            // Arrange
            List<EquippedEmoteData> testEquippedEmotes = new List<EquippedEmoteData>
            {
                new EquippedEmoteData { id = "TestId1", cachedThumbnail = null },
                new EquippedEmoteData { id = "TestId2", cachedThumbnail = null }
            };

            // Act
            emotesCustomizationComponentController.OnEquippedEmotesSet(testEquippedEmotes);

            // Assert
            Assert.AreEqual(testEquippedEmotes, emotesCustomizationDataStore.unsavedEquippedEmotes.Get());
        }

        [Test]
        public void RaiseIsStarMenuOpenChangedCorrectly()
        {
            // Act
            emotesCustomizationComponentController.IsStarMenuOpenChanged(true, false);

            // Assert
            emotesCustomizationComponentController.view.Received().SetEmoteInfoPanelActive(false);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RaiseOnAvatarEditorVisibleChangedCorrectly(bool isVisible)
        {
            // Act
            emotesCustomizationComponentController.OnAvatarEditorVisibleChanged(isVisible, !isVisible);

            // Assert
            emotesCustomizationComponentController.view.Received().SetActive(isVisible);
        }

        [Test]
        public void ProcessCatalogCorrectly()
        {
            // Arrange
            emotesCustomizationComponentController.emotesCustomizationDataStore.currentLoadedEmotes.Set(new List<string>());
            string testId1 = "TestId1";
            string testId2 = "TestId2";
            WearableItem[] emotes = new []
            {
                new WearableItem
                {
                    id = testId1,
                    emoteDataV0 = new Emotes.EmoteDataV0 { loop = false },
                    data = new WearableItem.Data { tags = new string[] { WearableLiterals.Tags.BASE_WEARABLE } },
                    i18n = new i18n[] { new i18n { code = "en", text = testId1 } }
                },
                new WearableItem
                {
                    id = testId2,
                    emoteDataV0 = new Emotes.EmoteDataV0 { loop = false },
                    data = new WearableItem.Data { tags = new string[] { WearableLiterals.Tags.BASE_WEARABLE } },
                    i18n = new i18n[] { new i18n { code = "en", text = testId2 } }
                }
            };

            // Act
            emotesCustomizationComponentController.SetEmotes(emotes);

            // Assert
            Assert.AreEqual(emotes.Length, emotesCustomizationComponentController.emotesCustomizationDataStore.currentLoadedEmotes.Count());
            Assert.AreEqual(testId1, emotesCustomizationComponentController.emotesCustomizationDataStore.currentLoadedEmotes.Get().ToList()[0]);
            Assert.AreEqual(testId2, emotesCustomizationComponentController.emotesCustomizationDataStore.currentLoadedEmotes.Get().ToList()[1]);
        }

        [Test]
        public void RefreshEmoteLoadingStateCorrectly()
        {
            // Arrange
            string emoteId = "TestId";
            EmoteCardComponentView testEmoteCard = new GameObject().AddComponent<EmoteCardComponentView>();
            emotesCustomizationComponentController.SetEquippedBodyShape("bodyShapeId");

            emotesCustomizationComponentController.emotesDataStore.animations
                .Add(("bodyShapeId", emoteId), new EmoteClipData(new AnimationClip()));

            testEmoteCard.model = new EmoteCardComponentModel { isLoading = true };
            emotesCustomizationComponentController.emotesInLoadingState.Add(emoteId, testEmoteCard);

            // Act
            emotesCustomizationComponentController.RefreshEmoteLoadingState(emoteId);

            // Assert
            Assert.IsFalse(testEmoteCard.model.isLoading);
            Assert.AreEqual(0, emotesCustomizationComponentController.emotesInLoadingState.Count());
        }

        [Test]
        public void ParseWearableItemIntoEmoteCardModelCorrectly()
        {
            // Arrange
            WearableItem testWearableItem = new WearableItem
            {
                id = "TestId",
                i18n = new i18n[] { new i18n { code = "en", text = "Test Name" } },
                description = "Test Desc",
                baseUrl = "",
                thumbnail = "",
                thumbnailSprite = null,
                rarity = "Epic"
            };

            // Act
            EmoteCardComponentModel result = emotesCustomizationComponentController.ParseWearableItemIntoEmoteCardModel(testWearableItem);

            // Assert
            Assert.AreEqual(testWearableItem.id, result.id);
            Assert.AreEqual(testWearableItem.i18n[0].text, result.name);
            Assert.AreEqual(testWearableItem.description, result.description);
            Assert.AreEqual("", result.pictureUri);
            Assert.AreEqual(null, result.pictureSprite);
            Assert.AreEqual(false, result.isAssignedInSelectedSlot);
            Assert.AreEqual(false, result.isSelected);
            Assert.AreEqual(-1, result.assignedSlot);
            Assert.AreEqual(testWearableItem.rarity, result.rarity);
            Assert.AreEqual(false, result.isInL2);
            Assert.AreEqual(false, result.isLoading);
            Assert.AreEqual(true, result.isCollectible);
        }


        [Test]
        public void UpdateEmoteSlotsCorrectly()
        {
            // Arrange
            string testId1 = "TestId1";
            string testId2 = "TestId2";

            WearableItem[] emotes = new []
            {

                new WearableItem
                {
                    id = testId1,
                    emoteDataV0 = new Emotes.EmoteDataV0 { loop = false },
                    data = new WearableItem.Data { tags = new string[] { WearableLiterals.Tags.BASE_WEARABLE } },
                    i18n = new i18n[] { new i18n { code = "en", text = testId1 } }
                },
                new WearableItem
                {
                    id = testId2,
                    emoteDataV0 = new Emotes.EmoteDataV0 { loop = false },
                    data = new WearableItem.Data { tags = new string[] { WearableLiterals.Tags.BASE_WEARABLE } },
                    i18n = new i18n[] { new i18n { code = "en", text = testId2 } }
                }
            };

            emotesCustomizationComponentController.ownedEmotes = emotes.ToDictionary(x => x.id, x => x);
            emotesCustomizationComponentController.emotesCustomizationDataStore.currentLoadedEmotes.Set(new List<string> { testId1, testId2 });

            emotesCustomizationDataStore.unsavedEquippedEmotes.Set(new List<EquippedEmoteData>
            {
                new EquippedEmoteData { id = testId1, cachedThumbnail = null },
                new EquippedEmoteData { id = testId2, cachedThumbnail = null }
            });

            // Act
            emotesCustomizationComponentController.UpdateEmoteSlots();

            // Assert
            emotesCustomizationComponentController.view.Received(2).EquipEmote(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), false, false);
        }

        [Test]
        public void RaiseOnEmoteEquippedCorrectly()
        {
            // Arrange
            string emoteId = "TestId";
            string receivedId1 = "";
            emotesCustomizationComponentController.onEmotePreviewed += (id) => { receivedId1 = id; };

            string receivedId2 = "";
            emotesCustomizationComponentController.onEmoteEquipped += (id) => { receivedId2 = id; };

            // Act
            emotesCustomizationComponentController.OnEmoteEquipped(emoteId, 0);

            // Assert
            Assert.AreEqual(emoteId, receivedId1);
            Assert.AreEqual(emoteId, receivedId2);
        }

        [Test]
        public void RaiseOnEmoteUnequippedCorrectly()
        {
            // Arrange
            string emoteId = "TestId";
            string receivedId = "";
            emotesCustomizationComponentController.onEmoteUnequipped += (id) => { receivedId = id; };

            // Act
            emotesCustomizationComponentController.OnEmoteUnequipped(emoteId, 0);

            // Assert
            Assert.AreEqual(emoteId, receivedId);
        }

        [Test]
        public void RaiseOnSellEmoteClickedCorrectly()
        {
            // Arrange
            string emoteId = "TestId";
            string receivedId = "";
            emotesCustomizationComponentController.onEmoteSell += (id) => { receivedId = id; };

            // Act
            emotesCustomizationComponentController.OnSellEmoteClicked(emoteId);

            // Assert
            Assert.AreEqual(emoteId, receivedId);
        }

        [Test]
        public void RaiseOnSlotSelectedCorrectly()
        {
            // Arrange
            string emoteId = "TestId";
            string receivedId = "";
            emotesCustomizationComponentController.onEmotePreviewed += (id) => { receivedId = id; };

            // Act
            emotesCustomizationComponentController.OnSlotSelected(emoteId, 0);

            // Assert
            Assert.AreEqual(emoteId, receivedId);
        }
    }
}
