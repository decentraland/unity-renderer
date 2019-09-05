using DCL.Interface;
using UnityEngine;

namespace DCL.Components
{
    public class OnPointerDownComponent : OnPointerEventComponent
    {
        public const string NAME = "pointerDown";

        public void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, RaycastHit hit)
        {
            if (!enabled)
            {
                return;
            }

            string meshName = GetMeshName(hit.collider);

            DCL.Interface.WebInterface.ReportOnPointerDownEvent(buttonId, scene.sceneData.id, model.uuid, entity.entityId, meshName, ray, hit);
        }
    }
}