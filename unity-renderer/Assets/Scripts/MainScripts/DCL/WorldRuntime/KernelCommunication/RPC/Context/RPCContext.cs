using RPC.Context;

namespace RPC
{
    public class RPCContext
    {
        public CRDTServiceContext crdt = new CRDTServiceContext();
        public TransportServiceContext transport = new TransportServiceContext();
        public RestrictedActionsContext restrictedActions = new RestrictedActionsContext();
    }
}
