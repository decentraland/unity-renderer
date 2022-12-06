using Cysharp.Threading.Tasks;
using RPC;
using rpc_csharp;
using UnityEngine;

namespace DCL
{
    public class RPC : IRPC
    {
        private ClientEmotesKernelService emotes;

        private readonly UniTaskCompletionSource modulesLoaded = new UniTaskCompletionSource();

        private RpcServer<RPCContext> rpcServer;

        public ClientEmotesKernelService Emotes() =>
            emotes;

        public UniTask EnsureRpc() =>
            modulesLoaded.Task;

        private async UniTaskVoid LoadRpcModulesAsync(RpcClientPort port)
        {
            emotes = new ClientEmotesKernelService(await port.LoadModule(EmotesKernelServiceCodeGen.ServiceName));
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
