using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoadingHUD
{

    public class LoadingTipsRandomizer : MonoBehaviour
    {
        [SerializeField] internal LoadingTip[] tips;
        [SerializeField] internal Image image;
        [SerializeField] internal TextMeshProUGUI label;
        [SerializeField] internal float secondsBetweenTips = 7.5f;

        private List<LoadingTip> randomizedTips = new List<LoadingTip>();
        private int tipIndex = -1;

        private void Awake()
        {
            randomizedTips = tips.OrderBy(x => Guid.NewGuid()).ToList();
            StartCoroutine(ShowRandomTipsRoutine());
        }

        private IEnumerator ShowRandomTipsRoutine()
        {
            while (true)
            {
                tipIndex = (tipIndex + 1) % randomizedTips.Count;
                SetTip(randomizedTips[tipIndex]);
                yield return WaitForSecondsCache.Get(secondsBetweenTips);
            }
        }

        private void SetTip(LoadingTip tip)
        {
            label.text = tip.text;
            image.sprite = tip.sprite;
            image.SetNativeSize();
        }
    }

    [Serializable]
    public class LoadingTip
    {
        public string text;
        public Sprite sprite;
    }

}