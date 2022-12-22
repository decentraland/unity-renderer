namespace DCL.LoadingScreen
{
    public interface ILoadingScreenView
    {
        void UpdateLoadingMessage();

        void Dispose();

        void FadeOut();
    }
}
