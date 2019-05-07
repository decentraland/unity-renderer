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

        public override bool HasContentsUrl(string url)
        {
            return !string.IsNullOrEmpty(url);
        }

        public override bool TryGetContentsUrl(string url, out string result)
        {
            result = url;

            if (string.IsNullOrEmpty(url)) return false;

            result = sceneData.baseUrl + "/" + url;

            if (VERBOSE)
                Debug.Log($">>> GetContentsURL from ... {url} ... RESULTING URL... = {result}");

            return true;
        }

        protected override void SendMetricsEvent()
        {
        }
    }
}
