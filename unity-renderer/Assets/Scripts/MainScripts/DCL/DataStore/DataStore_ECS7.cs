using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSComponents;
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
        public readonly BaseDictionary<string, BaseDictionary<long,PBUiTransform>> sceneCanvas = new BaseDictionary<string, BaseDictionary<long,PBUiTransform>>();
    }
}