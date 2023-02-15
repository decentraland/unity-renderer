using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.MapCameraController
{
    public partial class RentMapCameraController
    {
        public ObjectPool<IMapCameraController> Pool => pool;
    }
}
