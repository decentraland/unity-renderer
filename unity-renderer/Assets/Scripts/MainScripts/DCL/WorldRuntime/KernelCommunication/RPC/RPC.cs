using Cysharp.Threading.Tasks;
using DCL;
using rpc_csharp;

namespace DCL
{
    public class RPC : IRPC
    {
        private ClientAnalyticsKernelService analytics1;
        ClientEmotesKernelService IRPC.emotes { get; set; }
        ClientAnalyticsKernelService IRPC.analytics { get; set; }

        public static async UniTask LoadModules(RpcClientPort port, IRPC rpc)
        {
            rpc.emotes = new ClientEmotesKernelService(await port.LoadModule(EmotesKernelServiceCodeGen.ServiceName));
            rpc.analytics = new ClientAnalyticsKernelService(await port.LoadModule(AnalyticsKernelServiceCodeGen.ServiceName));
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
    }
}
