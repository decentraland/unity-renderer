using DCL.Interface;
using UnityEngine;
using DCL.Helpers;

namespace DCL.Components
{
    public class OnPointerUp : OnPointerEvent
    {
        public const string NAME = "pointerUp";

        public void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit, bool isHitInfoValid)
        {
            if (!enabled || !IsVisible()) return;

            if (IsAtHoverDistance(hit.distance) && (model.button == "ANY" || buttonId.ToString() == model.button))
            {
                string meshName = null;

                if (isHitInfoValid)
                    meshName = GetMeshName(hit.collider);

                DCL.Interface.WebInterface.ReportOnPointerUpEvent(buttonId, scene.sceneData.id, model.uuid, entity.entityId, meshName, ray, hit.point, hit.normal, hit.distance, isHitInfoValid);
            }
        }
    }
}