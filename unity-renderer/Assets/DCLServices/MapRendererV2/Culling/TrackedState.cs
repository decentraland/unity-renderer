using System.Collections.Generic;

namespace DCLServices.MapRendererV2.Culling
{
    public partial class MapCullingController
    {
        internal partial class TrackedState<T> : TrackedState where T: IMapPositionProvider
        {
            private readonly T obj;
            public override IMapPositionProvider Obj => obj;

            private readonly IMapCullingListener<T> listener;
            private bool? lastCalledWasVisible = null;

            public TrackedState(T obj, IMapCullingListener<T> listener)
            {
                nodeInQueue = new LinkedListNode<TrackedState>(this);
                this.obj = obj;
                this.listener = listener;
            }

            public override void CallListener(bool forceCall = false)
            {
                bool currentVisible = visibleFlag > 0;

                if (!forceCall)
                {
                    if (currentVisible == lastCalledWasVisible)
                        return;
                }

                lastCalledWasVisible = currentVisible;

                if (currentVisible)
                    listener.OnMapObjectBecameVisible(obj);
                else
                    listener.OnMapObjectCulled(obj);
            }
        }

        internal abstract class TrackedState
        {
            public abstract IMapPositionProvider Obj { get; }

            // Constant memory footprint => we don't add and remove State but keep a single instance of Node instead
            public LinkedListNode<TrackedState> nodeInQueue { get; protected set; }

            protected int visibleFlag;
            protected int dirtyCamerasFlag;

            public abstract void CallListener(bool forceCall = false);

            public void SetCameraFlag(int value)
            {
                dirtyCamerasFlag = value;
            }

            public void SetCameraFlag(int index, bool value)
            {
                if (value)
                    dirtyCamerasFlag |= (1 << index);
                else
                    dirtyCamerasFlag &= ~(1 << index);
            }

            public bool IsCameraDirty(int index) =>
                (dirtyCamerasFlag & (1 << index)) > 0;

            public void SetVisibleFlag(int value)
            {
                visibleFlag = value;
            }

            public void SetVisibleFlag(int index, bool value)
            {
                if (value)
                    visibleFlag |= (1 << index);
                else
                    visibleFlag &= ~(1 << index);
            }
        }

        IReadOnlyDictionary<IMapPositionProvider, TrackedState> IMapCullingController.TrackedObjects => trackedObjs;
    }
}
