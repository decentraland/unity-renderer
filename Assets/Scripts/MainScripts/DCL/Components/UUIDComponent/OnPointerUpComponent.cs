using DCL.Interface;
using UnityEngine;
using DCL.Helpers;

namespace DCL.Components
{
    public class OnPointerUpComponent : OnPointerEventComponent
    {
        public const string NAME = "pointerUp";

        public void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit, bool isHitInfoValid)
        {
            if (!enabled)
            {
                return;
            }

            string meshName = null;

            if (isHitInfoValid)
                meshName = GetMeshName(hit.collider);

            DCL.Interface.WebInterface.ReportOnPointerUpEvent(buttonId, scene.sceneData.id, model.uuid, entity.entityId, meshName, ray, hit.point, hit.normal, hit.distance, isHitInfoValid);
        }
    }
}