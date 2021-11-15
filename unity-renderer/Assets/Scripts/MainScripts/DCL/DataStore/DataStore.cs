using DCL.Builder;
using UnityEngine;
using Variables.RealmsInfo;

namespace DCL
{
    public enum AppMode
    {
        DEFAULT,
        BUILDER_IN_WORLD_EDITION
    }

    public class DataStore
    {
        private static DataStore instance = new DataStore();
        public static DataStore i { get => instance; }
        public static void Clear() => instance = new DataStore();

        public readonly CurrentRealmVariable playerRealm = new CurrentRealmVariable();
        public readonly RealmsVariable realmsInfo = new RealmsVariable();
        public readonly DebugConfig debugConfig = new DebugConfig();
        public readonly BaseVariable<bool> isSignUpFlow = new BaseVariable<bool>();
        public readonly BaseDictionary<string, WearableItem> wearables = new BaseDictionary<string, WearableItem>();
        public readonly DataStore_BuilderInWorld builderInWorld = new DataStore_BuilderInWorld();
        public readonly DataStore_Quests Quests = new DataStore_Quests();
        public readonly DataStore_HUDs HUDs = new DataStore_HUDs();
        public readonly BaseVariable<bool> isPlayerRendererLoaded = new BaseVariable<bool>();
        public readonly BaseVariable<AppMode> appMode = new BaseVariable<AppMode>();
        public readonly DataStore_Player player = new DataStore_Player();
        public readonly BaseVariable<NFTPromptModel> onOpenNFTPrompt = new BaseVariable<NFTPromptModel>();
        public readonly DataStore_AvatarsLOD avatarsLOD = new DataStore_AvatarsLOD();
        public readonly DataStore_VirtualAudioMixer virtualAudioMixer = new DataStore_VirtualAudioMixer();
        public readonly DataStore_Screen screen = new DataStore_Screen();
        public readonly DataStore_WSCommunication wsCommunication = new DataStore_WSCommunication();
        public readonly DataStore_WorldObjects sceneWorldObjects = new DataStore_WorldObjects();
        public readonly DataStore_ExploreV2 exploreV2 = new DataStore_ExploreV2();
        public readonly DataStore_FeatureFlag featureFlags = new DataStore_FeatureFlag();
        public readonly DataStore_Camera camera = new DataStore_Camera();
        public readonly DataStore_Settings settings = new DataStore_Settings();

        public class DataStore_Settings
        {
            public readonly BaseVariable<bool> profanityChatFilteringEnabled = new BaseVariable<bool>();
        }

        public class DataStore_WorldObjects
        {
            public class SceneData
            {
                public readonly BaseDictionary<Mesh, int> refCountedMeshes = new BaseDictionary<Mesh, int>();
                public readonly BaseHashSet<Rendereable> renderedObjects = new BaseHashSet<Rendereable>();
            }

            public readonly BaseDictionary<string, SceneData> sceneData = new BaseDictionary<string, SceneData>();
        }

        public class DataStore_BuilderInWorld
        {
            public readonly BaseDictionary<string, CatalogItem> catalogItemDict = new BaseDictionary<string, CatalogItem>();
            public readonly BaseDictionary<string, CatalogItem> currentSceneCatalogItemDict = new BaseDictionary<string, CatalogItem>();
            public readonly BaseDictionary<string, CatalogItemPack> catalogItemPackDict = new BaseDictionary<string, CatalogItemPack>();
            public readonly BaseVariable<PublishSceneResultPayload> unpublishSceneResult = new BaseVariable<PublishSceneResultPayload>();
            public readonly BaseVariable<bool> showTaskBar = new BaseVariable<bool>();
            public readonly BaseVariable<bool> isDevBuild = new BaseVariable<bool>();
            public readonly BaseVariable<LandWithAccess[]> landsWithAccess = new BaseVariable<LandWithAccess[]>();
            public readonly BaseVariable<ProjectData[]> projectData = new BaseVariable<ProjectData[]>();
            public readonly BaseVariable<Scene[]> scenesData = new BaseVariable<Scene[]>();
        }

        public class DataStore_Quests
        {
            public readonly BaseDictionary<string, QuestModel> quests = new BaseDictionary<string, QuestModel>();
            public readonly BaseCollection<string> pinnedQuests = new BaseCollection<string>();
        }

        public class DataStore_HUDs
        {
            public readonly BaseVariable<bool> questsPanelVisible = new BaseVariable<bool>(false);
            public readonly BaseVariable<bool> builderProjectsPanelVisible = new BaseVariable<bool>(false);
            public readonly BaseVariable<bool> signupVisible = new BaseVariable<bool>(false);
            public readonly BaseVariable<bool> controlsVisible = new BaseVariable<bool>(false);
            public readonly BaseVariable<bool> avatarEditorVisible = new BaseVariable<bool>(false);
            public readonly BaseVariable<bool> emotesVisible = new BaseVariable<bool>(false);
            public readonly BaseVariable<bool> navmapVisible = new BaseVariable<bool>(false);

            public readonly BaseVariable<bool> avatarNamesVisible = new BaseVariable<bool>(true);
            public readonly BaseVariable<float> avatarNamesOpacity = new BaseVariable<float>(1);
            public readonly LoadingHUD loadingHUD = new LoadingHUD();

            public class LoadingHUD
            {
                public readonly BaseVariable<bool> visible = new BaseVariable<bool>(false);
                public readonly BaseVariable<string> message = new BaseVariable<string>(null);
                public readonly BaseVariable<float> percentage = new BaseVariable<float>(0);
                public readonly BaseVariable<bool> showWalletPrompt = new BaseVariable<bool>(false);
                public readonly BaseVariable<bool> showTips = new BaseVariable<bool>(false);
            }
        }

        public class DataStore_Player
        {
            // NOTE: set when character is teleported (DCLCharacterController - Teleport)
            public readonly BaseVariable<Vector3> lastTeleportPosition = new BaseVariable<Vector3>(Vector3.zero);
            public readonly BaseDictionary<string, Player> otherPlayers = new BaseDictionary<string, Player>();
        }

        public class DataStore_AvatarsLOD
        {
            public const int DEFAULT_MAX_AVATAR = 50;
            public const int DEFAULT_MAX_IMPOSTORS = 70;

            public readonly BaseVariable<float> simpleAvatarDistance = new BaseVariable<float>(15f);
            public readonly BaseVariable<float> LODDistance = new BaseVariable<float>(30f);
            public readonly BaseVariable<int> maxAvatars = new BaseVariable<int>(DEFAULT_MAX_AVATAR);
            public readonly BaseVariable<int> maxImpostors = new BaseVariable<int>(DEFAULT_MAX_IMPOSTORS);
        }

        /// <summary>
        /// An "audio mixer" that handles muting/fading when entering special states like Avatar Editor, Tutorial, Builder In-World, etc.
        /// </summary>
        public class DataStore_VirtualAudioMixer
        {
            public readonly BaseVariable<float> musicVolume = new BaseVariable<float>(1f);
            public readonly BaseVariable<float> sceneSFXVolume = new BaseVariable<float>(1f);
            public readonly BaseVariable<float> voiceChatVolume = new BaseVariable<float>(1f);
            public readonly BaseVariable<float> uiSFXVolume = new BaseVariable<float>(1f);
            public readonly BaseVariable<float> avatarSFXVolume = new BaseVariable<float>(1f);
        }

        public class DataStore_Screen
        {
            public readonly BaseVariable<Vector2Int> size = new BaseVariable<Vector2Int>(Vector2Int.zero);
        }

        public class DataStore_WSCommunication
        {
            [System.NonSerialized]
            public string url = "ws://localhost:5000/";

            public readonly BaseVariable<bool> communicationEstablished = new BaseVariable<bool>();
            public readonly BaseVariable<bool> communicationReady = new BaseVariable<bool>();
        }

        public class DataStore_ExploreV2
        {
            public readonly BaseVariable<bool> isInitialized = new BaseVariable<bool>(false);
            public readonly BaseVariable<bool> isOpen = new BaseVariable<bool>(false);
        }

        public class DataStore_FeatureFlag
        {
            public readonly BaseVariable<FeatureFlag> flags = new BaseVariable<FeatureFlag>(new FeatureFlag());
        }

        public class DataStore_Camera
        {
            public readonly BaseVariable<Quaternion> rotation =  new BaseVariable<Quaternion>();
        }
    }
}