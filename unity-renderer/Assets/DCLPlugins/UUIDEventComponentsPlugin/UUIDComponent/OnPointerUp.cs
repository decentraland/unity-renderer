using DCL.Interface;
using DCL.Helpers;
using DCL.Models;
using Ray = UnityEngine.Ray;

namespace DCL.Components
{
    public class OnPointerUp : OnPointerEvent
    {
        public const string NAME = "pointerUp";

        public override void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled || !IsVisible())
                return;

            Model pointerEventModel = this.model as Model;

            if (pointerEventModel == null)
                return;

            bool validButton = pointerEventModel.button == "ANY" || buttonId.ToString() == pointerEventModel.button;

            if (IsAtHoverDistance(hit.distance) && validButton)
            {
                string meshName = pointerEventHandler.GetMeshName(hit.collider);
                string entityId = Environment.i.world.sceneController.entityIdHelper.GetOriginalId(entity.entityId);

                WebInterface.ReportOnPointerUpEvent(buttonId, scene.sceneData.id, pointerEventModel.uuid,
                    entityId, meshName, ray, hit.point, hit.normal, hit.distance);
            }
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.UUID_ON_UP;
        }

        public override PointerInputEventType GetEventType()
        {
            return PointerInputEventType.UP;
        }
    }
}