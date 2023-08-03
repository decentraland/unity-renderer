using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.ScreencaptureCamera
{
    [RequireComponent(typeof(Canvas))] [DisallowMultipleComponent]
    public class ScreencaptureCameraHUDView : MonoBehaviour
    {
        [SerializeField] private Canvas rootCanvas;

        [Header("BUTTONS")]
        [SerializeField] private Button cameraReelButton;
        [SerializeField] private Button takeScreenshotButton;
        [SerializeField] private Button shortcutButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private GameObject shortcutsHelpPanel;

        [Header("CAPTURE VFX")]
        [SerializeField] private Image whiteSplashImage;
        [SerializeField] private RectTransform cameraReelIcon;
        [SerializeField] private Image animatedImage;

        private Sequence currentVfxSequence;

        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public Image RefImage { get; private set; }

        public event Action CloseButtonClicked;
        public event Action ShortcutButtonClicked;
        public event Action CameraReelButtonClicked;
        public event Action TakeScreenshotButtonClicked;

        private void OnEnable()
        {
            cameraReelButton.onClick.AddListener(() => CameraReelButtonClicked?.Invoke());
            takeScreenshotButton.onClick.AddListener(() => TakeScreenshotButtonClicked?.Invoke());
            shortcutButton.onClick.AddListener(() => ShortcutButtonClicked?.Invoke());
            closeButton.onClick.AddListener(() => CloseButtonClicked?.Invoke());
        }

        private void OnDisable()
        {
            cameraReelButton.onClick.RemoveAllListeners();
            takeScreenshotButton.onClick.RemoveAllListeners();
            shortcutButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
        }

        public virtual void SwitchVisibility(bool isVisible) =>
            rootCanvas.enabled = isVisible;

        public void ToggleShortcutsHelpPanel() =>
            shortcutsHelpPanel.SetActive(!shortcutsHelpPanel.activeSelf);

        public void ScreenshotCaptureAnimation(Texture2D screenshotImage, float splashDuration, float transitionDuration)
        {
            currentVfxSequence?.Complete();
            currentVfxSequence?.Kill();

            animatedImage.sprite = Sprite.Create(screenshotImage, new Rect(0, 0, screenshotImage.width, screenshotImage.height), Vector2.zero);

            animatedImage.enabled = true;
            whiteSplashImage.enabled = true;

            currentVfxSequence = CaptureVFXSequence(splashDuration, transitionDuration).Play();
        }

        private Sequence CaptureVFXSequence(float splashDuration, float transitionDuration, float afterSplashPause = 0.25f)
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
    }
}
