using AvatarAssets;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.Emotes;
using Decentraland.Renderer.RendererServices;
using NSubstitute;
using NUnit.Framework;
using RPC;
using rpc_csharp;
using rpc_csharp.transport;
using RPC.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

namespace Tests
{
    public class TriggerSceneExpressionShould
    {
        private IWorldState worldState;
        private ISceneController sceneController;
        private UserProfile userProfile;
        private BaseVariable<Player> ownPlayer;

        private IDictionary<IParcelScene, HashSet<(string bodyShape, string emoteId)>> equippedEmotesByScene;
        private IDictionary<IParcelScene, CancellationTokenSource> cancellationTokenSources;
        private IClientEmotesRendererService client;
        private IEmotesService emotesService;
        private EmotesRendererServiceImpl emotesRendererServiceImpl;
        private IEmoteReference emoteReference;
        private IAvatarEmotesController avatar => ownPlayer.Get().avatar.GetEmotesController();

        [SetUp]
        public async Task SetUp()
        {
            worldState = Substitute.For<IWorldState>();
            sceneController = Substitute.For<ISceneController>();
            emotesService = Substitute.For<IEmotesService>();
            userProfile = UserProfile.GetOwnUserProfile();
            ownPlayer = new BaseVariable<Player>();
            equippedEmotesByScene = new Dictionary<IParcelScene, HashSet<(string bodyShape, string emoteId)>>();
            cancellationTokenSources = new Dictionary<IParcelScene, CancellationTokenSource>();
            Environment.i.serviceLocator.Register<IRPC>(() => Substitute.For<IRPC>());
            client = await CreateRpcClient();
            await Environment.i.serviceLocator.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            Resources.UnloadAsset(userProfile);
        }

        [Test]
        public async Task Fail_When_SceneNotLoaded()
        {
            var result = await client.TriggerSceneExpression(
                new TriggerSceneExpressionRequest
                {
                    Path = "local/emote",
                    SceneNumber = 666,
                });

            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task Fail_When_EmoteNotPresentInSceneMapping()
        {
            var contentProvider = new ContentProvider
            {
                contents =
                {
                    new ContentServerUtils.MappingPair
                        { file = "theEmote", hash = "0066688" },
                },
            };

            contentProvider.BakeHashes();

            IParcelScene scene = Substitute.For<IParcelScene>();
            scene.contentProvider.Returns(contentProvider);

            worldState.TryGetScene(666, out Arg.Any<IParcelScene>())
                      .Returns(info =>
                       {
                           info[1] = scene;
                           return true;
                       });

            var result = await client.TriggerSceneExpression(
                new TriggerSceneExpressionRequest
                {
                    Path = "NOT_THE_EMOTE",
                    SceneNumber = 666,
                });

            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task Fail_When_NoUserAvatarSet()
        {
            var contentProvider = new ContentProvider
            {
                contents =
                {
                    new ContentServerUtils.MappingPair
                        { file = "theEmote", hash = "0066688" },
                },
            };

            contentProvider.BakeHashes();

            IParcelScene scene = Substitute.For<IParcelScene>();
            scene.contentProvider.Returns(contentProvider);

            worldState.TryGetScene(666, out Arg.Any<IParcelScene>())
                      .Returns(info =>
                       {
                           info[1] = scene;
                           return true;
                       });

            var result = await client.TriggerSceneExpression(
                new TriggerSceneExpressionRequest
                {
                    Path = "theEmote",
                    SceneNumber = 666,
                });

            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task Success_LoadingAndPlayingEmote_Twice()
        {
            const string EMOTE_PATH = "theEmote";
            const int SCENE_NUMBER = 666;

            var (emoteData, scene) = SetUpWorkingEnvironment(SCENE_NUMBER, EMOTE_PATH);

            var result = await client.TriggerSceneExpression(
                new TriggerSceneExpressionRequest
                {
                    Path = EMOTE_PATH,
                    SceneNumber = SCENE_NUMBER,
                });

            result = await client.TriggerSceneExpression(
                new TriggerSceneExpressionRequest
                {
                    Path = EMOTE_PATH,
                    SceneNumber = SCENE_NUMBER,
                });

            Assert.IsTrue(result.Success);

            avatar.Received(1).EquipEmote(emoteData.emoteId, emoteReference);
            Assert.AreEqual(userProfile.avatar.expressionTriggerId, emoteData.emoteId);
        }

        [Test]
        public async Task Success_LoadingAndPlayingEmote()
        {
            const string EMOTE_PATH = "theEmote";
            const int SCENE_NUMBER = 666;

            var (emoteData, scene) = SetUpWorkingEnvironment(SCENE_NUMBER, EMOTE_PATH);

            var result = await client.TriggerSceneExpression(
                new TriggerSceneExpressionRequest
                {
                    Path = EMOTE_PATH,
                    SceneNumber = SCENE_NUMBER,
                });

            Assert.IsTrue(result.Success);
            avatar.Received(1).EquipEmote(emoteData.emoteId, emoteReference);
            Assert.AreEqual(emoteData.emoteId, userProfile.avatar.expressionTriggerId);
            Assert.IsTrue(equippedEmotesByScene[scene].Contains(emoteData));
        }

        [Test]
        public async Task Cancelled_When_SceneRemovedWhileLoading()
        {
            const string EMOTE_PATH = "theEmote";
            const int SCENE_NUMBER = 666;

            var (emoteData, scene) = SetUpWorkingEnvironment(SCENE_NUMBER, EMOTE_PATH);

            await UniTask.Yield();
            cancellationTokenSources[scene].Cancel();

            var result = await client.TriggerSceneExpression(
                new TriggerSceneExpressionRequest
                {
                    Path = EMOTE_PATH,
                    SceneNumber = SCENE_NUMBER,
                });

            Assert.IsFalse(result.Success);
            Assert.IsTrue(equippedEmotesByScene[scene].Count == 0);
            avatar.Received(1).EquipEmote(emoteData.emoteId, emoteReference);
        }

        [Test]
        public async Task CleanUp_When_SceneIsUnloaded()
        {
            const string EMOTE_PATH = "theEmote";
            const int SCENE_NUMBER = 666;

            var (emoteData, scene) = SetUpWorkingEnvironment(SCENE_NUMBER, EMOTE_PATH);

            var result = await client.TriggerSceneExpression(
                new TriggerSceneExpressionRequest
                {
                    Path = EMOTE_PATH,
                    SceneNumber = SCENE_NUMBER,
                });

            Assert.IsTrue(result.Success);
            Assert.IsTrue(equippedEmotesByScene[scene].Count == 1);

            sceneController.OnSceneRemoved += Raise.Event<Action<IParcelScene>>(scene);

            Assert.IsFalse(equippedEmotesByScene.ContainsKey(scene));
            emoteReference.Received(1).Dispose();
        }

        [Test]
        public async Task GenerateEmoteId_Correctly()
        {
            const string EMOTE_PATH = "theEmote";
            const int SCENE_NUMBER = 666;

            var (emoteData, scene) = SetUpWorkingEnvironment(SCENE_NUMBER, EMOTE_PATH);

            var result = await client.TriggerSceneExpression(
                new TriggerSceneExpressionRequest
                {
                    Path = EMOTE_PATH,
                    SceneNumber = SCENE_NUMBER,
                    Loop = true,
                });

            Assert.IsTrue(result.Success);
            Assert.IsTrue(SceneEmoteHelper.TryGetDataFromEmoteId(userProfile.avatar.expressionTriggerId, out string hash, out bool loop));
            Assert.IsTrue(loop);
            Assert.AreEqual(scene.contentProvider.contents[0].hash, hash);

            result = await client.TriggerSceneExpression(
                new TriggerSceneExpressionRequest
                {
                    Path = EMOTE_PATH,
                    SceneNumber = SCENE_NUMBER,
                    Loop = false,
                });

            Assert.IsTrue(result.Success);
            Assert.IsTrue(SceneEmoteHelper.TryGetDataFromEmoteId(userProfile.avatar.expressionTriggerId, out hash, out loop));
            Assert.IsFalse(loop);
            Assert.AreEqual(scene.contentProvider.contents[0].hash, hash);
        }

        private ((string bodyShape, string emoteId) emoteData, IParcelScene scene) SetUpWorkingEnvironment(int sceneNumber, string emotePath)
        {
            const string BODY_SHAPE = "someBodyShape";

            var contentProvider = new ContentProvider
            {
                contents =
                {
                    new ContentServerUtils.MappingPair
                        { file = emotePath, hash = "0066688" },
                },
            };

            contentProvider.BakeHashes();

            IParcelScene scene = Substitute.For<IParcelScene>();
            scene.contentProvider.Returns(contentProvider);

            worldState.TryGetScene(sceneNumber, out Arg.Any<IParcelScene>())
                      .Returns(info =>
                       {
                           info[1] = scene;
                           return true;
                       });

            worldState.ContainsScene(sceneNumber).Returns(true);

            var playerModel = new Player
            {
                avatar = Substitute.For<IAvatar>(),
            };

            ownPlayer.Set(playerModel);
            userProfile.avatar.bodyShape = BODY_SHAPE;

            SceneEmoteHelper.TryGenerateEmoteId(scene, emotePath, false, out string emoteId);
            emoteReference = Substitute.For<IEmoteReference>();
            emotesService.RequestEmote(BODY_SHAPE, emoteId, Arg.Any<CancellationToken>()).Returns(UniTask.FromResult(emoteReference));
            return (emoteData: (bodyShape: BODY_SHAPE, emoteId: emoteId), scene: scene);
        }

        private async UniTask<IClientEmotesRendererService> CreateRpcClient()
        {
            var (clientTransport, serverTransport) = MemoryTransport.Create();

            var context = new RPCContext();
            var rpcServer = new RpcServer<RPCContext>();
            rpcServer.AttachTransport(serverTransport, context);

            rpcServer.SetHandler((port, t, c) =>
            {
                emotesRendererServiceImpl = new EmotesRendererServiceImpl(
                    port,
                    worldState,
                    sceneController,
                    userProfile,
                    ownPlayer,
                    emotesService,
                    equippedEmotesByScene,
                    cancellationTokenSources
                );

                EmotesRendererServiceCodeGen.RegisterService(port,
                    emotesRendererServiceImpl);
            });

            RpcClient client = new RpcClient(clientTransport);
            RpcClientPort port = await client.CreatePort("scene-666999");
            RpcClientModule module = await port.LoadModule(EmotesRendererServiceCodeGen.ServiceName);
            IClientEmotesRendererService clientRpcSceneControllerService = new ClientEmotesRendererService(module);
            return clientRpcSceneControllerService;
        }
    }
}
