using UnityEngine;

namespace DCL.Configuration
{
    public static class ApplicationSettings
    {
        public static string version = "0.8";
    }

    public static class EnvironmentSettings
    {
        public static bool DEBUG = true;
        public static readonly Vector3 MORDOR = new Vector3(10000, 10000, 10000);
        public static readonly int MORDOR_SCALAR = 10000;
    }

    public static class InputSettings
    {
        public static KeyCode PrimaryButtonKeyCode = KeyCode.E;
        public static KeyCode SecondaryButtonKeyCode = KeyCode.F;
    }

    public static class PlayerSettings
    {
        public static float POSITION_REPORTING_DELAY = 0.1f; // In seconds
        public static float WORLD_REPOSITION_MINIMUM_DISTANCE = 100f;
    }

    public static class ParcelSettings
    {
        public static float DEBUG_FLOOR_HEIGHT = -0.1f;
        public static float PARCEL_SIZE = 16f;
        public static float PARCEL_BOUNDARIES_THRESHOLD = 0.01f;
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
        public static UnityEngine.Vector2
            NORMALIZED_DIMENSIONS =
                new UnityEngine.Vector2(512f, 512f); // The image dimensions that correspond to Vector3.One scale

        public static string DAR_API_URL = "https://schema.decentraland.org/dar";
    }

    public static class PhysicsLayers
    {
        public static int defaultLayer = LayerMask.NameToLayer("Default");
        public static int onPointerEventLayer = LayerMask.NameToLayer("OnPointerEvent");
        public static int characterLayer = LayerMask.NameToLayer("CharacterController");
        public static int characterOnlyLayer = LayerMask.NameToLayer("CharacterOnly");
        public static LayerMask physicsCastLayerMask = 1 << onPointerEventLayer;

        public static LayerMask physicsCastLayerMaskWithoutCharacter = (physicsCastLayerMask | (1 << defaultLayer))
                                                                       & ~(1 << characterLayer)
                                                                       & ~(1 << characterOnlyLayer);

        public static int friendsHUDPlayerMenu = LayerMask.NameToLayer("FriendsHUDPlayerMenu");
        public static int playerInfoCardMenu = LayerMask.NameToLayer("PlayerInfoCardMenu");
    }
}