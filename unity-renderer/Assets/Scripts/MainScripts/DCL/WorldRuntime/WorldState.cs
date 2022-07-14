using DCL.Controllers;
using DCL.Helpers;
using System.Collections.Generic;
using DCL.Components;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public class WorldState : IWorldState
    {
        public HashSet<string> readyScenes { get; set; } = new HashSet<string>();
        public Dictionary<string, IParcelScene> loadedScenes { get; set; } = new Dictionary<string, IParcelScene>();
        public List<IParcelScene> scenesSortedByDistance { get; set; } = new List<IParcelScene>();
        public List<string> globalSceneIds { get; set; } = new List<string>();
        public string currentSceneId { get; set; } = null;

        public IParcelScene GetScene(string id)
        {
            if (!Contains(id))
                return null;

            return loadedScenes[id];
        }

        public bool Contains(string id)
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

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }
    }
}