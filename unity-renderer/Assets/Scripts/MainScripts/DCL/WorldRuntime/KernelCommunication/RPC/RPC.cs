using Cysharp.Threading.Tasks;
using Decentraland.Renderer.KernelServices;
using RPC;
using rpc_csharp;
using System;
using UnityEngine;

namespace DCL
{
    public class RPC : IRPC
    {
        private ClientEmotesKernelService emotes;
        private ClientFriendRequestKernelService friendRequests;
        private ClientFriendsKernelService friends;
        private ClientSignRequestKernelService signRequest;

        private readonly UniTaskCompletionSource modulesLoaded = new ();

        private RpcServer<RPCContext> rpcServer;

        public ClientEmotesKernelService Emotes() =>
            emotes;

        public ClientFriendRequestKernelService FriendRequests() =>
            friendRequests;

        public ClientFriendsKernelService Friends() =>
            friends;

        public ClientSignRequestKernelService SignRequestKernelService() =>
            signRequest;

        public UniTask EnsureRpc() =>
            modulesLoaded.Task;

        private async UniTaskVoid LoadRpcModulesAsync(RpcClientPort port)
        {
            emotes = await SafeLoadModule(EmotesKernelServiceCodeGen.ServiceName, port,
                module => new ClientEmotesKernelService(module));

            friendRequests = await SafeLoadModule(FriendRequestKernelServiceCodeGen.ServiceName, port,
                module => new ClientFriendRequestKernelService(module));

            friends = await SafeLoadModule(FriendsKernelServiceCodeGen.ServiceName, port,
                module => new ClientFriendsKernelService(module));

            signRequest = await SafeLoadModule(SignRequestKernelServiceCodeGen.ServiceName, port,
                module => new ClientSignRequestKernelService(module));

            modulesLoaded.TrySetResult();
        }

        private async UniTask<T> SafeLoadModule<T>(string serviceName, RpcClientPort port, Func<RpcClientModule, T> builderFunction)
            where T: class
        {
            try
            {
                RpcClientModule module = await port.LoadModule(serviceName);
                return builderFunction.Invoke(module);
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                // TODO: may be improved by returning a valid instance with dummy behaviour. This way we force to do null-checks on usage
                return null;
            }
        }

        public void Initialize()
        {
            var context = DataStore.i.rpc.context;

            context.transport.OnLoadModules += port => { LoadRpcModulesAsync(port).Forget(); };

            context.crdt.MessagingControllersManager = Environment.i.messaging.manager;
            context.crdt.WorldState = Environment.i.world.state;
            context.crdt.SceneController = Environment.i.world.sceneController;

            rpcServer = RPCServerBuilder.BuildDefaultServer(context);
        }

        public void Dispose()
        {
            rpcServer.Dispose();
        }
    }
}
