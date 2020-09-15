namespace DCL
{
    public class DummyMessageHandler : IMessageProcessHandler
    {
        public void LoadParcelScenesExecute(string decentralandSceneJSON)
        {
        }

        public bool ProcessMessage(MessagingBus.QueuedSceneMessage_Scene msgObject, out CleanableYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;
            return true;
        }

        public void UnloadAllScenes()
        {
        }

        public void UnloadParcelSceneExecute(string sceneKey)
        {
        }

        public void UpdateParcelScenesExecute(string sceneKey)
        {
        }
    }
}
