﻿using System;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public interface ISceneController : IMessageProcessHandler, IMessageQueueHandler, IService
    {
        EntityIdHelper entityIdHelper { get; }
        bool enabled { get; set; }
        void Update();
        void LateUpdate();
        void SendSceneMessage(string payload);
        event Action<int> OnReadyScene;
        void SendSceneReady(int sceneNumber);
        void ActivateBuilderInWorldEditScene();
        void DeactivateBuilderInWorldEditScene();
        void UpdateParcelScenesExecute(LoadParcelScenesMessage.UnityParcelScene scene);

        void LoadUnityParcelScene(LoadParcelScenesMessage.UnityParcelScene sceneToLoad);
        void UnloadScene(int sceneNumber);
        void LoadParcelScenes(string JSONScenePayload);
        void UpdateParcelScenes(string JSONScenePayload);
        void UnloadAllScenesQueued();
        void CreateGlobalScene(CreateGlobalSceneMessage globalScene);
        void IsolateScene(IParcelScene sceneToActive);
        void ReIntegrateIsolatedScene();

        event Action OnSortScenes;
        event Action<IParcelScene, string> OnOpenExternalUrlRequest;
        event Action<IParcelScene> OnNewSceneAdded;
        event Action<IParcelScene> OnSceneRemoved;
    }
}
