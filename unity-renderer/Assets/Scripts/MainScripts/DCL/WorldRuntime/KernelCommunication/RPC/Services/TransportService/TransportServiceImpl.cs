using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Decentraland.Renderer.RendererServices;
using Google.Protobuf;
using rpc_csharp;
using rpc_csharp.protocol;
using rpc_csharp.transport;
using System;
using System.Threading;

namespace RPC.Services
{
    public class AsyncQueueEnumerable<T> : IUniTaskAsyncEnumerable<T> where T: class
    {
        private readonly ProtocolHelpers.AsyncQueue<T> queue;

        public AsyncQueueEnumerable(ProtocolHelpers.AsyncQueue<T> queue)
        {
            this.queue = queue;
        }

        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return queue;
        }
    }

    public class TransportServiceImpl : ITransportService<RPCContext>, ITransport
    {
        public event Action OnCloseEvent;
        public event Action<string> OnErrorEvent;
        public event Action<byte[]> OnMessageEvent;
        public event Action OnConnectEvent;

        private readonly ProtocolHelpers.AsyncQueue<Payload> queue;
        private RpcClient client;
        private RpcClientPort port;

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            TransportServiceCodeGen.RegisterService(port, new TransportServiceImpl());
        }

        public TransportServiceImpl()
        {
            queue = new ProtocolHelpers.AsyncQueue<Payload>((_, __) => { });
        }

        private async UniTaskVoid BuildClient(RPCContext context)
        {
            client = new RpcClient(this);
            port = await client.CreatePort("renderer-protocol");

            context.transport.OnLoadModules?.Invoke(port);
        }

        private async UniTaskVoid HandleMessages(IUniTaskAsyncEnumerable<Payload> streamRequest, CancellationToken token)
        {
            await foreach (Payload request in streamRequest)
            {
                if (token.IsCancellationRequested)
                    break;

                OnMessageEvent?.Invoke(request.Payload_.ToByteArray());
            }
        }

        public IUniTaskAsyncEnumerable<Payload> OpenTransportStream(IUniTaskAsyncEnumerable<Payload> streamRequest, RPCContext context)
        {
            // Client builder...
            BuildClient(context).Forget();

            OnConnectEvent?.Invoke();

            return UniTaskAsyncEnumerable.Create<Payload>(async (writer, token) =>
            {
                // Async call...
                HandleMessages(streamRequest, token).Forget();

                while (true)
                {
                    var nextFuture = queue.MoveNextAsync();

                    if (await nextFuture == false || token.IsCancellationRequested)
                    {
                        queue.Close();
                        OnCloseEvent?.Invoke();
                        break;
                    }

                    await writer.YieldAsync(queue.Current);
                }
            });
        }

        public void SendMessage(byte[] data)
        {
            queue.Enqueue(new Payload() { Payload_ = ByteString.CopyFrom(data) });
        }

        public void Close()
        {
            queue.Close();
        }

        public void Dispose() { }
    }
}
