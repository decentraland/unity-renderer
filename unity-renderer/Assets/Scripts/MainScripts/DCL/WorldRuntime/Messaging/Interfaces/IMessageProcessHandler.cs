using UnityEngine;

namespace DCL
{
    public interface IMessageProcessHandler
    {
        bool ProcessMessage(QueuedSceneMessage_Scene msgObject, out CustomYieldInstruction yieldInstruction);
        void LoadParcelScenesExecute(string scenePayload);
        void UnloadParcelSceneExecute(int sceneNumber);
        void UnloadAllScenes(bool includePersistent = false);
        void UpdateParcelScenesExecute(string sceneJSON);
    }
}