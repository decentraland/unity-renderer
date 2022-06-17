using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;
using UnityEngine;

namespace DCL
{
    public class DataStore_ECS7
    {
        public BaseDictionary<IParcelScene, ECSComponentsManager> componentsManagers = new BaseDictionary<IParcelScene, ECSComponentsManager>();
        public readonly BaseDictionary<string, BaseRefCountedCollection<object>> pendingSceneResources = new BaseDictionary<string, BaseRefCountedCollection<object>>();
        public ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
        public readonly BaseDictionary<long, GameObject> animatorShapesReady = new BaseDictionary<long, GameObject>();
    }
}