using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers
{
    public class GlobalScene : ParcelScene
    {
        public override bool IsInsideSceneBoundaries(Vector2 gridPosition)
        {
            return true;
        }

        protected override void SendMetricsEvent()
        {
        }
    }
}
