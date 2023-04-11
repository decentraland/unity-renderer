using System;

namespace DCLServices.MapRendererV2.MapLayers
{
    [Flags]
    public enum MapLayer
    {
        None,
        Atlas = 1,
        HomePoint = 1 << 1,
        ScenesOfInterest = 1 << 2,
        PlayerMarker = 1 << 3,
        HotUsersMarkers = 1 << 4,
        ColdUsersMarkers = 1 << 5,
        ParcelHoverHighlight = 1 << 6
        // Add yours
    }
}
