using DCL.Controllers;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public interface IWorldState : ISceneHandler
    {
        HashSet<string> readyScenes { get; set; }
        Dictionary<string, IParcelScene> loadedScenes { get; set; }
        List<IParcelScene> scenesSortedByDistance { get; set; }
        List<string> globalSceneIds { get; set; }
        string currentSceneId { get; set; }
        void Initialize();
        bool TryGetScene(string id, out IParcelScene scene);
        bool TryGetScene<T>(string id, out T scene) where T : class, IParcelScene;
        IParcelScene GetScene(string id);
        bool Contains(string id);
    }

    public class WorldState : IWorldState
    {
        public HashSet<string> readyScenes { get; set; } = new HashSet<string>();
        public Dictionary<string, IParcelScene> loadedScenes { get; set; } = new Dictionary<string, IParcelScene>();
        public List<IParcelScene> scenesSortedByDistance { get; set; } = new List<IParcelScene>();

        public List<string> globalSceneIds { get; set; }
        public string currentSceneId { get; set; }

        public void Initialize()
        {
            globalSceneIds = new List<string>();
            currentSceneId = null;
            readyScenes = new HashSet<string>();
            loadedScenes = new Dictionary<string, IParcelScene>();
            scenesSortedByDistance = new List<IParcelScene>();
        }

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

                if (!scene.sceneLifecycleHandler.isReady) continue;

                allLoadedParcelCoords.UnionWith(scene.parcels);
            }

            return allLoadedParcelCoords;
        }
    }
}