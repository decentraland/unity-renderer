using DCL.Controllers;
using System.Collections.Generic;
using DCL.Configuration;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public class WorldState : IWorldState
    {
        private Dictionary<string, IParcelScene> loadedScenes { get; } = new Dictionary<string, IParcelScene>();
        private Dictionary<Vector2Int, string> loadedScenesByCoordinate { get; } = new Dictionary<Vector2Int, string>();
        private List<IParcelScene> scenesSortedByDistance { get; } = new List<IParcelScene>();
        private List<string> globalSceneIds { get; } = new List<string>();
        private Vector2Int sortAuxiliaryVector = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);
        private readonly List<IParcelScene> globalScenes = new List<IParcelScene>();
        private string currentSceneId;
        
        public string GetCurrentSceneId() => currentSceneId;
        
        public IEnumerable<KeyValuePair<string, IParcelScene>> GetLoadedScenes() => loadedScenes;

        public List<IParcelScene> GetGlobalScenes() => globalScenes;
        
        public List<IParcelScene> GetScenesSortedByDistance() => scenesSortedByDistance;
        
        public IParcelScene GetScene(Vector2Int coords)
        {
            var id = GetSceneIdByCoords(coords);
            
            if (!ContainsScene(id))
                return null;

            return loadedScenes[id];
        }

        public IParcelScene GetScene(string id)
        {
            if (!ContainsScene(id))
                return null;

            return loadedScenes[id];
        }

        public bool ContainsScene(string id)
        {
            if (string.IsNullOrEmpty(id) || !loadedScenes.ContainsKey(id))
                return false;

            return true;
        }

        public bool TryGetScene(string id, out IParcelScene scene)
        {
            scene = null;

            if (string.IsNullOrEmpty(id) || !loadedScenes.ContainsKey(id))
                return false;

            scene = loadedScenes[id];
            return true;
        }

        public bool TryGetScene<T>(string id, out T scene)
            where T : class, IParcelScene
        {
            scene = default(T);
            bool result = TryGetScene(id, out IParcelScene baseScene);

            if (result)
                scene = baseScene as T;

            if (scene == null)
                result = false;

            return result;
        }

        public HashSet<Vector2Int> GetAllLoadedScenesCoords()
        {
            HashSet<Vector2Int> allLoadedParcelCoords = new HashSet<Vector2Int>();

            // Create fast (hashset) collection of loaded parcels coords
            foreach (var element in loadedScenes)
            {
                ParcelScene scene = element.Value as ParcelScene;

                if (!scene.sceneLifecycleHandler.isReady)
                    continue;

                allLoadedParcelCoords.UnionWith(scene.parcels);
            }

            return allLoadedParcelCoords;
        }

        private readonly Dictionary<GameObject, LoadWrapper>
            attachedLoaders = new Dictionary<GameObject, LoadWrapper>();

        public LoadWrapper GetLoaderForEntity(IDCLEntity entity)
        {
            if (entity.meshRootGameObject == null)
            {
                Debug.LogWarning("NULL meshRootGameObject at GetLoaderForEntity()");
                return null;
            }

            attachedLoaders.TryGetValue(entity.meshRootGameObject, out LoadWrapper result);
            return result;
        }

        public T GetOrAddLoaderForEntity<T>(IDCLEntity entity)
            where T : LoadWrapper, new()
        {
            if (!attachedLoaders.TryGetValue(entity.meshRootGameObject, out LoadWrapper result))
            {
                result = new T();
                attachedLoaders.Add(entity.meshRootGameObject, result);
            }

            return result as T;
        }

        public void RemoveLoaderForEntity(IDCLEntity entity)
        {
            if (entity == null || entity.meshRootGameObject == null)
                return;
            
            attachedLoaders.Remove(entity.meshRootGameObject);
        }
        
        public string GetSceneIdByCoords(Vector2Int coords)
        {
            if (loadedScenesByCoordinate.ContainsKey(coords))
                return loadedScenesByCoordinate[coords];
            
            return null;
        }
        
        public void SortScenesByDistance(Vector2Int position)
        {
            currentSceneId = null;
            scenesSortedByDistance.Sort((sceneA, sceneB) =>
            {
                sortAuxiliaryVector = sceneA.sceneData.basePosition - position;
                int dist1 = sortAuxiliaryVector.sqrMagnitude;

                sortAuxiliaryVector = sceneB.sceneData.basePosition - position;
                int dist2 = sortAuxiliaryVector.sqrMagnitude;

                return dist1 - dist2;
            });

            using var iterator = scenesSortedByDistance.GetEnumerator();

            while (iterator.MoveNext())
            {
                IParcelScene scene = iterator.Current;

                if (scene == null)
                    continue;

                bool characterIsInsideScene = WorldStateUtils.IsCharacterInsideScene(scene);
                bool isGlobalScene = globalSceneIds.Contains(scene.sceneData.id);

                if (isGlobalScene || !characterIsInsideScene)
                    continue;

                currentSceneId = scene.sceneData.id;

                break;
            }

        }
        public void ForceCurrentScene(string id)
        {
            currentSceneId = id;
        }
        
        public void AddScene(string id, IParcelScene newScene)
        {
            if (loadedScenes.ContainsKey(id))
            {
                Debug.LogWarning($"This scene already exists! {id}");
                return;
            }
            
            loadedScenes.Add(id, newScene);
            
            foreach (Vector2Int parcelPosition in newScene.GetParcels())
            {
                loadedScenesByCoordinate[parcelPosition] = id;
            }
                
            scenesSortedByDistance.Add(newScene);

            if (currentSceneId == null)
            {
                currentSceneId = id;
            }
        }
        
        public void RemoveScene(string id)
        {
            IParcelScene loadedScene = loadedScenes[id];

            foreach (Vector2Int sceneParcel in loadedScene.GetParcels())
            {
                loadedScenesByCoordinate.Remove(sceneParcel);
            }
            
            scenesSortedByDistance.Remove(loadedScene);
            
            loadedScenes.Remove(id);
            globalSceneIds.Remove(id);

            if (globalScenes.Contains(loadedScene))
            {
                globalScenes.Remove(loadedScene);
            }
        }
        
        public void AddGlobalScene(string sceneId, IParcelScene newScene)
        {
            if (globalSceneIds.Contains(sceneId))
            {
                Debug.LogWarning($"This GLOBAL scene already exists! {sceneId}");
                return;
            }
            
            globalSceneIds.Add(sceneId);
            globalScenes.Add(newScene);

            AddScene(sceneId, newScene);
        }

        public void Dispose()
        {
            loadedScenes.Clear();
            loadedScenesByCoordinate.Clear();
            scenesSortedByDistance.Clear();
            globalScenes.Clear();
            globalScenes.Clear();
            currentSceneId = null;
        }

        public void Initialize()
        {
        }
    }
}