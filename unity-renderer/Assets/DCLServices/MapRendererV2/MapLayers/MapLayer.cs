using System;

namespace DCLServices.MapRendererV2.MapLayers
{
    [Flags]
    public enum MapLayer
    {
        None,
        Atlas = 1,
        HomePoint = 1 << 1,
        PointsOfInterest = 1 << 2,
        // Add yours
    }
}
