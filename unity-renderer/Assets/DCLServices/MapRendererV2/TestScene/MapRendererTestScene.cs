using DCL;
using UnityEngine;

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapRendererTestScene : MonoBehaviour
    {
        [SerializeField] internal MapRendererTestSceneContainer container;
        [SerializeField] internal int parcelSize = 20;
        [SerializeField] internal int atlasChunkSize = 1020;
        [SerializeField] internal float cullingBoundsInParcels = 5;

        internal bool initialized { get; set; }

        public void OnDestroy()
        {
            if (initialized)
                Environment.Dispose();
        }
    }
}
