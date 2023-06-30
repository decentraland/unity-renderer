using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DCL.LoadingScreen.V2
{
    /// <summary>
    /// View responsible of showing the corresponding provided Hint by LoadingScreenHintsController
    /// </summary>
    public class HintView : MonoBehaviour, IHintView
    {
        [SerializeField] private TMP_Text hintTitleText;
        [SerializeField] private TMP_Text hintBodyText;
        [SerializeField] private Image hintImage;
        [SerializeField] private Image hintBackgroundImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private const float FADE_DURATION = 0.5f;
        private CancellationTokenSource fadeCts;

        public void Initialize(Hint hint, Texture2D texture, float fadeDuration = FADE_DURATION, bool startAsActive = false)
        {
            try
            {
                if (canvasGroup == null)
                    throw new WarningException("HintView - CanvasGroup has not been found!");
                if (hintTitleText == null)
                    throw new WarningException("HintView - HintText is not assigned!");
                if (hintTitleText == null)
                    throw new WarningException("HintView - HintBodyText is not assigned!");
                if (hintImage == null)
                    throw new WarningException("HintView - HintImage is not assigned!");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                throw;
            }

            hintTitleText.text = hint.Title;
            hintBodyText.text = hint.Body;

            if (texture != null)
            {
                var newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                hintImage.sprite = newSprite;
                hintBackgroundImage.sprite = newSprite;
            }

            ToggleHint(startAsActive);
        }

        public void ToggleHint(bool active)
        {
            if (this != null)
            {
                ToggleHintAsync(active);
            }
        }

        public void ToggleHintAsync(bool fadeIn)
        {
            if (fadeCts != null)
            {
                fadeCts.Cancel();
                fadeCts.Dispose();
            }

            fadeCts = new CancellationTokenSource();
            Fade(fadeIn, fadeCts.Token).Forget();
        }

        public void CancelAnyHintToggle()
        {
            if (fadeCts == null) return;

            fadeCts.Cancel();
            fadeCts.Dispose();
            fadeCts = null;
        }

        private async UniTask Fade(bool fadeIn, CancellationToken cancellationToken = default)
        {
            float startAlpha = fadeIn ? 0 : 1;
            float endAlpha = fadeIn ? 1 : 0;
            float elapsedTime = 0;

            if (fadeIn)
            {
                gameObject.SetActive(true);
            }

            while (elapsedTime < FADE_DURATION)
            {
                cancellationToken.ThrowIfCancellationRequested();

                elapsedTime += Time.unscaledDeltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / FADE_DURATION);
                canvasGroup.alpha = newAlpha;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            canvasGroup.alpha = endAlpha;

            if (!fadeIn)
            {
                gameObject.SetActive(false);
            }
        }

    }
}
