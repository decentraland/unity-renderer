using DCLServices.MapRendererV2.MapLayers;
using System.Collections.Generic;

namespace DCLServices.MapRendererV2
{
    /// <summary>
    /// Contains methods and properties that must be exposed to Unit Tests without exposing them in the original file
    /// </summary>
    public partial class MapRenderer
    {
        internal IReadOnlyCollection<MapLayer> initializedLayersInternal => layers.Keys;

        internal void EnableLayersInternal(MapLayer mask) =>
            EnableLayers(mask);

        internal void DisableLayersInternal(MapLayer mask) =>
            DisableLayers(mask);
    }
}
