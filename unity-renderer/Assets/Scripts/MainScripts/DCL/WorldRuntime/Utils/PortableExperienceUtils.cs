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
    }
}