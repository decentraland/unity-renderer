using Cysharp.Threading.Tasks;
using Decentraland.Quests;
using rpc_csharp;
using RPC.Transports;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DCLServices.QuestsService.TestScene
{
    /// <summary>
    /// This is a helper class to output all the quest updates and definitions received from the QuestServer to files
    /// It will
    /// </summary>
    public class QuestsServiceDumper : MonoBehaviour
    {
        private readonly List<QuestStateUpdate> updates = new List<QuestStateUpdate>();
        private readonly List<ProtoQuest> definitions = new List<ProtoQuest>();

        private void Awake()
        {
            Debug.Log($"Output file for updates can be found at: {ClientQuestsServiceMock.PATH_UPDATES}");
            Debug.Log($"Output file for definitions can be found at: {ClientQuestsServiceMock.PATH_DEFINITIONS}");
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            var userId = "UnityTest";
            WebSocketClientTransport webSocketClientTransport = new WebSocketClientTransport("wss://quests-rpc.decentraland.zone");
            RpcClient rpcClient = new RpcClient(webSocketClientTransport);
            var clientPort = await rpcClient.CreatePort("Test");
            var module = await clientPort.LoadModule(QuestsServiceCodeGen.ServiceName);

            ClientQuestsService client = new ClientQuestsService(module);
            var service = new QuestsService(client);
            await CollectUpdatesAsync(service, client, userId);
            await CollectQuestsDefinitionsAsync(service);
        }

        private async UniTask CollectQuestsDefinitionsAsync(QuestsService service)
        {
            definitions.Add(await service.GetDefinition("fa4d36f6-d5fe-484e-a27d-ac03bc8faca6"));
            definitions.Add(await service.GetDefinition("8e9a8bbf-2223-4f51-b7e5-660d35cedef4"));
            Debug.Log("Definitions are ready to be saved");
        }

        private async UniTask CollectUpdatesAsync(QuestsService service, ClientQuestsService client, string userId)
        {
            service.SetUserId(userId);
            service.OnQuestUpdated += (questUpdate) => { updates.Add(questUpdate); };

            // Do your interaction with the server here, such as:

            // Starting quests
            await StartQuest(service, "fa4d36f6-d5fe-484e-a27d-ac03bc8faca6");
            await StartQuest(service, "8e9a8bbf-2223-4f51-b7e5-660d35cedef4");

            // Progressing
            //await client.SendEvent(new Decentraland.Quests.Event() { Address = userId, Action = new Action() { Type = ..., Parameters = ... } });


            service.SetUserId(null);
            // Aborting every progress made to avoid adding garbage to the QuestServer
            foreach (var questStateUpdate in service.CurrentState)
            {
                var response = await service.AbortQuest(questStateUpdate.Value.QuestInstanceId);
                Debug.Log($"{questStateUpdate.Value.QuestInstanceId} aborted: {response.Accepted}");
            }

            Debug.Log("Updates are ready to be saved");
        }

        private async UniTask StartQuest(QuestsService service, string questId)
        {
            var response = await service.StartQuest(questId);
            Debug.Log($"{questId} quest started: {response.Accepted}");
        }

        [ContextMenu("Save collected updates")]
        private void SaveUpdates()
        {
            File.WriteAllLines(ClientQuestsServiceMock.PATH_UPDATES, updates.Select(x => x.ToString()));
        }

        [ContextMenu("Save collected definitions")]
        private void SaveDefinitions()
        {
            File.WriteAllLines(ClientQuestsServiceMock.PATH_DEFINITIONS, definitions.Select(x => x.ToString()));
        }
    }
}
