using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.MapCameraController;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCLServices.MapRendererV2.ConsumerUtils
{
    public class MapCameraDragBehavior : IDisposable
    {
        [Serializable]
        public class MapCameraDragBehaviorData
        {
            [SerializeField] internal bool inertia;
            [SerializeField] internal float decelerationRate;
        }

        private static Vector3[] worldCorners = new Vector3[4];

        internal bool dragging { get; private set; }

        private readonly RectTransform rectTransform;
        private readonly MapCameraDragBehaviorData data;

        private IMapCameraController mapCameraController;

        private Vector2 pointerPositionOnDragBegin;
        private Vector2 cameraPositionOnDragBegin;

        private float screenSpaceToLocalCameraPositionRatio;

        private CancellationTokenSource inertiaLoopCTS;

        public MapCameraDragBehavior(RectTransform rectTransform, MapCameraDragBehaviorData data)
        {
            this.rectTransform = rectTransform;
            this.data = data;
        }

        public void Activate(IMapCameraController cameraController)
        {
            mapCameraController = cameraController;
        }

        public void Deactivate()
        {
            mapCameraController = null;
            dragging = false;

            StopInertia();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            rectTransform.GetWorldCorners(worldCorners);

            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, worldCorners[0]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, worldCorners[2]);

            screenSpaceToLocalCameraPositionRatio = mapCameraController.GetVerticalSizeInLocalUnits() / (topRight - bottomLeft).y;

            pointerPositionOnDragBegin = eventData.position;
            cameraPositionOnDragBegin = mapCameraController.LocalPosition;

            dragging = true;

            if (data.inertia)
            {
                StopInertia();
                inertiaLoopCTS = new CancellationTokenSource();
                InertiaLoop(inertiaLoopCTS.Token).Forget();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            var pointerPosition = eventData.position;

            var pointerDelta = pointerPosition - pointerPositionOnDragBegin;
            var targetCameraLocalPosition = cameraPositionOnDragBegin - (pointerDelta * screenSpaceToLocalCameraPositionRatio);

            mapCameraController.SetLocalPosition(targetCameraLocalPosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            dragging = false;
        }

        private void StopInertia()
        {
            inertiaLoopCTS?.Cancel();
            inertiaLoopCTS?.Dispose();
            inertiaLoopCTS = null;
        }

        private async UniTaskVoid InertiaLoop(CancellationToken ct)
        {
            var prevPosition = mapCameraController.LocalPosition;
            var inertialVelocity = Vector2.zero;

            while (true)
            {
                await UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);
                if (ct.IsCancellationRequested) break;
                float unscaledDeltaTime = Time.unscaledDeltaTime;
                var cameraLocalPos = mapCameraController.LocalPosition;

                if (dragging)
                {
                    var newVelocity = (cameraLocalPos - prevPosition) / unscaledDeltaTime;
                    inertialVelocity = Vector2.Lerp(inertialVelocity, newVelocity, unscaledDeltaTime * 10);
                }
                else
                {
                    for (var axis = 0; axis < 2; axis++)
                    {
                        inertialVelocity[axis] *= Mathf.Pow(this.data.decelerationRate, unscaledDeltaTime);

                        if (Mathf.Abs(inertialVelocity[axis]) < 1)
                            inertialVelocity[axis] = 0;
                    }

                    mapCameraController.SetLocalPosition(cameraLocalPos + (inertialVelocity * unscaledDeltaTime));

                    if (Mathf.Approximately(inertialVelocity.x, 0) && Mathf.Approximately(inertialVelocity.y, 0))
                        break;
                }

                prevPosition = cameraLocalPos;
            }
        }

        public void Dispose()
        {
            StopInertia();
        }
    }
}
