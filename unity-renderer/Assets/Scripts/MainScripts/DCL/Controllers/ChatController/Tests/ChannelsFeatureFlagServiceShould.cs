using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCL.Chat.Channels
{
    public class ChannelsFeatureFlagServiceShould
    {
        private ChannelsFeatureFlagService service;
        private DataStore dataStore;
        private IUserProfileBridge userProfileBridge;

        [SetUp]
        public void SetUp()
        {
            dataStore = new DataStore();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = "ownId"
            });
            userProfileBridge.GetOwn().Returns(ownUserProfile);
            service = new ChannelsFeatureFlagService(dataStore, userProfileBridge);
        }

        [TearDown]
        public void TearDown()
        {
            service.Dispose();
        }

        [Test]
        public void NotAllowedWhenListIsNull()
        {
            GivenEmptyAllowedUsers();

            Assert.IsFalse(service.IsAllowedToCreateChannels());
        }

        private void GivenEmptyAllowedUsers()
        {
            var featureFlag = new FeatureFlag
            {
                flags =
                {
                    ["matrix_channels_enabled"] = true,
                    ["users_allowed_to_create_channels"] = true
                },
                variants =
                {
                    ["users_allowed_to_create_channels"] = new FeatureFlagVariant
                    {
                        enabled = true,
                        name = "allowedUsers",
                        payload = new FeatureFlagVariantPayload
                        {
                            type = "json",
                            value = "{}"
                        }
                    }
                }
            };
            dataStore.featureFlags.flags.Set(featureFlag);
        }
    }
}