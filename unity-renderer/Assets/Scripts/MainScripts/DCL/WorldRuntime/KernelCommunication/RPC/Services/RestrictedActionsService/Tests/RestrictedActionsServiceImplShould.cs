using Cysharp.Threading.Tasks;
using Decentraland.Renderer.RendererServices;
using NUnit.Framework;
using RPC;
using rpc_csharp;
using rpc_csharp.transport;
using RPC.Context;
using RPC.Services;
using System.Collections;
using UnityEngine.TestTools;

namespace Tests
{
    public class RestrictedActionsServiceImplShould
    {
        private RestrictedActionsContext restrictedActions;
        private RpcServer<RPCContext> rpcServer;
        private ClientRestrictedActionsService rpcClient;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                RPCContext rpcContext = new RPCContext();
                restrictedActions = rpcContext.restrictedActions;

                var (clientTransport, serverTransport) = MemoryTransport.Create();

                rpcServer = new RpcServer<RPCContext>();
                rpcServer.AttachTransport(serverTransport, rpcContext);

                rpcServer.SetHandler((port, t, c) =>
                {
                    RestrictedActionsServiceCodeGen.RegisterService(port, new RestrictedActionsServiceImpl());
                });

                RpcClient client = new RpcClient(clientTransport);
                RpcClientPort port = await client.CreatePort("scene-666999");
                RpcClientModule module = await port.LoadModule(RestrictedActionsServiceCodeGen.ServiceName);
                rpcClient = new ClientRestrictedActionsService(module);
            });
        }

        [TearDown]
        public void TearDown()
        {
            rpcServer.Dispose();
        }

        [UnityTest]
        public IEnumerator UrlPromptRequestSuccessCorrectly()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const int EXPECTED_SCENE_NUMBER = 666;
                const string EXPECTED_URL = "http://decentraland.org";

                bool requested = false;

                int requestedSceneId = 0;
                string requestedUrl = string.Empty;

                restrictedActions.OpenExternalUrlPrompt = (s, i) =>
                {
                    requested = true;
                    requestedSceneId = i;
                    requestedUrl = s;
                    return true;
                };

                restrictedActions.GetCurrentFrameCount = () => 0;
                restrictedActions.LastFrameWithInput = 0;

                var result = await rpcClient.OpenExternalUrl(
                    new OpenExternalUrlRequest()
                    {
                        Url = EXPECTED_URL, SceneNumber = EXPECTED_SCENE_NUMBER
                    });

                Assert.IsTrue(result.Success);
                Assert.IsTrue(requested);
                Assert.AreEqual(requestedSceneId, EXPECTED_SCENE_NUMBER);
                Assert.AreEqual(requestedUrl, EXPECTED_URL);
            });
        }

        [UnityTest]
        public IEnumerator UrlPromptRequestFailDueToFrameRequestDifference()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const int EXPECTED_SCENE_NUMBER = 666;
                const string EXPECTED_URL = "http://decentraland.org";

                bool requested = false;

                restrictedActions.OpenExternalUrlPrompt = (s, i) =>
                {
                    requested = true;
                    return true;
                };

                restrictedActions.GetCurrentFrameCount = () => RestrictedActionsServiceImpl.MAX_ELAPSED_FRAMES_SINCE_INPUT + 1;
                restrictedActions.LastFrameWithInput = 0;

                var result = await rpcClient.OpenExternalUrl(
                    new OpenExternalUrlRequest()
                    {
                        Url = EXPECTED_URL, SceneNumber = EXPECTED_SCENE_NUMBER
                    });

                Assert.IsFalse(result.Success);
                Assert.IsFalse(requested);
            });
        }
    }
}
