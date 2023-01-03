using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.LoadingScreen
{
    public class LoadingScreenTipsView : MonoBehaviour
    {
        private readonly TimeSpan SHOWING_TIME_TIPS = TimeSpan.FromSeconds(2);

        [SerializeField] private TMP_Text tipsText;
        [SerializeField] private Image tipsImage;

        private LoadingScreenTipsController tipsController;
        private CancellationTokenSource disposeCts = new ();

        public void SetLoadingTipsController(LoadingScreenTipsController tipsController)
        {
            this.tipsController = tipsController;
        }

        private async UniTask IterateTipsAsync()
        {
            while (true)
            {
                LoadingScreenTipsController.LoadingTip loadingTip = tipsController.GetNextLoadingTip();
                tipsText.text = loadingTip.text;
                tipsImage.sprite = loadingTip.sprite;
                await UniTask.Delay(SHOWING_TIME_TIPS, cancellationToken: disposeCts.Token);
            }
        }

        public void ShowTips()
        {
            disposeCts = new CancellationTokenSource();
            IterateTipsAsync();
            gameObject.SetActive(true);
        }

        public void HideTips()
        {
            disposeCts.Cancel();
            disposeCts?.Dispose();
            disposeCts = null;
            gameObject.SetActive(false);
        }
    }
}
