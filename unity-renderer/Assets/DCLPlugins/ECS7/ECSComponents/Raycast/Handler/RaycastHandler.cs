using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;

namespace DCLPlugins.ECSComponents.Raycast
{
    public class RaycastComponentHandler : IECSComponentHandler<PBRaycast>
    {
        private IInternalECSComponent<InternalRaycast> internalRaycastComponent;
        private InternalRaycast internalRaycastModel = new InternalRaycast();
        private PBRaycast previousModel;

        public RaycastComponentHandler(IInternalECSComponent<InternalRaycast> internalRaycastComponent)
        {
            this.internalRaycastComponent = internalRaycastComponent;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            internalRaycastComponent.RemoveFor(scene, entity);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBRaycast model)
        {
            if (!model.Continuous && previousModel != null && !previousModel.Continuous
                && previousModel.Timestamp == model.Timestamp)
                return;

            internalRaycastModel.raycastModel = model;

            // Ray casting is done in ECSRaycastSystem for all entities with the InternalRaycast component
            internalRaycastComponent.PutFor(scene, entity, internalRaycastModel);

            previousModel = model;
        }
    }
}
