using UnityEngine;

namespace DCL.Helpers
{
    public class RaycastHandler : IRaycastHandler
    {
        public RaycastHandler()
        {
        }

        private void SetHitInfo(ref HitInfo hitInfo, RaycastHit hit)
        {
            hitInfo.point = SceneController.i.ConvertUnityToScenePosition(hit.point);
            hitInfo.distance = hit.distance;
            hitInfo.normal = hit.normal;
            hitInfo.collider = hit.collider;
            hitInfo.rigidbody = hit.rigidbody;
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

        public RaycastResultInfo Raycast(Ray ray, float distance, LayerMask layerMaskTarget, string sceneId )
        {
            RaycastResultInfo raycastInfo = new RaycastResultInfo();

            raycastInfo.ray = ray;
            raycastInfo.hitInfo = new RaycastHitInfo();

            if (Raycast(raycastInfo.ray, out raycastInfo.hitInfo.hit, distance, layerMaskTarget))
            {
                SetRaycastInfoData(ref raycastInfo.hitInfo, sceneId);
            }

            return raycastInfo;
        }

        public RaycastResultInfoList RaycastAll(Ray ray, float distance, LayerMask layerMaskTarget, string sceneId)
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
                    SetRaycastInfoData(ref raycastInfo.hitInfo[i], sceneId);
                }
            }

            return raycastInfo;
        }

        private void SetRaycastInfoData(ref RaycastHitInfo hitInfo, string sceneId)
        {
            if (hitInfo.hit.collider != null && CollidersManager.i.GetInfo(hitInfo.hit.collider, out hitInfo.collider))
            {
                if (sceneId != null)
                    hitInfo.isValid = hitInfo.collider.sceneId == sceneId;
                else if (sceneId == null && SceneController.i.IsCharacterInsideScene(hitInfo.collider.sceneId))
                    hitInfo.isValid = true;
            }
        }
    }
}
