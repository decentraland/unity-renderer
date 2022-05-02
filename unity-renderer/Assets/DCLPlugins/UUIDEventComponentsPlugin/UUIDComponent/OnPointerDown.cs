using DCL.Interface;
using DCL.Helpers;
using DCL.Models;
using Ray = UnityEngine.Ray;

namespace DCL.Components
{
    public class OnPointerDown : OnPointerEvent
    {
        public const string NAME = "pointerDown";

        public override void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled || !IsVisible())
                return;

            Model model = this.model as OnPointerEvent.Model;

            if (ShouldReportEvent(buttonId, hit))
            {
                string meshName = pointerEventHandler.GetMeshName(hit.collider);
                string entityId = Environment.i.world.sceneController.entityIdHelper.GetOriginalId(entity.entityId);
                DCL.Interface.WebInterface.ReportOnPointerDownEvent(buttonId, scene.sceneData.id, model.uuid,
                    entityId, meshName, ray, hit.point, hit.normal, hit.distance);
            }
        }

        public bool ShouldReportEvent(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            Model model = this.model as Model;

            return IsVisible() &&
                   IsAtHoverDistance(hit.distance) &&
                   (model.button == "ANY" || buttonId.ToString() == model.button);
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.UUID_ON_DOWN;
        }

        public override PointerInputEventType GetEventType()
        {
            return PointerInputEventType.DOWN;
        }
    }
}