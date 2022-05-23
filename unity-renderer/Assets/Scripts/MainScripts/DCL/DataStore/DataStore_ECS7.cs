using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;

namespace DCL
{
    public class DataStore_ECS7
    {
        public BaseDictionary<IParcelScene, IECSComponentsManager> componentsManagers = new BaseDictionary<IParcelScene, IECSComponentsManager>();
        public BaseDictionary<string, BaseCollection<IECSResourceLoaderTracker>> sceneResources = new BaseDictionary<string, BaseCollection<IECSResourceLoaderTracker>>();
        public ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
    }
}