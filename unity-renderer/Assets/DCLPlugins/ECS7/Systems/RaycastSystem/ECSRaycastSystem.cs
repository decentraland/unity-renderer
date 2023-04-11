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
        private readonly IInternalECSComponent<InternalRaycast> internalRaycastComponent;
        private readonly IInternalECSComponent<InternalColliders> physicsColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> onPointerColliderComponent;
        private readonly IInternalECSComponent<InternalColliders> customLayerColliderComponent;
        private readonly IECSComponentWriter componentWriter;

        public ECSRaycastSystem(
            IInternalECSComponent<InternalRaycast> internalRaycastComponent,
            IInternalECSComponent<InternalColliders> physicsColliderComponent,
            IInternalECSComponent<InternalColliders> onPointerColliderComponent,
            IInternalECSComponent<InternalColliders> customLayerColliderComponent,
            IECSComponentWriter componentWriter)
        {
            this.internalRaycastComponent = internalRaycastComponent;
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
                int raycastUnityLayerMask = CreateRaycastLayerMask(model);

                RaycastHit[] hits = null;

                // RaycastAll is used because 'Default' layer represents a combination of ClPointer and ClPhysics
                // and 'SDKCustomLayer' layer represents 8 different SDK layers: ClCustom1~8
                hits = Physics.RaycastAll(ray, model.MaxDistance, raycastUnityLayerMask);
                if (hits != null)
                {
                    DCL.ECSComponents.RaycastHit closestHit = null;
                    for (int j = 0; j < hits.Length; j++)
                    {
                        RaycastHit currentHit = hits[j];
                        uint raycastSDKCollisionMask = model.GetCollisionMask();
                        KeyValuePair<IDCLEntity, int>? hitEntity = null;

                        if (LayerMaskUtils.IsInLayerMask(raycastSDKCollisionMask, (int)ColliderLayer.ClPhysics))
                            hitEntity = FindMatchingColliderEntity(physicsColliderComponent.GetForAll(), currentHit.collider);
                        if(hitEntity == null && LayerMaskUtils.IsInLayerMask(raycastSDKCollisionMask, (int)ColliderLayer.ClPointer))
                            hitEntity = FindMatchingColliderEntity(onPointerColliderComponent.GetForAll(), currentHit.collider);
                        if(hitEntity == null && LayerMaskUtils.LayerMaskHasAnySDKCustomLayer(raycastSDKCollisionMask))
                            hitEntity = FindMatchingColliderEntity(customLayerColliderComponent.GetForAll(), currentHit.collider);

                        var hit = CreateSDKRaycastHit(scene, model, currentHit, hitEntity, result.GlobalOrigin);
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

        private KeyValuePair<IDCLEntity, int>? FindMatchingColliderEntity(IReadOnlyList<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalColliders>>> componentGroup, Collider targetCollider)
        {
            int componentsCount = componentGroup.Count;
            for (int i = 0; i < componentsCount; i++)
            {
                var colliders = componentGroup[i].value.model.colliders;
                if (colliders.ContainsKey(targetCollider))
                {
                    return new KeyValuePair<IDCLEntity, int>(componentGroup[i].value.entity, colliders[targetCollider]);
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
                    if(scene.entities.TryGetValue(model.TargetEntity, out IDCLEntity targetEntity))
                        rayDirection = targetEntity.gameObject.transform.position - entityTransform.position;
                    else
                        Debug.LogError("Raycast error: can't find targetEntity, direction calculation failed.");
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

        private DCL.ECSComponents.RaycastHit CreateSDKRaycastHit(IParcelScene scene, PBRaycast model, RaycastHit unityRaycastHit, KeyValuePair<IDCLEntity, int>? hitEntity, Vector3 globalOrigin)
        {
            if (hitEntity == null) return null;

            DCL.ECSComponents.RaycastHit hit = new DCL.ECSComponents.RaycastHit();
            IDCLEntity entity = hitEntity.Value.Key;
            int collisionMask = hitEntity.Value.Value;

            // hitEntity has to be evaluated since 'Default' layer represents a combination of ClPointer
            // and ClPhysics, and 'SDKCustomLayer' layer represents 8 different SDK layers: ClCustom1~8
            if ((model.GetCollisionMask() & collisionMask) == 0)
                return null;

            hit.EntityId = (uint)entity.entityId;
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

        private int CreateRaycastLayerMask(PBRaycast model)
        {
            uint sdkLayerMask = 0;
            int unityLayerMask = (1 << PhysicsLayers.characterLayer);
            sdkLayerMask = model.GetCollisionMask();

            if (sdkLayerMask == (int)ColliderLayer.ClPointer)
            {
                unityLayerMask |= (1 << PhysicsLayers.onPointerEventLayer);
                unityLayerMask |= (1 << PhysicsLayers.defaultLayer); // Pointer + Physics ends up in 'Default' GO Layer
                return unityLayerMask;
            }

            if (sdkLayerMask == (int)ColliderLayer.ClPhysics)
            {
                unityLayerMask |= (1 << PhysicsLayers.characterOnlyLayer);
                unityLayerMask |= (1 << PhysicsLayers.defaultLayer); // Pointer + Physics ends up in 'Default' GO Layer
                return unityLayerMask;
            }

            unityLayerMask |= (1 << PhysicsLayers.defaultLayer)
                              | (1 << PhysicsLayers.characterOnlyLayer)
                              | (1 << PhysicsLayers.onPointerEventLayer);

            if (LayerMaskUtils.LayerMaskHasAnySDKCustomLayer(sdkLayerMask))
            {
                unityLayerMask |= (1 << PhysicsLayers.sdkCustomLayer);
            }

            return unityLayerMask;
        }
    }
}
