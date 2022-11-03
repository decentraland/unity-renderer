using Cysharp.Threading.Tasks;
using rpc_csharp;

namespace RPC.Context
{
    public class RpcClientContext
    {
        public ClientEmotesKernelService emotes;
    }

    public static class RpcClientBuilder
    {
        public static async UniTask LoadModules(RpcClientPort port, RpcClientContext clientContext)
        {
            clientContext.emotes = new ClientEmotesKernelService(await port.LoadModule(EmotesKernelServiceCodeGen.ServiceName));
        }
    }
}