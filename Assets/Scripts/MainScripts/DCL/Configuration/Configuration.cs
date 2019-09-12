namespace DCL.Configuration
{
    public static class ApplicationSettings
    {
        public static string version = "0.7.7";
    }

    public static class Environment
    {
        public static bool DEBUG = true;
    }
    public static class PlayerSettings
    {
        public static float POSITION_REPORTING_DELAY = 0.1f; // In seconds
    }

    public static class ParcelSettings
    {
        public static float DEBUG_FLOOR_HEIGHT = -0.1f;
        public static float PARCEL_SIZE = 16f;
        public static float UNLOAD_DISTANCE = PARCEL_SIZE * 12f;
        public static bool VISUAL_LOADING_ENABLED = true;
    }

    public static class TestSettings
    {
        public static int VISUAL_TESTS_APPROVED_AFFINITY = 95;
        public static float VISUAL_TESTS_PIXELS_CHECK_THRESHOLD = 2.5f;
        public static int VISUAL_TESTS_SNAPSHOT_WIDTH = 1280;
        public static int VISUAL_TESTS_SNAPSHOT_HEIGHT = 720;
    }

    public static class AssetManagerSettings
    {
        //When library item count gets above this threshold, unused items will get pruned on Get() method.
        public static int LIBRARY_CLEANUP_THRESHOLD = 10;
    }

    public static class MessageThrottlingSettings
    {
        public static float SIXTY_FPS_TIME = 1.0f / 60.0f;
        public static float GLOBAL_FRAME_THROTTLING_TIME = SIXTY_FPS_TIME / 8.0f;
        public static float LOAD_PARCEL_SCENES_THROTTLING_TIME = SIXTY_FPS_TIME / 4.0f;
    }

    public static class UISettings
    {
        public static float RESERVED_CANVAS_TOP_PERCENTAGE = 10f;
    }

    public static class NFTDataFetchingSettings
    {
        public static UnityEngine.Vector2 NORMALIZED_DIMENSIONS = new UnityEngine.Vector2(512f, 512f); // The image dimensions that correspond to Vector3.One scale
        public static string DAR_API_URL = "https://schema.decentraland.org/dar";
    }
}
