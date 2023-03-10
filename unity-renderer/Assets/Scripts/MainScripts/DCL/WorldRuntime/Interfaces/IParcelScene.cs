using System;
using System.Collections.Generic;
using DCL.Models;
using UnityEngine;

namespace DCL.Controllers
{
    public interface IParcelScene
    {
        event Action<float> OnLoadingStateUpdated;
        event Action<IDCLEntity> OnEntityAdded;
        event Action<IDCLEntity> OnEntityRemoved;

        IDCLEntity CreateEntity(long id);
        IDCLEntity GetEntityById(long entityId);
        Transform GetSceneTransform();
        Dictionary<long, IDCLEntity> entities { get; }
        IECSComponentsManagerLegacy componentsManagerLegacy { get; }
        LoadParcelScenesMessage.UnityParcelScene sceneData { get; }
        ContentProvider contentProvider { get; }
        bool isPersistent { get; }
        bool isPortableExperience { get; }
        bool isTestScene { get; }
        float loadingProgress { get; }
        string GetSceneName();
        ISceneMetricsCounter metricsCounter { get; }
        HashSet<Vector2Int> GetParcels();
        bool IsInsideSceneBoundaries(Bounds objectBounds);
        bool IsInsideSceneBoundaries(Vector2Int gridPosition, float height = 0f);
        bool IsInsideSceneBoundaries(Vector3 worldPosition, float height = 0f);
        bool IsInsideSceneOuterBoundaries(Bounds objectBounds);
        bool IsInsideSceneOuterBoundaries(Vector3 objectUnityPosition);
        void CalculateSceneLoadingState();
        void GetWaitingComponentsDebugInfo();
        void SetEntityParent(long entityId, long parentId);
        void RemoveEntity(long id, bool removeImmediatelyFromEntitiesList = true);
        bool IsInitMessageDone();
    }
}
