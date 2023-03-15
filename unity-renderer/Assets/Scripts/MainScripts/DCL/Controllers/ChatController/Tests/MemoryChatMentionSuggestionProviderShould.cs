using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Social.Chat.Mentions
{
    public class MemoryChatMentionSuggestionProviderShould
    {
        private MemoryChatMentionSuggestionProvider mentionSuggestionProvider;
        private UserProfileController userProfileController;
        private DataStore dataStore;

        [SetUp]
        public void SetUp()
        {
            userProfileController = new GameObject("UserProfileController").AddComponent<UserProfileController>();

            userProfileController.AddUserProfileToCatalog(new UserProfileModel
            {
                userId = "user1",
                name = "lucky",
            });

            userProfileController.AddUserProfileToCatalog(new UserProfileModel
            {
                userId = "user2",
                name = "womo",
            });

            userProfileController.AddUserProfileToCatalog(new UserProfileModel
            {
                userId = "user3",
                name = "heyun",
            });

            userProfileController.AddUserProfileToCatalog(new UserProfileModel
            {
                userId = "user4",
                name = "holy",
            });

            dataStore = new DataStore();
            mentionSuggestionProvider = new MemoryChatMentionSuggestionProvider(userProfileController, dataStore);
        }

        [TearDown]
        public void TearDown()
        {
            dataStore.channels.availableMembersByChannel.Clear();
            dataStore.player.otherPlayers.Clear();
            Object.Destroy(userProfileController.gameObject);
        }

        [UnityTest]
        public IEnumerator GetAllProfilesStartingWith() =>
            UniTask.ToCoroutine(async () =>
            {
                List<UserProfile> profiles = await mentionSuggestionProvider.GetProfilesStartingWith("h", 5, default(CancellationToken));

                Assert.AreEqual("user3", profiles[0].userId);
                Assert.AreEqual("user4", profiles[1].userId);
                Assert.AreEqual(2, profiles.Count);
            });

        [UnityTest]
        public IEnumerator GetMaxAmountOfProfilesStartingWith() =>
            UniTask.ToCoroutine(async () =>
            {
                List<UserProfile> profiles = await mentionSuggestionProvider.GetProfilesStartingWith("h", 1, default(CancellationToken));

                Assert.AreEqual("user3", profiles[0].userId);
                Assert.AreEqual(1, profiles.Count);
            });

        [UnityTest]
        public IEnumerator GetNearbyProfiles() =>
            UniTask.ToCoroutine(async () =>
            {
                dataStore.player.otherPlayers.Add("user3", new Player
                {
                    id = "user3",
                    name = "heyun",
                });

                dataStore.player.otherPlayers.Add("user4", new Player
                {
                    id = "user4",
                    name = "holy",
                });

                List<UserProfile> profiles = await mentionSuggestionProvider.GetNearbyProfilesStartingWith("h", 5, default(CancellationToken));

                Assert.AreEqual("user3", profiles[0].userId);
                Assert.AreEqual("user4", profiles[1].userId);
                Assert.AreEqual(2, profiles.Count);
            });

        [UnityTest]
        public IEnumerator GetMaxAmountOfNearbyProfiles() =>
            UniTask.ToCoroutine(async () =>
            {
                dataStore.player.otherPlayers.Add("user3", new Player
                {
                    id = "user3",
                    name = "heyun",
                });

                dataStore.player.otherPlayers.Add("user4", new Player
                {
                    id = "user4",
                    name = "holy",
                });

                List<UserProfile> profiles = await mentionSuggestionProvider.GetNearbyProfilesStartingWith("h", 1, default(CancellationToken));

                Assert.AreEqual("user3", profiles[0].userId);
                Assert.AreEqual(1, profiles.Count);
            });

        [UnityTest]
        public IEnumerator GetProfilesFromChannels() =>
            UniTask.ToCoroutine(async () =>
            {
                dataStore.channels.availableMembersByChannel.Add("channelId", new HashSet<string>
                {
                    "user1",
                    "user2",
                    "user3",
                });

                List<UserProfile> profiles = await mentionSuggestionProvider.GetProfilesFromChatChannelsStartingWith("hey", "channelId", 5, default(CancellationToken));

                Assert.AreEqual("user3", profiles[0].userId);
                Assert.AreEqual(1, profiles.Count);
            });

        [UnityTest]
        public IEnumerator GetMaxAmountOfProfilesFromChannels() =>
            UniTask.ToCoroutine(async () =>
            {
                dataStore.channels.availableMembersByChannel.Add("channelId", new HashSet<string>
                {
                    "user3",
                    "user4",
                });

                List<UserProfile> profiles = await mentionSuggestionProvider.GetProfilesFromChatChannelsStartingWith("h", "channelId", 1, default(CancellationToken));

                Assert.AreEqual("user3", profiles[0].userId);
                Assert.AreEqual(1, profiles.Count);
            });
    }
}
