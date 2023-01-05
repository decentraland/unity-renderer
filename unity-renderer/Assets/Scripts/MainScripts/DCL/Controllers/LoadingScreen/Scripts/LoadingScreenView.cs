using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Display the current state of the Loading Screen Controller
    /// </summary>
    public class LoadingScreenView : BaseComponentView, ILoadingScreenView
    {
        private static readonly string PATH = "_LoadingScreen";

        [SerializeField] private LoadingScreenTipsView tipsView;
        [SerializeField] private LoadingScreenPercentageView percentageView;

        public event Action<ShowHideAnimator> OnFadeInFinish;
        public RenderTexture blitRenderTexture;
        public RawImage renderTextureContainer;

        public static LoadingScreenView Create() =>
            Create<LoadingScreenView>(PATH);

        public override void Start()
        {
            base.Start();
            showHideAnimator.OnWillFinishStart += OnFadeInFinish;

            FadeIn(true);
            renderTextureContainer.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            base.Dispose();
            showHideAnimator.OnWillFinishStart -= OnFadeInFinish;
        }

        public LoadingScreenTipsView GetTipsView() =>
            tipsView;

        public LoadingScreenPercentageView GetPercentageView() =>
            percentageView;

        public void FadeIn(bool instant)
        {
            renderTextureContainer.gameObject.SetActive(true);
            if (isVisible) return;

            Show(instant);
        }

        public void FadeOut()
        {
            Hide();
        }

        public override void RefreshControl() { }

        public void SetRenderTexture()
        {
            renderTextureContainer.texture = blitRenderTexture;
        }
    }
}
