using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class OnClickComponent : OnPointerEventComponent
    {
        public const string NAME = "onClick";

        public void Report()
        {
            if (!enabled)
            {
                return;
            }

            DCL.Interface.WebInterface.ReportOnClickEvent(scene.sceneData.id, model.uuid);
        }
    }
}