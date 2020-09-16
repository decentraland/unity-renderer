using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Progress bar used by several tutorial steps to indicate their progress.
    /// </summary>
    public class Tutorial_ProgressBar : MonoBehaviour
    {
        [SerializeField] Animator progressBarAnimator;
        [SerializeField] RectTransform progressBarRectTransform;
        [SerializeField] TMP_Text percentageText;
        [SerializeField] float barSpeed = 3.0f;

        public event Action OnNewProgressBarSizeSet;

        private float maxBarWith;
        private Coroutine progressBarCoroutine;

        private static int ANIM_PROPERTY_SET_NEW_PERCENTAGE = Animator.StringToHash("SetNewPercentage");

        private void Awake()
        {
            maxBarWith = ((RectTransform)transform).sizeDelta.x;
        }

        private void OnDestroy()
        {
            if (progressBarCoroutine != null)
            {
                StopCoroutine(progressBarCoroutine);
                progressBarCoroutine = null;
            }
        }

        /// <summary>
        /// Set a new percentage value to the progress bar. It will make the bar size change.
        /// </summary>
        /// <param name="percentage">A value between 0 and 100.</param>
        /// <param name="applyAnimation">Indicates if the bar will progresively change until reach the new percentage value.</param>
        public void SetPercentage(int percentage, bool applyAnimation = true)
        {
            if (percentage < 0f || percentage > 100f)
                return;

            percentageText.text = string.Format("{0}%", percentage);

            if (applyAnimation)
            {
                progressBarAnimator.SetTrigger(ANIM_PROPERTY_SET_NEW_PERCENTAGE);
                progressBarCoroutine = StartCoroutine(ChangeProgressBarSize(progressBarRectTransform.sizeDelta.x, GetProgressBarWidthFromPercentage(percentage)));
            }
            else
            {
                progressBarRectTransform.sizeDelta = new Vector2(
                    GetProgressBarWidthFromPercentage(percentage),
                    progressBarRectTransform.sizeDelta.y);
            }
        }

        private float GetProgressBarWidthFromPercentage(int percentage)
        {
            return Mathf.Clamp(
                percentage * maxBarWith / 100f,
                0f,
                maxBarWith);
        }

        private IEnumerator ChangeProgressBarSize(float oldWidth, float newWidth)
        {
            float t = 0f;

            while (progressBarRectTransform.sizeDelta.x < newWidth)
            {
                t += barSpeed * Time.deltaTime;
                if (t <= 1.0f)
                    progressBarRectTransform.sizeDelta = new Vector2(Mathf.Lerp(oldWidth, newWidth, t), progressBarRectTransform.sizeDelta.y);
                else
                    progressBarRectTransform.sizeDelta = new Vector2(newWidth, progressBarRectTransform.sizeDelta.y);

                yield return null;
            }

            if (progressBarCoroutine != null)
            {
                StopCoroutine(progressBarCoroutine);
                progressBarCoroutine = null;
            }

            OnNewProgressBarSizeSet?.Invoke();
        }
    }
}