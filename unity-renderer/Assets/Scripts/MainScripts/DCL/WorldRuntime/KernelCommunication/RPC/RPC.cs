using Cysharp.Threading.Tasks;
using DCL;
using rpc_csharp;

namespace DCL
{
    public class RPC : IRPC
    {
        ClientEmotesKernelService IRPC.emotes { get; set; }

        public static async UniTask LoadModules(RpcClientPort port, IRPC rpc)
        {
            rpc.emotes = new ClientEmotesKernelService(await port.LoadModule(EmotesKernelServiceCodeGen.ServiceName));
        }

        public void Initialize()
        {
        }
        
        public void Dispose()
        {
        }
    }
}