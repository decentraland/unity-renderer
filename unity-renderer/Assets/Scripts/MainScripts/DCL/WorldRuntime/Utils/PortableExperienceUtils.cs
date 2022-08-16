using System.Collections.Generic;
using DCL.Controllers;

namespace DCL
{
    public static class PortableExperienceUtils
    {
        public static List<GlobalScene> GetActivePortableExperienceScenes()
        {
            List<GlobalScene> activePortableExperienceScenes = new List<GlobalScene>();
            IWorldState worldState = Environment.i.world.state;

            List<IParcelScene> parcelScenes = worldState.GetGlobalScenes();

            foreach (var parcelScene in parcelScenes)
            {
                var globalScene = (GlobalScene)parcelScene;

                if (globalScene.isPortableExperience)
                {
                    activePortableExperienceScenes.Add(globalScene);
                }
            }

            return activePortableExperienceScenes;
        }

        public static List<string> GetActivePortableExperienceIds()
        {
            List<string> currentSceneAndPortableExperiencesIds = new List<string>();
            IWorldState worldState = Environment.i.world.state;

            List<IParcelScene> parcelScenes = worldState.GetGlobalScenes();

            foreach (var parcelScene in parcelScenes)
            {
                var globalScene = (GlobalScene)parcelScene;

                if (globalScene.isPortableExperience)
                {
                    currentSceneAndPortableExperiencesIds.Add(globalScene.sceneName);
                }
            }

            return currentSceneAndPortableExperiencesIds;
        }
    }
}