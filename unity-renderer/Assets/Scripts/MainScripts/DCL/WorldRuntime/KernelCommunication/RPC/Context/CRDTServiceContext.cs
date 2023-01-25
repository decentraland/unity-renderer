using DCL;
using DCL.CRDT;
using System;
using System.Collections.Generic;

namespace RPC.Context
{
    public class CRDTServiceContext
    {
        public readonly Dictionary<int, DualKeyValueSet<int, long, CRDTMessage>> scenesOutgoingCrdts =
            new Dictionary<int, DualKeyValueSet<int, long, CRDTMessage>>(24);
        public IMessagingControllersManager MessagingControllersManager;
        public IWorldState WorldState;
        public ISceneController SceneController;
        public Action<int, CRDTMessage> CrdtMessageReceived;

        // TODO: we actually just want `CRDTProtocol` for this propose.
        // but we should first refactor `CRDTExecutor` so it receive it's `CRDTProtocol` using dependency injection
        public Dictionary<int, ICRDTExecutor> CrdtExecutors;
    }
}
