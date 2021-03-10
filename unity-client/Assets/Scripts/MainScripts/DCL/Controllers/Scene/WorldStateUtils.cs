using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public static class WorldStateUtils
    {
        public static bool IsGlobalScene(string sceneId)
        {
            var worldState = Environment.i.world.state;

            if (worldState.TryGetScene(sceneId, out IParcelScene scene))
            {
                return scene is GlobalScene;
            }

            return false;
        }

        public static string TryToGetSceneCoordsID(string id)
        {
            var worldState = Environment.i.world.state;

            if (worldState.TryGetScene(id, out IParcelScene parcelScene))
            {
                return parcelScene.sceneData.basePosition.ToString();
            }

            return id;
        }

        public static Vector3 ConvertUnityToScenePosition(Vector3 pos, IParcelScene scene = null)
        {
            if (scene == null)
            {
                scene = GetCurrentScene();

                if (scene == null)
                    return pos;
            }

            Vector3 worldPosition = PositionUtils.UnityToWorldPosition(pos);
            return worldPosition - Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
        }

        public static Vector3 ConvertSceneToUnityPosition(Vector3 pos, IParcelScene scene = null)
        {
            return ConvertPointInSceneToUnityPosition(pos, scene);
        }

        public static Vector3 ConvertScenePositionToUnityPosition(IParcelScene scene = null)
        {
            return ConvertPointInSceneToUnityPosition(Vector3.zero, scene);
        }

        public static Vector3 ConvertPointInSceneToUnityPosition(Vector3 pos, IParcelScene scene = null)
        {
            if (scene == null)
            {
                scene = GetCurrentScene();

                if (scene == null)
                    return pos;
            }

            return ConvertPointInSceneToUnityPosition(pos, new Vector2Int(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y));
        }

        public static Vector3 ConvertPointInSceneToUnityPosition(Vector3 pos, Vector2Int scenePoint)
        {
            Vector3 scenePosition = Utils.GridToWorldPosition(scenePoint.x, scenePoint.y) + pos;
            Vector3 worldPosition = PositionUtils.WorldToUnityPosition(scenePosition);

            return worldPosition;
        }

        public static bool IsCharacterInsideScene(IParcelScene scene)
        {
            return scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition);
        }

        public static List<GlobalScene> GetActivePortableExperienceScenes()
        {
            List<GlobalScene> activePortableExperienceScenes = new List<GlobalScene>();
            IWorldState worldState = Environment.i.world.state;

            foreach (var globalSceneId in worldState.globalSceneIds)
            {
                if (worldState.TryGetScene(globalSceneId, out GlobalScene scene))
                {
                    if (scene.isPortableExperience)
                    {
                        activePortableExperienceScenes.Add(scene);
                    }
                }
            }

            return activePortableExperienceScenes;
        }

        public static List<string> GetActivePortableExperienceIds()
        {
            List<string> currentSceneAndPortableExperiencesIds = new List<string>();
            IWorldState worldState = Environment.i.world.state;

            foreach (var globalSceneId in worldState.globalSceneIds)
            {
                if (worldState.TryGetScene(globalSceneId, out GlobalScene scene))
                {
                    if (scene.isPortableExperience)
                    {
                        currentSceneAndPortableExperiencesIds.Add(globalSceneId);
                    }
                }
            }

            return currentSceneAndPortableExperiencesIds;
        }


        static IParcelScene GetCurrentScene()
        {
            var worldState = Environment.i.world.state;
            string currentSceneId = worldState.currentSceneId;
            
            if (string.IsNullOrEmpty(currentSceneId))
                return null;

            bool foundCurrentScene = worldState.loadedScenes.TryGetValue(currentSceneId, out IParcelScene scene);

            if (!foundCurrentScene)
                return null;

            return scene;
        }
    }
}