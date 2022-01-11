using DCL.Models;

namespace DCLPlugins.PreviewModePlugin.Commons
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
                OnShapeUpdated();
            }

            entity.meshesInfo.OnUpdated += OnShapeUpdated;
        }

        public void Dispose()
        {
            entity.meshesInfo.OnUpdated -= OnShapeUpdated;
            listener.Dispose();
        }

        private void OnShapeUpdated()
        {
            if (entity.meshesInfo.currentShape != null)
            {
                listener.OnShapeUpdated(entity);
            }
        }
    }
}