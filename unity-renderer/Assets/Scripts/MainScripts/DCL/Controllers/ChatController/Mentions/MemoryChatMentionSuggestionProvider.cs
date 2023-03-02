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

        public MemoryChatMentionSuggestionProvider(UserProfileController userProfileController)
        {
            this.userProfileController = userProfileController;
        }

        public async UniTask<List<UserProfile>> GetProfilesStartingWith(string name, int count, CancellationToken cancellationToken)
        {
            var iterations = 0;
            List<UserProfile> results = new List<UserProfile>();

            foreach ((string userId, UserProfile profile) in userProfileController.AllProfiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

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
