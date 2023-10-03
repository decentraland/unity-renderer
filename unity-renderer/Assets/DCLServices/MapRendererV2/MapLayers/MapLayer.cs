using System;

namespace DCLServices.MapRendererV2.MapLayers
{
    [Flags]
    public enum MapLayer
    {
        None,
        ParcelsAtlas = 1,
        SatelliteAtlas = 1 << 1,
        HomePoint = 1 << 2,
        ScenesOfInterest = 1 << 3,
        HotUsersMarkers = 1 << 4,
        ColdUsersMarkers = 1 << 5,
        PlayerMarker = 1 << 6,
        ParcelHoverHighlight = 1 << 7,
        // Add yours
    }
}
