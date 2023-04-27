using Cysharp.Threading.Tasks;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.QuestsService
{
    public class QuestsService: IDisposable
    {
        private readonly IClientQuestsService clientQuestsService;

        private readonly Dictionary<string, UniTaskCompletionSource<QuestDefinition>> definitionCache = new ();
        private readonly Dictionary<string, UniTaskCompletionSource<QuestState>> stateCache = new ();

        public QuestsService(IClientQuestsService clientQuestsService)
        {
            this.clientQuestsService = clientQuestsService;
        }

        public IUniTaskAsyncEnumerable<UserUpdate> Subscribe(string userId)
        {
            return clientQuestsService.Subscribe(new UserAddress { UserAddress_ = userId });
        }

        public async UniTask<StartQuestResponse> StartQuest(string questId)
        {
            return await clientQuestsService.StartQuest(new StartQuestRequest { QuestId = questId });
        }

        public async UniTask<AbortQuestResponse> AbortQuest(string questId)
        {
            return await clientQuestsService.AbortQuest(new AbortQuestRequest { QuestInstanceId = questId });
        }

        public async UniTask<QuestDefinition> GetDefinition(string questId, CancellationToken cancellationToken = default)
        {
            UniTaskCompletionSource<QuestDefinition> definitionCompletionSource;
            async UniTask<QuestDefinition> RetrieveTask()
            {
                var definition = await clientQuestsService.GetDefinition(questId);
                definitionCompletionSource.TrySetResult(definition);
                return definition;
            }

            if (!definitionCache.TryGetValue(questId, out definitionCompletionSource))
            {
                definitionCompletionSource = new UniTaskCompletionSource<QuestDefinition>();
                definitionCache[questId] = definitionCompletionSource;
                RetrieveTask().Forget();
            }

            return await definitionCompletionSource.Task.AttachExternalCancellation(cancellationToken);
        }

        public async UniTask<QuestState> GetState(string questId, CancellationToken cancellationToken = default)
        {
            UniTaskCompletionSource<QuestState> stateCompletionSource;
            async UniTask<QuestState> RetrieveTask()
            {
                var state = await clientQuestsService.GetState(questId);
                stateCompletionSource.TrySetResult(state);
                return state;
            }

            if (!stateCache.TryGetValue(questId, out stateCompletionSource))
            {
                stateCompletionSource = new UniTaskCompletionSource<QuestState>();
                stateCache[questId] = stateCompletionSource;
                RetrieveTask().Forget();
            }

            return await stateCompletionSource.Task.AttachExternalCancellation(cancellationToken);
        }

        public void Dispose()
        {
        }
    }
}
