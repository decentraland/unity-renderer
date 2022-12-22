namespace DCL.LoadingScreen
{
    /// <summary>
    /// Display the current state of the Loading Screen Controller
    /// </summary>
    public class LoadingScreenView : BaseComponentView, ILoadingScreenView
    {
        private static readonly string PATH = "_LoadingScreen";

        public void UpdateLoadingMessage() { }

        public void FadeOut()
        {
            showHideAnimator.Hide();
        }

        public void FadeIn()
        {
            if (isVisible) return;

            //TODO: The blit to avoid the flash of the empty camera
            showHideAnimator.Show(true);
        }

        public ShowHideAnimator GetShowHideAnimator() =>
            showHideAnimator;

        public override void RefreshControl() { }

        public static LoadingScreenView Create() =>
            Create<LoadingScreenView>(PATH);
    }
}
