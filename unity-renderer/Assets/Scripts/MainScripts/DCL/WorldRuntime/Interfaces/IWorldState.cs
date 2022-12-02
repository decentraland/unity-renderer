﻿using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public interface IWorldState : ISceneHandler, IService
    {
        int GetCurrentSceneNumber();
        IEnumerable<KeyValuePair<int, IParcelScene>> GetLoadedScenes();
        List<IParcelScene> GetGlobalScenes();
        bool TryGetScene(int sceneNumber, out IParcelScene scene);
        bool TryGetScene<T>(int sceneNumber, out T scene) where T : class, IParcelScene;
        IParcelScene GetScene(Vector2Int coords);
        IParcelScene GetScene(int sceneNumber);
        IParcelScene GetPortableExperienceScene(string sceneId);
        bool ContainsScene(int sceneNumber);
        LoadWrapper GetLoaderForEntity(IDCLEntity entity);
        T GetOrAddLoaderForEntity<T>(IDCLEntity entity) where T : LoadWrapper, new();
        void RemoveLoaderForEntity(IDCLEntity entity);
        int GetSceneNumberByCoords(Vector2Int coords);
        List<IParcelScene> GetScenesSortedByDistance();
        void SortScenesByDistance(Vector2Int position);
        void AddScene(IParcelScene newScene);
        void RemoveScene(int sceneNumber);
        void ForceCurrentScene(int sceneNumber);
    }
}