using Cysharp.Threading.Tasks;
using DCL;
using rpc_csharp;
using System;

namespace DCL
{
    public class RPC : IRPC
    {
        ClientEmotesKernelService IRPC.emotes { get; set; }

        private readonly UniTaskCompletionSource modulesLoaded = new UniTaskCompletionSource();
        public UniTask EnsureRpc() =>
            modulesLoaded.Task;

        public static async UniTask LoadModules(RpcClientPort port, IRPC rpc)
        {
            rpc.emotes = new ClientEmotesKernelService(await port.LoadModule(EmotesKernelServiceCodeGen.ServiceName));

            ((RPC)rpc).modulesLoaded.TrySetResult();
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
    }
}
