using DCL.Controllers;
using System.Collections.Generic;
using DCL.Configuration;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public class WorldState : IWorldState
    {
        private Dictionary<int, IParcelScene> loadedScenes { get; } = new Dictionary<int, IParcelScene>();
        private Dictionary<string, IParcelScene> loadedPortableExperienceScenes { get; } = new Dictionary<string, IParcelScene>();
        private Dictionary<Vector2Int, int> loadedScenesByCoordinate { get; } = new Dictionary<Vector2Int, int>();
        private List<IParcelScene> scenesSortedByDistance { get; } = new List<IParcelScene>();
        private List<int> globalSceneNumbers { get; } = new List<int>();
        private Vector2Int sortAuxiliaryVector = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);
        private readonly List<IParcelScene> globalScenes = new List<IParcelScene>();
        private int currentSceneNumber;
        private string currentSceneHash;

        public int GetCurrentSceneNumber() => currentSceneNumber;
        public string GetCurrentSceneHash() => currentSceneHash;


        public IEnumerable<KeyValuePair<int, IParcelScene>> GetLoadedScenes() => loadedScenes;

        public List<IParcelScene> GetGlobalScenes() => globalScenes;

        public List<IParcelScene> GetScenesSortedByDistance() => scenesSortedByDistance;

        public IParcelScene GetScene(int sceneNumber)
        {
            if (!ContainsScene(sceneNumber))
                return null;

            return loadedScenes[sceneNumber];
        }
        
        public IParcelScene GetScene(Vector2Int coords)
        {
            return GetScene(GetSceneNumberByCoords(coords));
        }
        
        public IParcelScene GetPortableExperienceScene(string sceneId)
        {
            if (string.IsNullOrEmpty(sceneId) || !loadedPortableExperienceScenes.ContainsKey(sceneId))
                return null;

            return loadedPortableExperienceScenes[sceneId];
        }

        public bool ContainsScene(int sceneNumber)
        {
            if (sceneNumber <= 0 || !loadedScenes.ContainsKey(sceneNumber))
                return false;

            return true;
        }

        public bool TryGetScene(int sceneNumber, out IParcelScene scene)
        {
            scene = null;

            if (sceneNumber <= 0 || !loadedScenes.ContainsKey(sceneNumber))
                return false;

            scene = loadedScenes[sceneNumber];
            return true;
        }

        public bool TryGetScene<T>(int sceneNumber, out T scene)
            where T : class, IParcelScene
        {
            scene = default(T);
            bool result = TryGetScene(sceneNumber, out IParcelScene baseScene);

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
            if (entity == null)
            {
                Debug.LogWarning("NULL entity at GetLoaderForEntity()");
                return null;
            }    
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

        public int GetSceneNumberByCoords(Vector2Int coords)
        {
            if (loadedScenesByCoordinate.ContainsKey(coords))
                return loadedScenesByCoordinate[coords];

            return -1;
        }

        public void SortScenesByDistance(Vector2Int position)
        {
            currentSceneNumber = -1;
            currentSceneHash = "";

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
                bool isGlobalScene = globalSceneNumbers.Contains(scene.sceneData.sceneNumber);

                if (isGlobalScene || !characterIsInsideScene)
                    continue;

                currentSceneNumber = scene.sceneData.sceneNumber;
                currentSceneHash = scene.sceneData.id;

                break;
            }

        }

        public void ForceCurrentScene(int sceneNumber, string sceneHash)
        {
            currentSceneNumber = sceneNumber;
            currentSceneHash = sceneHash;
        }

        public void AddScene(IParcelScene newScene)
        {
            int sceneNumber = newScene.sceneData.sceneNumber;

            if (loadedScenes.ContainsKey(sceneNumber))
            {
                Debug.LogWarning($"The scene {newScene.sceneData.id} already exists! scene number: {sceneNumber}");
                return;
            }

            if (newScene.isPersistent)
            {
                globalSceneNumbers.Add(sceneNumber);
                globalScenes.Add(newScene);

                if (newScene.isPortableExperience)
                    loadedPortableExperienceScenes.Add(newScene.sceneData.id, newScene);
            }

            loadedScenes.Add(sceneNumber, newScene);

            foreach (Vector2Int parcelPosition in newScene.GetParcels())
            {
                loadedScenesByCoordinate[parcelPosition] = sceneNumber;
            }

            scenesSortedByDistance.Add(newScene);

            if (currentSceneNumber <= 0)
            {
                currentSceneNumber = sceneNumber;
                currentSceneHash = newScene.sceneData.id;
            }
        }

        public void RemoveScene(int sceneNumber)
        {
            IParcelScene loadedScene = loadedScenes[sceneNumber];

            foreach (Vector2Int sceneParcel in loadedScene.GetParcels())
            {
                loadedScenesByCoordinate.Remove(sceneParcel);
            }
            
            scenesSortedByDistance.Remove(loadedScene);
            
            loadedScenes.Remove(sceneNumber);
            globalSceneNumbers.Remove(sceneNumber);

            if (globalScenes.Contains(loadedScene))
            {
                globalScenes.Remove(loadedScene);

                if (!string.IsNullOrEmpty(loadedScene.sceneData.id) && loadedPortableExperienceScenes.ContainsKey(loadedScene.sceneData.id))
                    loadedPortableExperienceScenes.Remove(loadedScene.sceneData.id);
            }
        }

        public void Dispose()
        {
            loadedScenes.Clear();
            loadedScenesByCoordinate.Clear();
            scenesSortedByDistance.Clear();
            globalScenes.Clear();
            globalSceneNumbers.Clear();
            loadedPortableExperienceScenes.Clear();
            currentSceneNumber = -1;
            currentSceneHash = "";
        }

        public void Initialize() { }
    }
}
