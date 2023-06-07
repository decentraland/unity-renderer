using Cysharp.Threading.Tasks;
using DCL;
using Decentraland.Quests;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using rpc_csharp;
using RPC.Transports;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCLServices.QuestsService.TestScene
{
    /// <summary>
    /// This is a helper class to output all the quest updates and definitions received from the QuestServer to files
    /// It will
    /// </summary>
    public class QuestsServiceDumper : MonoBehaviour
    {
        private readonly List<UserUpdate> updates = new ();
        private readonly List<Quest> definitions = new ();

        private ClientQuestsService client;
        private CancellationTokenSource subscribeCTS = new CancellationTokenSource();
        private bool listeningToQuests = false;

        [ContextMenu("start")]
        private void StartDumper()
        {
            Debug.Log($"Output file for updates can be found at: {ClientQuestsServiceMock.PATH_UPDATES}");
            Debug.Log($"Output file for definitions can be found at: {ClientQuestsServiceMock.PATH_DEFINITIONS}");
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            // auth requires a fake wallet and a signing process, we dont have the tools to do that in Unity at the moment
            // If you encounter this in a review it means I couldnt solve the issue, please yell at me so I can remove it

            var rpcSignRequest = new RPCSignRequest(DCL.Environment.i.serviceLocator.Get<IRPC>());
            AuthedWebSocketClientTransport webSocketClientTransport = new AuthedWebSocketClientTransport(rpcSignRequest, "wss://quests-rpc.decentraland.zone");
            await webSocketClientTransport.Connect();
            Debug.Log($"RPC Authenticated");

            RpcClient rpcClient = new RpcClient(webSocketClientTransport);
            var clientPort = await rpcClient.CreatePort("UnityTest");
            var module = await clientPort.LoadModule(QuestsServiceCodeGen.ServiceName);
            client = new ClientQuestsService(module);

            await CollectQuestsDefinitionsAsync(client);
            ProgressQuestsAsync(client).Forget();
            await CollectUpdatesAsync(client, subscribeCTS.Token);
        }

        private async UniTask CollectQuestsDefinitionsAsync(ClientQuestsService client)
        {
            Debug.Log("Start collecting definitions");
            definitions.Add((await client.GetQuestDefinition(new GetQuestDefinitionRequest { QuestId = "fa4d36f6-d5fe-484e-a27d-ac03bc8faca6" })).Quest);
            definitions.Add((await client.GetQuestDefinition(new GetQuestDefinitionRequest { QuestId = "8e9a8bbf-2223-4f51-b7e5-660d35cedef4" })).Quest);
            Debug.Log("Definitions are ready to be saved");
        }

        private async UniTaskVoid ProgressQuestsAsync(ClientQuestsService client)
        {
            Debug.Log($"Start progressing quests:");

            await UniTask.WaitUntil(() => this.listeningToQuests);

            // Do your interaction with the server here, such as:
            // Starting quests
            string[] questsToStart =
            {
                "fa4d36f6-d5fe-484e-a27d-ac03bc8faca6",
                "8e9a8bbf-2223-4f51-b7e5-660d35cedef4",
            };

            foreach (string questId in questsToStart)
            {
                var startQuestRequest = new StartQuestRequest { QuestId = questId };
                var response = await client.StartQuest(startQuestRequest);
                Debug.Log(response.ToString());
            }

            // Progressing
            //await client.SendEvent(new Decentraland.Quests.Event() { Action = new Action() { Type = ..., Parameters = ... } });

            Debug.Log("All progress is done, waiting for updates to be saved...");
        }

        private async UniTask CollectUpdatesAsync(ClientQuestsService client, CancellationToken ct)
        {
            listeningToQuests = true;
            await foreach (var userUpdate in client.Subscribe(new Empty()).WithCancellation(ct))
            {
                updates.Add(userUpdate);
                switch (userUpdate.MessageCase)
                {
                    case UserUpdate.MessageOneofCase.QuestStateUpdate:
                        Debug.Log($"Quest updated: {userUpdate.ToString()}");
                        break;
                    case UserUpdate.MessageOneofCase.NewQuestStarted:
                        Debug.Log($"Quest started: {userUpdate.ToString()}");
                        break;
                }
            }

            Debug.Log("Updates are ready to be saved");
        }

        [ContextMenu("Finish")]
        private void Finish()
        {
            FinishAsync().Forget();
        }

        private async UniTaskVoid FinishAsync()
        {
            subscribeCTS?.Cancel();
            subscribeCTS?.Dispose();
            subscribeCTS = null;

            Debug.Log("Starting saving files");

            // Abort all quests to remove progress
            foreach (UserUpdate userUpdate in updates)
            {
                string questInstanceId = null;

                switch (userUpdate.MessageCase)
                {
                    case UserUpdate.MessageOneofCase.QuestStateUpdate:
                        questInstanceId = userUpdate.QuestStateUpdate.InstanceId;
                        break;
                    case UserUpdate.MessageOneofCase.NewQuestStarted:
                        questInstanceId = userUpdate.NewQuestStarted.Id;
                        break;
                }

                await client.AbortQuest(new AbortQuestRequest { QuestInstanceId = questInstanceId });
            }

            await File.WriteAllLinesAsync(ClientQuestsServiceMock.PATH_UPDATES, updates.Select(x => x.ToString()));
            await File.WriteAllLinesAsync(ClientQuestsServiceMock.PATH_DEFINITIONS, definitions.Select(x => x.ToString()));
            Debug.Log("Done saving files");
        }
    }
}
