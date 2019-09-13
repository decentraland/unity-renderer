using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;
using DCL.Helpers;
using DCL.Models;

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
            switch (query.queryType)
            {
                case "HitFirst":
                    HitFirst(query);
                    break;
                case "HitAll":
                    HitAll(query);
                    break;
            }
        }

        private void HitFirst(RaycastQuery query)
        {
            WebInterface.RaycastHitEntity hitEntity;

            RaycastResultInfo raycastInfo = raycastHandler.Raycast(GetUnityRayFromQuery(query), query.ray.distance, ~layerMaskTarget, query.sceneId);
            WebInterface.RayInfo rayInfo = GetRayInfoFromQuery(query);

            if (raycastInfo != null)
            {
                hitEntity = new WebInterface.RaycastHitEntity()
                {
                    didHit = raycastInfo.hitInfo.isValid,
                    hitNormal = raycastInfo.hitInfo.hit.normal,
                    hitPoint = raycastInfo.hitInfo.hit.point,
                    ray = rayInfo,
                    entity = new WebInterface.HitEntityInfo()
                    {
                        entityId = raycastInfo.hitInfo.collider.entityId,
                        meshName = raycastInfo.hitInfo.collider.meshName
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

            WebInterface.ReportRaycastHitFirstResult(query.sceneId, query.queryId, query.queryType, hitEntity);
        }

        private void HitAll(RaycastQuery query)
        {
            WebInterface.RaycastHitEntities raycastHitEntities = new WebInterface.RaycastHitEntities();

            RaycastResultInfoList raycastResults = raycastHandler.RaycastAll(GetUnityRayFromQuery(query), query.ray.distance, ~layerMaskTarget, query.sceneId);

            raycastHitEntities.ray = GetRayInfoFromQuery(query);

            if (raycastResults.hitInfo != null && raycastResults.hitInfo.Length > 0)
            {
                int count = raycastResults.hitInfo.Length;
                List<WebInterface.RaycastHitEntity> hitEntityInfoList = new List<WebInterface.RaycastHitEntity>();

                for (int i = 0; i < count; i++)
                {
                    if (raycastResults.hitInfo[i].isValid)
                    {
                        WebInterface.RaycastHitEntity hitEntity = new WebInterface.RaycastHitEntity();
                        hitEntity.didHit = true;
                        hitEntity.ray = raycastHitEntities.ray;
                        hitEntity.hitPoint = raycastResults.hitInfo[i].hit.point;
                        hitEntity.hitNormal = raycastResults.hitInfo[i].hit.normal;
                        hitEntity.entity = new WebInterface.HitEntityInfo();
                        hitEntity.entity.entityId = raycastResults.hitInfo[i].collider.entityId;
                        hitEntity.entity.meshName = raycastResults.hitInfo[i].collider.meshName;
                        hitEntityInfoList.Add(hitEntity);
                    }
                }

                raycastHitEntities.didHit = true;
                raycastHitEntities.hitPoint = raycastResults.hitInfo[0].hit.point;
                raycastHitEntities.hitNormal = raycastResults.hitInfo[0].hit.normal;
                raycastHitEntities.entities = hitEntityInfoList.ToArray();
            }

            WebInterface.ReportRaycastHitAllResult(query.sceneId, query.queryId, query.queryType, raycastHitEntities);
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