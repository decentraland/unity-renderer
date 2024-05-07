using Cysharp.Threading.Tasks;
using DCL.Helpers;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.QuestsService
{
    public interface IQuestsService : IDisposable
    {
        public IAsyncEnumerableWithEvent<QuestInstance> QuestStarted { get; }
        public IAsyncEnumerableWithEvent<QuestInstance> QuestUpdated { get; }
        IReadOnlyDictionary<string, QuestInstance> QuestInstances { get; }

        UniTask<StartQuestResponse> StartQuest(string questId);

        UniTask<AbortQuestResponse> AbortQuest(string questInstanceId);

        UniTask<Quest> GetDefinition(string questId, CancellationToken cancellationToken = default);
        UniTask<IReadOnlyList<QuestReward>> GetQuestRewards(string questId, CancellationToken cancellationToken = default);
    }
}
