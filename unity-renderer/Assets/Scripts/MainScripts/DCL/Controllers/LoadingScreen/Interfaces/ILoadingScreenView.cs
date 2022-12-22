namespace DCL.LoadingScreen
{
    public interface ILoadingScreenView : IBaseComponentView
    {
        void UpdateLoadingMessage();

        void Dispose();

        void FadeOut();
    }
}
