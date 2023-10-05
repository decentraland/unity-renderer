using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.MapRendererV2.MapCameraController;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL
{
    public class NavmapZoomViewController : IDisposable, INavmapZoomViewController
    {
        private const float MOUSE_WHEEL_THRESHOLD = 0.04f;

        private readonly NavmapZoomView view;

        private AnimationCurve normalizedCurve;
        private int zoomSteps;

        private bool active;

        private CancellationTokenSource cts;
        private bool isScaling;

        private float targetNormalizedZoom;
        private int currentZoomLevel;

        private IMapCameraController cameraController;
        private readonly BaseVariable<FeatureFlag> featureFlagsFlags;

        public NavmapZoomViewController(NavmapZoomView view, BaseVariable<FeatureFlag> featureFlagsFlags)
        {
            this.view = view;
            this.featureFlagsFlags = featureFlagsFlags;

            if (featureFlagsFlags.Get().IsInitialized)
                HandleFeatureFlag();
            else
                featureFlagsFlags.OnChange += OnFeatureFlagsChanged;

            normalizedCurve = view.normalizedZoomCurve;
            zoomSteps = normalizedCurve.length;

            CurveClamp01();
        }

        private void OnFeatureFlagsChanged(FeatureFlag current, FeatureFlag previous)
        {
            featureFlagsFlags.OnChange -= OnFeatureFlagsChanged;
            HandleFeatureFlag();
        }

        private void HandleFeatureFlag()
        {
            if (featureFlagsFlags.Get().IsFeatureEnabled("map_focus_home_or_user")) return;

            view.zoomVerticalRange = new Vector2Int(view.zoomVerticalRange.x, 40);

            normalizedCurve = new AnimationCurve();
            normalizedCurve.AddKey(0, 0);
            normalizedCurve.AddKey(1, 0.25f);
            normalizedCurve.AddKey(2, 0.5f);
            normalizedCurve.AddKey(3, 0.75f);
            normalizedCurve.AddKey(4, 1);
            zoomSteps = normalizedCurve.length;
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
        }

        private void CurveClamp01()
        {
            // Keys should be int for zoomSteps to work properly
            for (var i = 0; i < normalizedCurve.keys.Length; i++)
            {
                Keyframe keyFrame = normalizedCurve.keys[i];

                if (i == 0)
                {
                    keyFrame.time = 0;
                    keyFrame.value = 0;
                }
                else if (i == normalizedCurve.length - 1)
                {
                    keyFrame.time = Mathf.RoundToInt(keyFrame.time);
                    keyFrame.value = 1;
                }
                else
                {
                    keyFrame.time = Mathf.RoundToInt(keyFrame.time);
                    keyFrame.value = keyFrame.value;
                }

                normalizedCurve.MoveKey(i, keyFrame);
            }
        }

        public float ResetZoomToMidValue()
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

            DCLAction_Hold zoomAction = value > 0 ? DCLAction_Hold.ZoomIn : DCLAction_Hold.ZoomOut;
            Zoom(zoomAction);
        }

        private void SetZoomLevel(int zoomLevel)
        {
            currentZoomLevel = Mathf.Clamp(zoomLevel, 0, zoomSteps - 1);
            targetNormalizedZoom = normalizedCurve.Evaluate(currentZoomLevel);

            SetUiButtonsInteractivity();
        }

        private void Zoom(DCLAction_Hold action)
        {
            if (!active || isScaling)
                return;

            EventSystem.current.SetSelectedGameObject(null);

            switch (action)
            {
                case DCLAction_Hold.ZoomIn when Mathf.Approximately(targetNormalizedZoom, 1f):
                case DCLAction_Hold.ZoomOut when Mathf.Approximately(targetNormalizedZoom, 0f):
                    return;
            }

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
            float scaleDuration = view.scaleDuration;

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
    }
}
