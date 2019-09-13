using DCL.Models;
using UnityEngine;

namespace DCL.Controllers
{
    public class GlobalScene : ParcelScene
    {
        public override bool IsInsideSceneBoundaries(Vector2Int gridPosition, float height = 0)
        {
            return true;
        }

        public override void SetData(LoadParcelScenesMessage.UnityParcelScene data)
        {
            this.sceneData = data;

            contentProvider = new ContentProvider_Dummy();
            contentProvider.baseUrl = data.baseUrl;

            this.name = gameObject.name = $"ui scene:{data.id}";
        }

        protected override void SendMetricsEvent()
        {
        }
    }
}
