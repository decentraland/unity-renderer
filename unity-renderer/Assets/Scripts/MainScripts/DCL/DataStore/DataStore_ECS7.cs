using System.Collections.Generic;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using UnityEngine;

namespace DCL
{
    public class DataStore_ECS7
    {
        public readonly BaseDictionary<string, BaseRefCountedCollection<object>> pendingSceneResources = new BaseDictionary<string, BaseRefCountedCollection<object>>();
        public readonly BaseDictionary<long, List<IPointerInputEvent>> entityEvents = new BaseDictionary<long, List<IPointerInputEvent>>();
        public readonly BaseDictionary<long, GameObject> shapesReady = new BaseDictionary<long, GameObject>();
    }
}