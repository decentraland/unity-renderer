using DCL;
using DCL.CRDT;
using System;
using System.Collections.Generic;

namespace RPC.Context
{
    public class CRDTServiceContext
    {
        public readonly Dictionary<int, DualKeyValueSet<int, long, CrdtMessage>> scenesOutgoingCrdts =
            new Dictionary<int, DualKeyValueSet<int, long, CrdtMessage>>(24);
        public IMessagingControllersManager MessagingControllersManager;
        public IWorldState WorldState;
        public ISceneController SceneController;
        public Action<int, CrdtMessage> CrdtMessageReceived;
        public IReadOnlyDictionary<int, ICRDTExecutor> CrdtExecutors;
    }
}
