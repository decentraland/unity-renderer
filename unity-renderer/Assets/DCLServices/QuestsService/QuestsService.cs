using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Tasks;
using Decentraland.Quests;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading;
using DCL;
using UnityEngine;
using UnityEngine.Networking;
using DCLServices.QuestsService;

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
        private const bool VERBOSE = false;

        public IAsyncEnumerableWithEvent<QuestInstance> QuestStarted => questStarted;
        public IAsyncEnumerableWithEvent<QuestInstance> QuestUpdated => questUpdated;

        private readonly AsyncEnumerableWithEvent<QuestInstance> questStarted = new ();
        private readonly AsyncEnumerableWithEvent<QuestInstance> questUpdated = new ();

        public IReadOnlyDictionary<string, QuestInstance> QuestInstances => questInstances;
        public IReadOnlyDictionary<string, object> QuestRewards { get; }

        internal readonly Dictionary<string, QuestInstance> questInstances = new ();

        internal readonly IClientQuestsService clientQuestsService;
        internal readonly IQuestRewardsResolver questRewardsResolver;
        private Service<IWebRequestController> webRequestController;
        internal readonly Dictionary<string, UniTaskCompletionSource<Quest>> definitionCache = new ();
        internal readonly Dictionary<string, UniTaskCompletionSource<IReadOnlyList<QuestReward>>> rewardsCache = new ();

        internal readonly CancellationTokenSource disposeCts = new ();
        internal readonly UniTaskCompletionSource gettingInitialState = new ();

        public QuestsService(IClientQuestsService clientQuestsService, IQuestRewardsResolver questRewardsResolver)
        {
            this.clientQuestsService = clientQuestsService;
            this.questRewardsResolver = questRewardsResolver;
            Subscribe().Forget();
        }

        private async UniTaskVoid Subscribe()
        {
            //Obtain initial state
            var allquests = await clientQuestsService.GetAllQuests(new Empty());

            if(VERBOSE)
                Debug.Log("[QuestsService] Getting all quests");

            foreach (QuestInstance questInstance in allquests.Quests.Instances)
            {
                if(VERBOSE)
                    Debug.Log($"[QuestsService]\n{questInstance}");
                questInstances[questInstance.Id] = questInstance;
                string questId = questInstance.Quest.Id;

                if (!definitionCache.TryGetValue(questId, out var completionSource))
                    definitionCache[questId] = completionSource = new UniTaskCompletionSource<Quest>();

                completionSource.TrySetResult(questInstance.Quest);

                questUpdated.Write(questInstance);
            }

            //Listen to updates
            var enumerable = clientQuestsService.Subscribe(new Empty());

            if(VERBOSE)
                Debug.Log($"[QuestsService] Subscribing");

            await foreach (UserUpdate userUpdate in enumerable.WithCancellation(disposeCts.Token))
            {
                if(VERBOSE)
                    Debug.Log($"[QuestsService] Update:\n{userUpdate}");
                switch (userUpdate.MessageCase)
                {
                    case UserUpdate.MessageOneofCase.Subscribed:
                        gettingInitialState.TrySetResult();
                        break;
                    case UserUpdate.MessageOneofCase.QuestStateUpdate:
                        if (!questInstances.TryGetValue(userUpdate.QuestStateUpdate.InstanceId, out var questUpdatedInstance))
                        {
                            if(VERBOSE)
                                Debug.Log($"Received quest update which instance was not received before: {userUpdate.ToString()}");

                            continue;
                        }
                        questUpdatedInstance.State = userUpdate.QuestStateUpdate.QuestState;
                        questUpdated.Write(questUpdatedInstance);
                        break;

                    case UserUpdate.MessageOneofCase.NewQuestStarted:
                        var questInstance = userUpdate.NewQuestStarted;
                        questInstances[questInstance.Id] = questInstance;

                        string questId = questInstance.Quest.Id;
                        if (!definitionCache.TryGetValue(questId, out var completionSource))
                            definitionCache[questId] = completionSource = new UniTaskCompletionSource<Quest>();

                        questStarted.Write(questInstance);
                        completionSource.TrySetResult(questInstance.Quest);
                        break;
                }
            }
        }

        public async UniTask<StartQuestResponse> StartQuest(string questId)
        {
            await gettingInitialState.Task;
            return await clientQuestsService.StartQuest(new StartQuestRequest { QuestId = questId });
        }

        public async UniTask<AbortQuestResponse> AbortQuest(string questInstanceId)
        {
            await gettingInitialState.Task;
            AbortQuestResponse abortQuestResponse = await clientQuestsService.AbortQuest(new AbortQuestRequest { QuestInstanceId = questInstanceId });

            if (abortQuestResponse.ResponseCase == AbortQuestResponse.ResponseOneofCase.Accepted)
                questInstances.Remove(questInstanceId);
            return abortQuestResponse;
        }

        public UniTask<Quest> GetDefinition(string questId, CancellationToken cancellationToken = default)
        {
            UniTaskCompletionSource<Quest> definitionCompletionSource;

            async UniTask<Quest> RetrieveTask()
            {
                GetQuestDefinitionResponse definition = await clientQuestsService.GetQuestDefinition(new GetQuestDefinitionRequest { QuestId = questId });

                if (definitionCache.TryGetValue(definition.Quest.Id, out definitionCompletionSource))
                    definitionCache[definition.Quest.Id].TrySetResult(definition.Quest);

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

        public UniTask<IReadOnlyList<QuestReward>> GetQuestRewards(string questId, CancellationToken cancellationToken = default)
        {
            UniTaskCompletionSource<IReadOnlyList<QuestReward>> rewardsCompletionSource;

            async UniTask<IReadOnlyList<QuestReward>> RetrieveTask()
            {
                IReadOnlyList<QuestReward> response = await questRewardsResolver.ResolveRewards(questId, cancellationToken);

                if (rewardsCache.TryGetValue(questId, out rewardsCompletionSource))
                    rewardsCache[questId].TrySetResult(response);

                return response;
            }

            if (!rewardsCache.TryGetValue(questId, out rewardsCompletionSource))
            {
                rewardsCompletionSource = new UniTaskCompletionSource<IReadOnlyList<QuestReward>>();
                rewardsCache[questId] = rewardsCompletionSource;
                RetrieveTask().Forget();
            }

            return rewardsCompletionSource.Task.AttachExternalCancellation(cancellationToken);
        }

        public void Dispose()
        {
            disposeCts.SafeCancelAndDispose();
            questStarted.Dispose();
            questUpdated.Dispose();
        }
    }
}
