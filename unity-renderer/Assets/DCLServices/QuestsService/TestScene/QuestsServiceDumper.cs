using Cysharp.Threading.Tasks;
using Decentraland.Quests;
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
        private const string USER_ID = "UnityTest";
        private readonly List<UserUpdate> updates = new ();
        private readonly List<Quest> definitions = new ();

        private ClientQuestsService client;
        private CancellationTokenSource subscribeCTS = new CancellationTokenSource();

        private void Awake()
        {
            Debug.Log($"Output file for updates can be found at: {ClientQuestsServiceMock.PATH_UPDATES}");
            Debug.Log($"Output file for definitions can be found at: {ClientQuestsServiceMock.PATH_DEFINITIONS}");
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            Debug.Log($"Initializing");
            WebSocketClientTransport webSocketClientTransport = new WebSocketClientTransport("wss://quests-rpc.decentraland.zone");
            RpcClient rpcClient = new RpcClient(webSocketClientTransport);
            Debug.Log($"Initializing 1");
            var clientPort = await rpcClient.CreatePort("UnityTest");
            Debug.Log($"Initializing 2");
            var module = await clientPort.LoadModule(QuestsServiceCodeGen.ServiceName);
            Debug.Log($"Initializing 3");
            client = new ClientQuestsService(module);

            await CollectQuestsDefinitionsAsync(client);
            ProgressQuestsAsync(client).Forget();
            await CollectUpdatesAsync(client, subscribeCTS.Token);
        }

        private async UniTask CollectQuestsDefinitionsAsync(ClientQuestsService client)
        {
            Debug.Log("Start collecting definitions");
            definitions.Add((await client.GetQuestDefinition(new GetQuestDefinitionRequest{QuestId = "fa4d36f6-d5fe-484e-a27d-ac03bc8faca6"})).Quest);
            definitions.Add((await client.GetQuestDefinition(new GetQuestDefinitionRequest{QuestId = "8e9a8bbf-2223-4f51-b7e5-660d35cedef4"})).Quest);
            Debug.Log("Definitions are ready to be saved");
        }

        private async UniTaskVoid ProgressQuestsAsync(ClientQuestsService client)
        {
            Debug.Log("Start progressing quests");
            // Do your interaction with the server here, such as:
            // Starting quests
            string[] questsToStart =
            {
                "fa4d36f6-d5fe-484e-a27d-ac03bc8faca6",
                "8e9a8bbf-2223-4f51-b7e5-660d35cedef4",
            };

            foreach (string questId in questsToStart)
            {
                var response = await client.StartQuest(new StartQuestRequest { UserAddress = USER_ID, QuestId = questId });
            }

            // Progressing
            //await client.SendEvent(new Decentraland.Quests.Event() { Address = userId, Action = new Action() { Type = ..., Parameters = ... } });

            Debug.Log("All progress is done, waiting for updates to be saved...");
        }

        private async UniTask CollectUpdatesAsync(ClientQuestsService client, CancellationToken ct)
        {
            var  enumerator = client.Subscribe(new UserAddress { UserAddress_ = USER_ID }).GetAsyncEnumerator(ct);
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    var userUpdate = enumerator.Current;
                    updates.Add(userUpdate);

                    switch (userUpdate.MessageCase)
                    {
                        case UserUpdate.MessageOneofCase.QuestStateUpdate:
                            Debug.Log($"Quest updated: {userUpdate.QuestStateUpdate.QuestData.Name} with id {userUpdate.QuestStateUpdate.QuestData.QuestInstanceId}");
                            break;
                        case UserUpdate.MessageOneofCase.NewQuestStarted:
                            Debug.Log($"Quest started: {userUpdate.QuestStateUpdate.QuestData.Name} with id {userUpdate.QuestStateUpdate.QuestData.QuestInstanceId}");
                            break;
                    }
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
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
                        questInstanceId = userUpdate.QuestStateUpdate.QuestData.QuestInstanceId;
                        break;
                    case UserUpdate.MessageOneofCase.NewQuestStarted:
                        questInstanceId = userUpdate.NewQuestStarted.QuestInstanceId;
                        break;
                }

                await client.AbortQuest(new AbortQuestRequest { UserAddress = USER_ID, QuestInstanceId = questInstanceId });
            }

            await File.WriteAllLinesAsync(ClientQuestsServiceMock.PATH_UPDATES, updates.Select(x => x.ToString()));
            await File.WriteAllLinesAsync(ClientQuestsServiceMock.PATH_DEFINITIONS, definitions.Select(x => x.ToString()));
            Debug.Log("Done saving files");
        }
    }
}
