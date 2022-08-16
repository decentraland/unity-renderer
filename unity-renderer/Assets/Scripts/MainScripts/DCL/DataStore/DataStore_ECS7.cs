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
        public readonly BaseDictionary<string, BaseRefCountedCollection<object>> pendingSceneResources = new BaseDictionary<string, BaseRefCountedCollection<object>>();
        public readonly BaseDictionary<long, List<IPointerInputEvent>> entityEvents = new BaseDictionary<long, List<IPointerInputEvent>>();
        public readonly BaseDictionary<long, GameObject> shapesReady = new BaseDictionary<long, GameObject>();
        public IUIDataContainer uiDataContainer = new UIDataContainer();
        
        // Those are related to the OnPointerEvents
        public readonly BaseDictionary<long, List<IPointerInputEvent>> entityPointerEvents = new BaseDictionary<long, List<IPointerInputEvent>>();
        public readonly BaseRefCounter<long> entitiesOnPointerEventCounter = new BaseRefCounter<long>();
        public readonly BaseDictionary<long, List<GameObject>> entityOnPointerEventColliderGameObject = new BaseDictionary<long, List<GameObject>>();
    }
}