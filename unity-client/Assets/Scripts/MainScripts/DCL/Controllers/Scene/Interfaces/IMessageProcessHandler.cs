namespace DCL
{
    public interface IMessageProcessHandler
    {
        bool ProcessMessage(MessagingBus.QueuedSceneMessage_Scene msgObject, out CleanableYieldInstruction yieldInstruction);
        void LoadParcelScenesExecute(string decentralandSceneJSON);
        void UnloadParcelSceneExecute(string sceneKey);
        void UnloadAllScenes();
        void UpdateParcelScenesExecute(string sceneKey);
    }
}