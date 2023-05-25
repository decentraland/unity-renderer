using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class PreviewCameraPanningController
    {
        public event Action<Vector3> OnPanning;

        private readonly InputAction_Hold secondClickAction;
        private readonly InputAction_Hold middleClickAction;
        private readonly float panSpeed;
        private readonly bool allowVerticalPanning;
        private readonly bool allowHorizontalPanning;
        private readonly float inertiaDuration;
        private readonly IPreviewCameraPanningDetector previewCameraPanningDetector;

        private Vector3 lastMousePosition;
        private Vector3 lastPanningDeltaBeforeEndDrag;
        private CancellationTokenSource cts = new ();

        public PreviewCameraPanningController(
            InputAction_Hold secondClickAction,
            InputAction_Hold middleClickAction,
            float panSpeed,
            bool allowVerticalPanning,
            bool allowHorizontalPanning,
            float inertiaDuration,
            IPreviewCameraPanningDetector previewCameraPanningDetector)
        {
            this.secondClickAction = secondClickAction;
            this.middleClickAction = middleClickAction;
            this.panSpeed = panSpeed;
            this.allowVerticalPanning = allowVerticalPanning;
            this.allowHorizontalPanning = allowHorizontalPanning;
            this.inertiaDuration = inertiaDuration;
            this.previewCameraPanningDetector = previewCameraPanningDetector;

            previewCameraPanningDetector.OnDragStarted += OnBeginDrag;
            previewCameraPanningDetector.OnDragging += OnDrag;
            previewCameraPanningDetector.OnDragFinished += OnEndDrag;
        }

        private void OnBeginDrag()
        {
            if (!middleClickAction.isOn && !secondClickAction.isOn)
                return;

            cts = cts.SafeRestart();
            lastMousePosition = Input.mousePosition;
            AudioScriptableObjects.buttonClick.Play(true);
        }

        private void OnDrag()
        {
            if (!middleClickAction.isOn && !secondClickAction.isOn)
                return;

            var panningDelta = Input.mousePosition - lastMousePosition;
            panningDelta.y *= -1;

            if (!allowVerticalPanning)
                panningDelta.y = 0f;

            if (!allowHorizontalPanning)
                panningDelta.x = 0f;

            panningDelta *= panSpeed * Time.deltaTime;
            lastPanningDeltaBeforeEndDrag = panningDelta;
            lastMousePosition = Input.mousePosition;

            OnPanning?.Invoke(panningDelta);
        }

        private void OnEndDrag()
        {
            if (lastPanningDeltaBeforeEndDrag.magnitude >= 0.01f)
                InertiaAsync(cts.Token).Forget();

            AudioScriptableObjects.buttonRelease.Play(true);
        }

        private async UniTask InertiaAsync(CancellationToken ct)
        {
            float inverseTimer = 1f / inertiaDuration;
            float timeLeft = inertiaDuration;

            while (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                OnPanning?.Invoke(Vector3.Lerp(lastPanningDeltaBeforeEndDrag, Vector3.zero, 1 - (timeLeft * inverseTimer)));
                await UniTask.NextFrame(ct);
            }
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            previewCameraPanningDetector.OnDragStarted -= OnBeginDrag;
            previewCameraPanningDetector.OnDragging -= OnDrag;
            previewCameraPanningDetector.OnDragFinished -= OnEndDrag;
        }
    }
}
