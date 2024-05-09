namespace DCLServices.MapRendererV2.ComponentsFactory
{
    /// <summary>
    /// Corresponds to the sprites sorting order of map elements
    /// </summary>
    internal static class MapRendererDrawOrder
    {
        internal const int ATLAS = 1;
        internal const int SATELLITE_ATLAS = 2;
        internal const int HOME_POINT = 3;
        internal const int COLD_USER_MARKERS = 10;
        internal const int HOT_USER_MARKERS = 11;
        internal const int FRIEND_USER_MARKERS = 13;
        internal const int SCENES_OF_INTEREST = 15;
        internal const int FAVORITES = 17;
        internal const int PLAYER_MARKER = 19;
        internal const int PARCEL_HIGHLIGHT = 30;

    }
}
