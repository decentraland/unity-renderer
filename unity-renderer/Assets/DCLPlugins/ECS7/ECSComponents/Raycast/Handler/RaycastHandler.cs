using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;

namespace DCLPlugins.ECSComponents.Raycast
{
    public class RaycastComponentHandler : IECSComponentHandler<PBRaycast>
    {
        public RaycastComponentHandler(IECSComponentWriter componentWriter, IInternalECSComponent<InternalColliders> physicsColliderComponent)
        {
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBRaycast model)
        {
        }
    }
}
