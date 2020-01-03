using DCL.Controllers;
using DCL.Models;
using UnityEngine;
using DCL.Interface;


namespace DCL.Components
{
    public class OnClick : OnPointerEvent
    {
        public const string NAME = "onClick";

        public void Report(WebInterface.ACTION_BUTTON buttonId)
        {
            if (!enabled) return;

            if (model.button == "ANY" || buttonId.ToString() == model.button)
                DCL.Interface.WebInterface.ReportOnClickEvent(scene.sceneData.id, model.uuid);
        }
    }
}