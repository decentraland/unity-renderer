using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class MetadataSidePanelAnimator
    {
        private readonly RectTransform panelRectTransform;
        private readonly Image infoButtonImage;
        private readonly float initOffset;

        private float rightOffset;
        private Tweener currentTween;

        public MetadataSidePanelAnimator(RectTransform panelRectTransform, Image infoButtonImage)
        {
            this.infoButtonImage = infoButtonImage;
            this.panelRectTransform = panelRectTransform;

            initOffset = panelRectTransform.offsetMax.x;
            rightOffset = -initOffset;
        }

        public void ToggleSizeMode(bool toFullScreen, float duration)
        {
            currentTween.Kill();

            if (toFullScreen)
            {
                currentTween = DOVirtual.Float(rightOffset, 0, duration, UpdateSizeMode);
                infoButtonImage.DOFade(0f, duration);
            }
            else
            {
                currentTween = DOVirtual.Float(rightOffset, -initOffset, duration, UpdateSizeMode);
                infoButtonImage.DOFade(1f, duration);
            }
        }

        private void UpdateSizeMode(float value)
        {
            rightOffset = value;
            panelRectTransform.offsetMax = new Vector2(-rightOffset, panelRectTransform.offsetMax.y);
        }
    }
}
