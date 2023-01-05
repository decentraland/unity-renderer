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

        public event Action<ShowHideAnimator> OnFadeInFinish;
        // Declare a render texture and a material that uses the render texture
        public RenderTexture renderTexture;
        public RawImage rawImage;


        public static LoadingScreenView Create() =>
            Create<LoadingScreenView>(PATH);

        public override void Start()
        {
            base.Start();
            showHideAnimator.OnWillFinishStart += OnFadeInFinish;
            //showHideAnimator.OnWillFinishStart += (e) => renderTextureContainer.gameObject.SetActive(false);

            FadeIn(true);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (renderTexture == null)
                {
                    renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
                    rawImage.texture = renderTexture;
                }
                // Call Graphics.Blit with the material as the argument. This will blit the screen into the render texture
                Graphics.Blit(null, renderTexture);
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                GetComponent<CanvasGroup>().alpha = GetComponent<CanvasGroup>().alpha.Equals(0) ? 1 : 0;
            }
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
            //renderTextureContainer.gameObject.SetActive(true);
            if (isVisible) return;

            Show(instant);
        }

        public void FadeOut()
        {
            Hide();
        }

        public override void RefreshControl() { }

    }
}
