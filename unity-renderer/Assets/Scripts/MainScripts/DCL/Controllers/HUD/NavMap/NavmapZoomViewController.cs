using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.MapCameraController;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL
{
    public class NavmapZoomViewController : IDisposable
    {
        private const float MOUSE_WHEEL_THRESHOLD = 0.04f;

        private readonly NavmapZoom view;

        private bool active;

        private CancellationTokenSource cts;
        private bool isScaling;

        private float targetNormalizedZoom;
        private int currentZoomLevel;

        private IMapCameraController cameraController;

        private readonly AnimationCurve normalizedCurve;
        private readonly int zoomSteps;

        public NavmapZoomViewController(NavmapZoom view)
        {
            this.view = view;

            // Keys should be between [0;1]
            var keys = view.normalizedZoomCurve.keys;

            var firstKey = keys[0];
            firstKey.time = 0;
            firstKey.value = 0;
            keys[0] = firstKey;

            var lastKey = keys[^1];
            lastKey.value = 1;
            keys[^1] = lastKey;

            normalizedCurve = new AnimationCurve(keys);
            zoomSteps = keys.Length;
        }

        public float ResetZoom()
        {
            SetZoomLevel(Mathf.FloorToInt((zoomSteps - 1) / 2f));
            return targetNormalizedZoom;
        }

        public void Activate(IMapCameraController mapCameraController)
        {
            if (active)
            {
                if (cameraController == mapCameraController)
                    return;

                Deactivate();
            }

            cts = new CancellationTokenSource();

            cameraController = mapCameraController;

            view.MouseWheelAction.OnValueChanged += OnMouseWheelValueChanged;
            view.ZoomIn.InputAction.OnStarted += Zoom;
            view.ZoomOut.InputAction.OnStarted += Zoom;
            view.ZoomIn.Button.onClick.AddListener(() => Zoom(DCLAction_Hold.ZoomIn));
            view.ZoomOut.Button.onClick.AddListener(() => Zoom(DCLAction_Hold.ZoomOut));

            active = true;
        }

        public void Deactivate()
        {
            if (!active) return;

            cts.Cancel();
            cts.Dispose();
            cts = null;

            view.MouseWheelAction.OnValueChanged -= OnMouseWheelValueChanged;
            view.ZoomIn.InputAction.OnStarted -= Zoom;
            view.ZoomOut.InputAction.OnStarted -= Zoom;
            view.ZoomIn.Button.onClick.RemoveAllListeners();
            view.ZoomOut.Button.onClick.RemoveAllListeners();

            active = false;
        }

        private void OnMouseWheelValueChanged(DCLAction_Measurable action, float value)
        {
            if (value == 0 || Mathf.Abs(value) < MOUSE_WHEEL_THRESHOLD)
                return;

            var zoomAction = value > 0 ? DCLAction_Hold.ZoomIn : DCLAction_Hold.ZoomOut;
            Zoom(zoomAction);
        }

        private void SetZoomLevel(int zoomLevel)
        {
            currentZoomLevel = Mathf.Clamp(zoomLevel, 0, zoomSteps - 1);
            targetNormalizedZoom = normalizedCurve.Evaluate(currentZoomLevel);
        }

        private void Zoom(DCLAction_Hold action)
        {
            if (!active || isScaling)
                return;

            switch (action)
            {
                case DCLAction_Hold.ZoomIn when Mathf.Approximately(targetNormalizedZoom, 1f):
                case DCLAction_Hold.ZoomOut when Mathf.Approximately(targetNormalizedZoom, 0f):
                    return;
            }

            EventSystem.current.SetSelectedGameObject(null);

            SetZoomLevel(currentZoomLevel + (action == DCLAction_Hold.ZoomIn ? 1 : -1));

            ScaleOverTime(cameraController.Zoom, targetNormalizedZoom, cts.Token).Forget();

            SetUiButtonsInteractivity();
        }

        private void SetUiButtonsInteractivity()
        {
            view.ZoomIn.SetUiInteractable(isInteractable: currentZoomLevel < zoomSteps - 1);
            view.ZoomOut.SetUiInteractable(isInteractable: currentZoomLevel > 0);
        }

        private async UniTaskVoid ScaleOverTime(float from, float to, CancellationToken ct)
        {
            isScaling = true;
            var scaleDuration = view.scaleDuration;

            for (float timer = 0; timer < scaleDuration; timer += Time.deltaTime)
            {
                if (ct.IsCancellationRequested)
                    break;

                cameraController.SetZoom(Mathf.Lerp(from, to, timer / scaleDuration));

                // omit CT, handle cancellation gracefully
                await UniTask.NextFrame();
            }

            isScaling = false;
        }

        public void Dispose()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
}
