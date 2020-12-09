using DCL.Components;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Helpers
{
    public class RaycastHandler : IRaycastHandler
    {
        private void SetHitInfo(ref HitInfo hitInfo, RaycastHit hit)
        {
            hitInfo.point = Environment.i.worldState.ConvertUnityToScenePosition(hit.point);
            hitInfo.distance = hit.distance;
            hitInfo.normal = hit.normal;
            hitInfo.collider = hit.collider;
        }

        private bool Raycast(Ray ray, out HitInfo hitInfo, float distance, LayerMask layerMask)
        {
            RaycastHit hit;
            hitInfo = new HitInfo();

            if (Physics.Raycast(ray, out hit, distance, layerMask))
            {
                SetHitInfo(ref hitInfo, hit);
                return true;
            }

            return false;
        }

        public RaycastResultInfo Raycast(Ray ray, float distance, LayerMask layerMaskTarget, ParcelScene scene)
        {
            RaycastResultInfo raycastInfo = new RaycastResultInfo();

            raycastInfo.ray = ray;
            raycastInfo.hitInfo = new RaycastHitInfo();

            if (Raycast(raycastInfo.ray, out raycastInfo.hitInfo.hit, distance, layerMaskTarget))
            {
                SetRaycastInfoData(ref raycastInfo.hitInfo, scene);
            }

            return raycastInfo;
        }

        public RaycastResultInfoList RaycastAll(Ray ray, float distance, LayerMask layerMaskTarget, ParcelScene scene)
        {
            RaycastResultInfoList raycastInfo = new RaycastResultInfoList();
            raycastInfo.ray = ray;

            RaycastHit[] result = Physics.RaycastAll(raycastInfo.ray, distance, layerMaskTarget);

            if (result != null)
            {
                raycastInfo.hitInfo = new RaycastHitInfo[result.Length];
                int count = result.Length;

                for (int i = 0; i < count; i++)
                {
                    raycastInfo.hitInfo[i] = new RaycastHitInfo();
                    raycastInfo.hitInfo[i].hit = new HitInfo();
                    SetHitInfo(ref raycastInfo.hitInfo[i].hit, result[i]);
                    SetRaycastInfoData(ref raycastInfo.hitInfo[i], scene);
                }
            }

            return raycastInfo;
        }

        private void SetRaycastInfoData(ref RaycastHitInfo hitInfo, ParcelScene scene)
        {
            if (hitInfo.hit.collider == null)
                return;

            if (!CollidersManager.i.GetColliderInfo(hitInfo.hit.collider, out ColliderInfo info))
                return;

            if (scene != null)
                hitInfo.isValid = info.scene == scene;
            else if (scene == null && Environment.i.worldState.IsCharacterInsideScene(info.scene))
                hitInfo.isValid = true;
        }
    }
}