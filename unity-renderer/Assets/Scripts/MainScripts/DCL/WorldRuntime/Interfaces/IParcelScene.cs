﻿using System.Collections.Generic;
using DCL.Components;
using DCL.Models;
using UnityEngine;

namespace DCL.Controllers
{
    public interface IParcelScene
    {
        IDCLEntity CreateEntity(string id);
        Transform GetSceneTransform();
        Dictionary<string, IDCLEntity> entities { get; }
        Dictionary<string, ISharedComponent> disposableComponents { get; }
        T GetSharedComponent<T>() where T : class;
        ISharedComponent GetSharedComponent(string id);
        event System.Action<IDCLEntity> OnEntityAdded;
        event System.Action<IDCLEntity> OnEntityRemoved;
        LoadParcelScenesMessage.UnityParcelScene sceneData { get; }
        ContentProvider contentProvider { get; }
        bool isPersistent { get; }
        bool isTestScene { get; }
        float loadingProgress { get; }
        ISceneMetricsCounter metricsCounter { get; }
        bool IsInsideSceneBoundaries(Bounds objectBounds);
        bool IsInsideSceneBoundaries(Vector2Int gridPosition, float height = 0f);
        bool IsInsideSceneBoundaries(Vector3 worldPosition, float height = 0f);
        void CalculateSceneLoadingState();
        void GetWaitingComponentsDebugInfo();
        IEntityComponent EntityComponentCreateOrUpdateWithModel(string entityId, CLASS_ID_COMPONENT classId, object data);
    }
}