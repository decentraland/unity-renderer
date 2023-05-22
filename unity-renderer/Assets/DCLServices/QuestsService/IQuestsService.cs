using Cysharp.Threading.Tasks;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.QuestsService
{
    public interface IQuestsService : IDisposable
    {
        event Action<QuestStateWithData> OnQuestStarted;
        event Action<QuestStateWithData> OnQuestUpdated;
        IReadOnlyDictionary<string, QuestStateWithData> CurrentState { get; }

        void SetUserId(string userId);

        UniTask<StartQuestResponse> StartQuest(string questId);

        UniTask<AbortQuestResponse> AbortQuest(string questInstanceId);

        UniTask<Quest> GetDefinition(string questId, CancellationToken cancellationToken = default);
    }
}
