using Cysharp.Threading.Tasks;
using RPC;
using rpc_csharp;

namespace DCL
{
    public class RPC : IRPC
    {
        private ClientEmotesKernelService emotes;

        private ClientAnalyticsKernelService analytics;

        private readonly UniTaskCompletionSource modulesLoaded = new UniTaskCompletionSource();

        public ClientEmotesKernelService Emotes() =>
            emotes;

        public ClientAnalyticsKernelService Analytics() =>
            analytics;

        public UniTask EnsureRpc() =>
            modulesLoaded.Task;

        private async UniTaskVoid LoadRpcModulesAsync(RpcClientPort port)
        {
            emotes = new ClientEmotesKernelService(await port.LoadModule(EmotesKernelServiceCodeGen.ServiceName));
            analytics = new ClientAnalyticsKernelService(await port.LoadModule(AnalyticsKernelServiceCodeGen.ServiceName));
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

            RPCServerBuilder.BuildDefaultServer(context);
        }

        public void Dispose()
        {
        }
    }
}
