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
        private IBaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> animations;
        private BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesInUse;
        private IDictionary<IParcelScene, HashSet<(string bodyshapeId, string emoteId)>> pendingEmotesByScene;
        private IDictionary<IParcelScene, HashSet<(string bodyshapeId, string emoteId)>> equippedEmotesByScene;
        private IDictionary<IParcelScene, CancellationTokenSource> cancellationTokenSources;
        private IClientEmotesRendererService client;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            worldState = Substitute.For<IWorldState>();
            sceneController = Substitute.For<ISceneController>();
            userProfile = UserProfile.GetOwnUserProfile();
            ownPlayer = new BaseVariable<Player>();
            animations = new BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData>();
            emotesInUse = new BaseRefCountedCollection<(string bodyshapeId, string emoteId)>();
            pendingEmotesByScene = new Dictionary<IParcelScene, HashSet<(string bodyshapeId, string emoteId)>>();
            equippedEmotesByScene = new Dictionary<IParcelScene, HashSet<(string bodyshapeId, string emoteId)>>();
            cancellationTokenSources = new Dictionary<IParcelScene, CancellationTokenSource>();

            Environment.i.serviceLocator.Register<IRPC>(() => Substitute.For<IRPC>());

            yield return UniTask.ToCoroutine(async () =>
            {
                client = await CreateRpcClient();
                await Environment.i.serviceLocator.Initialize();
            });
        }

        [TearDown]
        public void TearDown()
        {
            Resources.UnloadAsset(userProfile);
        }

        [UnityTest]
        public IEnumerator Fail_When_SceneNotLoaded()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                var result = await client.TriggerSceneExpression(
                    new TriggerSceneExpressionRequest()
                    {
                        Path = "local/emote",
                        SceneNumber = 666
                    });

                Assert.IsFalse(result.Success);
            });
        }

        [UnityTest]
        public IEnumerator Fail_When_EmoteNotPresentInSceneMapping()
        {
            var contentProvider = new ContentProvider()
            {
                contents =
                {
                    new ContentServerUtils.MappingPair() { file = "theEmote", hash = "0066688" }
                }
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

            yield return UniTask.ToCoroutine(async () =>
            {
                var result = await client.TriggerSceneExpression(
                    new TriggerSceneExpressionRequest()
                    {
                        Path = "NOT_THE_EMOTE",
                        SceneNumber = 666
                    });

                Assert.IsFalse(result.Success);
            });
        }

        [UnityTest]
        public IEnumerator Fail_When_NoUserAvatarSet()
        {
            var contentProvider = new ContentProvider()
            {
                contents =
                {
                    new ContentServerUtils.MappingPair() { file = "theEmote", hash = "0066688" }
                }
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

            yield return UniTask.ToCoroutine(async () =>
            {
                var result = await client.TriggerSceneExpression(
                    new TriggerSceneExpressionRequest()
                    {
                        Path = "theEmote",
                        SceneNumber = 666
                    });

                Assert.IsFalse(result.Success);
            });
        }

        [UnityTest]
        public IEnumerator Success_LoadingAndPlayingEmote()
        {
            const string EMOTE_PATH = "theEmote";
            const int SCENE_NUMBER = 666;

            var (emoteData, scene) = SetUpWorkingEnvironment(SCENE_NUMBER, EMOTE_PATH);

            emotesInUse.OnRefCountUpdated += (tuple, i) => animations[tuple] = new EmoteClipData(new AnimationClip());

            yield return UniTask.ToCoroutine(async () =>
            {
                var result = await client.TriggerSceneExpression(
                    new TriggerSceneExpressionRequest()
                    {
                        Path = EMOTE_PATH,
                        SceneNumber = SCENE_NUMBER
                    });

                Assert.IsTrue(result.Success);
                Assert.AreEqual(1, emotesInUse.GetRefCount(emoteData));
                var avatar = ownPlayer.Get().avatar;
                avatar.Received(1).EquipEmote(emoteData.emoteId, animations[emoteData]);
                Assert.AreEqual(emoteData.emoteId, userProfile.avatar.expressionTriggerId);
                Assert.IsTrue(equippedEmotesByScene[scene].Contains(emoteData));
                Assert.IsTrue(pendingEmotesByScene[scene].Count == 0);
            });
        }

        [UnityTest]
        public IEnumerator Success_LoadingAndPlayingEmote_Twice()
        {
            const string EMOTE_PATH = "theEmote";
            const int SCENE_NUMBER = 666;

            var (emoteData, scene) = SetUpWorkingEnvironment(SCENE_NUMBER, EMOTE_PATH);

            emotesInUse.OnRefCountUpdated += (tuple, i) => animations[tuple] = new EmoteClipData(new AnimationClip());

            yield return UniTask.ToCoroutine(async () =>
            {
                var result = await client.TriggerSceneExpression(
                    new TriggerSceneExpressionRequest()
                    {
                        Path = EMOTE_PATH,
                        SceneNumber = SCENE_NUMBER
                    });

                result = await client.TriggerSceneExpression(
                    new TriggerSceneExpressionRequest()
                    {
                        Path = EMOTE_PATH,
                        SceneNumber = SCENE_NUMBER
                    });

                Assert.IsTrue(result.Success);
                Assert.AreEqual(1, emotesInUse.GetRefCount(emoteData));
                Assert.IsTrue(equippedEmotesByScene[scene].Contains(emoteData));
                Assert.IsTrue(pendingEmotesByScene[scene].Count == 0);
            });
        }

        [UnityTest]
        public IEnumerator Cancelled_When_SceneRemovedWhileLoading()
        {
            const string EMOTE_PATH = "theEmote";
            const int SCENE_NUMBER = 666;

            var (emoteData, scene) = SetUpWorkingEnvironment(SCENE_NUMBER, EMOTE_PATH);

            yield return UniTask.ToCoroutine(async () =>
            {
                UniTask.Create(async () =>
                {
                    await UniTask.Yield();
                    cancellationTokenSources[scene].Cancel();
                });

                var result = await client.TriggerSceneExpression(
                    new TriggerSceneExpressionRequest()
                    {
                        Path = EMOTE_PATH,
                        SceneNumber = SCENE_NUMBER
                    });

                Assert.IsFalse(result.Success);
                Assert.AreEqual(0, emotesInUse.GetRefCount(emoteData));
                Assert.IsTrue(equippedEmotesByScene[scene].Count == 0);
                Assert.IsTrue(pendingEmotesByScene[scene].Count == 0);
            });
        }

        [UnityTest]
        public IEnumerator CleanUp_When_SceneIsUnloaded()
        {
            const string EMOTE_PATH = "theEmote";
            const int SCENE_NUMBER = 666;

            var (emoteData, scene) = SetUpWorkingEnvironment(SCENE_NUMBER, EMOTE_PATH);

            emotesInUse.OnRefCountUpdated += (tuple, i) =>
            {
                if (i == 0)
                    animations.Remove(tuple);
                else
                    animations[tuple] = new EmoteClipData(new AnimationClip());
            };

            yield return UniTask.ToCoroutine(async () =>
            {
                var result = await client.TriggerSceneExpression(
                    new TriggerSceneExpressionRequest()
                    {
                        Path = EMOTE_PATH,
                        SceneNumber = SCENE_NUMBER
                    });

                Assert.IsTrue(result.Success);
                Assert.AreEqual(1, emotesInUse.GetRefCount(emoteData));
                Assert.IsTrue(animations.ContainsKey(emoteData));
                Assert.IsTrue(equippedEmotesByScene[scene].Count == 1);
                Assert.IsTrue(equippedEmotesByScene[scene].Count == 1);
            });

            sceneController.OnSceneRemoved += Raise.Event<Action<IParcelScene>>(scene);

            Assert.AreEqual(0, emotesInUse.GetRefCount(emoteData));
            Assert.IsFalse(animations.ContainsKey(emoteData));
            Assert.IsFalse(equippedEmotesByScene.ContainsKey(scene));
            Assert.IsFalse(pendingEmotesByScene.ContainsKey(scene));
        }

        [UnityTest]
        public IEnumerator GenerateEmoteId_Correctly()
        {
            const string EMOTE_PATH = "theEmote";
            const int SCENE_NUMBER = 666;

            var (emoteData, scene) = SetUpWorkingEnvironment(SCENE_NUMBER, EMOTE_PATH);

            emotesInUse.OnRefCountUpdated += (tuple, i) => animations[tuple] = new EmoteClipData(new AnimationClip());

            yield return UniTask.ToCoroutine(async () =>
            {
                var result = await client.TriggerSceneExpression(
                    new TriggerSceneExpressionRequest()
                    {
                        Path = EMOTE_PATH,
                        SceneNumber = SCENE_NUMBER,
                        Loop = true
                    });

                Assert.IsTrue(result.Success);
                Assert.IsTrue(SceneEmoteHelper.TryGetDataFromEmoteId(userProfile.avatar.expressionTriggerId, out string hash, out bool loop));
                Assert.IsTrue(loop);
                Assert.AreEqual(scene.contentProvider.contents[0].hash, hash);

                result = await client.TriggerSceneExpression(
                    new TriggerSceneExpressionRequest()
                    {
                        Path = EMOTE_PATH,
                        SceneNumber = SCENE_NUMBER,
                        Loop = false
                    });

                Assert.IsTrue(result.Success);
                Assert.IsTrue(SceneEmoteHelper.TryGetDataFromEmoteId(userProfile.avatar.expressionTriggerId, out hash, out loop));
                Assert.IsFalse(loop);
                Assert.AreEqual(scene.contentProvider.contents[0].hash, hash);
            });
        }

        private ((string bodyShae, string emoteId)emoteData, IParcelScene scene) SetUpWorkingEnvironment(int sceneNumber, string emotePath)
        {
            const string BODY_SHAPE = "someBodyShape";

            var contentProvider = new ContentProvider()
            {
                contents =
                {
                    new ContentServerUtils.MappingPair() { file = emotePath, hash = "0066688" }
                }
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

            var playerModel = new Player()
            {
                avatar = Substitute.For<IAvatar>()
            };

            ownPlayer.Set(playerModel);
            userProfile.avatar.bodyShape = BODY_SHAPE;

            SceneEmoteHelper.TryGenerateEmoteId(scene, emotePath, false, out var emoteId);
            return (emoteData: (bodyShae: BODY_SHAPE, emoteId: emoteId), scene: scene);
        }

        private async UniTask<IClientEmotesRendererService> CreateRpcClient()
        {
            var (clientTransport, serverTransport) = MemoryTransport.Create();

            var context = new RPCContext();
            var rpcServer = new RpcServer<RPCContext>();
            rpcServer.AttachTransport(serverTransport, context);

            rpcServer.SetHandler((port, t, c) =>
            {
                EmotesRendererServiceCodeGen.RegisterService(port,
                    new EmotesRendererServiceImpl(
                        port,
                        worldState,
                        sceneController,
                        userProfile,
                        ownPlayer,
                        animations,
                        emotesInUse,
                        pendingEmotesByScene,
                        equippedEmotesByScene,
                        cancellationTokenSources
                    ));
            });

            RpcClient client = new RpcClient(clientTransport);
            RpcClientPort port = await client.CreatePort("scene-666999");
            RpcClientModule module = await port.LoadModule(EmotesRendererServiceCodeGen.ServiceName);
            IClientEmotesRendererService clientRpcSceneControllerService = new ClientEmotesRendererService(module);
            return clientRpcSceneControllerService;
        }
    }
}
