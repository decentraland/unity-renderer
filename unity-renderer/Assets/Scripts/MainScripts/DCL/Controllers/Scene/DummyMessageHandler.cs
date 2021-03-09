namespace DCL
{
    public class DummyMessageHandler : IMessageProcessHandler
    {
        public void LoadParcelScenesExecute(string scenePayload)
        {
        }

        public bool ProcessMessage(QueuedSceneMessage_Scene msgObject, out CleanableYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;
            return true;
        }

        public void UnloadAllScenes(bool includePersistent = false)
        {
        }

        public void UnloadParcelSceneExecute(string sceneId)
        {
        }

        public void UpdateParcelScenesExecute(string sceneId)
        {
        }
    }
}