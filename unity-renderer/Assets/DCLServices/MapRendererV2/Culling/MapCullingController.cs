using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.MapCameraController;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.MapRendererV2.Culling
{
    public partial class MapCullingController : IMapCullingController
    {
        private const int MAX_DIRTY_OBJECTS_PER_FRAME = 10;

        private readonly IMapCullingVisibilityChecker cullingVisibilityChecker;
        private readonly List<IMapCameraControllerInternal> cameras = new ();
        private readonly LinkedList<TrackedState> dirtyObjects = new ();

        private readonly Dictionary<IMapPositionProvider, TrackedState> trackedObjs = new ();

        private int dirtyCamerasFlag = 0;
        private CancellationTokenSource disposingCts = new ();

        internal MapCullingController(IMapCullingVisibilityChecker cullingVisibilityChecker)
        {
            this.cullingVisibilityChecker = cullingVisibilityChecker;
            ResolveDirtyCamerasRoutineAsync(disposingCts.Token).Forget();
            ResolveDirtyObjectsRoutineAsync(disposingCts.Token).Forget();
        }

        private void SetCameraDirtyInternal(int index)
        {
            dirtyCamerasFlag |= (1 << index);
        }

        private bool IsCameraDirty(int index) =>
            (dirtyCamerasFlag & (1 << index)) > 0;

        void IMapCullingController.OnCameraAdded(IMapCameraControllerInternal cameraController)
        {
            if (cameras.Count == sizeof(int))
                throw new Exception("Camera out of range");

            SetCameraDirtyInternal(cameras.Count);
            cameras.Add(cameraController);
        }

        void IMapCullingController.OnCameraRemoved(IMapCameraControllerInternal cameraController)
        {
            int index = cameras.IndexOf(cameraController);

            if (index == -1)
                return;

            for (int i = index; i < cameras.Count; i++)
            {
                SetCameraDirtyInternal(i); //We set dirty all the cameras onwards due to index shifting
            }

            cameras.Remove(cameraController);
        }

        void IMapCullingController.SetCameraDirty(IMapCameraControllerInternal cameraController)
        {
            int index = cameras.IndexOf(cameraController);

            if (index == -1)
                throw new Exception($"Tried to set not tracked camera dirty");

            SetCameraDirtyInternal(index);
        }

        public void SetTrackedObjectPositionDirty<T>(T obj) where T: IMapPositionProvider
        {
            if (!trackedObjs.TryGetValue(obj, out TrackedState state))
                throw new Exception("Tried to set not tracked object dirty");

            SetTrackedStateDirty(state);
        }

        private void SetTrackedStateDirty(TrackedState state)
        {
            // shifting to the right will add zeroes on the left
            state.SetCameraFlag((-1 >> (cameras.Count - 1)));

            if (IsTrackedStateDirty(state))
                return;

            dirtyObjects.AddLast(state);
        }

        void IMapCullingController.StartTracking<T>(T obj, IMapCullingListener<T> listener)
        {
            if (trackedObjs.ContainsKey(obj))
                return;

            TrackedState<T> state = new TrackedState<T>(obj, listener);
            trackedObjs.Add(obj, state);
            SetTrackedStateDirty(state);
        }

        public void StopTracking<T>(T obj) where T: IMapPositionProvider
        {
            if (trackedObjs.TryGetValue(obj, out var state)) { dirtyObjects.Remove(state.nodeInQueue); }

            trackedObjs.Remove(obj);
        }

        private async UniTaskVoid ResolveDirtyCamerasRoutineAsync(CancellationToken ct)
        {
            while (true)
            {
                if (ct.IsCancellationRequested)
                    return;

                ResolveDirtyCameras();
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            }
        }

        private void ResolveDirtyCameras()
        {
            for (var i = 0; i < sizeof(int); i++)
            {
                if (!IsCameraDirty(i))
                    continue;

                foreach ((IMapPositionProvider obj, TrackedState cullable) in trackedObjs)
                {
                    //If index is higher than camera count we have a dirty flag for a no longer tracked camera, we ignore it by setting the flag as 0
                    bool visible = i < cameras.Count && cullingVisibilityChecker.IsVisible(obj, cameras[i]);
                    cullable.SetVisibleFlag(i, visible);
                    cullable.SetCameraFlag(i, false);
                }
            }

            dirtyCamerasFlag = 0;
        }

        private async UniTaskVoid ResolveDirtyObjectsRoutineAsync(CancellationToken ct)
        {
            while (true)
            {
                if (ct.IsCancellationRequested)
                    return;

                ResolveDirtyObjects(MAX_DIRTY_OBJECTS_PER_FRAME);

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }
        }

        private void ResolveDirtyObjects(int count)
        {
            var stateNode = dirtyObjects.First;

            while (count > 0 && stateNode != null)
            {
                ResolveDirtyObject(stateNode.Value);

                var processedNode = stateNode;
                stateNode = stateNode.Next;

                // Remove the processed node
                // it will nullify `List` reference
                dirtyObjects.Remove(processedNode);

                count--;
            }
        }

        private void ResolveDirtyObject(TrackedState state)
        {
            for (int i = 0; i < sizeof(int); i++)
            {
                if (!state.IsCameraDirty(i))
                    continue;

                //If index is higher than camera count we have a dirty flag for a no longer tracked camera, we ignore it by setting the flag as 0
                bool visible = i < cameras.Count && cullingVisibilityChecker.IsVisible(state.Obj, cameras[i]);
                state.SetVisibleFlag(i, visible);
                state.SetCameraFlag(i, false);
            }
        }

        private bool IsTrackedStateDirty(TrackedState state) =>
            state.nodeInQueue.List != null;

        public void Dispose()
        {
            disposingCts?.Cancel();
            disposingCts?.Dispose();
            disposingCts = null;
        }
    }
}
