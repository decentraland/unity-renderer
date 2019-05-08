
namespace DCL.Configuration
{
    public static class ApplicationSettings
    {
        public static float version = 0.2f;
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
        public static float DEBUG_FLOOR_HEIGHT = 0.01f;
        public static float PARCEL_SIZE = 16f;
        public static float UNLOAD_DISTANCE = PARCEL_SIZE * 12f;
        public static bool VISUAL_LOADING_ENABLED = true;
    }

    public static class TestSettings
    {
        public static int VISUAL_TESTS_APPROVED_AFFINITY = 95;
        public static float VISUAL_TESTS_PIXELS_CHECK_THRESHOLD = 1.5f;
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
}
