namespace DCL
{
    public static class MessagingTypes
    {
        public const string ENTITY_COMPONENT_CREATE_OR_UPDATE = "UpdateEntityComponent";
        public const string ENTITY_CREATE = "CreateEntity";
        public const string ENTITY_REPARENT = "SetEntityParent";
        public const string ENTITY_COMPONENT_DESTROY = "ComponentRemoved";
        public const string SHARED_COMPONENT_ATTACH = "AttachEntityComponent";
        public const string SHARED_COMPONENT_CREATE = "ComponentCreated";
        public const string SHARED_COMPONENT_DISPOSE = "ComponentDisposed";
        public const string SHARED_COMPONENT_UPDATE = "ComponentUpdated";
        public const string ENTITY_DESTROY = "RemoveEntity";
        public const string SCENE_LOAD = "LoadScene";
        public const string SCENE_UPDATE = "UpdateScene";
        public const string SCENE_DESTROY = "UnloadScene";
        public const string INIT_DONE = "InitMessagesFinished";
        public const string QUERY = "Query";
        public const string OPEN_EXTERNAL_URL = "OpenExternalUrl";
        public const string OPEN_NFT_DIALOG = "OpenNFTDialog";

        public const string PB_SHARED_COMPONENT_UPDATE = "PBComponentUpdated";
        public const string PB_ENTITY_COMPONENT_CREATE_OR_UPDATE = "PBUpdateEntityComponent";
    }
}