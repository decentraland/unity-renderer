using System;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.MapCameraController
{
    public partial class RentMapCameraController : IDisposable
    {
        private readonly ObjectPool<IMapCameraController> pool;

        public RentMapCameraController(Func<IMapCameraController> cameraControllerBuilder)
        {
            pool = new ObjectPool<IMapCameraController>(
                cameraControllerBuilder,
                x => x.SetActive(true),
                x => x.SetActive(false),
                x => x.Dispose()
            );
        }

        public IMapCameraController Get(float zoom, Vector2Int centerPosition)
        {
            var cameraController = pool.Get();
            cameraController.SetZoom(zoom);
            cameraController.SetPosition(centerPosition);
            return cameraController;
        }

        public void Release(IMapCameraController cameraController)
        {
            if (cameraController == null)
                return;

            pool.Release(cameraController);
        }

        public void Dispose()
        {
            pool?.Dispose();
        }
    }
}
