using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;

namespace DCL
{
    public class DataStore_ECS7
    {
        public BaseDictionary<IParcelScene, IECSComponentsManager> componentsManagers = new BaseDictionary<IParcelScene, IECSComponentsManager>();
        public readonly BaseRefCountedCollection<(string sceneId, object model)> pendingSceneResources = new BaseRefCountedCollection<(string sceneId, object model)>();
        public ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
    }
}