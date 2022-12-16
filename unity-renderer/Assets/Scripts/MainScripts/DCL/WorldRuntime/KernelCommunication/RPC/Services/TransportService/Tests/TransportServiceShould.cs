using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Decentraland.Renderer.KernelServices;
using Decentraland.Renderer.RendererServices;
using NUnit.Framework;
using RPC;
using rpc_csharp;
using rpc_csharp.transport;
using RPC.Services;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    class TestEmoteKernelServiceImpl : IEmotesKernelService<RPCContext>
    {
        public TriggerExpressionRequest lastRequest;
        public UniTask<TriggerExpressionResponse> TriggerExpression(TriggerExpressionRequest request, RPCContext context, CancellationToken ct)
        {
            lastRequest = request;
            return default;
        }
    }

    public class TransportServiceShould
    {
        private Action tearDownDispose;
        private ClientEmotesKernelService clientEmotesKernelService;
        private TestEmoteKernelServiceImpl serverEmotesKernelService;

        [SetUp]
        public void SetUp()
        {
            UniTask.Awaiter awaiter = SetupInverseTranspport().GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                Thread.Sleep(1);
            }
        }

        private async UniTask SetupInverseTranspport()
        {
            // Stage 1, Setup normal RPC
            var futureServerTransport = new UniTaskCompletionSource<TransportServiceImpl>();

            // 1. Create RPC Server (renderer=server, kernel=client)
            var context = new RPCContext();

            var (clientTransport, serverTransport) = MemoryTransport.Create();
            var rpcServer = new RpcServer<RPCContext>();
            rpcServer.AttachTransport(serverTransport, context);

            rpcServer.SetHandler((port, t, c) =>
            {
                var transportServiceTransport = new TransportServiceImpl();
                TransportServiceCodeGen.RegisterService(port, transportServiceTransport);
                futureServerTransport.TrySetResult(transportServiceTransport);
            });
            var testCancellationSource = new CancellationTokenSource();

            // 2. Create RPC Client (renderer=server, kernel=client)
            RpcClient client = new RpcClient(clientTransport);
            RpcClientPort clientPort = await client.CreatePort("test-port");

            // 3. Loads TransportService
            RpcClientModule module = await clientPort.LoadModule(TransportServiceCodeGen.ServiceName);
            ClientTransportService clientTransportService = new ClientTransportService(module);

            // Stage 2, Setup inverse RPC

            // 4. Use transport service for inverse transport
            var inverseTransportClient = new InverseTransportClient(clientTransportService);

            // 5. Waits for server transports be created
            var inverseTransportServer = await futureServerTransport.Task;

            // 6. Create inverse RPC server (kernel=server, renderer=client)
            RpcServer<RPCContext> inverseServer = new RpcServer<RPCContext>();
            inverseServer.AttachTransport(inverseTransportServer, context);
            inverseServer.SetHandler((port, t, c) =>
            {
                serverEmotesKernelService = new TestEmoteKernelServiceImpl();
                EmotesKernelServiceCodeGen.RegisterService(port, serverEmotesKernelService);
            });

            // Create inverse RPC client (kernel=server, renderer=client)
            RpcClient inverseClient = new RpcClient(inverseTransportClient);
            var inverseClientPort = await inverseClient.CreatePort("inverse-port");
            var inverseClientModule = await inverseClientPort.LoadModule(EmotesKernelServiceCodeGen.ServiceName);
            clientEmotesKernelService = new ClientEmotesKernelService(inverseClientModule);

            tearDownDispose = () =>
            {
                rpcServer.Dispose();
                inverseServer.Dispose();
                testCancellationSource.Cancel();
            };
        }

        [TearDown]
        public void TearDown()
        {
            tearDownDispose.Invoke();
        }

        [UnityTest]
        public IEnumerator TestInverseService()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                // Request expression
                TriggerExpressionRequest request = new TriggerExpressionRequest()
                {
                    Id = "le-id",
                    Timestamp = 1231451
                };

                // This RPC cal will go through the inverse transport
                // So if it works, it means, all the way that it takes, it's ok
                await clientEmotesKernelService.TriggerExpression(request);

                // Check if the server receives the correct request
                Assert.NotNull(serverEmotesKernelService.lastRequest);
                Assert.AreEqual(request.Id, serverEmotesKernelService.lastRequest.Id);
                Assert.AreEqual(request.Timestamp, serverEmotesKernelService.lastRequest.Timestamp);
            });
        }
    }
}
