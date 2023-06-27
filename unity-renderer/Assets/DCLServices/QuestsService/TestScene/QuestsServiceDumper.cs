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
using WebSocketSharp;

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
        private bool isSubscribed = false;

        // You can find more quests here: https://quests.decentraland.zone/quests
        public string[] questsToStart =
        {
            "7d795b67-11f6-4d73-bd88-5d89cf33d679",
            "e382bd60-3292-484a-996e-7c7b0e4794c9",
        };

        [ContextMenu("start")]
        private void StartDumper()
        {
            MyLog($"Output file can be found at: {QuestsServiceTestScene_Utils.GET_ALL_QUESTS_FILE}");
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            // auth requires a fake wallet and a signing process, we dont have the tools to do that in Unity at the moment
            // If you encounter this in a review it means I couldnt solve the issue, please yell at me so I can remove it

            var rpcSignRequest = new RPCSignRequest(DCL.Environment.i.serviceLocator.Get<IRPC>());
            AuthedWebSocketClientTransport webSocketClientTransport = new AuthedWebSocketClientTransport(rpcSignRequest, "wss://quests-rpc.decentraland.zone");
            //TODO Quest Server is not accepting the correct url and by now it needs "/". Change it as soon as QuestServer is ready to have a generic authed WebSocket Client
            await webSocketClientTransport.Connect("/");
            MyLog($"RPC Authenticated");

            RpcClient rpcClient = new RpcClient(webSocketClientTransport);
            var clientPort = await rpcClient.CreatePort("UnityTest");
            var module = await clientPort.LoadModule(QuestsServiceCodeGen.ServiceName);
            client = new ClientQuestsService(module);

            if (justAbortAllOnGoinQuests)
            {
                var response = await client.GetAllQuests(new Empty());
                foreach (QuestInstance questsInstance in response.Quests.Instances)
                {
                    MyLog($"Aborting quest instance {questsInstance.Id}");
                    await client.AbortQuest(new AbortQuestRequest { QuestInstanceId = questsInstance.Id });
                }
                webSocketClientTransport.Close();
                return;
            }

            isSubscribed = false;
            await CollectQuestsDefinitionsAsync();
            await GetAllQuestsAsync();
            CollectUpdatesAsync(subscribeCTS.Token).Forget();

            await UniTask.WaitUntil(() => isSubscribed);


            ProgressQuestsAsync().Forget();
        }

        private async UniTask CollectQuestsDefinitionsAsync()
        {
            MyLog("Start collecting definitions");
            foreach (string questId in questsToStart)
            {
                definitions.Add((await client.GetQuestDefinition(new GetQuestDefinitionRequest { QuestId = questId })).Quest);
            }
            MyLog("Definitions are ready to be saved");
        }

        private async UniTask GetAllQuestsAsync()
        {
            MyLog("Getting all quests");
            var response = await client.GetAllQuests(new Empty());
            getAllQuests = response.Quests;
            MyLog("Done getting all quests");
        }

        private async UniTaskVoid ProgressQuestsAsync()
        {
            MyLog($"Start progressing quests:");

            // Do your interaction with the server here, such as:
            // Starting quests

            foreach (string questId in questsToStart)
            {
                var startQuestRequest = new StartQuestRequest { QuestId = questId };
                var response = await client.StartQuest(startQuestRequest);
                MyLog(response.ToString());
            }

            // Progressing
            await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "bCdjn5J3uz"},
            }} });
            await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "WPJjSiPSPL"},
            }} });
            await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "2cvNHyXQfw"}
            }} });
            await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "0T9iP8SA2I"}
            }} });
            /*await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "jus9C7Nx6v"}
            }} });
            await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "VkJEhYIW9q"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "FzQrSuvvTD"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "teCVvT2avh"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "4tQG9K093S"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "vE3Kgk6kxs"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "mcvXd3t1Sz"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "Qmsj8cOMfe"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "S5tS6Dyljp"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "x7oxKiWatD"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "lbHA0V4ODF"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "EMY3T1FbfL"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "yVXDSVfjyP"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "a3dlL3FVrw"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "EIQ9xKXFtV"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "3GT6ivQQYW"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "Uz20JJ2Jzn"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "xFJtODcwvK"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "EzgJnuSmSf"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "Mt0x92HDZc"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "T5zrtHeZWk"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "jMsR7bmbqT"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "H3a6wdo3KM"}
            }} });await client.SendEvent(new Decentraland.Quests.EventRequest() { Action = new Decentraland.Quests.Action() { Type = "CUSTOM", Parameters =
            {
                {"id", "6MgAVjxNi8"}
            }} });*/

            MyLog("All progress is done, waiting for updates to be saved...");
        }

        private async UniTask CollectUpdatesAsync(CancellationToken ct)
        {
            MyLog("Start collecting updates");
            await foreach (var userUpdate in client.Subscribe(new Empty()).WithCancellation(ct))
            {
                MyLog($"Update received: {userUpdate}");
                userUpdates.Add(userUpdate);
                switch (userUpdate.MessageCase)
                {
                    //Re add when proto is updated
                    case UserUpdate.MessageOneofCase.Subscribed:
                        isSubscribed = true;
                        MyLog($"Subscription ready: {userUpdate.Subscribed}");
                        break;
                    case UserUpdate.MessageOneofCase.QuestStateUpdate:
                        MyLog($"Quest updated: {userUpdate.QuestStateUpdate.InstanceId}");
                        break;
                    case UserUpdate.MessageOneofCase.NewQuestStarted:
                        MyLog($"Quest started: {userUpdate.NewQuestStarted.Quest.Id} with instanceId: {userUpdate.NewQuestStarted.Id}");
                        break;
                }
            }

            MyLog("Updates are ready to be saved");
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

            MyLog("Starting saving files");

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

                MyLog($"Aborting on-going quest {questInstanceId}");
                await client.AbortQuest(new AbortQuestRequest { QuestInstanceId = questInstanceId });
            }

            await File.WriteAllTextAsync(QuestsServiceTestScene_Utils.GET_ALL_QUESTS_FILE, getAllQuests.ToString());
            await File.WriteAllLinesAsync(QuestsServiceTestScene_Utils.USER_UPDATES_FILE, userUpdates.Select(x => x.ToString()));
            await File.WriteAllLinesAsync(QuestsServiceTestScene_Utils.DEFINITIONS_FILE, definitions.Select(x => x.ToString()));
            MyLog("Done saving files");
        }

        private void MyLog(string l)
        {
            bool old = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;
            Debug.Log($"TestQuests: {l}");
            Debug.unityLogger.logEnabled = old;
        }
    }
}
