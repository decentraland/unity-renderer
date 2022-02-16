using System;
using System.Collections.Generic;
using DCL.Controllers;

namespace DCL
{
    public interface IWorldState : ISceneHandler, IService
    {
        HashSet<string> readyScenes { get; set; }
        Dictionary<string, IParcelScene> loadedScenes { get; set; }
        List<IParcelScene> scenesSortedByDistance { get; set; }
        List<string> globalSceneIds { get; set; }
        string currentSceneId { get; set; }
        bool TryGetScene(string id, out IParcelScene scene);
        bool TryGetScene<T>(string id, out T scene) where T : class, IParcelScene;
        IParcelScene GetScene(string id);
        bool Contains(string id);
    }
}