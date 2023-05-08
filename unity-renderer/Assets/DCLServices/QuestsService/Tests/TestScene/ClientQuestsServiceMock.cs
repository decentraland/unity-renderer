using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Decentraland.Quests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Event = Decentraland.Quests.Event;

namespace DCLServices.QuestsService.Tests.TestScene
{
    public class ClientQuestsServiceMock : MonoBehaviour, IClientQuestsService
    {
        [SerializeField] TextAsset questDefinitionsText;
        [SerializeField] TextAsset questUpdatesText;

        private Queue<QuestStateUpdate> questStateUpdates = new Queue<QuestStateUpdate>();
        private Dictionary<string, ProtoQuest> questDefinitions = new Dictionary<string, ProtoQuest>();
        private int amountToDequeue = 0;


        private void Awake()
        {
            var updates = JsonConvert.DeserializeObject<QuestStateUpdate[]>(questUpdatesText.text);

            foreach (var update in updates) { this.questStateUpdates.Enqueue(update); }

            var definitions = JsonConvert.DeserializeObject<(string id, ProtoQuest quest)[]>(questDefinitionsText.text);
            questDefinitions = definitions.ToDictionary(x => x.id, x => x.quest);
        }

        public async UniTask<ProtoQuest> GetQuestDefinition(QuestDefinitionRequest request) => questDefinitions[request.QuestId];


        public IUniTaskAsyncEnumerable<UserUpdate> Subscribe(UserAddress request)
        {
            return UniTaskAsyncEnumerable.Create<UserUpdate>(async (writer, cancellationToken) =>
            {
                while (true)
                {
                    while(amountToDequeue > 0 && questStateUpdates.Count > 0)
                    {
                        amountToDequeue--;
                        await writer.YieldAsync(new UserUpdate
                        {
                            EventIgnored = (int)UserUpdate.MessageOneofCase.QuestState,
                            QuestState = questStateUpdates.Dequeue()
                        });
                    }

                    await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
                }
            });
        }

        public void EnqueueChanges(int amount)
        {
            amountToDequeue += amount;
        }

#region not needed for the mock
        // Not needed for the mock
        public UniTask<StartQuestResponse> StartQuest(StartQuestRequest request) =>
            throw new NotImplementedException();

        // Not needed for the mock
        public UniTask<AbortQuestResponse> AbortQuest(AbortQuestRequest request) =>
            throw new NotImplementedException();

        // Not needed for the mock
        public UniTask<EventResponse> SendEvent(Event request) =>
            throw new NotImplementedException();

        // Not needed for the mock
        public UniTask<Quests> GetAllQuests(UserAddress request) =>
            throw new NotImplementedException();
#endregion
    }
}
