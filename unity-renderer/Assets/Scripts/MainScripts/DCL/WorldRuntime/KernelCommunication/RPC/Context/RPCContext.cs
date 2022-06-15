using RPC.Context;

namespace RPC
{
    public static class RPCGlobalContext
    {
        public static RPCContext context = new RPCContext();
    }

    public class RPCContext
    {
        public CRDTServiceContext crdtContext = new CRDTServiceContext();
    }
}