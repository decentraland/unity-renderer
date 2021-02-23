using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    public class PhysicsCast : Singleton<PhysicsCast>
    {
        private LayerMask layerMaskTarget;

        private IRaycastHandler raycastHandler;

        public PhysicsCast()
        {
            layerMaskTarget = 1 << LayerMask.NameToLayer("OnPointerEvent");
            raycastHandler = new RaycastHandler();
        }

        public void Query(RaycastQuery query)
        {
            switch (query.raycastType)
            {
                case RaycastType.HIT_FIRST:
                    HitFirst(query);
                    break;
                case RaycastType.HIT_ALL:
                    HitAll(query);
                    break;
            }
        }

        private void HitFirst(RaycastQuery query)
        {
            WebInterface.RaycastHitEntity hitEntity;

            Environment.i.world.state.TryGetScene(query.sceneId, out IParcelScene scene);

            RaycastResultInfo raycastInfo = raycastHandler.Raycast(GetUnityRayFromQuery(query), query.ray.distance, ~layerMaskTarget, scene);
            WebInterface.RayInfo rayInfo = GetRayInfoFromQuery(query);

            if (raycastInfo != null)
            {
                CollidersManager.i.GetColliderInfo(raycastInfo.hitInfo.hit.collider, out ColliderInfo colliderInfo);

                hitEntity = new WebInterface.RaycastHitEntity()
                {
                    didHit = raycastInfo.hitInfo.isValid,
                    hitNormal = raycastInfo.hitInfo.hit.normal,
                    hitPoint = raycastInfo.hitInfo.hit.point,
                    ray = rayInfo,
                    entity = new WebInterface.HitEntityInfo()
                    {
                        entityId = colliderInfo.entity != null ? colliderInfo.entity.entityId : null,
                        meshName = colliderInfo.meshName
                    }
                };
            }
            else
            {
                hitEntity = new WebInterface.RaycastHitEntity()
                {
                    didHit = false,
                    ray = rayInfo
                };
            }

            WebInterface.ReportRaycastHitFirstResult(query.sceneId, query.id, query.raycastType, hitEntity);
        }

        private void HitAll(RaycastQuery query)
        {
            WebInterface.RaycastHitEntities raycastHitEntities = new WebInterface.RaycastHitEntities();

            IParcelScene scene = null;
            Environment.i.world.state.TryGetScene(query.sceneId, out scene);

            RaycastResultInfoList raycastResults = raycastHandler.RaycastAll(GetUnityRayFromQuery(query), query.ray.distance, ~layerMaskTarget, scene);

            raycastHitEntities.ray = GetRayInfoFromQuery(query);

            if (raycastResults.hitInfo != null && raycastResults.hitInfo.Length > 0)
            {
                int count = raycastResults.hitInfo.Length;
                List<WebInterface.RaycastHitEntity> hitEntityInfoList = new List<WebInterface.RaycastHitEntity>();

                for (int i = 0; i < count; i++)
                {
                    var hitInfo = raycastResults.hitInfo[i];
                    CollidersManager.i.GetColliderInfo(hitInfo.hit.collider, out ColliderInfo colliderInfo);

                    if (hitInfo.isValid)
                    {
                        WebInterface.RaycastHitEntity hitEntity = new WebInterface.RaycastHitEntity();
                        hitEntity.didHit = true;
                        hitEntity.ray = raycastHitEntities.ray;
                        hitEntity.hitPoint = hitInfo.hit.point;
                        hitEntity.hitNormal = hitInfo.hit.normal;
                        hitEntity.entity = new WebInterface.HitEntityInfo();
                        hitEntity.entity.entityId = colliderInfo.entity != null ? colliderInfo.entity.entityId : null;
                        hitEntity.entity.meshName = colliderInfo.meshName;
                        hitEntityInfoList.Add(hitEntity);
                    }
                }

                raycastHitEntities.didHit = true;
                raycastHitEntities.hitPoint = raycastResults.hitInfo[0].hit.point;
                raycastHitEntities.hitNormal = raycastResults.hitInfo[0].hit.normal;
                raycastHitEntities.entities = hitEntityInfoList.ToArray();
            }

            WebInterface.ReportRaycastHitAllResult(query.sceneId, query.id, query.raycastType, raycastHitEntities);
        }

        private void HitFirstAvatar(RaycastQuery query)
        {
        }

        private void HitAllAvatars(RaycastQuery query)
        {
        }

        private UnityEngine.Ray GetUnityRayFromQuery(RaycastQuery query)
        {
            UnityEngine.Ray ray = new UnityEngine.Ray();

            ray.origin = query.ray.unityOrigin;
            ray.direction = query.ray.direction;

            return ray;
        }

        private WebInterface.RayInfo GetRayInfoFromQuery(RaycastQuery query)
        {
            WebInterface.RayInfo rayInfo = new WebInterface.RayInfo();

            rayInfo.direction = query.ray.direction;
            rayInfo.distance = query.ray.distance;
            rayInfo.origin = query.ray.origin;

            return rayInfo;
        }
    }
}