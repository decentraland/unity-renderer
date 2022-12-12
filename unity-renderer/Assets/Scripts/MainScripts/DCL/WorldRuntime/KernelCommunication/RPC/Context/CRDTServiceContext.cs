using DCL;
using DCL.CRDT;
using System;
using System.Collections.Generic;

namespace RPC.Context
{
    public class CRDTServiceContext
    {
        public readonly Dictionary<int, CRDTProtocol> scenesOutgoingCrdts = new Dictionary<int, CRDTProtocol>(24);
        public IMessagingControllersManager MessagingControllersManager;
        public IWorldState WorldState;
        public ISceneController SceneController;
        public Action<int, CRDTMessage> CrdtMessageReceived;
    }
}
