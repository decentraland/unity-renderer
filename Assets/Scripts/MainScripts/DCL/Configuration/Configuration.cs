
namespace DCL.Configuration
{
    public static class ApplicationSettings
    {
        public static float version = 0.01f;
    }

    public static class Environment
    {
        public static bool DEBUG = true;
    }

    public static class ParcelSettings
    {
        public static float DEBUG_FLOOR_HEIGHT = 0.01f;
        public static float PARCEL_SIZE = 16f;
        public static bool VISUAL_LOADING_ENABLED = true;
    }

    public static class TestSettings
    {
        public static int VISUAL_TESTS_APPROVED_AFFINITY = 95;
        public static float VISUAL_TESTS_PIXELS_CHECK_THRESHOLD = 1.5f;
        public static int VISUAL_TESTS_SNAPSHOT_WIDTH = 1280;
        public static int VISUAL_TESTS_SNAPSHOT_HEIGHT = 720;
    }
}
