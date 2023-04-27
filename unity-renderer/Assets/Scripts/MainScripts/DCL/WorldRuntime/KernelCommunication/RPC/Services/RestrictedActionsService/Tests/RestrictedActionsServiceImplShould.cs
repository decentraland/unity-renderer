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

        [Test]
        public void ParseUrnCorrectly()
        {
            const string CONTRACT_ADDRESS = "0x0123512313223123123123123";
            const string TOKEN_ID = "645433";

            Assert.IsTrue(RestrictedActionsServiceImpl.TryParseUrn(
                $"urn:ethereum:erc1150:{CONTRACT_ADDRESS}:{TOKEN_ID}",
                out string contractAddress,
                out string tokenId));

            Assert.AreEqual(CONTRACT_ADDRESS, contractAddress);
            Assert.AreEqual(TOKEN_ID, tokenId);
        }

        [Test]
        public void ParseUrnFailsCorrectly()
        {
            string contractAddress;
            string tokenId;

            // "urn" missing
            Assert.IsFalse(RestrictedActionsServiceImpl.TryParseUrn(
                $"ethereum:erc1150:0x0000temptation:666",
                out contractAddress,
                out tokenId));

            // "chain" missing
            Assert.IsFalse(RestrictedActionsServiceImpl.TryParseUrn(
                $"urn:erc1150:0x0000temptation:666",
                out contractAddress,
                out tokenId));

            // "chain" invalid
            Assert.IsFalse(RestrictedActionsServiceImpl.TryParseUrn(
                $"urn:some-chain:erc1150:0x0000temptation:666",
                out contractAddress,
                out tokenId));

            // contract address missing
            Assert.IsFalse(RestrictedActionsServiceImpl.TryParseUrn(
                $"urn:ethereum:erc1150:666",
                out contractAddress,
                out tokenId));

            // token id missing
            Assert.IsFalse(RestrictedActionsServiceImpl.TryParseUrn(
                $"urn:ethereum:erc1150:0x0000temptation",
                out contractAddress,
                out tokenId));

            // empty string
            Assert.IsFalse(RestrictedActionsServiceImpl.TryParseUrn(
                "",
                out contractAddress,
                out tokenId));
        }

        [UnityTest]
        public IEnumerator NftPromptRequestSuccessCorrectly()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const string EXPECTED_CONTRACT_ADDRESS = "0x0temptation";
                const string EXPECTED_TOKEN_ID = "1234";

                bool requested = false;

                string contractAddress = string.Empty;
                string tokenId = string.Empty;

                restrictedActions.OpenNftPrompt = (contract, token) =>
                {
                    requested = true;
                    contractAddress = contract;
                    tokenId = token;
                };

                restrictedActions.GetCurrentFrameCount = () => 0;
                restrictedActions.LastFrameWithInput = 0;

                var result = await rpcClient.OpenNftDialog(
                    new OpenNftDialogRequest()
                    {
                        Urn = $"urn:ethereum:erc1150:{EXPECTED_CONTRACT_ADDRESS}:{EXPECTED_TOKEN_ID}"
                    });

                Assert.IsTrue(result.Success);
                Assert.IsTrue(requested);
                Assert.AreEqual(contractAddress, EXPECTED_CONTRACT_ADDRESS);
                Assert.AreEqual(tokenId, EXPECTED_TOKEN_ID);
            });
        }

        [UnityTest]
        public IEnumerator NftPromptRequestFailDueToFrameRequestDifference()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const string EXPECTED_CONTRACT_ADDRESS = "0x0temptation";
                const string EXPECTED_TOKEN_ID = "1234";

                bool requested = false;

                restrictedActions.OpenNftPrompt = (contract, token) =>
                {
                    requested = true;
                };

                restrictedActions.GetCurrentFrameCount = () => RestrictedActionsServiceImpl.MAX_ELAPSED_FRAMES_SINCE_INPUT + 1;
                restrictedActions.LastFrameWithInput = 0;

                var result = await rpcClient.OpenNftDialog(
                    new OpenNftDialogRequest()
                    {
                        Urn = $"urn:ethereum:erc1150:{EXPECTED_CONTRACT_ADDRESS}:{EXPECTED_TOKEN_ID}"
                    });

                Assert.IsFalse(result.Success);
                Assert.IsFalse(requested);
            });
        }

        [UnityTest]
        public IEnumerator NftPromptRequestFailDueToBadUrn()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                bool requested = false;

                restrictedActions.OpenNftPrompt = (contract, token) =>
                {
                    requested = true;
                };

                restrictedActions.GetCurrentFrameCount = () => 0;
                restrictedActions.LastFrameWithInput = 0;

                var result = await rpcClient.OpenNftDialog(
                    new OpenNftDialogRequest()
                    {
                        Urn = string.Empty
                    });

                Assert.IsFalse(result.Success);
                Assert.IsFalse(requested);
            });
        }
    }
}
