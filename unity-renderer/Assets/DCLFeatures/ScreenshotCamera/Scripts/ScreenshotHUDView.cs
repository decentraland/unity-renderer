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
        public void ScreenshotCaptureAnimation(Texture2D screenshotImage)
        {
            animatedImage.sprite = Sprite.Create(screenshotImage, new Rect(0, 0, screenshotImage.width, screenshotImage.height), Vector2.zero);
            animatedImage.enabled = true;
            AnimateSplash();
            AnimateTransition();
        }
        private void AnimateTransition()
        {
            Vector3 cachedPosition = animatedImage.rectTransform.position;
            animatedImage.enabled = true;
            animatedImage.rectTransform.DOMove(cameraReelIcon.position, 1f)
                         .SetDelay(1.1f)
                         .SetEase(Ease.InOutQuad);
            animatedImage.rectTransform.DOScale(Vector2.zero, 1f)
                         .SetDelay(1.1f)
                         .SetEase(Ease.InOutQuad)
                         .OnComplete(() =>
                          {
                              animatedImage.enabled = false;
                              animatedImage.rectTransform.position = cachedPosition;
                              animatedImage.rectTransform.localScale = Vector2.one;
                          });
        }
        private void AnimateSplash()
        {
            whiteSplashImage.enabled = true;
            whiteSplashImage.DOFade(0f, 1f)
                            .SetEase(Ease.InOutQuad)
                            .OnComplete(() =>
                             {
                                 whiteSplashImage.enabled = false;
                                 whiteSplashImage.color = Color.white;
                             });
        }
    }
}
