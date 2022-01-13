using System;
using DCL.Controllers;
using DCL.Models;

namespace DCLPlugins.PreviewModePlugin.Commons
{
    public class WatchSceneHandler : IDisposable
    {
        private readonly IParcelScene scene;
        private readonly ISceneListener sceneListener;

        public WatchSceneHandler(IParcelScene scene, ISceneListener sceneListener)
        {
            this.scene = scene;
            this.sceneListener = sceneListener;

            scene.OnEntityAdded += SceneOnOnEntityAdded;

            foreach (IDCLEntity entity in scene.entities.Values)
            {
                sceneListener.OnEntityAdded(entity);
            }
        }

        public void Dispose()
        {
            scene.OnEntityAdded -= SceneOnOnEntityAdded;
            sceneListener.Dispose();
        }

        private void SceneOnOnEntityAdded(IDCLEntity entity)
        {
            sceneListener.OnEntityAdded(entity);
        }
    }
}