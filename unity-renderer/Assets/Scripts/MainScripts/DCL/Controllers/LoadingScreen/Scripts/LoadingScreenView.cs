using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
        private RenderTexture renderTexture;

        public event Action<ShowHideAnimator> OnFadeInFinish;
        public RawImage rawImage;

        public static LoadingScreenView Create() =>
            Create<LoadingScreenView>(PATH);

        public override void Start()
        {
            base.Start();
            showHideAnimator.OnWillFinishStart += FadeInFinish;

            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            rawImage.texture = renderTexture;
            rawImage.gameObject.SetActive(false);
            FadeIn(true, false);
        }

        private void FadeInFinish(ShowHideAnimator obj)
        {
            OnFadeInFinish?.Invoke(obj);
            rawImage.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            base.Dispose();
            showHideAnimator.OnWillFinishStart -= FadeInFinish;
        }

        public LoadingScreenTipsView GetTipsView() =>
            tipsView;

        public LoadingScreenPercentageView GetPercentageView() =>
            percentageView;

        public void FadeIn(bool instant, bool blitTexture)
        {
            if (isVisible) return;

            if(blitTexture)
                BlitTexture();
            Show(instant);
        }

        public void FadeOut()
        {
            Hide();
        }

        public void BlitTexture()
        {
            // Call Graphics.Blit with the material as the argument. This will blit the screen into the render texture
            Graphics.Blit(null, renderTexture);
            rawImage.gameObject.SetActive(true);
        }

        public override void RefreshControl() { }

    }
}
