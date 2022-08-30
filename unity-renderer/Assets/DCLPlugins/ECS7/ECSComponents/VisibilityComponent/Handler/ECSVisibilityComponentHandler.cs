using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSVisibilityComponentHandler : IECSComponentHandler<PBVisibilityComponent>
    {
        public ECSVisibilityComponentHandler() { }

        private void SetVisibility(IDCLEntity entity, bool visible)
        {
            MeshesInfo meshesInfo = entity.meshesInfo;
            
            ShapeRepresentation shape = meshesInfo.currentShape as ShapeRepresentation;
            shape?.UpdateModel(visible, shape.HasCollisions());
            
            ECSComponentsUtils.ConfigurePrimitiveShapeVisibility(entity.gameObject, visible, meshesInfo.renderers);
        }
        
        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            SetVisibility(entity, true);
        }
        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBVisibilityComponent model)
        {
            SetVisibility(entity, model.GetVisible());
        }
    }
}