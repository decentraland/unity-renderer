using UnityEngine;
using Variables.RealmsInfo;

namespace DCL
{
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
        public readonly BaseDictionary<string, Item> items = new BaseDictionary<string, Item>();
        public readonly BuilderInWorld builderInWorld = new BuilderInWorld();
        public readonly DataStore_Quests Quests = new DataStore_Quests();
        public readonly DataStore_HUDs HUDs = new DataStore_HUDs();
        public readonly BaseVariable<bool> isPlayerRendererLoaded = new BaseVariable<bool>();
        public readonly DataStore_Player player = new DataStore_Player();

        public class BuilderInWorld
        {
            public readonly BaseDictionary<string, CatalogItem> catalogItemDict = new BaseDictionary<string, CatalogItem>();
            public readonly BaseDictionary<string, CatalogItemPack> catalogItemPackDict = new BaseDictionary<string, CatalogItemPack>();
        }

        public class DataStore_Quests
        {
            public readonly BaseDictionary<string, QuestModel> quests = new BaseDictionary<string, QuestModel>();
            public readonly BaseCollection<string> pinnedQuests = new BaseCollection<string>();

            public readonly BaseCollection<string> questsCompletedNotification = new BaseCollection<string>();
            public readonly BaseCollection<(string, string)> rewardsNotification = new BaseCollection<(string, string)>();
        }

        public class DataStore_HUDs
        {
            public readonly BaseVariable<bool> questsPanelVisible = new BaseVariable<bool>(false);
            public readonly BaseVariable<bool> builderProjectsPanelVisible = new BaseVariable<bool>(false);
        }

        public class DataStore_Player
        {
            // NOTE: set when character is teleported (DCLCharacterController - Teleport)
            public readonly BaseTriggerVariable<Vector3> lastTeleportPosition = new BaseTriggerVariable<Vector3>(Vector3.zero);
        }
    }
}