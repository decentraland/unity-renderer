using Cysharp.Threading.Tasks;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Event = Decentraland.Quests.Event;

namespace DCLServices.QuestsService.TestScene
{
    public class ClientQuestsServiceMock : MonoBehaviour, IClientQuestsService
    {
        public static string PATH_UPDATES => Application.dataPath + "/../../updates.txt";
        public static string PATH_DEFINITIONS => Application.dataPath + "/../../definitions.txt";


        public int QuestStateUpdatesCount => questStateUpdates.Count;
        private Queue<QuestStateUpdate> questStateUpdates = new Queue<QuestStateUpdate>();
        private Dictionary<string, ProtoQuest> questDefinitions = new Dictionary<string, ProtoQuest>();
        private Channel<UserUpdate> userUpdateChannel = Channel.CreateSingleConsumerUnbounded<UserUpdate>();


        private void Awake()
        {
            string[] updates = File.ReadAllLines(PATH_UPDATES);
            foreach (string update in updates)
            {
                questStateUpdates.Enqueue(QuestStateUpdate.Parser.ParseJson(update));
                Debug.Log($"enqueued: {questStateUpdates.Last().Name}");
            }
        }

        public async UniTask<ProtoQuest> GetQuestDefinition(QuestDefinitionRequest request) => questDefinitions[request.QuestId];


        public IUniTaskAsyncEnumerable<UserUpdate> Subscribe(UserAddress request) =>
            userUpdateChannel.Reader.ReadAllAsync();

        public void EnqueueChanges(int amount)
        {
            Debug.Log($"Enqueueing {amount}");
            for(int i = 0; i < amount && questStateUpdates.Count > 0; i++)
            {
                userUpdateChannel.Writer.TryWrite(new UserUpdate
                {
                    EventIgnored = (int)UserUpdate.MessageOneofCase.QuestState,
                    QuestState = questStateUpdates.Dequeue()
                });
            }
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
