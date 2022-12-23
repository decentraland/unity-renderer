using System;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Display the current state of the Loading Screen Controller
    /// </summary>
    public class LoadingScreenView : BaseComponentView, ILoadingScreenView
    {
        private static readonly string PATH = "_LoadingScreen";
        public event Action<ShowHideAnimator> OnFadeInFinish;

        public static LoadingScreenView Create() =>
            Create<LoadingScreenView>(PATH);

        public override void Start()
        {
            base.Start();
            showHideAnimator.OnWillFinishStart += OnFadeInFinish;
        }

        public override void Dispose()
        {
            base.Dispose();
            showHideAnimator.OnWillFinishStart -= OnFadeInFinish;
        }

        public void FadeIn(bool instant)
        {
            if (isVisible) return;

            //TODO: The blit to avoid the flash of the empty camera
            Show(instant);
        }

        public void FadeOut() =>
            Hide();

        public void UpdateLoadingMessage() { }

        public override void RefreshControl() { }
    }
}
