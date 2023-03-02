using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Social.Chat
{
    public interface IChatMentionSuggestionProvider
    {
        UniTask<List<UserProfile>> GetProfilesStartingWith(string name, int count, CancellationToken cancellationToken);
    }
}
