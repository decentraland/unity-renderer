using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
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
        [FormerlySerializedAs("hintBagroundImage")] [SerializeField] private Image hintBackgroundImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private const float FADE_DURATION = 0.4f;

        public void Initialize(Hint hint, Texture2D texture, bool startAsActive = false)
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
                ToggleHintAsync(active).Forget();
            }
        }

        public UniTask ToggleHintAsync(bool active)
        {
            if (this != null)
            {
                return Fade(active);
            }
            return UniTask.CompletedTask;
        }

        private async UniTask Fade(bool fadeIn)
        {
            if (fadeIn)
            {
                gameObject.SetActive(true);
            }

            float startAlpha = fadeIn ? 0 : 1;
            float endAlpha = fadeIn ? 1 : 0;
            float elapsedTime = 0;

            while (elapsedTime < FADE_DURATION)
            {
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
