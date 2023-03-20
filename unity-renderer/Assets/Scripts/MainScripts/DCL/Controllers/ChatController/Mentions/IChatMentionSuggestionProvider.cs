using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Social.Chat.Mentions
{
    public interface IChatMentionSuggestionProvider
    {
        UniTask<List<UserProfile>> GetNearbyProfilesStartingWith(string name, int count, CancellationToken cancellationToken);
        UniTask<List<UserProfile>> GetProfilesFromChatChannelsStartingWith(string name, string channelId, int count, CancellationToken cancellationToken);
        UniTask<List<UserProfile>> GetProfilesStartingWith(string name, int count, CancellationToken cancellationToken);
        UniTask<List<UserProfile>> GetProfilesStartingWith(string name, int count, IEnumerable<UserProfile> profiles, CancellationToken cancellationToken);
    }
}
