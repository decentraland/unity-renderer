using DCL.Social.Chat;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class ChannelMemberEntryShould
    {
        private ChannelMemberEntry channelMemberEntryComponent;

        [SetUp]
        public void SetUp()
        {
            channelMemberEntryComponent = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<ChannelMemberEntry>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Prefabs/ChannelMemberEntry.prefab"));

            channelMemberEntryComponent.userThumbnail.imageObserver = Substitute.For<ILazyTextureObserver>();
        }

        [TearDown]
        public void TearDown()
        {
            channelMemberEntryComponent.Dispose();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ConfigureEntryCorrectly(bool isOnline)
        {
            // Arrange
            ChannelMemberEntryModel testModel = new ChannelMemberEntryModel
            {
                userId = "testId",
                userName = "testName",
                thumnailUrl = "testUri",
                isOnline = isOnline
            };

            // Act
            channelMemberEntryComponent.Configure(testModel);

            // Assert
            Assert.AreEqual(testModel, channelMemberEntryComponent.model);
            Assert.AreEqual(testModel.userName, channelMemberEntryComponent.nameLabel.text);
            Assert.AreEqual(isOnline, channelMemberEntryComponent.onlineMark.activeSelf);
            Assert.AreEqual(!isOnline, channelMemberEntryComponent.offlineMark.activeSelf);
        }
    }
}
