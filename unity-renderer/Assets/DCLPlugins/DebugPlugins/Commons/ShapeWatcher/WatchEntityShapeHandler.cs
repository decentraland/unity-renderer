using DCL.Models;

namespace DCLPlugins.DebugPlugins.Commons
{
    public class WatchEntityShapeHandler
    {
        private readonly IDCLEntity entity;
        private readonly IShapeListener listener;

        public WatchEntityShapeHandler(IDCLEntity entity, IShapeListener listener)
        {
            this.entity = entity;
            this.listener = listener;

            if (entity.meshesInfo.currentShape != null)
            {
                OnShapeUpdated(entity);
            }

            entity.OnMeshesInfoUpdated += OnShapeUpdated;
            entity.OnMeshesInfoCleaned += OnShapeCleaned;
        }

        public void Dispose()
        {
            entity.OnMeshesInfoUpdated -= OnShapeUpdated;
            entity.OnMeshesInfoCleaned -= OnShapeCleaned;

            listener.Dispose();
        }

        private void OnShapeUpdated(IDCLEntity entity)
        {
            if (entity.meshesInfo.currentShape != null
                && entity.meshesInfo.meshRootGameObject != null)
            {
                listener.OnShapeUpdated(entity);
            }
        }

        private void OnShapeCleaned(IDCLEntity entity)
        {
            listener.OnShapeCleaned(entity);
        }
    }
}