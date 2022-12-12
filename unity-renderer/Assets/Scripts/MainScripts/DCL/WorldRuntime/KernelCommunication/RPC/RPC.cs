﻿using Cysharp.Threading.Tasks;
using Decentraland.Renderer.KernelServices;
using RPC;
using rpc_csharp;

namespace DCL
{
    public class RPC : IRPC
    {
        private ClientEmotesKernelService emotes;
        private ClientFriendRequestKernelService friendRequests;

        private readonly UniTaskCompletionSource modulesLoaded = new UniTaskCompletionSource();

        private RpcServer<RPCContext> rpcServer;

        public ClientEmotesKernelService Emotes() =>
            emotes;

        public ClientFriendRequestKernelService FriendRequests() =>
            friendRequests;

        public UniTask EnsureRpc() =>
            modulesLoaded.Task;

        private async UniTaskVoid LoadRpcModulesAsync(RpcClientPort port)
        {
            emotes = new ClientEmotesKernelService(await port.LoadModule(EmotesKernelServiceCodeGen.ServiceName));
            friendRequests = new ClientFriendRequestKernelService(await port.LoadModule(FriendRequestKernelServiceCodeGen.ServiceName));
            modulesLoaded.TrySetResult();
        }

        public void Initialize()
        {
            var context = DataStore.i.rpc.context;

            context.transport.OnLoadModules += port =>
            {
                LoadRpcModulesAsync(port).Forget();
            };

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
