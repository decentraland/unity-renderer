using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.MapCameraController;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Profiling;

namespace DCLServices.MapRendererV2.Culling
{
    public partial class MapCullingController : IMapCullingController
    {
        private const int MAX_DIRTY_OBJECTS_PER_FRAME = 10;
        private const int MAX_CAMERAS_COUNT = sizeof(int) * 8;

        private readonly IMapCullingVisibilityChecker cullingVisibilityChecker;
        private readonly List<CameraState> cameraStates = new ();
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
            if (cameraStates.Count == MAX_CAMERAS_COUNT)
                throw new ArgumentOutOfRangeException(nameof(cameraController), "Camera out of range");

            SetCameraDirtyInternal(cameraStates.Count);

            var cameraState = new CameraState
            {
                CameraController = cameraController,
                Rect = cameraController.GetCameraRect(),
            };

            cameraStates.Add(cameraState);
        }

        void IMapCullingController.OnCameraRemoved(IMapCameraControllerInternal cameraController)
        {
            int index = GetCameraIndex(cameraController);

            if (index == -1)
                return;

            for (int i = index; i < cameraStates.Count; i++)
            {
                SetCameraDirtyInternal(i); //We set dirty all the cameras onwards due to index shifting
            }

            cameraStates.RemoveAt(index);
        }

        void IMapCullingController.SetCameraDirty(IMapCameraControllerInternal cameraController)
        {
            int index = GetCameraIndex(cameraController);

            if (index == -1)
                throw new Exception($"Tried to set not tracked camera dirty");

            SetCameraDirtyInternal(index);
            cameraStates[index].Rect = cameraController.GetCameraRect();
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
            state.SetCameraFlag((-1 >> (cameraStates.Count - 1)));

            if (IsTrackedStateDirty(state))
                return;

            dirtyObjects.AddLast(state.nodeInQueue);
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
            if (!trackedObjs.TryGetValue(obj, out var state))
                return;

            if (IsTrackedStateDirty(state))
                dirtyObjects.Remove(state.nodeInQueue);
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
            Profiler.BeginSample(nameof(ResolveDirtyCameras));

            for (var i = 0; i < MAX_CAMERAS_COUNT; i++)
            {
                if (!IsCameraDirty(i))
                    continue;

                foreach ((IMapPositionProvider obj, TrackedState cullable) in trackedObjs)
                {
                    //If index is higher than camera count we have a dirty flag for a no longer tracked camera, we ignore it by setting the flag as 0
                    bool visible = i < cameraStates.Count && cullingVisibilityChecker.IsVisible(obj, cameraStates[i]);
                    cullable.SetVisibleFlag(i, visible);
                    cullable.SetCameraFlag(i, false);
                    cullable.CallListener();
                }
            }

            dirtyCamerasFlag = 0;

            Profiler.EndSample();
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
            Profiler.BeginSample(nameof(ResolveDirtyObjects));

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

            Profiler.EndSample();
        }

        private void ResolveDirtyObject(TrackedState state)
        {
            for (int i = 0; i < MAX_CAMERAS_COUNT; i++)
            {
                if (!state.IsCameraDirty(i))
                    continue;

                //If index is higher than camera count we have a dirty flag for a no longer tracked camera, we ignore it by setting the flag as 0
                bool visible = i < cameraStates.Count && cullingVisibilityChecker.IsVisible(state.Obj, cameraStates[i]);
                state.SetVisibleFlag(i, visible);
                state.SetCameraFlag(i, false);
                state.CallListener();
            }
        }

        private bool IsTrackedStateDirty(TrackedState state) =>
            state.nodeInQueue.List != null;

        private int GetCameraIndex(IMapCameraControllerInternal cameraController)
        {
            for (var i = 0; i < cameraStates.Count; i++)
            {
                if (cameraStates[i].CameraController == cameraController)
                    return i;
            }

            return -1;
        }

        public void Dispose()
        {
            disposingCts?.Cancel();
            disposingCts?.Dispose();
            disposingCts = null;
        }

        IReadOnlyList<CameraState> IMapCullingController.CameraStates => cameraStates;
    }
}
