using DCLServices.MapRendererV2.MapCameraController;
using System.Collections.Generic;

namespace DCLServices.MapRendererV2.Culling
{
    /// <summary>
    /// Exposing private methods to tests
    /// </summary>
    public partial class MapCullingController
    {
        internal List<CameraState> CameraStates => cameraStates;
        internal LinkedList<TrackedState> DirtyObjects => dirtyObjects;
        internal Dictionary<IMapPositionProvider, TrackedState> TrackedObjs => trackedObjs;

        internal int DirtyCamerasFlag
        {
            get => dirtyCamerasFlag;
            set => dirtyCamerasFlag = value;
        }

        internal void SetCameraDirtyInternal_Test(int index) =>
            SetCameraDirtyInternal(index);

        internal bool IsCameraDirty_Test(int index) =>
            IsCameraDirty(index);

        internal void OnCameraAdded_Test(IMapCameraControllerInternal cameraController)
        {
            ((IMapCullingController)this).OnCameraAdded(cameraController);
        }

        internal void OnCameraRemoved_Test(IMapCameraControllerInternal cameraController)
        {
            ((IMapCullingController)this).OnCameraRemoved(cameraController);
        }

        internal void ResolveDirtyCameras_Test()
        {
            ResolveDirtyCameras();
        }

        internal void ResolveDirtyObjects_Test(int count)
        {
            ResolveDirtyObjects(count);
        }

        internal partial class TrackedState<T> where T: IMapPositionProvider
        {
            internal int DirtyCamerasFlag
            {
                get => dirtyCamerasFlag;
                set => dirtyCamerasFlag = value;
            }

            internal int VisibleFlag
            {
                get => visibleFlag;
                set => visibleFlag = value;
            }

            internal IMapCullingListener<T> Listener => listener;
        }
    }
}
