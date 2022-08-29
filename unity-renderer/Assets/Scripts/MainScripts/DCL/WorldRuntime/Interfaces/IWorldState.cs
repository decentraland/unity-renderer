using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public interface IWorldState : ISceneHandler, IService
    {
        string GetCurrentSceneId();
        IEnumerable<KeyValuePair<string, IParcelScene>> GetLoadedScenes();
        List<IParcelScene> GetGlobalScenes();
        bool TryGetScene(string id, out IParcelScene scene);
        bool TryGetScene<T>(string id, out T scene) where T : class, IParcelScene;
        IParcelScene GetScene(Vector2Int coords);
        IParcelScene GetScene(string id);
        bool ContainsScene(string id);
        LoadWrapper GetLoaderForEntity(IDCLEntity entity);
        T GetOrAddLoaderForEntity<T>(IDCLEntity entity) where T : LoadWrapper, new();
        void RemoveLoaderForEntity(IDCLEntity entity);
        string GetSceneIdByCoords(Vector2Int coords);
        List<IParcelScene> GetScenesSortedByDistance();
        void SortScenesByDistance(Vector2Int position);
        void AddScene(string id, IParcelScene newScene);
        void RemoveScene(string id);
        void AddGlobalScene(string sceneId, IParcelScene newScene);
        void ForceCurrentScene(string sceneDataID);
    }
}