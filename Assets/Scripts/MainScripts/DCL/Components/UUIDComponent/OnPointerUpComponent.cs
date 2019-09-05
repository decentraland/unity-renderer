using DCL.Interface;
using UnityEngine;

namespace DCL.Components
{
    public class OnPointerUpComponent : OnPointerEventComponent
    {
        public const string NAME = "pointerUp";

        public void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, RaycastHit hit, bool isHitInfoValid)
        {
            if (!enabled)
            {
                return;
            }

            string meshName = null;

            if (isHitInfoValid)
                meshName = GetMeshName(hit.collider);

            DCL.Interface.WebInterface.ReportOnPointerUpEvent(buttonId, scene.sceneData.id, model.uuid, entity.entityId, meshName, ray, hit, isHitInfoValid);
        }
    }
}