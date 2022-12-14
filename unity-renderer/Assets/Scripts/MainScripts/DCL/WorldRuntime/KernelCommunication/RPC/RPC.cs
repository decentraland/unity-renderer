using Cysharp.Threading.Tasks;
using Decentraland.Renderer.KernelServices;
using RPC;
using rpc_csharp;


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
            emotes = await SafeLoadModule(EmotesKernelServiceCodeGen.ServiceName, port,
                module => new ClientEmotesKernelService(module));

            friendRequests = await SafeLoadModule(FriendRequestKernelServiceCodeGen.ServiceName, port,
                module => new ClientFriendRequestKernelService(module));

            analytics = await SafeLoadModule(AnalyticsKernelServiceCodeGen.ServiceName, port,
                module => new ClientAnalyticsKernelService(module));

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
