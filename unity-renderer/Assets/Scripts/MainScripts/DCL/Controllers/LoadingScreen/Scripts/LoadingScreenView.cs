using DCL.LoadingScreen.V2;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DCL.LoadingScreen
{
    /// <summary>
    ///     Display the current state of the Loading Screen Controller
    /// </summary>
    public class LoadingScreenView : BaseComponentView, ILoadingScreenView
    {
        [SerializeField] private LoadingScreenPercentageView percentageView;
        [SerializeField] private LoadingScreenTimeoutView timeoutView;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private GameObject containerViewV1;
        [SerializeField] private GameObject containerViewV2;
        [SerializeField] private RectTransform hintContainer;
        [SerializeField] private LoadingScreenV2HintsPanelView loadingScreenV2HintsPanelView;
        private RenderTexture renderTexture;
        private RectTransform rawImageRectTransform;

        public event Action<ShowHideAnimator> OnFadeInFinish;

        public void Start()
        {
            showHideAnimator.OnWillFinishStart += FadeInFinish;

            rawImageRectTransform = rawImage.GetComponent<RectTransform>();
            SetupBlitTexture();
            FadeIn(true, false);
        }

        public override void Dispose()
        {
            base.Dispose();
            showHideAnimator.OnWillFinishStart -= FadeInFinish;
        }

        public RectTransform GetHintContainer() =>
            hintContainer;

        public LoadingScreenPercentageView GetPercentageView() =>
            percentageView;

        public ILoadingScreenTimeoutView GetTimeoutView() =>
            timeoutView;

        public LoadingScreenV2HintsPanelView GetHintsPanelView() =>
            loadingScreenV2HintsPanelView;

        public void FadeIn(bool instant, bool blitTexture)
        {
            if (isVisible) return;

            //We blit the texture in case we need a static image when teleport starts
            if(blitTexture)
                BlitTexture();

            Show(instant);
        }

        public void FadeOut()
        {
            if (!isVisible) return;
            rawImage.gameObject.SetActive(false);

            Hide();
        }

        public override void RefreshControl() { }

        public void ToggleLoadingScreenV2(bool active)
        {
            containerViewV1.SetActive(!active);
            containerViewV2.SetActive(active);
        }

        private void FadeInFinish(ShowHideAnimator obj)
        {
            OnFadeInFinish?.Invoke(obj);
            rawImage.gameObject.SetActive(false);
        }

        private void BlitTexture()
        {
            //We need to add this check just in case that the resolution has changed
            if (renderTexture.width != Screen.width || renderTexture.height != Screen.height)
                SetupBlitTexture();

            Graphics.Blit(null, renderTexture);
            rawImage.gameObject.SetActive(true);
        }

        private void SetupBlitTexture()
        {
            //Blit null works differently in WebGL than in desktop platform. We have to deal with it and rotate the resultant image accordingly
            if (Application.isEditor || Application.platform != RuntimePlatform.WebGLPlayer)
                rawImageRectTransform.eulerAngles = new Vector3(180, 0, 0);

            if (renderTexture) renderTexture.Release();
            renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            rawImage.texture = renderTexture;
        }

    }
}
