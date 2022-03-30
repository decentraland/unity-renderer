using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
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
        private IUserProfileBridge userProfileBridge;
        private BaseDictionary<string, WearableItem> catalog;
        private IEmotesCustomizationComponentView emotesCustomizationComponentView;

        [SetUp]
        public void SetUp()
        {
            emotesCustomizationDataStore = new DataStore_EmotesCustomization();
            emotesDataStore = new DataStore_Emotes();
            exploreV2DataStore = new DataStore_ExploreV2();
            hudsDataStore = new DataStore_HUDs();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            userProfileBridge.GetOwn().Returns(GivenMyOwnUserProfile());
            catalog = new BaseDictionary<string, WearableItem>();
            emotesCustomizationComponentView = Substitute.For<IEmotesCustomizationComponentView>();
            emotesCustomizationComponentController = Substitute.ForPartsOf<EmotesCustomizationComponentController>();
            emotesCustomizationComponentController.Configure().CreateView().Returns(info => emotesCustomizationComponentView);
            emotesCustomizationComponentController.Initialize(
                emotesCustomizationDataStore,
                emotesDataStore,
                exploreV2DataStore,
                hudsDataStore,
                userProfileBridge.GetOwn(),
                catalog);
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

        private UserProfile GivenMyOwnUserProfile()
        {
            var myUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            myUserProfile.UpdateData(new UserProfileModel { userId = "myUserId" });
            return myUserProfile;
        }
    }
}