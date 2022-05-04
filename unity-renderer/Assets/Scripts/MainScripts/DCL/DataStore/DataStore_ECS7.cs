using DCL.Controllers;
using DCL.ECSRuntime;
using UnityEngine;

namespace DCL
{
    public class DataStore_ECS7 : MonoBehaviour
    {
        public BaseDictionary<IParcelScene, ECSComponentsManager> componentsManagers = new BaseDictionary<IParcelScene, ECSComponentsManager>();
        public ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
    }
}