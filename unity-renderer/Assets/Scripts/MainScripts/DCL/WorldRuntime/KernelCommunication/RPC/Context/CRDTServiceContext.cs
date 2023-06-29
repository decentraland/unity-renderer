using DCL;
using DCL.CRDT;
using DCL.ECS7;
using System;
using System.Collections.Generic;

namespace RPC.Context
{
    public class CRDTServiceContext
    {
        // TODO: remove `scenesOutgoingCrdts`
        public readonly Dictionary<int, DualKeyValueSet<int, long, CrdtMessage>> scenesOutgoingCrdts =
            new Dictionary<int, DualKeyValueSet<int, long, CrdtMessage>>(24);
        public IMessagingControllersManager MessagingControllersManager;
        public IWorldState WorldState;
        public ISceneController SceneController;
        public Action<int, CrdtMessage> CrdtMessageReceived;
        public IReadOnlyDictionary<int, ICRDTExecutor> CrdtExecutors;
        public Func<int, uint> GetSceneTick;
        public Action<int> IncreaseSceneTick;
        public Func<int, bool> IsSceneGltfLoadingFinished;
        public readonly Dictionary<int, DualKeyValueSet<long, int, WriteData>> ScenesOutgoingMsgs =
            new Dictionary<int, DualKeyValueSet<long, int, WriteData>>(24);
    }
}
