using System;
using UnityEngine;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Display the current state of the Loading Screen Controller
    /// </summary>
    public class LoadingScreenView : BaseComponentView, ILoadingScreenView
    {
        private static readonly string PATH = "_LoadingScreen";

        [SerializeField] private LoadingScreenTipsView tipsView;
        public event Action<ShowHideAnimator> OnFadeInFinish;

        public static LoadingScreenView Create() =>
            Create<LoadingScreenView>(PATH);

        public override void Start()
        {
            base.Start();
            showHideAnimator.OnWillFinishStart += OnFadeInFinish;
            showHideAnimator.OnWillFinishHide += OnFadeOutFinish;

            FadeIn(true);
        }

        public override void Dispose()
        {
            base.Dispose();
            showHideAnimator.OnWillFinishStart -= OnFadeInFinish;
            showHideAnimator.OnWillFinishHide -= OnFadeOutFinish;
        }

        public void FadeIn(bool instant)
        {
            if (isVisible) return;

            Show(instant);
            tipsView.ShowTips();
        }

        public void FadeOut()
        {
            Hide();
        }

        public void SetLoadingTipsController(LoadingScreenTipsController tipsController)
        {
            tipsView.SetLoadingTipsController(tipsController);
        }

        public void UpdateLoadingMessage() { }

        public override void RefreshControl() { }

        private void OnFadeOutFinish(ShowHideAnimator obj) =>
            tipsView.HideTips();
    }
}
