using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Ray = UnityEngine.Ray;
using RaycastHit = UnityEngine.RaycastHit;
using Vector3 = DCL.ECSComponents.Vector3;

namespace DCLPlugins.ECSComponents.Raycast
{
    public class RaycastComponentHandler : IECSComponentHandler<PBRaycast>
    {
        private IECSComponentWriter componentWriter;
        private LayerMask raycastLayerMaskTarget;
        private IInternalECSComponent<InternalColliders> physicsColliderComponent;
        
        public RaycastComponentHandler(IECSComponentWriter componentWriter, IInternalECSComponent<InternalColliders> physicsColliderComponent)
        {
            this.componentWriter = componentWriter;
            this.physicsColliderComponent = physicsColliderComponent;

            // Cast all layers except the OnPointerEvent one
            raycastLayerMaskTarget = ~(1 << PhysicsLayers.onPointerEventLayer);
        }
        
        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBRaycast model)
        {
            var worldGridPosition = Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
            UnityEngine.Vector3 origin = new UnityEngine.Vector3(model.Origin.X, model.Origin.Y, model.Origin.Z);
            Ray ray = new Ray();
            ray.origin = PositionUtils.WorldToUnityPosition(origin + worldGridPosition);
            ray.direction = new UnityEngine.Vector3(model.Direction.X, model.Direction.Y, model.Direction.Z);
            
            PBRaycastResult result = new PBRaycastResult();
            result.Direction = model.Direction.Clone();
            result.Origin = model.Origin.Clone();
            result.Timestamp = model.Timestamp;
            
            RaycastHit[] hits = null;
            if (model.QueryType == RaycastQueryType.RqtHitFirst)
            {
                bool hasHit = Physics.Raycast(ray, out RaycastHit hit, model.MaxDistance, raycastLayerMaskTarget);
                if (hasHit)
                {
                    hits = new RaycastHit[1];
                    hits[0] = hit;
                }
            }
            else if (model.QueryType == RaycastQueryType.RqtQueryAll)
            {
                hits = Physics.RaycastAll(ray, model.MaxDistance, raycastLayerMaskTarget);
            }

            if (hits != null)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    IDCLEntity collisionEntity = null;

                    foreach (var currentEntity in scene.entities.Values)
                    {
                        var collider = physicsColliderComponent.GetFor(scene, currentEntity);
                        if (collider == null)
                            continue;

                        if (collider.model.colliders.Contains(hits[i].collider))
                        {
                            collisionEntity = currentEntity;
                            break;
                        }
                    }

                    DCL.ECSComponents.RaycastHit hit = new DCL.ECSComponents.RaycastHit();
                    hit.MeshName = hits[i].collider.name;
                    hit.Length = hits[i].distance;
                    hit.Origin = model.Origin.Clone();

                    var worldPosition = PositionUtils.UnityToWorldPosition(hits[i].point - worldGridPosition);
                    hit.Position = new Vector3();
                    hit.Position.X = worldPosition.x;
                    hit.Position.Y = worldPosition.y;
                    hit.Position.Z = worldPosition.z;

                    hit.NormalHit = new Vector3();
                    hit.NormalHit.X = hits[i].normal.x;
                    hit.NormalHit.Y = hits[i].normal.y;
                    hit.NormalHit.Z = hits[i].normal.z;

                    if (collisionEntity != null)
                    {
                        hit.EntityId = (int)collisionEntity.entityId;
                    }

                    result.Hits.Add(hit);
                }
            }

            componentWriter.PutComponent(
                scene.sceneData.id, entity.entityId, 
                ComponentID.RAYCAST_RESULT, 
                result, 
                ECSComponentWriteType.WRITE_STATE_LOCALLY | ECSComponentWriteType.SEND_TO_SCENE
            );
        }
    }
} 