using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSVisibilityComponentHandler : IECSComponentHandler<PBVisibilityComponent>
    {
        private readonly IInternalECSComponent<InternalVisibility> visibilityInternalComponent;

        public ECSVisibilityComponentHandler(IInternalECSComponent<InternalVisibility> visibilityInternalComponent)
        {
            this.visibilityInternalComponent = visibilityInternalComponent;
        }
        
        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            visibilityInternalComponent.RemoveFor(scene, entity, new InternalVisibility() { visible = true });
        }
        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBVisibilityComponent model)
        {
            var internalModel = new InternalVisibility() { visible = model.GetVisible() };
            visibilityInternalComponent.PutFor(scene, entity, internalModel);
        }
    }
}