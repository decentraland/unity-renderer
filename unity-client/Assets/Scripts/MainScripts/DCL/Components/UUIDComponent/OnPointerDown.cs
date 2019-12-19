using DCL.Interface;
using UnityEngine;
using DCL.Helpers;

namespace DCL.Components
{
    public class OnPointerDown : OnPointerEvent
    {
        public const string NAME = "pointerDown";

        public void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled)
            {
                return;
            }

            string meshName = GetMeshName(hit.collider);

            DCL.Interface.WebInterface.ReportOnPointerDownEvent(buttonId, scene.sceneData.id, model.uuid, entity.entityId, meshName, ray, hit.point, hit.normal, hit.distance);
        }
    }
}