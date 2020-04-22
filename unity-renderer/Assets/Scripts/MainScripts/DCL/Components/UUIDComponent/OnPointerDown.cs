using DCL.Interface;
using UnityEngine;
using DCL.Helpers;

namespace DCL.Components
{
    public class OnPointerDown : OnPointerEvent
    {
        public const string NAME = "pointerDown";

        public virtual void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled || !IsVisible()) return;

            if (ShouldReportEvent(buttonId, hit))
            {
                string meshName = GetMeshName(hit.collider);

                DCL.Interface.WebInterface.ReportOnPointerDownEvent(buttonId, scene.sceneData.id, model.uuid, entity.entityId, meshName, ray, hit.point, hit.normal, hit.distance);
            }
        }

        protected bool ShouldReportEvent(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            return IsVisible() && IsAtHoverDistance(hit.distance) && (model.button == "ANY" || buttonId.ToString() == model.button);
        }
    }
}