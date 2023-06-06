using Cysharp.Threading.Tasks;
using Decentraland.Quests;
using Google.Protobuf.WellKnownTypes;
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
        private Queue<UserUpdate> questStateUpdates = new ();
        private Dictionary<string, Quest> questDefinitions = new ();
        private Channel<UserUpdate> userUpdateChannel = Channel.CreateSingleConsumerUnbounded<UserUpdate>();

        private void Awake()
        {
            string[] updates = File.ReadAllLines(PATH_UPDATES);

            foreach (string update in updates) { questStateUpdates.Enqueue(UserUpdate.Parser.ParseJson(update)); }
        }

        public IUniTaskAsyncEnumerable<UserUpdate> Subscribe(Empty request) =>
            userUpdateChannel.Reader.ReadAllAsync();

        public async UniTask<GetQuestDefinitionResponse> GetQuestDefinition(GetQuestDefinitionRequest request) =>
            new () { Quest = questDefinitions[request.QuestId] };

        public void EnqueueChanges(int amount)
        {
            for (int i = 0; i < amount && questStateUpdates.Count > 0; i++) { userUpdateChannel.Writer.TryWrite(questStateUpdates.Dequeue()); }
        }

#region not needed for the mock
        // Not needed for the mock
        public UniTask<GetAllQuestsResponse> GetAllQuests(Empty request) =>
            throw new NotImplementedException();

        // Not needed for the mock
        public UniTask<StartQuestResponse> StartQuest(StartQuestRequest request) =>
            throw new NotImplementedException();

        // Not needed for the mock
        public UniTask<AbortQuestResponse> AbortQuest(AbortQuestRequest request) =>
            throw new NotImplementedException();

        // Not needed for the mock
        public UniTask<EventResponse> SendEvent(EventRequest request) =>
            throw new NotImplementedException();
#endregion
    }
}
