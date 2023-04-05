using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSComponents.Utils;
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
        private readonly ECSComponent<PBMeshCollider> meshCollider;
        private readonly IInternalECSComponent<InternalRaycast> internalRaycastComponent;
        private readonly IInternalECSComponent<InternalColliders> physicsColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> onPointerColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> customLayerColliderComponent;
        private readonly IECSComponentWriter componentWriter;

        public ECSRaycastSystem(
            ECSComponent<PBMeshCollider> meshCollider,
            IInternalECSComponent<InternalRaycast> internalRaycastComponent,
            IInternalECSComponent<InternalColliders> physicsColliderComponent,
            IInternalECSComponent<InternalColliders> onPointerColliderComponent,
            IInternalECSComponent<InternalColliders> customLayerColliderComponent,
            IECSComponentWriter componentWriter)
        {
            this.internalRaycastComponent = internalRaycastComponent;
            this.meshCollider = meshCollider;
            this.physicsColliderComponent = physicsColliderComponent;
            this.onPointerColliderComponent = onPointerColliderComponent;
            this.customLayerColliderComponent = customLayerColliderComponent;
            this.componentWriter = componentWriter;
        }

        public void Update()
        {
            var raycastComponentGroup = internalRaycastComponent.GetForAll();
            int entitiesCount = raycastComponentGroup.Count;

            // Note: the components are traversed backwards as some internal raycast components may be removed during iteration
            for (int i = entitiesCount - 1; i >= 0 ; i--)
            {
                PBRaycast model = raycastComponentGroup[i].value.model.raycastModel;
                IDCLEntity entity = raycastComponentGroup[i].value.entity;
                IParcelScene scene = raycastComponentGroup[i].value.scene;

                if (model.QueryType == RaycastQueryType.RqtNone)
                {
                    internalRaycastComponent.RemoveFor(scene, entity);
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

                // Hit everything by default except 'OnPointer' layer
                int raycastLayerMask = (1 << PhysicsLayers.defaultLayer)
                                     | (1 << PhysicsLayers.characterLayer)
                                     | (1 << PhysicsLayers.characterOnlyLayer);

                if (model.HasCollisionMask)
                    raycastLayerMask = CreateRaycastLayerMask(model.CollisionMask);

                RaycastHit[] hits = null;

                // If the raycast layerMask has SDKCustomLayer we have to use RaycastAll
                // because that  layer represents 8 different SDK layers: ClCustom1~8
                if (model.QueryType == RaycastQueryType.RqtQueryAll || LayerMaskUtils.LayerMaskHasAnySDKCustomLayer(model.CollisionMask))
                    hits = Physics.RaycastAll(ray, model.MaxDistance, raycastLayerMask);
                else if (Physics.Raycast(ray, out RaycastHit hit, model.MaxDistance, raycastLayerMask))
                    hits = new RaycastHit[] { hit };

                if (hits != null)
                {
                    DCL.ECSComponents.RaycastHit closestHit = null;

                    for (int j = 0; j < hits.Length; j++)
                    {
                        RaycastHit currentHit = hits[j];

                        IDCLEntity hitColliderEntity = FindMatchingColliderEntity(physicsColliderComponent.GetForAll(), currentHit.collider)
                                                     ?? FindMatchingColliderEntity(onPointerColliderComponent.GetForAll(), currentHit.collider)
                                                     ?? FindMatchingColliderEntity(customLayerColliderComponent.GetForAll(), currentHit.collider);

                        var hit = CreateSDKRaycastHit(scene, model, currentHit, hitColliderEntity, result.GlobalOrigin);
                        if (hit == null) continue;

                        if (model.QueryType == RaycastQueryType.RqtHitFirst)
                        {
                            // Since Unity's RaycastAll() resulting collection order is random (not based on hit distance),
                            // the closest hit has to be identified and populate the final collection with only that one
                            if (closestHit == null || currentHit.distance < closestHit.Length)
                                closestHit = hit;
                        }
                        else
                        {
                            result.Hits.Add(hit);
                        }
                    }

                    if (model.QueryType == RaycastQueryType.RqtHitFirst && closestHit != null)
                        result.Hits.Add(closestHit);
                }

                componentWriter.PutComponent(
                    scene.sceneData.sceneNumber, entity.entityId,
                    ComponentID.RAYCAST_RESULT,
                    result
                );

                // If the raycast on that entity isn't 'continuous' the internal component is removed and won't
                // be used for raycasting on the next system iteration. If its timestamp changes, the handler
                // adds the internal raycast component again.
                if(!model.HasContinuous || !model.Continuous)
                    internalRaycastComponent.RemoveFor(scene, entity);
            }
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

        private DCL.ECSComponents.RaycastHit CreateSDKRaycastHit(IParcelScene scene, PBRaycast model, RaycastHit unityRaycastHit, IDCLEntity hitEntity, Vector3 globalOrigin)
        {
            DCL.ECSComponents.RaycastHit hit = new DCL.ECSComponents.RaycastHit();
            if (hitEntity != null)
            {
                // If a collider with sdkCustomLayer is hit, the raycast layerMask has an SDK Custom Layer
                if (model.HasCollisionMask
                    && unityRaycastHit.transform.gameObject.layer == PhysicsLayers.sdkCustomLayer
                    && meshCollider.HasComponent(scene, hitEntity))
                {
                    int hitColliderRealCollisionMask = meshCollider.Get(scene, hitEntity).model.CollisionMask;

                    // If the meshCollider collision mask is not in the raycast collision mask, we ignore that entity
                    if ((model.CollisionMask & hitColliderRealCollisionMask) == 0)
                        return null;
                }

                hit.EntityId = (uint)hitEntity.entityId;
            }
            hit.MeshName = unityRaycastHit.collider.name;
            hit.Length = unityRaycastHit.distance;
            hit.GlobalOrigin = globalOrigin;

            var worldPosition = DCL.WorldStateUtils.ConvertUnityToScenePosition(unityRaycastHit.point, scene);
            hit.Position = new Vector3();
            hit.Position.X = worldPosition.x;
            hit.Position.Y = worldPosition.y;
            hit.Position.Z = worldPosition.z;

            hit.NormalHit = new Vector3();
            hit.NormalHit.X = unityRaycastHit.normal.x;
            hit.NormalHit.Y = unityRaycastHit.normal.y;
            hit.NormalHit.Z = unityRaycastHit.normal.z;

            return hit;
        }

        private int CreateRaycastLayerMask(uint sdkLayerMask)
        {
            bool hasPhysicsLayer = false;
            bool hasPointerLayer = false;
            int unityLayerMask = 0;

            if (LayerMaskUtils.IsInLayerMask(sdkLayerMask, (int)ColliderLayer.ClPointer))
            {
                unityLayerMask |= (1 << PhysicsLayers.onPointerEventLayer);
                hasPointerLayer = true;
            }

            if (LayerMaskUtils.IsInLayerMask(sdkLayerMask, (int)ColliderLayer.ClPhysics))
            {
                unityLayerMask |= (1 << PhysicsLayers.characterOnlyLayer);
                hasPhysicsLayer = true;
            }

            if(hasPointerLayer && hasPhysicsLayer)
                unityLayerMask |= (1 << PhysicsLayers.defaultLayer);

            if (LayerMaskUtils.LayerMaskHasAnySDKCustomLayer(sdkLayerMask))
            {
                unityLayerMask |= (1 << PhysicsLayers.sdkCustomLayer);
            }

            return unityLayerMask;
        }
    }
}
