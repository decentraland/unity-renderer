using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Social.Chat.Mentions
{
    public class MemoryChatMentionSuggestionProvider : IChatMentionSuggestionProvider
    {
        private const int ITERATIONS_PER_FRAME = 10;

        private readonly UserProfileController userProfileController;
        private readonly DataStore dataStore;

        public MemoryChatMentionSuggestionProvider(UserProfileController userProfileController,
            DataStore dataStore)
        {
            this.userProfileController = userProfileController;
            this.dataStore = dataStore;
        }

        public async UniTask<List<UserProfile>> GetNearbyProfilesStartingWith(string name, int count, CancellationToken cancellationToken)
        {
            var iterations = 0;
            List<UserProfile> results = new List<UserProfile>();

            foreach ((string key, Player value) in dataStore.player.otherPlayers)
            {
                if (value.name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                {
                    UserProfile profile = userProfileController.AllProfiles.Get(key);
                    if (profile == null) continue;

                    results.Add(profile);

                    if (results.Count >= count) break;
                }

                if (iterations >= ITERATIONS_PER_FRAME)
                    await UniTask.NextFrame(cancellationToken);

                iterations++;
            }

            return results;
        }

        public async UniTask<List<UserProfile>> GetProfilesFromChatChannelsStartingWith(string name, string channelId, int count, CancellationToken cancellationToken)
        {
            var iterations = 0;
            List<UserProfile> results = new List<UserProfile>();

            if (!dataStore.channels.availableMembersByChannel.TryGetValue(channelId, out var availableMembers))
                return results;

            foreach (string userId in availableMembers)
            {
                UserProfile profile = userProfileController.AllProfiles.Get(userId);
                if (profile == null) continue;

                if (profile.userName.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(profile);

                    if (results.Count >= count) break;
                }

                if (iterations >= ITERATIONS_PER_FRAME)
                    await UniTask.NextFrame(cancellationToken);

                iterations++;
            }

            return results;
        }

        public async UniTask<List<UserProfile>> GetProfilesStartingWith(string name, int count, CancellationToken cancellationToken) =>
            await GetProfilesStartingWith(name, count, userProfileController.AllProfiles.GetValues(), cancellationToken);

        public async UniTask<List<UserProfile>> GetProfilesStartingWith(string name, int count, IEnumerable<UserProfile> profiles,
            CancellationToken cancellationToken)
        {
            var iterations = 0;
            List<UserProfile> results = new List<UserProfile>();

            foreach (UserProfile profile in profiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (profile == null) continue;

                if (profile.userName.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(profile);

                    if (results.Count >= count) break;
                }

                if (iterations >= ITERATIONS_PER_FRAME)
                    await UniTask.NextFrame(cancellationToken);

                iterations++;
            }

            return results;
        }
    }
}
