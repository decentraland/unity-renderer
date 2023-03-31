using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
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
        private readonly ECSComponent<PBMeshCollider> meshCollider;
        private readonly IInternalECSComponent<InternalColliders> physicsColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> onPointerColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> customLayerColliderComponent;
        private readonly IECSComponentWriter componentWriter;

        public ECSRaycastSystem(
            ECSComponent<PBRaycast> raycastComponent,
            ECSComponent<PBRaycastResult> raycastResultComponent,
            ECSComponent<PBMeshCollider> meshCollider,
            IInternalECSComponent<InternalColliders> physicsColliderComponent,
            IInternalECSComponent<InternalColliders> onPointerColliderComponent,
            IInternalECSComponent<InternalColliders> customLayerColliderComponent,
            IECSComponentWriter componentWriter)
        {
            this.raycastComponent = raycastComponent;
            this.raycastResultComponent = raycastResultComponent;
            this.meshCollider = meshCollider;
            this.physicsColliderComponent = physicsColliderComponent;
            this.onPointerColliderComponent = onPointerColliderComponent;
            this.customLayerColliderComponent = customLayerColliderComponent;
            this.componentWriter = componentWriter;
        }

        public void Update()
        {
            var raycasts = raycastComponent.Get();
            int raycastsCount = raycasts.Count;
            for (int i = 0; i < raycastsCount; i++)
            {
                PBRaycast model = raycasts[i].value.model;
                IDCLEntity entity = raycasts[i].value.entity;
                IParcelScene scene = raycasts[i].value.scene;

                if (model.QueryType == RaycastQueryType.RqtNone) continue;

                // If the entity has a raycastResult, the first ray was already cast
                if (raycastResultComponent.HasComponent(scene, entity))
                {
                    bool isContinuous = model.HasContinuous && model.Continuous;
                    if (!isContinuous && raycastResultComponent.Get(scene, entity).model.Timestamp == model.Timestamp)
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
                bool layerMaskHasSDKCustomLayer = false;
                LayerMask raycastLayerMask = new LayerMask()
                                             | (1 << PhysicsLayers.onPointerEventLayer)
                                             | (1 << PhysicsLayers.characterOnlyLayer)
                                             | (1 << PhysicsLayers.defaultLayer)
                                             | (1 << PhysicsLayers.sdkCustomLayer);

                if (model.HasCollisionMask)
                {
                    raycastLayerMask = ProtoConvertUtils.SDKCollisionMaskToUnityLayerMask(model.CollisionMask);
                    layerMaskHasSDKCustomLayer = ProtoConvertUtils.LayerMaskHasAnySDKCustomLayer(model.CollisionMask);
                }

                RaycastHit[] hits = null;

                // If the raycast layerMask has SDKCustomLayer we have to use RaycastAll
                // because that  layer represents 8 different SDK layers: ClCustom1~8
                if (layerMaskHasSDKCustomLayer || model.QueryType == RaycastQueryType.RqtQueryAll)
                {
                    // TODO: Use nonAlloc
                    hits = Physics.RaycastAll(ray, model.MaxDistance, raycastLayerMask);
                }
                else if (Physics.Raycast(ray, out RaycastHit hit, model.MaxDistance, raycastLayerMask))
                {
                    // TODO: Use nonAlloc
                    hits = new RaycastHit[] { hit };
                }

                if (hits != null)
                {
                    for (int j = 0; j < hits.Length; j++)
                    {
                        RaycastHit currentHit = hits[j];

                        IDCLEntity hitColliderEntity = FindMatchingColliderEntity(physicsColliderComponent.GetForAll(), currentHit.collider)
                                                     ?? FindMatchingColliderEntity(onPointerColliderComponent.GetForAll(), currentHit.collider)
                                                     ?? FindMatchingColliderEntity(customLayerColliderComponent.GetForAll(), currentHit.collider);

                        DCL.ECSComponents.RaycastHit hit = new DCL.ECSComponents.RaycastHit();
                        if (hitColliderEntity != null)
                        {
                            // If a collider with sdkCustomLayer is hit, the layerMask has an SDK Custom Layer
                            if (currentHit.transform.gameObject.layer == PhysicsLayers.sdkCustomLayer
                                && meshCollider.HasComponent(scene, hitColliderEntity))
                            {
                                int hitColliderRealCollisionMask = meshCollider.Get(scene, hitColliderEntity).model.CollisionMask;

                                // If the meshCollider collision mask is not in the raycast collision mask, we ignore that entity
                                if ((model.CollisionMask & hitColliderRealCollisionMask) == 0)
                                    continue;
                            }

                            hit.EntityId = (uint)hitColliderEntity.entityId;
                        }
                        hit.MeshName = currentHit.collider.name;
                        hit.Length = currentHit.distance;
                        hit.GlobalOrigin = result.GlobalOrigin;

                        var worldPosition = DCL.WorldStateUtils.ConvertUnityToScenePosition(currentHit.point, scene);
                        hit.Position = new Vector3();
                        hit.Position.X = worldPosition.x;
                        hit.Position.Y = worldPosition.y;
                        hit.Position.Z = worldPosition.z;

                        hit.NormalHit = new Vector3();
                        hit.NormalHit.X = currentHit.normal.x;
                        hit.NormalHit.Y = currentHit.normal.y;
                        hit.NormalHit.Z = currentHit.normal.z;

                        result.Hits.Add(hit);

                        // TODO: Improve this escape??
                        if (model.QueryType == RaycastQueryType.RqtHitFirst && result.Hits.Count == 1)
                            break;
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

        private IDCLEntity FindMatchingColliderEntity(IReadOnlyList<KeyValueSetTriplet<IParcelScene,long,ECSComponentData<InternalColliders>>> componentGroup, Collider targetCollider)
        {
            int componentsCount = componentGroup.Count;
            for (int k = 0; k < componentsCount; k++)
            {
                if (componentGroup[k].value.model.colliders.Contains(targetCollider))
                {
                    return componentGroup[k].value.entity;
                }
            }

            return null;
        }
    }
}
