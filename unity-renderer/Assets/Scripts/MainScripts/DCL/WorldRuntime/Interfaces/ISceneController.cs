using System;
using DCL.Controllers;
using DCL.Models;

namespace DCL
{
    public interface ISceneController : IMessageProcessHandler, IMessageQueueHandler
    {
        bool enabled { get; set; }
        bool deferredMessagesDecoding { get; set; }
        bool prewarmSceneMessagesPool { get; set; }
        bool prewarmEntitiesPool { get; set; }
        void Initialize();
        void Start();
        void Dispose();
        void Update();
        void LateUpdate();

        void EnsureEntityPool(); // TODO: Move to PoolManagerFactory
        void ParseQuery(object payload, string sceneId);
        void SendSceneMessage(string payload);
        event Action<string> OnReadyScene;
        IParcelScene CreateTestScene(LoadParcelScenesMessage.UnityParcelScene data = null);
        void SendSceneReady(string sceneId);
        void ActivateBuilderInWorldEditScene();
        void DeactivateBuilderInWorldEditScene();
        void SortScenesByDistance();
        void UpdateParcelScenesExecute(LoadParcelScenesMessage.UnityParcelScene scene);
        void UnloadScene(string sceneKey);
        void LoadParcelScenes(string decentralandSceneJSON);
        void UpdateParcelScenes(string decentralandSceneJSON);
        void UnloadAllScenesQueued();
        void CreateGlobalScene(string json);
        void IsolateScene(ParcelScene sceneToActive);
        void ReIntegrateIsolatedScene();
        event Action OnSortScenes;
        event Action<ParcelScene, string> OnOpenExternalUrlRequest;
        event Action<ParcelScene> OnNewSceneAdded;
        event Action<GlobalScene> OnNewPortableExperienceSceneAdded;
        event Action<string> OnNewPortableExperienceSceneRemoved;
        event SceneController.OnOpenNFTDialogDelegate OnOpenNFTDialogRequest;
    }
}