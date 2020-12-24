namespace DCL
{
    public interface IMessageProcessHandler
    {
        bool ProcessMessage(MessagingBus.QueuedSceneMessage_Scene msgObject, out CleanableYieldInstruction yieldInstruction);
        void LoadParcelScenesExecute(string scenePayload);
        void UnloadParcelSceneExecute(string sceneId);
        void UnloadAllScenes(bool includePersistent = false);
        void UpdateParcelScenesExecute(string sceneId);
    }
}