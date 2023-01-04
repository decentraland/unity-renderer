using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// View responsible of showing the corresponding provided tip by LoadingScreenTipsController
    /// </summary>
    public class LoadingScreenTipsView : MonoBehaviour
    {
        [SerializeField] private TMP_Text tipsText;
        [SerializeField] private Image tipsImage;

        [SerializeField] internal List<LoadingTip> defaultTips;

        public void ShowTip(LoadingTip loadingTip)
        {
            tipsText.text = loadingTip.text;
            tipsImage.sprite = loadingTip.sprite;
        }
    }
}
