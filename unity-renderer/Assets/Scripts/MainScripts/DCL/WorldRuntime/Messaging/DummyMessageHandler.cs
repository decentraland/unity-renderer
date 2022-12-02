using UnityEngine;

namespace DCL
{
    public class DummyMessageHandler : IMessageProcessHandler
    {
        public void LoadParcelScenesExecute(string scenePayload)
        {
        }

        public bool ProcessMessage(QueuedSceneMessage_Scene msgObject, out CustomYieldInstruction yieldInstruction)
        {
            yieldInstruction = null;
            return true;
        }

        public void UnloadAllScenes(bool includePersistent = false)
        {
        }

        public void UnloadParcelSceneExecute(int sceneNumber)
        {
        }

        public void UpdateParcelScenesExecute(string sceneJSON)
        {
        }
    }
}