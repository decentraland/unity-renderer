using Cysharp.Threading.Tasks;
using Decentraland.Renderer.KernelServices;
using RPC;
using rpc_csharp;
using UnityEngine.Device;

namespace DCL
{
    public class RPC : IRPC
    {
        private ClientEmotesKernelService emotes;
        private ClientFriendRequestKernelService friendRequests;
        private ClientAnalyticsKernelService analytics;

        private readonly UniTaskCompletionSource modulesLoaded = new UniTaskCompletionSource();

        private RpcServer<RPCContext> rpcServer;

        public ClientEmotesKernelService Emotes() =>
            emotes;

        public ClientFriendRequestKernelService FriendRequests() =>
            friendRequests;

        public ClientAnalyticsKernelService Analytics() =>
            analytics;

        public UniTask EnsureRpc() =>
            modulesLoaded.Task;

        private async UniTaskVoid LoadRpcModulesAsync(RpcClientPort port)
        {
            emotes = new ClientEmotesKernelService(await port.LoadModule(EmotesKernelServiceCodeGen.ServiceName));
            friendRequests = new ClientFriendRequestKernelService(await port.LoadModule(FriendRequestKernelServiceCodeGen.ServiceName));
            analytics = new ClientAnalyticsKernelService(await port.LoadModule(AnalyticsKernelServiceCodeGen.ServiceName));
            modulesLoaded.TrySetResult();
            await this.StartRpc();
        }

        private async UniTask StartRpc()
        {
            await UniTask.SwitchToMainThread();

            // this event should be the last one to be executed after initialization
            // it is used by the kernel to signal "EngineReady" or something like that
            // to prevent race conditions like "SceneController is not an object",
            // aka sending events before unity is ready
            await analytics.SystemInfoReport(new SystemInfoReportRequest()
            {
                GraphicsDeviceName = SystemInfo.graphicsDeviceName,
                GraphicsDeviceVersion = SystemInfo.graphicsDeviceVersion,
                GraphicsMemorySize = (uint)SystemInfo.graphicsMemorySize,
                ProcessorType = SystemInfo.processorType,
                ProcessorCount = (uint)SystemInfo.processorCount,
                SystemMemorySize = (uint)SystemInfo.systemMemorySize,

                // TODO: remove useBinaryTransform after ECS7 is fully in prod
                UseBinaryTransform = true,
            });
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
