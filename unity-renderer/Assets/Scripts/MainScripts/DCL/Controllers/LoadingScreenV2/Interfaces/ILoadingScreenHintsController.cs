namespace DCL.LoadingScreen.V2
{
    public interface ILoadingScreenHintsController
    {
        void StartHintsCarousel();
        void StopHintsCarousel();
        void CarouselNextHint();
        void CarouselPreviousHint();
        void SetHint(int index);
        void Dispose();
    }
}
