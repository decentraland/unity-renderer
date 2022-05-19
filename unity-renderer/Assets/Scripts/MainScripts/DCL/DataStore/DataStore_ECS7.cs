using DCL.Controllers;
using DCL.ECSRuntime;

namespace DCL
{
    public class DataStore_ECS7
    {
        public BaseDictionary<IParcelScene, ECSComponentsManager> componentsManagers = new BaseDictionary<IParcelScene, ECSComponentsManager>();
        public ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
        public IECSComponentWriter componentsWriter = null;
    }
}