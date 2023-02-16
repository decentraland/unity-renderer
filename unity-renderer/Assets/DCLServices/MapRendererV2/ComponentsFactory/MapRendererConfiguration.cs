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
    }
}
