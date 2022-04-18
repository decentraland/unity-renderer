using UnityEngine;
using DCL.Interface;
using DCL.Helpers;
using DCL.Models;
using Ray = UnityEngine.Ray;

namespace DCL.Components
{
    public class OnClick : OnPointerEvent
    {
        public const string NAME = "onClick";

        public override void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled || !IsVisible())
                return;

            Model model = (Model) this.model;

            if (IsAtHoverDistance(hit.distance)
                && (model.button == "ANY" || buttonId.ToString() == model.button))
            {
                DCL.Interface.WebInterface.ReportOnClickEvent(scene.sceneData.id, model.uuid);
            }
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.UUID_ON_CLICK;
        }

        public override PointerInputEventType GetEventType()
        {
            return PointerInputEventType.CLICK;
        }
    }
}