using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using UnityEngine;

namespace DCL
{
    public class DataStore_ECS7
    {
        public BaseDictionary<IParcelScene, ECSComponentsManager> componentsManagers = new BaseDictionary<IParcelScene, ECSComponentsManager>();
        public readonly BaseDictionary<string, BaseRefCountedCollection<object>> pendingSceneResources = new BaseDictionary<string, BaseRefCountedCollection<object>>();
        public ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
        public readonly BaseDictionary<long, List<IPointerInputEvent>> entityEvents = new BaseDictionary<long, List<IPointerInputEvent>>();
        public readonly BaseDictionary<long, GameObject> shapesReady = new BaseDictionary<long, GameObject>();
        public readonly BaseDictionary<string, BaseDictionary<long,PBUiTransform>> sceneCanvas = new BaseDictionary<string, BaseDictionary<long,PBUiTransform>>();
    }
}