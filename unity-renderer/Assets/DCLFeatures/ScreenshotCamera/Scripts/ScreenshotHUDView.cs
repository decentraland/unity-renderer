using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.ScreenshotCamera
{
    [RequireComponent(typeof(Canvas))] [DisallowMultipleComponent]
    public class ScreenshotHUDView : MonoBehaviour
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public Image RefImage { get; private set; }
        [field: SerializeField] public Button CloseButton { get; protected set; }
        [field: SerializeField] public Button TakeScreenshotButton { get; protected set; }

        [Header("SHORTCUTS PANEL")]
        [SerializeField] private Button shortcutButton;
        [SerializeField] private GameObject shortcutsHelpPanel;

        [Header("CAPTURE VFX")]
        [SerializeField] private Image whiteSplashImage;
        [SerializeField] private RectTransform cameraReelIcon;
        [SerializeField] private Image animatedImage;

        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            shortcutButton.onClick.AddListener(ToggleShortcutsHelpPanel);
        }

        public virtual void SwitchVisibility(bool isVisible) =>
            canvas.enabled = isVisible;

        private void ToggleShortcutsHelpPanel() =>
            shortcutsHelpPanel.SetActive(!shortcutsHelpPanel.activeSelf);

        public void ScreenshotCaptureAnimation(Texture2D screenshotImage, float splashDuration, float transitionDuration)
        {
            animatedImage.sprite = Sprite.Create(screenshotImage, new Rect(0, 0, screenshotImage.width, screenshotImage.height), Vector2.zero);

            animatedImage.enabled = true;
            whiteSplashImage.enabled = true;

            currentVfxSequence?.Complete();
            currentVfxSequence?.Kill();

            currentVfxSequence = CaptureVFXSequence(splashDuration, transitionDuration).Play();
        }

        private Sequence currentVfxSequence;

        private Sequence CaptureVFXSequence(float splashDuration, float transitionDuration, float afterSplashPause = 0.1f)
        {
            Sequence sequence = DOTween.Sequence();

            sequence.Append(AnimateSplash(splashDuration));
            sequence.AppendInterval(afterSplashPause); // Delay between splash and transition
            sequence.Append(AnimateVFXImageTransition(transitionDuration));
            sequence.Join(AnimateVFXImageScale(transitionDuration));

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
