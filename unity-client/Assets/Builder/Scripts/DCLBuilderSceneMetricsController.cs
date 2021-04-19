using DCL;
using DCL.Controllers;
using DCL.Models;

namespace Builder
{
    public class DCLBuilderSceneMetricsController : SceneMetricsController
    {
        public DCLBuilderSceneMetricsController(ParcelScene sceneOwner) : base(sceneOwner)
        {
            Enable();
            isDirty = true;
        }

        protected override void OnEntityAdded(IDCLEntity e)
        {
            e.OnMeshesInfoUpdated += OnEntityMeshInfoUpdated;
            e.OnMeshesInfoCleaned += OnEntityMeshInfoCleaned;
        }

        protected override void OnEntityRemoved(IDCLEntity e)
        {
            e.OnMeshesInfoUpdated -= OnEntityMeshInfoUpdated;
            e.OnMeshesInfoCleaned -= OnEntityMeshInfoCleaned;

            if (!e.components.ContainsKey(CLASS_ID_COMPONENT.SMART_ITEM))
            {
                SubstractMetrics(e);
                model.entities = entitiesMetrics.Count;
            }
        }

        protected override void OnEntityMeshInfoUpdated(IDCLEntity entity)
        {
            //builder should only check scene limits for not smart items entities
            if (!entity.components.ContainsKey(CLASS_ID_COMPONENT.SMART_ITEM))
            {
                AddOrReplaceMetrics(entity);
                model.entities = entitiesMetrics.Count;
            }
            else
            {
                SubstractMetrics(entity);
                model.entities = entitiesMetrics.Count;
            }
        }
    }
}