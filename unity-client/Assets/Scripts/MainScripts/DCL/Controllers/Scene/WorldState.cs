using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class WorldState : ISceneHandler
    {
        public HashSet<string> readyScenes = new HashSet<string>();
        public Dictionary<string, ParcelScene> loadedScenes = new Dictionary<string, ParcelScene>();
        public List<ParcelScene> scenesSortedByDistance = new List<ParcelScene>();

        public string globalSceneId { get; set; }
        public string currentSceneId { get; set; }

        public void Initialize()
        {
            globalSceneId = null;
            currentSceneId = null;
            readyScenes = new HashSet<string>();
            loadedScenes = new Dictionary<string, ParcelScene>();
            scenesSortedByDistance = new List<ParcelScene>();
        }

        public string TryToGetSceneCoordsID(string id)
        {
            if (loadedScenes.ContainsKey(id))
                return loadedScenes[id].sceneData.basePosition.ToString();

            return id;
        }

        public bool TryGetScene(string id, out ParcelScene scene)
        {
            scene = null;

            if (!loadedScenes.ContainsKey(id))
                return false;

            scene = loadedScenes[id];
            return true;
        }

        public Vector3 ConvertUnityToScenePosition(Vector3 pos, ParcelScene scene = null)
        {
            if (scene == null)
            {
                string sceneId = currentSceneId;

                if (!string.IsNullOrEmpty(sceneId) && loadedScenes.ContainsKey(sceneId))
                    scene = loadedScenes[currentSceneId];
                else
                    return pos;
            }

            Vector3 worldPosition = DCLCharacterController.i.characterPosition.UnityToWorldPosition(pos);
            return worldPosition - Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
        }

        public Vector3 ConvertSceneToUnityPosition(Vector3 pos, ParcelScene scene = null)
        {
            if (scene == null)
            {
                string sceneId = currentSceneId;

                if (!string.IsNullOrEmpty(sceneId) && loadedScenes.ContainsKey(sceneId))
                    scene = loadedScenes[currentSceneId];
                else
                    return pos;
            }

            Vector3 sceneRealPosition = scene.gameObject.transform.position;
            Vector3 sceneFictionPosition = new Vector3(scene.sceneData.basePosition.x, 0, scene.sceneData.basePosition.y);
            Vector3 sceneOffset = sceneRealPosition - sceneFictionPosition;
            Vector3 solvedPosition = pos + sceneOffset;
            return solvedPosition;
        }

        public bool IsCharacterInsideScene(ParcelScene scene)
        {
            return scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition);
        }

        public HashSet<Vector2Int> GetAllLoadedScenesCoords()
        {
            HashSet<Vector2Int> allLoadedParcelCoords = new HashSet<Vector2Int>();

            // Create fast (hashset) collection of loaded parcels coords
            foreach (var element in loadedScenes)
            {
                if (!element.Value.sceneLifecycleHandler.isReady) continue;

                allLoadedParcelCoords.UnionWith(element.Value.parcels);
            }

            return allLoadedParcelCoords;
        }
    }
}