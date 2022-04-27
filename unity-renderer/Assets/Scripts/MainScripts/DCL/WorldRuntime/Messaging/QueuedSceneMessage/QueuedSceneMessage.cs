namespace DCL
{
    public enum MessagingBusType
    {
        NONE,
        UI,
        INIT,
        SYSTEM
    }

    public class QueuedSceneMessage_Scene : QueuedSceneMessage
    {
        public string method;
        public object payload; //PB_SendSceneMessage
    }

    public class QueuedSceneMessage
    {
        public enum Type
        {
            NONE,
            SCENE_MESSAGE,
            LOAD_PARCEL,
            UPDATE_PARCEL,
            TELEPORT,
            UNLOAD_SCENES,
            UNLOAD_PARCEL,
            SCENE_STARTED
        }

        public string tag;
        public Type type;
        public string sceneId;
        public string message;
        public bool isUnreliable;
        public string unreliableMessageKey;
    }
}