using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSHiddenComponentHandler : IECSComponentHandler<PBHidden>
    {
        public ECSHiddenComponentHandler() { }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            entity.gameObject.SetActive(false);
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            entity.gameObject.SetActive(true);
        }
        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBHidden model) { }
    }
}