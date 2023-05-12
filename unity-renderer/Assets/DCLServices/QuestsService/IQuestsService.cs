using Cysharp.Threading.Tasks;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.QuestsService
{
    public interface IQuestsService : IDisposable
    {
        event Action<QuestStateUpdate> OnQuestUpdated;
        IReadOnlyDictionary<string, QuestStateUpdate> CurrentState { get; }

        void SetUserId(string userId);

        UniTask<StartQuestResponse> StartQuest(string questId);

        UniTask<AbortQuestResponse> AbortQuest(string questInstanceId);

        UniTask<ProtoQuest> GetDefinition(string questId, CancellationToken cancellationToken = default);
    }
}
