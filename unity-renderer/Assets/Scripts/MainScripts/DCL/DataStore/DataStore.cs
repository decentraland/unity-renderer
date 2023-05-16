using DCL.ServerTime;
using System;
using System.Collections.Generic;

namespace DCL
{
    public enum AppMode
    {
        DEFAULT,
        BUILDER_IN_WORLD_EDITION,
    }

    public class DataStore
    {
        public static DataStore i { get; private set; } = new ();

        private readonly Dictionary<Type, object> dataStores = new ();

        public void Set<T>(T data) where T: class
        {
            if (!dataStores.ContainsKey(typeof(T)))
                dataStores.Add(typeof(T), data);
            else
                dataStores[typeof(T)] = data;
        }

        public T Get<T>() where T: class, new()
        {
            if (!dataStores.ContainsKey(typeof(T)))
                Set(new T());

            return dataStores[typeof(T)] as T;
        }

        public static void Clear() =>
            i = new DataStore();

        public DataStore_World world => i.Get<DataStore_World>();
        public DataStore_Common common => i.Get<DataStore_Common>();
        public DataStore_Realm realm => i.Get<DataStore_Realm>();
        public DebugConfig debugConfig => i.Get<DebugConfig>();
        public DataStore_Quests Quests => i.Get<DataStore_Quests>();
        public DataStore_HUDs HUDs => i.Get<DataStore_HUDs>();
        public DataStore_Player player => i.Get<DataStore_Player>();
        public DataStore_AvatarsLOD avatarsLOD => i.Get<DataStore_AvatarsLOD>();
        public DataStore_VirtualAudioMixer virtualAudioMixer => i.Get<DataStore_VirtualAudioMixer>();
        public DataStore_Screen screen => i.Get<DataStore_Screen>();
        public DataStore_WSCommunication wsCommunication => i.Get<DataStore_WSCommunication>();
        public DataStore_WorldObjects sceneWorldObjects => i.Get<DataStore_WorldObjects>();
        public DataStore_ExploreV2 exploreV2 => i.Get<DataStore_ExploreV2>();
        public DataStore_FeatureFlag featureFlags => i.Get<DataStore_FeatureFlag>();
        public DataStore_Camera camera => i.Get<DataStore_Camera>();
        public DataStore_Settings settings => i.Get<DataStore_Settings>();
        public DataStore_SkyboxConfig skyboxConfig => i.Get<DataStore_SkyboxConfig>();
        public WorldTimer worldTimer => i.Get<WorldTimer>();
        public DataStore_Performance performance => i.Get<DataStore_Performance>();
        public DataStore_ExperiencesViewer experiencesViewer => i.Get<DataStore_ExperiencesViewer>();
        public DataStore_Emotes emotes => i.Get<DataStore_Emotes>();
        public DataStore_EmotesCustomization emotesCustomization => i.Get<DataStore_EmotesCustomization>();
        public DataStore_SceneBoundariesChecker sceneBoundariesChecker => i.Get<DataStore_SceneBoundariesChecker>();
        public DataStore_ECS7 ecs7 => i.Get<DataStore_ECS7>();
        public DataStore_VoiceChat voiceChat => i.Get<DataStore_VoiceChat>();
        public DataStore_TextureConfig textureConfig => i.Get<DataStore_TextureConfig>();
        public DataStore_FriendNotifications friendNotifications => i.Get<DataStore_FriendNotifications>();
        public DataStore_AvatarConfig avatarConfig => i.Get<DataStore_AvatarConfig>();
        public DataStore_Rpc rpc => i.Get<DataStore_Rpc>();
        public DataStore_Channels channels => i.Get<DataStore_Channels>();
        public DataStore_WorldBlockers worldBlockers => i.Get<DataStore_WorldBlockers>();
        public DataStore_Notifications notifications => i.Get<DataStore_Notifications>();
        public DataStore_Outliner outliner => i.Get<DataStore_Outliner>();
        public DataStore_Mentions mentions => i.Get<DataStore_Mentions>();
        public DataStore_BackpackV2 backpackV2 => i.Get<DataStore_BackpackV2>();
    }

    public struct DataStoreRef<T> where T: class, new()
    {
        private T @ref;

        public T Ref => @ref ??= DataStore.i.Get<T>();
    }

}



