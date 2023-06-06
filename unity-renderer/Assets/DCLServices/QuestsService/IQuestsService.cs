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
        public IAsyncEnumerableWithEvent<QuestStateWithData> QuestStarted { get; }
        public IAsyncEnumerableWithEvent<QuestStateWithData> QuestUpdated { get; }
        IReadOnlyDictionary<string, QuestStateWithData> CurrentState { get; }

        void SetUserId(string userId);

        UniTask<StartQuestResponse> StartQuest(string questId);

        UniTask<AbortQuestResponse> AbortQuest(string questInstanceId);

        UniTask<Quest> GetDefinition(string questId, CancellationToken cancellationToken = default);
    }
}
