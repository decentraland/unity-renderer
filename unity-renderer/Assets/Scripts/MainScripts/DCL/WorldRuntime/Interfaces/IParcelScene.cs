using System;
using System.Collections.Generic;
using DCL.Components;
using DCL.Models;
using UnityEngine;

namespace DCL.Controllers
{
    public interface IParcelScene
    {
        event Action<float> OnLoadingStateUpdated;
        event Action<IDCLEntity> OnEntityAdded;
        event Action<IDCLEntity> OnEntityRemoved;

        IDCLEntity CreateEntity(string id);
        IDCLEntity GetEntityById(string entityId);
        Transform GetSceneTransform();
        Dictionary<string, IDCLEntity> entities { get; }
        IECSComponentsManagerLegacy componentsManagerLegacy { get; }
        LoadParcelScenesMessage.UnityParcelScene sceneData { get; }
        ContentProvider contentProvider { get; }
        bool isPersistent { get; }
        bool isTestScene { get; }
        float loadingProgress { get; }
        string GetSceneName();
        ISceneMetricsCounter metricsCounter { get; }
        bool IsInsideSceneBoundaries(Bounds objectBounds);
        bool IsInsideSceneBoundaries(Vector2Int gridPosition, float height = 0f);
        bool IsInsideSceneBoundaries(Vector3 worldPosition, float height = 0f);
        void CalculateSceneLoadingState();
        void GetWaitingComponentsDebugInfo();
        void SetEntityParent(string entityId, string parentId);
        void RemoveEntity(string id, bool removeImmediatelyFromEntitiesList = true);
    }
}