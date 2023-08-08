using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.ScreencaptureCamera.UI
{
    [RequireComponent(typeof(Canvas))] [DisallowMultipleComponent]
    public class ScreencaptureCameraHUDView : MonoBehaviour
    {
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private GameObject noSpaceInfo;
        [SerializeField] private Image goldenRuleFrame;

        [Header("BUTTONS")]
        [SerializeField] private Button cameraReelButton;
        [SerializeField] private Button takeScreenshotButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button shortcutsInfoButton;

        [Header("SHORTCUTS INFO PANEL")]
        [SerializeField] private GameObject shortcutsInfoPanel;
        [SerializeField] private Image openShortcutsIcon;
        [SerializeField] private Image closeShortcutsIcon;

        [Header("CAPTURE VFX")]
        [SerializeField] private Image whiteSplashImage;
        [SerializeField] private RectTransform cameraReelIcon;
        [SerializeField] private Image animatedImage;

        private Sequence currentVfxSequence;

        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public Image RefImage { get; private set; }

        public bool IsVisible => rootCanvas.enabled;

        public event Action CloseButtonClicked;
        public event Action ShortcutsInfoButtonClicked;
        public event Action CameraReelButtonClicked;
        public event Action TakeScreenshotButtonClicked;

        private void OnEnable()
        {
            cameraReelButton.onClick.AddListener(() => CameraReelButtonClicked?.Invoke());
            takeScreenshotButton.onClick.AddListener(() => TakeScreenshotButtonClicked?.Invoke());
            shortcutsInfoButton.onClick.AddListener(() => ShortcutsInfoButtonClicked?.Invoke());
            closeButton.onClick.AddListener(() => CloseButtonClicked?.Invoke());
        }

        private void OnDisable()
        {
            cameraReelButton.onClick.RemoveAllListeners();
            takeScreenshotButton.onClick.RemoveAllListeners();
            shortcutsInfoButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
        }

        public virtual void SetVisibility(bool isVisible, bool hasStorageSpace)
        {
            takeScreenshotButton.interactable = hasStorageSpace;
            noSpaceInfo.SetActive(!hasStorageSpace);

            if (rootCanvas.enabled != isVisible)
                rootCanvas.enabled = isVisible;
        }

        public void ScreenshotCleanView(bool isVisible)
        {
            goldenRuleFrame.enabled = isVisible;
        }

        public void ToggleShortcutsInfosHelpPanel()
        {
            bool setEnabled = shortcutsInfoPanel.activeSelf == false;

            shortcutsInfoPanel.SetActive(setEnabled);

            openShortcutsIcon.enabled = !setEnabled;
            closeShortcutsIcon.enabled = setEnabled;
        }

        public void ScreenshotCaptureAnimation(Texture2D screenshotImage, float splashDuration, float afterSplashPause, float transitionDuration)
        {
            currentVfxSequence?.Complete();
            currentVfxSequence?.Kill();

            animatedImage.sprite = Sprite.Create(screenshotImage, new Rect(0, 0, screenshotImage.width, screenshotImage.height), Vector2.zero);

            animatedImage.enabled = true;
            whiteSplashImage.enabled = true;

            currentVfxSequence = CaptureVFXSequence(splashDuration, afterSplashPause, transitionDuration).Play();
        }

        private Sequence CaptureVFXSequence(float splashDuration, float afterSplashPause, float transitionDuration)
        {
            Sequence sequence = DOTween.Sequence();

            sequence.Append(AnimateSplash(splashDuration));
            sequence.AppendInterval(afterSplashPause); // Delay between splash and transition
            sequence.Append(AnimateVFXImageTransition(transitionDuration));
            sequence.Join(AnimateVFXImageScale(transitionDuration));
            sequence.Join(AnimateVFXImageScale(transitionDuration));
            sequence.OnComplete(() => animatedImage.enabled = false);

            return sequence;
        }

        private Tween AnimateVFXImageTransition(float duration)
        {
            Vector3 cachedPosition = animatedImage.rectTransform.position;

            return animatedImage.rectTransform.DOMove(cameraReelIcon.position, duration)
                                .SetEase(Ease.InOutQuad)
                                .OnComplete(() => { animatedImage.rectTransform.position = cachedPosition; });
        }

        private Tween AnimateVFXImageScale(float duration) =>
            animatedImage.rectTransform.DOScale(Vector2.zero, duration)
                         .SetEase(Ease.InOutQuad)
                         .OnComplete(() => { animatedImage.rectTransform.localScale = Vector2.one; });

        private Tween AnimateSplash(float duration)
        {
            return whiteSplashImage.DOFade(0f, duration)
                                   .SetEase(Ease.InOutQuad)
                                   .OnComplete(() =>
                                    {
                                        whiteSplashImage.enabled = false;
                                        whiteSplashImage.color = Color.white;
                                    });
        }

        [ContextMenu(nameof(ScaleCheck))]
        private void ScaleCheck()
        {
            //=====---- Settings
            float targetFrameHeight = 1080;
            float targetFrameWidth = 1920;
            float targetAspectRatio = targetFrameWidth / targetFrameHeight;
            var frameScale = 0.87f;
            Debug.Log($"targetAspectRatio {targetAspectRatio}");

            //=====---- Current
            Debug.Log("CURRENT");
            float currentScreenWidth = RectTransform.rect.width * RectTransform.lossyScale.x;
            float currentScreenHeight = RectTransform.rect.height * RectTransform.lossyScale.y;
            float currentScreenAspectRatio = currentScreenWidth / currentScreenHeight;
            Debug.Log($"currentScreen {currentScreenWidth}, {currentScreenHeight}");
            Debug.Log($"currentScreenAspectRatio {currentScreenAspectRatio}");

            float currentFrameWidth;
            float currentFrameHeight;

            Debug.Log($"fame simple scale {currentScreenWidth * frameScale}, {currentScreenHeight * frameScale}");

            // Adjust current by smallest side
            if (currentScreenAspectRatio > targetAspectRatio) // Height is the limiting dimension, so scaling width based on it
            {
                currentFrameHeight = currentScreenHeight * frameScale;
                currentFrameWidth = currentFrameHeight * targetAspectRatio;
            }
            else // Width is the limiting dimension, so scaling height based on it
            {
                currentFrameWidth = currentScreenWidth * frameScale;
                currentFrameHeight = currentFrameWidth / targetAspectRatio;
            }

            Debug.Log($"currentFrame {currentFrameWidth}, {currentFrameHeight}");

            //=====---- Target
            Debug.Log("TARGET");

            float upscaleFrameWidth = targetFrameWidth / currentFrameWidth;
            float upscaleFrameHeight = targetFrameHeight / currentFrameHeight;
            Debug.Assert(Math.Abs(upscaleFrameWidth - upscaleFrameHeight) < 0.0001f);
            Debug.Log($"targetUpscale {upscaleFrameWidth}, {upscaleFrameHeight}");
            float targetUpscale = upscaleFrameWidth;

            float calculatedTargetFrameWidth = currentFrameWidth *targetUpscale;
            float calculatedTargetFrameHeight = currentFrameHeight * targetUpscale;
            float targetScreenWidth = currentScreenWidth * targetUpscale;
            float targetScreenHeight = currentScreenHeight * targetUpscale;
            Debug.Log($"target Frame and Screen {calculatedTargetFrameWidth}:{calculatedTargetFrameHeight}, {targetScreenWidth}:{targetScreenHeight}");

            //=====---- Rounded Upscaled
            Debug.Log("UPSCALED");

            int upscaleFactor = Mathf.CeilToInt(targetUpscale);
            Debug.Log($"rounded Upscale {upscaleFactor}");

            float upscaledFrameWidth = currentFrameWidth * upscaleFactor;
            float upscaledFrameHeight = currentFrameHeight * upscaleFactor;
            float upscaledScreenWidth = currentScreenWidth * upscaleFactor;
            float upscaledScreenHeight = currentScreenHeight * upscaleFactor;

            Debug.Log($"Upscaled Frame and Screen {upscaledFrameWidth}:{upscaledFrameHeight}, {upscaledScreenWidth}:{upscaledScreenHeight}");

            //=====---- Downscaled from Rounded
            Debug.Log("DOWNSCALED");

            float downscaleScreenWidth = targetScreenWidth / upscaledScreenWidth;
            float downscaleScreenHeight = targetScreenHeight / upscaledScreenHeight;
            Debug.Assert(Math.Abs(downscaleScreenWidth - downscaleScreenHeight) < 0.0001f);
            Debug.Log($"{downscaleScreenWidth}, {downscaleScreenHeight}");
            float targetDownscale = downscaleScreenWidth;
            Debug.Log($"{targetDownscale}");

            float downscaledFrameWidth = upscaledFrameWidth * targetDownscale;
            float downscaledFrameHeight = upscaledFrameHeight * targetDownscale;
            float downscaledScreenWidth = upscaledScreenWidth * targetDownscale;
            float downscaledScreenHeight = upscaledScreenHeight * targetDownscale;

            Debug.Log($"Downscaled Frame and Screen {downscaledFrameWidth}:{downscaledFrameHeight}, {downscaledScreenWidth}:{downscaledScreenHeight}");
        }
    }
}
