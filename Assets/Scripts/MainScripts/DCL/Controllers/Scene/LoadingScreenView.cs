using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DCL
{
    public class LoadingScreenView : MonoBehaviour
    {
        private const float INIT_PERCENTAGE = 0.5f;

        public float spinnerSpeed = 750f;

        public TextMeshProUGUI percentageText;
        public GameObject fillingImage;
        public RectTransform spinnerRectTransform;
        public CanvasGroup canvasGroup;

        public void SetNormalizedPercentage(float percentage)
        {
            percentageText.text = Mathf.Clamp((int)Math.Ceiling(percentage * 100), 0, 100) + "%";
            fillingImage.transform.localScale = new Vector3(INIT_PERCENTAGE + (percentage * (1 - INIT_PERCENTAGE)), 1, 1);
        }

        void Update()
        {
            spinnerRectTransform.Rotate(-Vector3.forward * spinnerSpeed * Time.deltaTime);
        }

        public IEnumerator FadeOutAndDestroy()
        {
            enabled = false;
            spinnerRectTransform.gameObject.SetActive(false);

            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            Destroy(gameObject);
        }
    }
}