using RPC.Context;

namespace RPC
{
    public class RPCContext
    {
        public CRDTServiceContext crdtContext = new CRDTServiceContext();
        public TeleportServiceContext teleportContext = new TeleportServiceContext();
    }
}