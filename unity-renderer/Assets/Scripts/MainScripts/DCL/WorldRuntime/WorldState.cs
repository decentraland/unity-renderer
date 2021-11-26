using DCL.Controllers;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class WorldState : IWorldState
    {
        public HashSet<string> readyScenes { get; set; }
        public Dictionary<string, IParcelScene> loadedScenes { get; set; }
        public List<IParcelScene> scenesSortedByDistance { get; set; }

        public List<string> globalSceneIds { get; set; }
        public string currentSceneId { get; set; }

        public WorldState ()
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

                if (!scene.sceneLifecycleHandler.isReady)
                    continue;

                allLoadedParcelCoords.UnionWith(scene.parcels);
            }

            return allLoadedParcelCoords;
        }

        public void Dispose()
        {
        }
    }
}