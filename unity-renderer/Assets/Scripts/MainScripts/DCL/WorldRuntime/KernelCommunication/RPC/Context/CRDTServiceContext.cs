using System;
using System.Collections.Generic;
using DCL;
using DCL.CRDT;

namespace RPC.Context
{
    public class CRDTServiceContext
    {
        public readonly Dictionary<int, CRDTProtocol> scenesOutgoingCrdts = new Dictionary<int, CRDTProtocol>(24);
        public IMessagingControllersManager MessagingControllersManager;
        public Action<int, CRDTMessage> CrdtMessageReceived;
    }
}