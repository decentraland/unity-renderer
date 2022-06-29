using System;
using System.Collections.Generic;

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

        private Dictionary<Type, object> dataStores = new Dictionary<Type, object>();

        public void Set<T>(T data) where T : class
        {
            if (!dataStores.ContainsKey(typeof(T)))
                dataStores.Add(typeof(T), data);
            else
                dataStores[typeof(T)] = data;
        }

        public T Get<T>() where T : class, new()
        {
            if (!dataStores.ContainsKey(typeof(T)))
                Set(new T());

            return dataStores[typeof(T)] as T;
        }

        public static void Clear() => instance = new DataStore();

        public DataStore_World world => i.Get<DataStore_World>();
        public DataStore_Common common => i.Get<DataStore_Common>();
        public DataStore_Realm realm => i.Get<DataStore_Realm>();
        public DebugConfig debugConfig => i.Get<DebugConfig>();
        public DataStore_BuilderInWorld builderInWorld => i.Get<DataStore_BuilderInWorld>();
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
        public ServerTime.WorldTimer worldTimer => i.Get<ServerTime.WorldTimer>();
        public DataStore_Performance performance => i.Get<DataStore_Performance>();
        public DataStore_ExperiencesViewer experiencesViewer => i.Get<DataStore_ExperiencesViewer>();
        public DataStore_Emotes emotes => i.Get<DataStore_Emotes>();
        public DataStore_EmotesCustomization emotesCustomization => i.Get<DataStore_EmotesCustomization>();
        public DataStore_SceneBoundariesChecker sceneBoundariesChecker => i.Get<DataStore_SceneBoundariesChecker>();
        public DataStore_ECS7 ecs7 => i.Get<DataStore_ECS7>();
        public DataStore_VoiceChat voiceChat => i.Get<DataStore_VoiceChat>();
        public DataStore_TextureSize textureSize => i.Get<DataStore_TextureSize>();
        public DataStore_FriendNotifications friendNotifications => i.Get<DataStore_FriendNotifications>();
    }
}