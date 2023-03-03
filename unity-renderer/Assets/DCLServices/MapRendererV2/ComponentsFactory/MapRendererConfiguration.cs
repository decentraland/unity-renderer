using UnityEngine;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    /// <summary>
    /// Placed on the prefab, contains the hierarchical structure needed for Map Layers
    /// </summary>
    public class MapRendererConfiguration : MonoBehaviour
    {
        [field: SerializeField]
        public Transform AtlasRoot { get; private set; }

        [field: SerializeField]
        public Transform ColdUserMarkersRoot { get; private set; }

        [field: SerializeField]
        public Transform HotUserMarkersRoot { get; private set; }

        [field: SerializeField]
        public Transform ScenesOfInterestMarkersRoot { get; private set; }

        [field: SerializeField]
        public Transform HomePointRoot { get; private set; }

        [field: SerializeField]
        public Transform PlayerMarkerRoot { get; private set; }

        [field: SerializeField]
        public Transform ParcelHighlightRoot { get; private set; }

        [field: SerializeField]
        public Transform MapCamerasRoot { get; private set; }
    }
}
