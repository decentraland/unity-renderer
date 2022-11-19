using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.protocol;
using rpc_csharp.transport;

namespace Tests
{
    class AsyncQueueWrapper<T> : IUniTaskAsyncEnumerable<T> where T : class
    {
        public readonly ProtocolHelpers.AsyncQueue<T> queue = new ProtocolHelpers.AsyncQueue<T>((_, __) => {});
        public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return queue;
        }
    }

    public class InverseTransportClient : ITransport
    {
        private bool closed = false;
        private AsyncQueueWrapper<Payload> queueWrapper;
        public InverseTransportClient(ClientTransportService service)
        {
            queueWrapper = new AsyncQueueWrapper<Payload>();
            OnConnectEvent?.Invoke();
            var streamResponse = service.OpenTransportStream(queueWrapper);
            Handler(streamResponse).Forget();
        }

        private async UniTask Handler(IUniTaskAsyncEnumerable<Payload> streamResponse)
        {
            await foreach (Payload message in streamResponse)
            {
                if (closed)
                    break;
                OnMessageEvent?.Invoke(message.Payload_.ToByteArray());
            }
        }

        public void SendMessage(byte[] data)
        {
            queueWrapper.queue.Enqueue(new Payload() { Payload_ = ByteString.CopyFrom(data) });
        }
        public void Close()
        {
            queueWrapper.queue.Close();
            closed = true;
        }

        public event Action OnCloseEvent;
        public event Action<string> OnErrorEvent;
        public event Action<byte[]> OnMessageEvent;
        public event Action OnConnectEvent;
    }
}