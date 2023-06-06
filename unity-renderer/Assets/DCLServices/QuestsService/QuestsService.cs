using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Tasks;
using Decentraland.Quests;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.QuestsService
{
    /* TODO Alex:
        - Add service to ServiceLocator
        - Find a good place to call QuestsService.SetUserId
        All these requirements are only needed to launch quests,
        in the meantime the service is ready to be tested by mocking a ClientQuestService (look at the test scene)
     */
    public class QuestsService : IQuestsService
    {
        public IAsyncEnumerableWithEvent<QuestStateWithData> QuestStarted => questStarted;
        public IAsyncEnumerableWithEvent<QuestStateWithData> QuestUpdated => questUpdated;

        private readonly AsyncEnumerableWithEvent<QuestStateWithData> questStarted = new ();
        private readonly AsyncEnumerableWithEvent<QuestStateWithData> questUpdated = new ();

        public IReadOnlyDictionary<string, QuestStateWithData> CurrentState => stateCache;
        internal readonly Dictionary<string, QuestStateWithData> stateCache = new ();

        internal readonly IClientQuestsService clientQuestsService;
        internal string userId = null;
        internal readonly Dictionary<string, UniTaskCompletionSource<Quest>> definitionCache = new ();
        internal CancellationTokenSource userSubscribeCt = null;

        public QuestsService(IClientQuestsService clientQuestsService)
        {
            this.clientQuestsService = clientQuestsService;
        }

        public void SetUserId(string userId)
        {
            if (userId == this.userId)
                return;

            this.userId = userId;

            // Definitions are not user specific, so we only need to clear the state cache
            stateCache.Clear();
            userSubscribeCt.SafeCancelAndDispose();
            userSubscribeCt = null;

            if (!string.IsNullOrEmpty(userId))
            {
                userSubscribeCt = new CancellationTokenSource();
                Subscribe(userSubscribeCt.Token).Forget();
            }
        }

        private async UniTaskVoid Subscribe(CancellationToken ct)
        {
            var enumerable = clientQuestsService.Subscribe(new UserAddress { UserAddress_ = userId });
            await foreach (var userUpdate in enumerable.WithCancellation(ct))
            {
                switch (userUpdate.MessageCase)
                {
                    case UserUpdate.MessageOneofCase.QuestStateUpdate:
                        stateCache[userUpdate.QuestStateUpdate.QuestData.QuestInstanceId] = userUpdate.QuestStateUpdate.QuestData;
                        questUpdated.Write(userUpdate.QuestStateUpdate.QuestData);
                        break;
                    case UserUpdate.MessageOneofCase.NewQuestStarted:
                        stateCache[userUpdate.NewQuestStarted.QuestInstanceId] = userUpdate.NewQuestStarted;
                        questStarted.Write(userUpdate.NewQuestStarted);
                        break;
                }
            }
        }

        public async UniTask<StartQuestResponse> StartQuest(string questId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new UserIdNotSetException();

            return await clientQuestsService.StartQuest(new StartQuestRequest { QuestId = questId, UserAddress = userId });
        }

        public async UniTask<AbortQuestResponse> AbortQuest(string questInstanceId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new UserIdNotSetException();

            return await clientQuestsService.AbortQuest(new AbortQuestRequest { QuestInstanceId = questInstanceId, UserAddress = userId });
        }

        public UniTask<Quest> GetDefinition(string questId, CancellationToken cancellationToken = default)
        {
            UniTaskCompletionSource<Quest> definitionCompletionSource;

            async UniTask<Quest> RetrieveTask()
            {
                GetQuestDefinitionResponse definition = await clientQuestsService.GetQuestDefinition(new GetQuestDefinitionRequest { QuestId = questId });
                definitionCompletionSource.TrySetResult(definition.Quest);
                return definition.Quest;
            }

            if (!definitionCache.TryGetValue(questId, out definitionCompletionSource))
            {
                definitionCompletionSource = new UniTaskCompletionSource<Quest>();
                definitionCache[questId] = definitionCompletionSource;
                RetrieveTask().Forget();
            }

            return definitionCompletionSource.Task.AttachExternalCancellation(cancellationToken);
        }

        public void Dispose()
        {
            userSubscribeCt.SafeCancelAndDispose();
            userSubscribeCt = null;
            questStarted.Dispose();
            questUpdated.Dispose();
        }
    }
}
