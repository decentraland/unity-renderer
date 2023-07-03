using Cysharp.Threading.Tasks;
using Decentraland.Social.Friendships;
using rpc_csharp;
using rpc_csharp.transport;
using RPC.Transports;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class RPCSocialClientProvider : ISocialClientProvider
    {
        private readonly string url;

        public event Action OnTransportError;

        public RPCSocialClientProvider(string url)
        {
            this.url = url;
        }

        public async UniTask<IClientFriendshipsService> Provide(CancellationToken cancellationToken)
        {
            var transport = await CreateWebSocketAndConnect(cancellationToken);
            var client = new RpcClient(transport);
            var socialPort = await client.CreatePort("social-service-port");

            cancellationToken.ThrowIfCancellationRequested();

            var module = await socialPort.LoadModule(FriendshipsServiceCodeGen.ServiceName);

            cancellationToken.ThrowIfCancellationRequested();

            return new ClientFriendshipsService(module);
        }

        private UniTask<ITransport> CreateWebSocketAndConnect(CancellationToken cancellationToken)
        {
            UniTaskCompletionSource<ITransport> task = new ();
            WebSocketClientTransport transport = new (url);

            void CompleteTaskAndUnsubscribe()
            {
                Debug.Log("SocialClient.Transport.Connected");

                if (cancellationToken.IsCancellationRequested)
                {
                    task.TrySetCanceled(cancellationToken);
                    return;
                }

                task.TrySetResult(transport);
            }

            void FailTaskAndUnsubscribe(string error)
            {
                Debug.LogError($"SocialClient.Transport.Error: {error}");

                if (cancellationToken.IsCancellationRequested)
                {
                    task.TrySetCanceled(cancellationToken);
                    return;
                }

                task.TrySetException(new Exception(error));

                OnTransportError?.Invoke();
            }

            void FailTaskByDisconnectionAndUnsubscribe()
            {
                Debug.Log("SocialClient.Transport.Disconnected");

                if (cancellationToken.IsCancellationRequested)
                {
                    task.TrySetCanceled(cancellationToken);
                    return;
                }

                task.TrySetException(new Exception("Cannot connect to social service server, connection closed"));
            }

            transport.OnConnectEvent += CompleteTaskAndUnsubscribe;
            transport.OnErrorEvent += FailTaskAndUnsubscribe;
            transport.OnCloseEvent += FailTaskByDisconnectionAndUnsubscribe;

            try { transport.Connect(); }
            catch (Exception e) { Debug.LogException(e); }

            return task.Task.AttachExternalCancellation(cancellationToken);
        }
    }
}
