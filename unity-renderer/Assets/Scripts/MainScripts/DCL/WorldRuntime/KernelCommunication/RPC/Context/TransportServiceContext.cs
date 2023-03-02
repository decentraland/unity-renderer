using System;
using System.Collections.Generic;
using DCL;
using DCL.CRDT;
using rpc_csharp;

namespace RPC.Context
{
    public class TransportServiceContext
    {
        public Action<RpcClientPort> OnLoadModules;
    }
}
