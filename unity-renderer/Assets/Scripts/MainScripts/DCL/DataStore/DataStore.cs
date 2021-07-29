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
        public readonly DataStore_BuilderInWorld dataStoreBuilderInWorld = new DataStore_BuilderInWorld();
        public readonly DataStore_Quests Quests = new DataStore_Quests();
        public readonly DataStore_HUDs HUDs = new DataStore_HUDs();
        public readonly BaseVariable<bool> isPlayerRendererLoaded = new BaseVariable<bool>();
        public readonly BaseVariable<AppMode> appMode = new BaseVariable<AppMode>();
        public readonly DataStore_Player player = new DataStore_Player();
        public readonly BaseVariable<NFTPromptModel> onOpenNFTPrompt = new BaseVariable<NFTPromptModel>();
        public readonly DataStore_AvatarsLOD avatarsLOD = new DataStore_AvatarsLOD();
        public readonly DataStore_VirtualAudioMixer virtualAudioMixer = new DataStore_VirtualAudioMixer();

        public class DataStore_BuilderInWorld
        {
            public readonly BaseDictionary<string, CatalogItem> catalogItemDict = new BaseDictionary<string, CatalogItem>();
            public readonly BaseDictionary<string, CatalogItemPack> catalogItemPackDict = new BaseDictionary<string, CatalogItemPack>();
            public readonly BaseVariable<PublishSceneResultPayload> unpublishSceneResult = new BaseVariable<PublishSceneResultPayload>();
            public readonly BaseVariable<bool> showTaskBar = new BaseVariable<bool>();
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
            public readonly BaseDictionary<string, PlayerStatus> otherPlayersStatus = new BaseDictionary<string, PlayerStatus>();
        }

        public class DataStore_AvatarsLOD
        {
            public readonly BaseVariable<bool> LODEnabled = new BaseVariable<bool>(false); // false because of feature flag...
            public readonly BaseVariable<float> LODDistance = new BaseVariable<float>(16f);
            public readonly BaseVariable<int> maxNonLODAvatars = new BaseVariable<int>(20);
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
    }
}