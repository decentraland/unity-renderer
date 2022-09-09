using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECS7.UI;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using UnityEngine;

namespace DCL
{
    public class DataStore_ECS7
    {
        public readonly BaseList<IParcelScene> scenes = new BaseList<IParcelScene>();
        public readonly BaseDictionary<int, BaseRefCountedCollection<object>> pendingSceneResources = new BaseDictionary<int, BaseRefCountedCollection<object>>();
        public readonly BaseDictionary<long, List<IPointerInputEvent>> entityEvents = new BaseDictionary<long, List<IPointerInputEvent>>();
        public readonly BaseDictionary<long, GameObject> shapesReady = new BaseDictionary<long, GameObject>();
        public IUIDataContainer uiDataContainer = new UIDataContainer();
    }
}