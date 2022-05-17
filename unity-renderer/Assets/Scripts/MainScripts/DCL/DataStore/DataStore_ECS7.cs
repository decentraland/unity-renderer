using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;

namespace DCL
{
    public class DataStore_ECS7
    {
        public BaseDictionary<IParcelScene, ECSComponentsManager> componentsManagers = new BaseDictionary<IParcelScene, ECSComponentsManager>();
        public ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
    }
}