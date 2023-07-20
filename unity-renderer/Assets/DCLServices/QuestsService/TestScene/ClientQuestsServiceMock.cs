using Cysharp.Threading.Tasks;
using Decentraland.Quests;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
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
        public int UserUpdatesCount => userUpdates.Count;
        private Quests getAllQuests = new ();
        private readonly Queue<UserUpdate> userUpdates = new ();
        private readonly Dictionary<string, Quest> questDefinitions = new ();
        private Channel<UserUpdate> userUpdateChannel = Channel.CreateSingleConsumerUnbounded<UserUpdate>();

        private void Awake()
        {
            userUpdates.Clear();
            questDefinitions.Clear();
            userUpdateChannel = Channel.CreateSingleConsumerUnbounded<UserUpdate>();

            #if UNITY_EDITOR
            getAllQuests = Quests.Parser.ParseJson(File.ReadAllText(QuestsServiceTestScene_Utils.GET_ALL_QUESTS_FILE));
            string[] userUpdatesEntries = File.ReadAllLines(QuestsServiceTestScene_Utils.USER_UPDATES_FILE);
            foreach (string userUpdatesEntry in userUpdatesEntries)
                userUpdates.Enqueue(UserUpdate.Parser.ParseJson(userUpdatesEntry));

            string[] definitionsEntries = File.ReadAllLines(QuestsServiceTestScene_Utils.DEFINITIONS_FILE);
            foreach (string definitionEntry in definitionsEntries)
            {
                var definition = Quest.Parser.ParseJson(definitionEntry);
                questDefinitions[definition.Id] = definition;
            }
            #endif
        }

        public UniTask<GetAllQuestsResponse> GetAllQuests(Empty request) => new (new GetAllQuestsResponse() { Quests = getAllQuests });

        public IUniTaskAsyncEnumerable<UserUpdate> Subscribe(Empty request) => userUpdateChannel.Reader.ReadAllAsync();

        public async UniTask<GetQuestDefinitionResponse> GetQuestDefinition(GetQuestDefinitionRequest request) => new () { Quest = questDefinitions[request.QuestId] };

        public void EnqueueChanges(int amount)
        {
            for (int i = 0; i < amount && userUpdates.Count > 0; i++) { userUpdateChannel.Writer.TryWrite(userUpdates.Dequeue()); }
        }

#region not needed for the mock
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
