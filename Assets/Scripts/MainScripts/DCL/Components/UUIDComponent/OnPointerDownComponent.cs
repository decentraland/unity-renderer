using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class OnPointerDownComponent : OnPointerEventComponent
    {
        public const string NAME = "pointerDown";

        public void Report(Ray ray, RaycastHit hit)
        {
            if (!enabled)
            {
                return;
            }

            string meshName = GetMeshName(hit.collider);

            DCL.Interface.WebInterface.ReportOnPointerDownEvent(scene.sceneData.id, model.uuid, entity.entityId, meshName, ray, hit);
        }
    }
}