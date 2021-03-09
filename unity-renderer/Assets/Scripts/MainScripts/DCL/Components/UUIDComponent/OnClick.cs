using UnityEngine;
using DCL.Interface;
using DCL.Helpers;

namespace DCL.Components
{
    public class OnClick : OnPointerEvent
    {
        public const string NAME = "onClick";

        public void Report(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            if (!enabled || !IsVisible()) return;

            if (IsAtHoverDistance(hit.distance) && (model.button == "ANY" || buttonId.ToString() == model.button))
                DCL.Interface.WebInterface.ReportOnClickEvent(scene.sceneData.id, model.uuid);
        }
    }
}