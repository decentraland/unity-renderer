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
using Vector3 = Decentraland.Common.Vector3;

namespace ECSSystems.ECSRaycastSystem
{
    public class ECSRaycastSystem
    {
        private readonly ECSComponent<PBRaycast> raycastComponent;
        private readonly ECSComponent<PBRaycastResult> raycastResultComponent;
        private readonly IECSComponentWriter componentWriter;
        private readonly IInternalECSComponent<InternalColliders> physicsColliderComponent;

        public ECSRaycastSystem(
            ECSComponent<PBRaycast> raycastComponent,
            ECSComponent<PBRaycastResult> raycastResultComponent,
            IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalColliders> physicsColliderComponent)
        {
            this.raycastComponent = raycastComponent;
            this.componentWriter = componentWriter;
            this.physicsColliderComponent = physicsColliderComponent;
            this.raycastResultComponent = raycastResultComponent;
        }

        public void Update()
        {
            var raycasts = raycastComponent.Get();
            int count = raycasts.Count;
            for (int i = 0; i < count; i++)
            {
                PBRaycast model = raycasts[i].value.model;
                IDCLEntity entity = raycasts[i].value.entity;
                IParcelScene scene = raycasts[i].value.scene;

                // If the entity has a raycastResult, the first ray was already cast
                if (raycastResultComponent.HasComponent(scene, entity))
                {
                    bool isContinuous = model.HasContinuous && model.Continuous;
                    uint resultTimestamp = raycastResultComponent.Get(scene, entity).model.Timestamp;
                    if (!isContinuous && resultTimestamp == model.Timestamp)
                        continue;
                }

                Ray ray = CreateRay(scene, entity, model);
                if (ray.direction == UnityEngine.Vector3.zero)
                {
                    Debug.LogError("Raycast error: direction cannot be Vector3.Zero(); Raycast aborted.");
                    return;
                }

                PBRaycastResult result = new PBRaycastResult
                {
                    Direction = ProtoConvertUtils.UnityVectorToPBVector(ray.direction),
                    GlobalOrigin = ProtoConvertUtils.UnityVectorToPBVector(ray.origin),
                    Timestamp = model.Timestamp
                };

                // Hit everything by default
                int raycastLayerMaskTarget = PhysicsLayers.onPointerEventLayer | PhysicsLayers.characterOnlyLayer | PhysicsLayers.sdkCustomLayer;
                if (model.HasCollisionMask)
                    raycastLayerMaskTarget = ProtoConvertUtils.SDKCollisionMaskToUnityLayerMask(model.CollisionMask);

                // TODO: Deal with possibly more than one object in line, with custom collider layer, using always RaycastAll...

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
                    for (int j = 0; j < hits.Length; j++)
                    {
                        IDCLEntity collisionEntity = null;

                        foreach (var currentEntity in scene.entities.Values)
                        {
                            var collider = physicsColliderComponent.GetFor(scene, currentEntity);
                            if (collider == null)
                                continue;

                            if (collider.model.colliders.Contains(hits[j].collider))
                            {
                                collisionEntity = currentEntity;
                                break;
                            }
                        }

                        DCL.ECSComponents.RaycastHit hit = new DCL.ECSComponents.RaycastHit();
                        hit.MeshName = hits[j].collider.name;
                        hit.Length = hits[j].distance;
                        hit.GlobalOrigin = result.GlobalOrigin;

                        var worldPosition = DCL.WorldStateUtils.ConvertUnityToScenePosition(hits[j].point, scene);
                        hit.Position = new Vector3();
                        hit.Position.X = worldPosition.x;
                        hit.Position.Y = worldPosition.y;
                        hit.Position.Z = worldPosition.z;

                        hit.NormalHit = new Vector3();
                        hit.NormalHit.X = hits[j].normal.x;
                        hit.NormalHit.Y = hits[j].normal.y;
                        hit.NormalHit.Z = hits[j].normal.z;

                        if (collisionEntity != null)
                        {
                            hit.EntityId = (uint)collisionEntity.entityId;
                        }

                        result.Hits.Add(hit);
                    }
                }

                componentWriter.PutComponent(
                    scene.sceneData.sceneNumber, entity.entityId,
                    ComponentID.RAYCAST_RESULT,
                    result
                );

            }
        }

        private Ray CreateRay(IParcelScene scene, IDCLEntity entity, PBRaycast model)
        {
            Transform entityTransform = entity.gameObject.transform;
            UnityEngine.Vector3 sceneWorldPosition = Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
            UnityEngine.Vector3 sceneUnityPosition = PositionUtils.WorldToUnityPosition(sceneWorldPosition);
            UnityEngine.Vector3 rayOrigin = entityTransform.position + (model.OriginOffset != null ? ProtoConvertUtils.PBVectorToUnityVector(model.OriginOffset) : UnityEngine.Vector3.zero);
            UnityEngine.Vector3 rayDirection = UnityEngine.Vector3.zero;
            switch (model.DirectionCase)
            {
                case PBRaycast.DirectionOneofCase.LocalDirection:
                    // The direction of the ray in local coordinates (relative to the origin point)
                    rayDirection = entityTransform.rotation * ProtoConvertUtils.PBVectorToUnityVector(model.LocalDirection);
                    break;
                case PBRaycast.DirectionOneofCase.GlobalTarget:
                    // Target position to cast the ray towards, in global coordinates
                    rayDirection = sceneUnityPosition + ProtoConvertUtils.PBVectorToUnityVector(model.GlobalTarget) - entityTransform.position;
                    break;
                case PBRaycast.DirectionOneofCase.TargetEntity:
                    // Target entity to cast the ray towards
                    IDCLEntity targetEntity = scene.GetEntityById(model.TargetEntity);
                    rayDirection = targetEntity.gameObject.transform.position - entityTransform.position;
                    break;
                case PBRaycast.DirectionOneofCase.GlobalDirection:
                    // The direction of the ray in global coordinates
                    rayDirection = ProtoConvertUtils.PBVectorToUnityVector(model.GlobalDirection);
                    break;
            }

            Ray ray = new Ray
            {
                origin = rayOrigin,
                direction = rayDirection.normalized
            };
            return ray;
        }
    }
}
