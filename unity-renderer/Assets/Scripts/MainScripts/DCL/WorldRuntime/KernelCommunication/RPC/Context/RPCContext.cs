using RPC.Context;

namespace RPC
{
    public class RPCContext
    {
        public RpcClientContext rpcClient = new RpcClientContext();
        public CRDTServiceContext crdt = new CRDTServiceContext();
    }
}