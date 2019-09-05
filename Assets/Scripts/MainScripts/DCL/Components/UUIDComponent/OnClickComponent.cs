using DCL.Controllers;
using DCL.Models;
using UnityEngine;
using DCL.Interface;


namespace DCL.Components
{
    public class OnClickComponent : OnPointerEventComponent
    {
        public const string NAME = "onClick";

        public void Report(WebInterface.ACTION_BUTTON buttonId)
        {
            if (!enabled)
            {
                return;
            }

            if (buttonId == WebInterface.ACTION_BUTTON.POINTER)
                DCL.Interface.WebInterface.ReportOnClickEvent(scene.sceneData.id, model.uuid);
        }
    }
}