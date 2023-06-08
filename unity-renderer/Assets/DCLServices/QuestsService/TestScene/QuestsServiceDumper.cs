using Cysharp.Threading.Tasks;
using DCL;
using Decentraland.Quests;
using Google.Protobuf.WellKnownTypes;
using rpc_csharp;
using RPC.Transports;
using System;
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
        public bool justAbortAllOnGoinQuests = false;

        private Quests getAllQuests = null;
        private readonly List<UserUpdate> userUpdates = new ();
        private readonly List<Quest> definitions = new ();

        private ClientQuestsService client;
        private CancellationTokenSource subscribeCTS = new CancellationTokenSource();

        // You can find more quests here: https://quests.decentraland.zone/quests
        public string[] questsToStart =
        {
            "7d795b67-11f6-4d73-bd88-5d89cf33d679",
            "e382bd60-3292-484a-996e-7c7b0e4794c9",
        };

        [ContextMenu("start")]
        private void StartDumper()
        {
            Debug.Log($"Output file can be found at: {QuestsServiceTestScene_Utils.GET_ALL_QUESTS_FILE}");
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

            if (justAbortAllOnGoinQuests)
            {
                var response = await client.GetAllQuests(new Empty());
                foreach (QuestInstance questsInstance in response.Quests.Instances)
                {
                    Debug.Log($"Aborting quest instance {questsInstance.Id}");
                    await client.AbortQuest(new AbortQuestRequest { QuestInstanceId = questsInstance.Id });
                }
                webSocketClientTransport.Close();
                return;
            }

            await CollectQuestsDefinitionsAsync();
            await GetAllQuestsAsync();
            CollectUpdatesAsync(subscribeCTS.Token).Forget();

            //Wait 2 seconds to give time to all the subscriptions to ready themselves
            UniTask.Delay(TimeSpan.FromSeconds(2));

            ProgressQuestsAsync().Forget();
        }

        private async UniTask CollectQuestsDefinitionsAsync()
        {
            Debug.Log("Start collecting definitions");
            foreach (string questId in questsToStart)
            {
                definitions.Add((await client.GetQuestDefinition(new GetQuestDefinitionRequest { QuestId = questId })).Quest);
            }
            Debug.Log("Definitions are ready to be saved");
        }

        private async UniTask GetAllQuestsAsync()
        {
            Debug.Log("Getting all quests");
            var response = await client.GetAllQuests(new Empty());
            getAllQuests = response.Quests;
            Debug.Log("Done getting all quests");
        }

        private async UniTaskVoid ProgressQuestsAsync()
        {
            Debug.Log($"Start progressing quests:");

            // Do your interaction with the server here, such as:
            // Starting quests

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

        private async UniTask CollectUpdatesAsync(CancellationToken ct)
        {
            Debug.Log("Start collecting updates");
            await foreach (var userUpdate in client.Subscribe(new Empty()).WithCancellation(ct))
            {
                Debug.Log($"Update received: {userUpdate}");
                userUpdates.Add(userUpdate);
                switch (userUpdate.MessageCase)
                {
                    case UserUpdate.MessageOneofCase.QuestStateUpdate:
                        Debug.Log($"Quest updated: {userUpdate.QuestStateUpdate.InstanceId}");
                        break;
                    case UserUpdate.MessageOneofCase.NewQuestStarted:
                        Debug.Log($"Quest started: {userUpdate.NewQuestStarted.Quest.Id} with instanceId: {userUpdate.NewQuestStarted.Id}");
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
            foreach (UserUpdate userUpdate in userUpdates)
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

                Debug.Log($"Aborting on-going quest {questInstanceId}");
                await client.AbortQuest(new AbortQuestRequest { QuestInstanceId = questInstanceId });
            }

            await File.WriteAllTextAsync(QuestsServiceTestScene_Utils.GET_ALL_QUESTS_FILE, getAllQuests.ToString());
            await File.WriteAllLinesAsync(QuestsServiceTestScene_Utils.USER_UPDATES_FILE, userUpdates.Select(x => x.ToString()));
            await File.WriteAllLinesAsync(QuestsServiceTestScene_Utils.DEFINITIONS_FILE, definitions.Select(x => x.ToString()));
            Debug.Log("Done saving files");
        }
    }
}
