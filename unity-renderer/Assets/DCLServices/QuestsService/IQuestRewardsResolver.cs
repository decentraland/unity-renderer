using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.QuestsService
{
    public interface IQuestRewardsResolver
    {
        UniTask<IReadOnlyList<QuestReward>> ResolveRewards(string questId, CancellationToken cancellationToken);
    }
}
